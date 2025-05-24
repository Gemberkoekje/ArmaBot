using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs;

public sealed class ConnectionResponder : IResponder<IResumed>, IResponder<IInvalidSession>
{
    private readonly ILogger<ConnectionResponder> _logger;

    public ConnectionResponder(ILogger<ConnectionResponder> logger)
    {
        _logger = logger;
    }

    public Task<Result> RespondAsync(IResumed gatewayEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Bot resumed connection.");
        return Task.FromResult(Result.FromSuccess());
    }

    public Task<Result> RespondAsync(IInvalidSession gatewayEvent, CancellationToken ct = default)
    {
        _logger.LogWarning("Invalid session detected. Reconnecting...");
        return Task.FromResult(Result.FromSuccess());
    }
}
