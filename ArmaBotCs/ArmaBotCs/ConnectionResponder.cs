using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs;

internal sealed class ConnectionResponder(ILogger<ConnectionResponder> logger) : IResponder<IResumed>, IResponder<IInvalidSession>
{
    public Task<Result> RespondAsync(IResumed gatewayEvent, CancellationToken ct = default)
    {
        logger.LogInformation("Bot resumed connection.");
        return Task.FromResult(Result.FromSuccess());
    }

    public Task<Result> RespondAsync(IInvalidSession gatewayEvent, CancellationToken ct = default)
    {
        logger.LogWarning("Invalid session detected. Reconnecting...");
        return Task.FromResult(Result.FromSuccess());
    }
}
