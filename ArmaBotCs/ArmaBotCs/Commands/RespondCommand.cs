#nullable enable

using ArmaBot.Core.Enums;
using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using ArmaBotCs.Extensions;
using ArmaBotCs.LocalId;
using ArmaBotCs.Posts;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Results;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Commands;

internal sealed class RespondCommand(
    FeedbackService feedback,
    IAggregateRepository<Guid, Mission> repository,
    ICommandContext context,
    IPostRepository getMostRecentPost,
    IPostRepository updatePost,
    ILogger<RespondCommand> logger,
    ILocalIdRepository localIdRepository
) : CommandGroup
{
    [Command("respond")]
    [Ephemeral]
    [Description("Respond to a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleRespondCommand(
        [Description("Response")] Rsvp rsvp,
        [Description("Side")] ArmaBot.Core.Enums.Side side,
        [Description("Primary role")] ArmaBot.Core.Enums.Role primaryRole,
        [Description("Secondary role")] ArmaBot.Core.Enums.Role secondaryRole = ArmaBot.Core.Enums.Role.None,
        [Description("Tertiary role")] ArmaBot.Core.Enums.Role tertiaryRole = ArmaBot.Core.Enums.Role.None,
        [Description("Mission ID")] string? id = null)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation("Responding to mission with response: {Response}, side: {Side}, primary role: {PrimaryRole}, secondary role: {SecondaryRole}, tertary role: {TertiaryRole}, id: {Id}", rsvp, side, primaryRole, secondaryRole, tertiaryRole, id);
        try
        {
            CancellationToken ct = default;
            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            Guid? guid;
            if (id == null)
            {
                guid = getMostRecentPost.GetMostRecentPost(guild);
            }
            else
            {
                guid = localIdRepository.GetMissionIdByLocalId(id);
            }
            if (guid == null)
            {
                return await feedback.SendContextualAsync("No mission ID provided and no recent post found.");
            }

            var mission = await repository.LoadAsync(guid.Value, ct);

            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            context.TryGetUserID(out var userId);

            if (rsvp != Rsvp.No && primaryRole == ArmaBot.Core.Enums.Role.None)
                return await feedback.SendContextualAsync("You must select a primary role when responding with a yes or maybe.");

            var response = new Response()
            {
                User = userId,
                Rsvp = rsvp,
                Side = side,
                PrimaryRole = primaryRole,
                SecondaryRole = secondaryRole,
                TertiaryRole = tertiaryRole,
            };

            var result = mission.Respond(response);

            if (!result.IsValid)
            {
                return await feedback.SendContextualAsync("Failed to respond: " + result.Messages.ToFormattedString());
            }

            mission = result.Value;

            await repository.SaveAsync(mission, ct);

            await updatePost.UpdatePostAsync(guild, guid.Value);

            var embedBuilder = new EmbedBuilder()
                .WithTitle("You have replied!")
                .WithColour(Color.DarkOrange)
                .AddField("Mission", mission.GetMissionData().Op, inline: false).Entity
                .AddField("Response", rsvp.ToString(), inline: false).Entity
                .AddField("Side", side.ToString(), inline: false).Entity;

            embedBuilder.AddField("Primary pick", primaryRole.ToString(), inline: false);
            if (secondaryRole != ArmaBot.Core.Enums.Role.None)
                embedBuilder.AddField("Secondary pick", secondaryRole.ToString(), inline: false);
            if (tertiaryRole != ArmaBot.Core.Enums.Role.None)
                embedBuilder.AddField("Tertiary pick", tertiaryRole.ToString(), inline: false);

            var embed = embedBuilder.Build().Entity;

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while responding to mission with id: {Id}", id);
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }

    [Command("unrespond")]
    [Ephemeral]
    [Description("Respond that you won't come to a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleUnrespondCommand(
        [Description("Mission ID")] string? id = null)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation(
            "Unresponding to mission with response: id: {Id}",
            id);
        try
        {
            CancellationToken ct = default;
            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            Guid? guid;
            if (id == null)
            {
                guid = getMostRecentPost.GetMostRecentPost(guild);
            }
            else
            {
                guid = localIdRepository.GetMissionIdByLocalId(id);
            }
            if (guid == null)
            {
                return await feedback.SendContextualAsync("No mission ID provided and no recent post found.");
            }

            var mission = await repository.LoadAsync(guid.Value, ct);

            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            context.TryGetUserID(out var userId);

            mission = mission.Unrespond(userId).Value;

            await repository.SaveAsync(mission, ct);

            await updatePost.UpdatePostAsync(guild, guid.Value);

            var embedBuilder = new EmbedBuilder()
                .WithTitle("You have unreplied!")
                .WithColour(Color.DarkOrange)
                .AddField("Mission", mission.GetMissionData().Op, inline: false).Entity
                .AddField("Response", "No", inline: false).Entity;

            var embed = embedBuilder.Build().Entity;

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while unresponding to mission with id: {Id}", id);
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }
}
