#nullable enable

using ArmaBot.Core.Enums;
using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using ArmaBotCs.LocalId;
using ArmaBotCs.Permissions;
using ArmaBotCs.Posts;
using Microsoft.Extensions.Logging;
using Qowaiv.Identifiers;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Commands;

internal sealed class ShowMissionCommand(
    FeedbackService feedback,
    IAggregateRepository<Guid, Mission> repository,
    ICommandContext context,
    IPostRepository postRepository,
    ILogger<ShowMissionCommand> logger,
    ILocalIdRepository localIdRepository,
    ICheckPermissions permissionChecker
) : CommandGroup
{
    [Command("showmission")]
    [Ephemeral]
    [Description("Shows a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleShowMissionCommand(
        [Description("Mission ID")] string id)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation(
            "Showing mission with id: {Id}",
            id);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualErrorAsync("You do not have permission to show missions in this server.");
            }
            CancellationToken ct = default;
            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualErrorAsync("This command must be used in a server (guild) context.");

            var mission = await repository.LoadAsync(localIdRepository.GetMissionIdByLocalId(id) ?? Guid.Empty, ct);

            if (mission == null)
                return await feedback.SendContextualErrorAsync("Mission not found.");

            if (mission.GetMissionData().Guild != guild)
                return await feedback.SendContextualErrorAsync("This mission has not been made in this discord server.");

            return await feedback.SendContextualAsync($"<@&{mission.GetMissionData().RoleToPing}> on <#{mission.GetMissionData().Channel}>", embeds: new[] { mission.GetMissionDataEmbed(id), mission.GetMissionSidesEmbed(), mission.GetMissionResponsesEmbed() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while showing mission with id: {Id}", id);
            return await feedback.SendContextualErrorAsync("An error occurred while processing your request. Please try again later.");
        }
    }

    [Command("showmissions")]
    [Ephemeral]
    [Description("Shows all missions.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleShowMissionsCommand()
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation(
            "Showing all missions");
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualErrorAsync("You do not have permission to show missions in this server.",options: new FeedbackMessageOptions(MessageFlags: MessageFlags.Ephemeral));
            }
            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualErrorAsync("This command must be used in a server (guild) context.");

            var posts = postRepository.GetAllPosts(guild);

            var list = string.Empty;
            foreach (var post in posts)
            {
                var unixTimestamp = new DateTimeOffset(post.MissionDate).ToUnixTimeSeconds();
                list = $"{list}{await localIdRepository.GetOrAddLocalIdAsync(post.Id)} | {post.Op} | <t:{unixTimestamp}:F>\n";
            }

            var embed = new Embed(
                Title: "All missions in database:",
                Description: list);

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while showing all missions");
            return await feedback.SendContextualErrorAsync("An error occurred while processing your request. Please try again later.");
        }
    }
}
