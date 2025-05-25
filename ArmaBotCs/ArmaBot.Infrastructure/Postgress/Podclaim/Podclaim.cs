using Marten;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.Postgress.Podclaim;

public interface IPodClaimService
{
    Task<bool> TryClaimPodAsync(string discordToken, string podId, CancellationToken cancellationToken);

    Task ReleasePodAsync(string discordToken, string podId, CancellationToken cancellationToken);
}

public class PodClaim
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Required by Marten
    public string DiscordToken { get; set; }
    public string PodId { get; set; }
    public DateTime ClaimedAt { get; set; }
}

public class PodClaimService : IPodClaimService, IDisposable
{
    private readonly IDocumentStore _store;
    private readonly TimeSpan _claimTimeout = TimeSpan.FromMinutes(1);

    private string? _discordToken;
    private string? _podId;
    private bool _hasClaim;

    public PodClaimService(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<bool> TryClaimPodAsync(string discordToken, string podId, CancellationToken cancellationToken)
    {
        using var session = _store.LightweightSession();

        var existing = await session.Query<PodClaim>()
            .Where(c => c.DiscordToken == discordToken)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;

        if (existing != null)
        {
            if (existing.PodId != podId && existing.ClaimedAt > now - _claimTimeout)
                return false;
        }

        session.Store(new PodClaim
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            DiscordToken = discordToken,
            PodId = podId,
            ClaimedAt = now
        });

        await session.SaveChangesAsync(cancellationToken);

        // Store claim info for disposal
        _discordToken = discordToken;
        _podId = podId;
        _hasClaim = true;

        return true;
    }

    public async Task ReleasePodAsync(string discordToken, string podId, CancellationToken cancellationToken)
    {
        using var session = _store.LightweightSession();
        var claim = await session.Query<PodClaim>()
            .Where(c => c.DiscordToken == discordToken && c.PodId == podId)
            .FirstOrDefaultAsync(cancellationToken);

        if (claim != null)
        {
            session.Delete(claim);
            await session.SaveChangesAsync(cancellationToken);
        }

        // Clear claim info
        if (_discordToken == discordToken && _podId == podId)
            _hasClaim = false;
    }

    public void Dispose()
    {
        if (_hasClaim && _discordToken != null && _podId != null)
        {
            // Fire and forget, as Dispose cannot be async
            _ = ReleasePodAsync(_discordToken, _podId, CancellationToken.None);
        }
    }
}