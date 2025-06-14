using System.Linq;
using System.Text;

namespace ArmaBot.Core.Models;

/// <summary>
/// Provides a method to generate a formatted string representation of the mission, including campaign details, scheduling, Discord context, and the full organizational structure.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// Generates a detailed, human-readable string representation of the mission, including campaign, modset, operation, date, description, Discord channel, user to ping, and the hierarchical structure of sides, divisions, subdivisions, and roles.
    /// </summary>
    /// <returns>
    /// A formatted <see cref="string"/> containing all mission details and organizational structure.
    /// </returns>
    public string GetMission()
    {
        if(MissionData == null)
            return "Mission data is not set.";

        var sb = new StringBuilder();
        sb.AppendLine($"ID: {Id}");
        sb.AppendLine($"Campaign: {MissionData.Campaign}");
        sb.AppendLine($"Modset: {MissionData.Modset}");
        sb.AppendLine($"Op: {MissionData.Op}");
        sb.AppendLine($"Date: {MissionData.Date.ToString("yyyy-MM-dd")}");
        sb.AppendLine($"Description: {MissionData.Description}");
        sb.AppendLine($"Channel: {MissionData.Channel}");
        sb.AppendLine($"UserToPing: {MissionData.RoleToPing}");

        foreach (var side in Sides)
        {
            sb.AppendLine($"{side}");
            var divisionNames = Divisions.Where(d => d.Side == side.MySide).Select(d => d.Name);
            foreach (var divisionName in divisionNames)
            {
                sb.AppendLine($"  Division: {divisionName}");
                var subdivisionNames = Subdivisions.Where(s => s.Side == side.MySide && s.Division == divisionName).Select(s => s.Name);
                foreach (var subdivisionName in subdivisionNames)
                {
                    sb.AppendLine($"    Subdivision: {subdivisionName}");
                    var roles = Roles.Where(r => r.Side == side.MySide && r.Division == divisionName && r.Subdivision == subdivisionName);
                    foreach (var role in roles)
                    {
                        sb.AppendLine($"      {role.MyRole}");
                    }
                }
            }
        }
        return sb.ToString();
    }
}
