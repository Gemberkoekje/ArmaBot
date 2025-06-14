#nullable enable

using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
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

internal sealed class CreateMissionCommand(
    FeedbackService feedback,
    IDiscordRestChannelAPI channelApi,
    IDiscordRestGuildAPI guildApi,
    MissionRepository repository,
    ICommandContext context
) : CommandGroup
{
    [Command("missioncreate")]
    [Description("Creates a new mission with the provided details.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleCreateMissionCommandAsync(
        [Description("Campaign name")] string campaign,
        [Description("Modset name")] string modset,
        [Description("Operation name")] string op,
        [Description("Date (yyyy-MM-dd)")] string date,
        [Description("Description")] string description,
        [Description("Channel to use")] IChannel channel,
        [Description("Role to use")] IRole role
        )
#pragma warning restore S107 // Methods should not have too many parameters
    {
        CancellationToken ct = default;
        if (!context.TryGetGuildID(out var guild))
            return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

        if (!DateTime.TryParse(date, out var parsedDate))
        {
            return await feedback.SendContextualAsync("Invalid date format. Please use yyyy-MM-dd.");
        }

        // Fetch channel
        var channelResult = await channelApi.GetChannelAsync(channel.ID, ct);
        if (!channelResult.IsSuccess)
            return await feedback.SendContextualAsync("Could not resolve the selected channel.");

        // Fetch role
        var rolesResult = await guildApi.GetGuildRolesAsync(guild, ct);
        if (!rolesResult.IsSuccess)
            return await feedback.SendContextualAsync("Could not fetch roles for the selected guild.");

        var resolvedRole = rolesResult.Entity.FirstOrDefault(r => r.ID == role.ID);
        if (resolvedRole is null)
            return await feedback.SendContextualAsync("Could not resolve the selected role.");

        // You may want to store the role or its ID in your mission data
        var missionData = new MissionData
        {
            Campaign = campaign,
            Modset = modset,
            Op = op,
            Date = parsedDate,
            Description = description,
            Channel = channel.ID,
            Guild = guild,
            RoleToPing = role.ID,
        };

        var mission = new Mission();
        var result = mission.SetData(missionData);
        if (!result.IsValid)
        {
            return await feedback.SendContextualAsync("Failed to create mission: " + result.Messages);
        }
        mission = result.Value;
        await repository.SaveAsync(mission, ct);

        var embed = new Embed(
            Title: "Mission Created",
            Description: mission.GetMission()
        );

        return await feedback.SendContextualEmbedAsync(embed);
    }
}