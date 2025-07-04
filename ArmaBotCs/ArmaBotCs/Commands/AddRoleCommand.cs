#nullable enable

using ArmaBot.Core.Enums;
using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using ArmaBotCs.Extensions;
using ArmaBotCs.LocalId;
using ArmaBotCs.Permissions;
using ArmaBotCs.Posts;
using DotNetProjectFile.MsBuild;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Commands;

internal sealed class AddRoleCommand(
    FeedbackService feedback,
    IAggregateRepository<Guid, Mission> repository,
    ICommandContext context,
    IPostRepository postRepository,
    ILogger<AddRoleCommand> logger,
    ILocalIdRepository localIdRepository,
    ICheckPermissions permissionChecker
) : CommandGroup
{
    [Command("addrole")]
    [Ephemeral]
    [Description("Sets a role for a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleAddRoleCommandAsync(
        [Description("Mission id")] string id,
        [Description("Side")] Side side,
        [Description("Division")] string division,
        [Description("Subdivision")] string subdivision,
        [Description("Role")] ArmaBot.Core.Enums.Role role)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation("Adding role with id: {Id}, side: {Side}, division: {Division}, subdivision: {Subdivision}, role: {Role}", id, side, division, subdivision, role);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to add roles in this server.");
            }
            var guid = localIdRepository.GetMissionIdByLocalId(id) ?? Guid.Empty;
            CancellationToken ct = default;

            var mission = await repository.LoadAsync(guid, ct);
            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            if (mission.GetMissionData().Guild != guild)
                return await feedback.SendContextualAsync("This mission has not been made in this discord server.");

            var result = mission.AddSide(side);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to add side: " + result.Messages.ToFormattedString());
            }
            result = result.Value.AddDivision(side, division);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to add division: " + result.Messages.ToFormattedString());
            }
            result = result.Value.AddSubdivision(side, division, subdivision);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to add subdivision: " + result.Messages.ToFormattedString());
            }
            result = result.Value.AddRole(side, division, subdivision, role);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to add role: " + result.Messages.ToFormattedString());
            }

            mission = result.Value;
            await repository.SaveAsync(mission, ct);

            await postRepository.UpdatePostAsync(mission.GetMissionData().Guild, mission.Id);

            var embed = new Embed(
                Title: "Mission Updated",
                Description: mission.GetMission(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)));

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while adding role to mission with id: {Id}", id);
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }

    [Command("removerole")]
    [Ephemeral]
    [Description("Removes a role for a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleRemoveRoleCommandAsync(
        [Description("Mission id")] string id,
        [Description("Side")] Side side,
        [Description("Division")] string division,
        [Description("Subdivision")] string subdivision,
        [Description("Role")] ArmaBot.Core.Enums.Role role)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation("Removing role with id: {Id}, side: {Side}, division: {Division}, subdivision: {Subdivision}, role: {Role}", id, side, division, subdivision, role);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to remove roles in this server.");
            }
            var guid = localIdRepository.GetMissionIdByLocalId(id) ?? Guid.Empty;
            CancellationToken ct = default;

            var mission = await repository.LoadAsync(guid, ct);
            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            if (mission.GetMissionData().Guild != guild)
                return await feedback.SendContextualAsync("This mission has not been made in this discord server.");

            var result = mission.RemoveRole(side, division, subdivision, role);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to remove role: " + result.Messages.ToFormattedString());
            }

            mission = result.Value;
            await repository.SaveAsync(mission, ct);

            await postRepository.UpdatePostAsync(mission.GetMissionData().Guild, mission.Id);

            var embed = new Embed(
                Title: "Mission Updated",
                Description: mission.GetMission(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)));

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while removing role to mission with id: {Id}", id);
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }

    [Command("removesubdivision")]
    [Ephemeral]
    [Description("Removes a subdivision for a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleRemoveSubdivisionCommandAsync(
        [Description("Mission id")] string id,
        [Description("Side")] Side side,
        [Description("Division")] string division,
        [Description("Subdivision")] string subdivision)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation("Removing subdivision with id: {Id}, side: {Side}, division: {Division}, subdivision: {Subdivision}", id, side, division, subdivision);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to remove subdivisions in this server.");
            }
            var guid = localIdRepository.GetMissionIdByLocalId(id) ?? Guid.Empty;
            CancellationToken ct = default;

            var mission = await repository.LoadAsync(guid, ct);
            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            if (mission.GetMissionData().Guild != guild)
                return await feedback.SendContextualAsync("This mission has not been made in this discord server.");

            var result = mission.RemoveSubdivision(side, division, subdivision);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to remove subdivision: " + result.Messages.ToFormattedString());
            }

            mission = result.Value;
            await repository.SaveAsync(mission, ct);

            await postRepository.UpdatePostAsync(mission.GetMissionData().Guild, mission.Id);

            var embed = new Embed(
                Title: "Mission Updated",
                Description: mission.GetMission(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)));

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while removing subdivision to mission with id: {Id}", id);
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }

    [Command("removedivision")]
    [Ephemeral]
    [Description("Removes a division for a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleRemoveSubdivisionCommandAsync(
        [Description("Mission id")] string id,
        [Description("Side")] Side side,
        [Description("Division")] string division)
#pragma warning restore S107 // Methods should not have too many parameters
    {

        logger.LogInformation("Removing division with id: {Id}, side: {Side}, division: {Division}", id, side, division);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to remove divisions in this server.");
            }
            var guid = localIdRepository.GetMissionIdByLocalId(id) ?? Guid.Empty;
            CancellationToken ct = default;

            var mission = await repository.LoadAsync(guid, ct);
            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            if (mission.GetMissionData().Guild != guild)
                return await feedback.SendContextualAsync("This mission has not been made in this discord server.");

            var result = mission.RemoveDivision(side, division);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to remove division: " + result.Messages.ToFormattedString());
            }

            mission = result.Value;
            await repository.SaveAsync(mission, ct);

            await postRepository.UpdatePostAsync(mission.GetMissionData().Guild, mission.Id);

            var embed = new Embed(
                Title: "Mission Updated",
                Description: mission.GetMission(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)));

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while removing division to mission with id: {Id}", id);
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }

    [Command("removeside")]
    [Ephemeral]
    [Description("Removes a side for a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleRemoveSideCommandAsync(
        [Description("Mission id")] string id,
        [Description("Side")] Side side)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation("Removing side with id: {Id}, side: {Side}", id, side);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to remove sides in this server.");
            }
            var guid = localIdRepository.GetMissionIdByLocalId(id) ?? Guid.Empty;
            CancellationToken ct = default;

            var mission = await repository.LoadAsync(guid, ct);
            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            if (mission.GetMissionData().Guild != guild)
                return await feedback.SendContextualAsync("This mission has not been made in this discord server.");

            var result = mission.RemoveSide(side);
            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to remove side: " + result.Messages.ToFormattedString());
            }

            mission = result.Value;
            await repository.SaveAsync(mission, ct);

            await postRepository.UpdatePostAsync(mission.GetMissionData().Guild, mission.Id);

            var embed = new Embed(
                Title: "Mission Updated",
                Description: mission.GetMission(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)));

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while removing side to mission with id: {Id}", id);
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }
}
