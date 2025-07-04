using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs;

internal sealed class ReadyResponder(
    ILogger<ReadyResponder> logger,
    SlashService slashService,
    IConfiguration configuration
) : IResponder<IReady>
{
    private readonly ILogger<ReadyResponder> _logger = logger;
    private readonly SlashService _slashService = slashService;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Bot is ready! Logged in as {Username}", gatewayEvent.User.Username);

#if DEBUG
        // Read GuildId from configuration
        var guildIdString = _configuration["Discord:GuildId"];
        if (!ulong.TryParse(guildIdString, out var guildId))
        {
            _logger.LogError("Invalid or missing Discord:GuildId in configuration.");
            return Result.FromError(new InvalidOperationError("Invalid or missing Discord:GuildId in configuration."));
        }

        var result = await _slashService.UpdateSlashCommandsAsync(new Snowflake(guildId), ct: ct);
        _logger.LogInformation("Registered slash commands for guild {GuildId}", guildId);
#else
        // Register (refresh) slash commands globally
        var result = await _slashService.UpdateSlashCommandsAsync(ct: ct);
        _logger.LogInformation("Registered global slash commands");
#endif

        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to register Discord commands: {Error}", result.Error?.Message);
            return Result.FromError(result.Error!);
        }

        _logger.LogInformation("Discord commands registered successfully.");
        return Result.FromSuccess();
    }
}
