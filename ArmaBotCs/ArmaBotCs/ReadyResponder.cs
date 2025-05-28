using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs;

internal sealed class ReadyResponder(ILogger<ReadyResponder> logger) : IResponder<IReady>
{
    private readonly ILogger<ReadyResponder> _logger = logger;

    public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Bot is ready! Logged in as {Username}", gatewayEvent.User.Username);
        return Task.FromResult(Result.FromSuccess());
    }
}
