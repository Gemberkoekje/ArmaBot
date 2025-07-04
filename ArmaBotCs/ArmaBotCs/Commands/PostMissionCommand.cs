#nullable enable

using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
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
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Commands;

#pragma warning disable S107 // Methods should not have too many parameters
internal sealed class PostMissionCommand(
    FeedbackService feedback,
    IDiscordRestChannelAPI channelApi,
    IAggregateRepository<Guid, Mission> repository,
    ICommandContext context,
    IPostRepository postUpdater,
    ILogger<PostMissionCommand> logger,
    ILocalIdRepository localIdRepository,
    ICheckPermissions permissionChecker
) : CommandGroup
#pragma warning restore S107 // Methods should not have too many parameters
{
    [Command("postmission")]
    [Ephemeral]
    [Description("Posts a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandlePostMissionCommand(
        [Description("Mission ID")] string id)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation(
            "Posting mission with id: {Id}",
            id);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to post missions in this server.");
            }
            CancellationToken ct = default;
            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            var mission = await repository.LoadAsync(localIdRepository.GetMissionIdByLocalId(id) ?? Guid.Empty, ct);

            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            if (mission.GetMissionData().Guild != guild)
                return await feedback.SendContextualAsync("This mission has not been made in this discord server.");

            var result = await channelApi.CreateMessageAsync(
                 mission.GetMissionData().Channel, // This should be a Snowflake
                 mission.GetMission(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)),
                 ct: ct);

            var post = new Post()
            {
                Id = mission.Id,
                Op = mission.GetMissionData().Op,
                PostId = result.Entity.ID,
                MissionDate = mission.GetMissionData().Date,
                Guild = mission.GetMissionData().Guild,
            };

            await postUpdater.AddPostAsync(post);

            var embed = new Embed(
                Title: "Mission Posted");

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while posting mission with id: {Id}", id);
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }
}
