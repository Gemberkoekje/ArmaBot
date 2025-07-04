using Remora.Discord.API.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ArmaBot.Core.Models;

/// <summary>
/// Provides methods to generate formatted representations of the mission, including campaign details, scheduling, Discord context, and the full organizational structure.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// Generates a detailed, human-readable string representation of the mission, including campaign, modset, operation, date, description, Discord channel, user to ping, and the hierarchical structure of sides, divisions, subdivisions, and roles.
    /// </summary>
    /// <returns>
    /// A formatted <see cref="string"/> containing all mission details and organizational structure.
    /// </returns>
    public string GetMission(string localId)
    {
        if (MissionData == null)
            return "Mission data is not set.";

        var unixTimestamp = new DateTimeOffset(MissionData.Date).ToUnixTimeSeconds();
        var sb = new StringBuilder();
        sb.AppendLine($"ID: {localId}");
        sb.AppendLine($"Campaign: {MissionData.Campaign}");
        sb.AppendLine($"Modset: {MissionData.Modset}");
        sb.AppendLine($"Op: {MissionData.Op}");
        sb.AppendLine($"Date: <t:{unixTimestamp}:F> <t:{unixTimestamp}:R>");
        sb.AppendLine($"Description: {MissionData.Description}");
        sb.AppendLine($"Channel: <#{MissionData.Channel}>");
        sb.AppendLine($"UserToPing: <@&{MissionData.RoleToPing}>");

        var sideNames = Sides.Select(side => side.MySide);
        foreach (var sideName in sideNames)
        {
            sb.AppendLine($"{sideName}");
            var divisionNames = Divisions.Where(d => d.Side == sideName).Select(d => d.Name);
            foreach (var divisionName in divisionNames)
            {
                sb.AppendLine($" - Division: {divisionName}");
                var subdivisionNames = Subdivisions.Where(s => s.Side == sideName && s.Division == divisionName).Select(s => s.Name);
                foreach (var subdivisionName in subdivisionNames)
                {
                    sb.AppendLine($"   - Subdivision: {subdivisionName}");
                    var roles = Roles.Where(r => r.Side == sideName && r.Division == divisionName && r.Subdivision == subdivisionName);
                    foreach (var role in roles)
                    {
                        sb.AppendLine($"     - {role.MyRole}");
                    }
                }
            }
        }
        sb.AppendLine($"Responses:");
        foreach (var response in Responses)
        {
            sb.AppendLine($" - User: <@{response.User}> | Rsvp: {response.Rsvp} | Side: {response.Side} | Primary Role: {response.PrimaryRole} | Secondary Role: {response.SecondaryRole} | Tertiary Role: {response.TertiaryRole}");
        }

        sb.AppendLine($"Total Responses: {Responses.Count()}");
        sb.AppendLine($"Yes Responses: {Responses.Count(r => r.Rsvp == Enums.Rsvp.Yes)}");
        sb.AppendLine($"Maybe Responses: {Responses.Count(r => r.Rsvp == Enums.Rsvp.Maybe)}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a Discord embed containing the main mission data, such as campaign, modset, operation, date, and description.
    /// </summary>
    /// <returns>
    /// An <see cref="Embed"/> object with the mission's primary details formatted for Discord.
    /// </returns>
    public Embed GetMissionDataEmbed(string localId)
    {
        var unixTimestamp = new DateTimeOffset(MissionData.Date).ToUnixTimeSeconds();
        return new Embed()
        {
            Title = $"Mission: {MissionData?.Op} ({localId})",
            Colour = Color.DarkOrange,
            Fields = new[]
            {
                new EmbedField("Campaign", MissionData?.Campaign ?? "N/A", IsInline: false),
                new EmbedField("Modset", MissionData?.Modset ?? "N/A", IsInline: false),
                new EmbedField("Op", MissionData?.Op ?? "N/A", IsInline: false),
                new EmbedField("Date", $"<t:{unixTimestamp}:F> <t:{unixTimestamp}:R>" ?? "N/A", IsInline: false),
                new EmbedField("Description", MissionData?.Description ?? "N/A", IsInline: false),
            },
        };
    }

    /// <summary>
    /// Generates a Discord embed representing the hierarchical structure of the mission's sides, divisions, subdivisions, and roles.
    /// </summary>
    /// <returns>
    /// An <see cref="Embed"/> object displaying the composition of the mission.
    /// </returns>
    public Embed GetMissionSidesEmbed()
    {
        var embedfields = new List<EmbedField>();
        var sideNames = Sides.Select(side => side.MySide);
        foreach (var sideName in sideNames)
        {
            var sb = new StringBuilder();
            sb.AppendLine("```");
            var divisionNames = Divisions.Where(d => d.Side == sideName).Select(d => d.Name);
            foreach (var divisionName in divisionNames)
            {
                sb.AppendLine($"{divisionName}");
                var subdivisionNames = Subdivisions.Where(s => s.Side == sideName && s.Division == divisionName).Select(s => s.Name);
                foreach (var subdivisionName in subdivisionNames)
                {
                    sb.AppendLine($"  {subdivisionName}");
                    var roles = Roles.Where(r => r.Side == sideName && r.Division == divisionName && r.Subdivision == subdivisionName);
                    foreach (var role in roles)
                    {
                        sb.AppendLine($"    {role.MyRole}");
                    }
                }
            }
            sb.AppendLine("```");
            embedfields.Add(new EmbedField(sideName.ToString(), sb.ToString()));
        }
        return new Embed()
        {
            Title = $"Composition",
            Colour = Color.DarkOrange,
            Fields = embedfields.ToArray(),
        };
    }

    /// <summary>
    /// Generates a Discord embed summarizing all user responses to the mission, including their selected roles and RSVP status.
    /// </summary>
    /// <returns>
    /// An <see cref="Embed"/> object listing user responses and RSVP statistics.
    /// </returns>
    public Embed GetMissionResponsesEmbed()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Responses:");
        foreach (var response in Responses.Where(r => r.Rsvp != Enums.Rsvp.No))
        {
            sb.Append($"**<@{response.User}>**: {response.PrimaryRole}");
            if (response.SecondaryRole != Enums.Role.None)
            {
                sb.Append($" | {response.SecondaryRole}");
            }
            if (response.TertiaryRole != Enums.Role.None)
            {
                sb.Append($" | {response.TertiaryRole}");
            }
            sb.AppendLine();
        }
        foreach (var response in Responses.Where(r => r.Rsvp == Enums.Rsvp.No))
        {
            sb.Append($"**<@{response.User}>**: Will not attend.");
            sb.AppendLine();
        }
        return new Embed()
        {
            Title = $"Composition",
            Colour = Color.DarkOrange,
            Fields = new[]
            {
                new EmbedField(string.Empty, sb.ToString()),
                new EmbedField("Total rsvps", $"{Responses.Where(r => r.Rsvp != Enums.Rsvp.No).Count()}", IsInline: false),
                new EmbedField("Maybe rsvps", $"{Responses.Count(r => r.Rsvp == Enums.Rsvp.Maybe)}", IsInline: false),
                new EmbedField(string.Empty, "Please use /respond to reply whether you want to join this mission!\r\nYou can use /respond multiple times to change your reply,\r\nor you can use /unrespond to remove your response if you can't come after all!"),
            },
        };
    }
}
