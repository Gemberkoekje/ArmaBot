#nullable enable

using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using ArmaBotCs.Extensions;
using ArmaBotCs.LocalId;
using ArmaBotCs.Permissions;
using ArmaBotCs.Posts;
using Microsoft.Extensions.Logging;
using Qowaiv.Identifiers;
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Commands;

#pragma warning disable S107 // Methods should not have too many parameters
internal sealed class ExportMissionCommand(
    FeedbackService feedback,
    IAggregateRepository<Guid, Mission> repository,
    ICommandContext context,
    IPostRepository postUpdater,
    JsonSerializerOptions jsonSerializerOptions,
    ILogger<ExportMissionCommand> logger,
    ILocalIdRepository localIdRepository,
    ICheckPermissions permissionChecker
) : CommandGroup
#pragma warning restore S107 // Methods should not have too many parameters
{
    [Command("exportmission")]
    [Ephemeral]
    [Description("Exports a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
    public async Task<IResult> HandleExportMissionCommand(
        [Description("Mission ID")] string id)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation(
            "Exporting mission with id: {Id}",
            id);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to export missions in this server.");
            }
            CancellationToken ct = default;
            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");

            var mission = await repository.LoadAsync(localIdRepository.GetMissionIdByLocalId(id) ?? Guid.Empty, ct);

            if (mission == null)
                return await feedback.SendContextualAsync("Mission not found.");

            if (mission.GetMissionData().Guild != guild)
                return await feedback.SendContextualAsync("This mission has not been made in this discord server.");

            var export = mission.ExportMission();

            var serializedExport = JsonSerializer.Serialize(export, jsonSerializerOptions);

            var embed = new Embed(
                Title: "Mission Exported:",
                Description: $"```{serializedExport}```");

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while exporting mission with id: {Id}", id);
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }

    [Command("importmission")]
    [Ephemeral]
    [Description("Imports a mission.")]
#pragma warning disable S107 // Methods should not have too many parameters
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
    public async Task<IResult> HandleImportMissionCommand(
        [Description("json")] string json)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S107 // Methods should not have too many parameters
    {
        logger.LogInformation(
            "Importing mission with json: {Json}",
            json);
        try
        {
            if (!await permissionChecker.CheckPermissionsAsync(context, DiscordPermission.ManageChannels))
            {
                return await feedback.SendContextualAsync("You do not have permission to import missions in this server.");
            }
            CancellationToken ct = default;
            if (!context.TryGetGuildID(out var guild))
                return await feedback.SendContextualAsync("This command must be used in a server (guild) context.");
            ArmaBot.Core.ImportExport.Mission? importedMission = null;
            try
            {
                importedMission = JsonSerializer.Deserialize<ArmaBot.Core.ImportExport.Mission>(json, jsonSerializerOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize imported mission JSON: {Json}", json);
                return await feedback.SendContextualAsync($"{ex.Message}");
            }
            if (importedMission == null)
                return await feedback.SendContextualAsync("Failed to deserialize imported mission.");

            var mission = new Mission();

            mission = mission.SetData(new UpdateMissionData()
            {
                Campaign = importedMission.Campaign,
                Modset = importedMission.Modset,
                Op = importedMission.Op,
                Date = importedMission.Date,
                Description = importedMission.Description,
                Channel = importedMission.Channel,
                Guild = guild,
                RoleToPing = importedMission.RoleToPing,
            }).Value;

            foreach (var side in importedMission.Sides)
            {
                var sideresult = mission.AddSide(side.SideName);
                if (!sideresult.IsValid)
                {
                    return await feedback.SendContextualAsync("Failed to add side: " + sideresult.Messages.ToFormattedString());
                }
                mission = sideresult.Value;
                foreach (var division in side.Divisions)
                {
                    var divisionresult = mission.AddDivision(side.SideName, division.Name);
                    if (!divisionresult.IsValid)
                    {
                        return await feedback.SendContextualAsync("Failed to add division: " + divisionresult.Messages.ToFormattedString());
                    }
                    mission = divisionresult.Value;
                    foreach (var subdivision in division.Subdivisions)
                    {
                        var subdivisionresult = mission.AddSubdivision(side.SideName, division.Name, subdivision.Name);
                        if (!subdivisionresult.IsValid)
                        {
                            return await feedback.SendContextualAsync("Failed to add subdivision: " + subdivisionresult.Messages.ToFormattedString());
                        }
                        mission = subdivisionresult.Value;
                        foreach (var role in subdivision.Roles)
                        {
                            var result = mission.AddRole(side.SideName, division.Name, subdivision.Name, role);
                            if (!result.IsValid)
                            {
                                return await feedback.SendContextualAsync("Failed to add role: " + result.Messages.ToFormattedString());
                            }
                            mission = result.Value;
                        }
                    }
                }
            }

            await repository.SaveAsync(mission, ct);

            await postUpdater.UpdatePostAsync(guild, mission.Id);

            var embed = new Embed(
                Title: "Mission Imported",
                Description: mission.GetMission(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)));

            return await feedback.SendContextualEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while importing mission");
            return await feedback.SendContextualAsync("An error occurred while processing your request. Please try again later.");
        }
    }
}
