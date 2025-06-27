using Marten;
using Qowaiv;
using Qowaiv.Hashing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.Postgress.Podclaim;

/// <summary>
/// Provides functionality to claim and release pods using a Discord token as an identifier.
/// Handles concurrency and ensures that pods are not double-claimed within a timeout period.
/// </summary>
public sealed class PodClaimService(IDocumentStore store) : IPodClaimService, IDisposable
{
    private readonly TimeSpan _claimTimeout = TimeSpan.FromMinutes(1);

    private string _discordToken;
    private string _podId;
    private bool _hasClaim;

    /// <summary>
    /// Attempts to claim a pod for the specified Discord user.
    /// Ensures that a pod cannot be double-claimed within the claim timeout period.
    /// </summary>
    /// <param name="discordToken">The Discord token identifying the user attempting to claim the pod.</param>
    /// <param name="podId">The unique identifier of the pod to claim.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains <c>true</c> if the pod was successfully claimed; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> TryClaimPodAsync(string discordToken, string podId, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Starting tryclaimpod: {discordToken.GetHashCode()}, {podId.GetHashCode()}");
        using var session = store.LightweightSession();

        var existing = await session.Query<PodClaim>()
            .Where(c => c.DiscordToken == discordToken)
            .FirstOrDefaultAsync(cancellationToken);

        var now = Clock.UtcNow();

        if (existing != null && existing.PodId != podId && existing.ClaimedAt > now - _claimTimeout)
        {
            Console.WriteLine("Already claimed");
            return false;
        }

        session.Store(new PodClaim
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            DiscordToken = discordToken,
            PodId = podId,
            ClaimedAt = now,
        });
        try
        {
            await session.SaveChangesAsync(cancellationToken);
        }
        catch (Marten.Exceptions.ConcurrentUpdateException)
        {
            // If we hit a concurrency exception, it means another claim was made in the meantime.
            // We can safely return false here.
            Console.WriteLine("Concurrency exception");
            return false;
        }

        // Store claim info for disposal
        _discordToken = discordToken;
        _podId = podId;
        _hasClaim = true;

        return true;
    }

    /// <summary>
    /// Releases a previously claimed pod for the specified Discord user.
    /// Removes the claim from the data store if it exists.
    /// </summary>
    /// <param name="discordToken">The Discord token identifying the user releasing the pod.</param>
    /// <param name="podId">The unique identifier of the pod to release.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ReleasePodAsync(string discordToken, string podId, CancellationToken cancellationToken)
    {
        using var session = store.LightweightSession();
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

    /// <summary>
    /// Releases the claimed pod if one exists when the service is disposed.
    /// </summary>
    public void Dispose()
    {
        if (_hasClaim && _discordToken != null && _podId != null)
        {
            // Fire and forget, as Dispose cannot be async
            _ = ReleasePodAsync(_discordToken, _podId, CancellationToken.None);
        }
    }
}
