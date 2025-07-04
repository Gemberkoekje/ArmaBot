#nullable enable

using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using ArmaBotCs.Extensions;
using ArmaBotCs.LocalId;
using ArmaBotCs.Permissions;
using ArmaBotCs.Posts;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Commands;

#pragma warning disable S107 // Methods should not have too many parameters
internal sealed class CreateMissionCommand(
    FeedbackService feedback,
    IDiscordRestChannelAPI channelApi,
    IDiscordRestGuildAPI guildApi,
    IAggregateRepository<Guid, Mission> repository,
    ICommandContext context,
    IPostRepository postUpdater,
    ILogger<CreateMissionCommand> logger,
    ILocalIdRepository localIdRepository,
    ICheckPermissions permissionChecker
) : CommandGroup
#pragma warning restore S107 // Methods should not have too many parameters
{
    [Command("createmission")]
    [Ephemeral]
    [Description("Creates a new mission with the provided details.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleCreateMissionCommandAsync(
        [Description("Campaign name")] string campaign,
        [Description("Modset name")] string modset,
        [Description("Operation name")] string op,
        [Description("Date (yyyy-MM-dd hh:mm:ss)")] string date,
        [Description("Channel to use")] IChannel channel,
        [Description("Role to use")] IRole role,
        [Description("Description")] string? description = null)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation("Creating mission with campaign: {Campaign}, modset: {Modset}, op: {Op}, date: {Date}, description: {Description}, channel: {Channel}, role: {Role}", campaign, modset, op, date, description, channel?.ID, role?.ID);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to create missions in this server.");
            }
            CancellationToken ct = default;
            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            // Assume 'date' is the user input string, e.g., "2025-07-01 20:00:00"
            if (!DateTime.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var localDateTime))
            {
                return await feedback.SendContextualAsync("Invalid date format. Please use yyyy-MM-dd hh:mm:ss.");
            }

            // Specify the Dutch timezone
            var dutchTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");

            // Treat the parsed time as 'Unspecified' (local to Netherlands)
            var unspecifiedLocal = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);

            // Convert to UTC
            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(unspecifiedLocal, dutchTimeZone);

            if (channel == null)
            {
                return await feedback.SendContextualAsync("Channel cannot be null. Please specify a valid channel.");
            }

            // Fetch channel
            var channelResult = await channelApi.GetChannelAsync(channel.ID, ct);
            if (!channelResult.IsSuccess)
                return await feedback.SendContextualAsync("Could not resolve the selected channel.");

            // Fetch role
            var rolesResult = await guildApi.GetGuildRolesAsync(guild, ct);
            if (!rolesResult.IsSuccess)
                return await feedback.SendContextualAsync("Could not fetch roles for the selected guild.");

            if (role == null)
            {
                return await feedback.SendContextualAsync("Role cannot be null. Please specify a valid role.");
            }

            var resolvedRole = rolesResult.Entity.FirstOrDefault(r => r.ID == role.ID);
            if (resolvedRole is null)
                return await feedback.SendContextualAsync("Could not resolve the selected role.");

            // You may want to store the role or its ID in your mission data
            var missionData = new UpdateMissionData
            {
                Campaign = campaign,
                Modset = modset,
                Op = op,
                Date = utcDateTime,
                Description = description,
                Channel = channel.ID,
                Guild = guild,
                RoleToPing = role.ID,
            };

            var mission = new Mission();
            var result = mission.SetData(missionData);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to create mission: " + result.Messages.ToFormattedString());
            }
            mission = result.Value;
            await repository.SaveAsync(mission, ct);

            await postUpdater.UpdatePostAsync(guild, mission.Id);

            var embed = new Embed(
                Title: "Mission Created",
                Description: mission.GetMission(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)));

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating mission with campaign: {Campaign}, modset: {Modset}, op: {Op}, date: {Date}, description: {Description}, channel: {Channel}, role: {Role}", campaign, modset, op, date, description, channel?.ID, role?.ID);
            return await feedback.SendContextualAsync("An error occurred while creating the mission. Please try again later.");
        }
    }

    [Command("updatemission")]
    [Ephemeral]
    [Description("Updates a mission with the provided details.")]
#pragma warning disable S107 // Methods should not have too many parameters
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
    public async Task<IResult> HandleUpdateMissionCommandAsync(
        [Description("Mission ID")] string id,
        [Description("Campaign name")] string? campaign = null,
        [Description("Modset name")] string? modset = null,
        [Description("Operation name")] string? op = null,
        [Description("Date (yyyy-MM-dd hh:mm:ss)")] string? date = null,
        [Description("Description")] string? description = null,
        [Description("Channel to use")] IChannel? channel = null,
        [Description("Role to use")] IRole? role = null)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation("Updating mission with ID: {Id}, campaign: {Campaign}, modset: {Modset}, op: {Op}, date: {Date}, description: {Description}, channel: {Channel}, role: {Role}", id, campaign, modset, op, date, description, channel?.ID, role?.ID);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to update missions in this server.");
            }
            CancellationToken ct = default;
            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            DateTime? utcDateTime = null;

            if (date != null)
            {
                // Assume 'date' is the user input string, e.g., "2025-07-01 20:00:00"
                if (!DateTime.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var localDateTime))
                {
                    return await feedback.SendContextualAsync("Invalid date format. Please use yyyy-MM-dd hh:mm:ss.");
                }

                // Specify the Dutch timezone
                var dutchTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");

                // Treat the parsed time as 'Unspecified' (local to Netherlands)
                var unspecifiedLocal = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);

                // Convert to UTC
                utcDateTime = TimeZoneInfo.ConvertTimeToUtc(unspecifiedLocal, dutchTimeZone);
            }
            if (channel != null)
            {
                // Fetch channel
                var channelResult = await channelApi.GetChannelAsync(channel.ID, ct);
                if (!channelResult.IsSuccess)
                    return await feedback.SendContextualAsync("Could not resolve the selected channel.");
            }

            if (role != null)
            {
                // Fetch role
                var rolesResult = await guildApi.GetGuildRolesAsync(guild, ct);
                if (!rolesResult.IsSuccess)
                    return await feedback.SendContextualAsync("Could not fetch roles for the selected guild.");

                var resolvedRole = rolesResult.Entity.FirstOrDefault(r => r.ID == role.ID);
                if (resolvedRole is null)
                    return await feedback.SendContextualAsync("Could not resolve the selected role.");
            }

            var mission = await repository.LoadAsync(localIdRepository.GetMissionIdByLocalId(id) ?? Guid.Empty, ct);

            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            // You may want to store the role or its ID in your mission data
            var missionData = new UpdateMissionData
            {
                Campaign = campaign,
                Modset = modset,
                Op = op,
                Date = utcDateTime,
                Description = description,
                Channel = channel?.ID,
                Guild = guild,
                RoleToPing = role?.ID,
            };

            var result = mission.SetData(missionData);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to update mission: " + result.Messages.ToFormattedString());
            }
            mission = result.Value;
            await repository.SaveAsync(mission, ct);

            await postUpdater.UpdatePostAsync(guild, mission.Id);

            var embed = new Embed(
                Title: "Mission Updated",
                Description: mission.GetMission(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)));

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating mission with ID: {Id}, campaign: {Campaign}, modset: {Modset}, op: {Op}, date: {Date}, description: {Description}, channel: {Channel}, role: {Role}", id, campaign, modset, op, date, description, channel?.ID, role?.ID);
            return await feedback.SendContextualAsync("An error occurred while updating the mission. Please try again later.");
        }
    }
}
