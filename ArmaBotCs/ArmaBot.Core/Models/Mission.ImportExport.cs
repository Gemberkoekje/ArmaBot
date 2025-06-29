using System.Collections.Immutable;
using System.Linq;

namespace ArmaBot.Core.Models;

public partial class Mission
{
    public ImportExport.Mission ExportMission()
    {
        return new ImportExport.Mission
        {
            Campaign = MissionData.Campaign,
            Modset = MissionData.Modset,
            Op = MissionData.Op,
            Date = MissionData.Date,
            Description = MissionData.Description,
            Channel = MissionData.Channel,
            RoleToPing = MissionData.RoleToPing,
            Sides = Sides.Select(s => ExportSide(s.MySide)).ToImmutableArray(),
        };
    }

    private ImportExport.Mission.Side ExportSide(Enums.Side side)
    {
        return new ImportExport.Mission.Side
        {
            SideName = side,
            Divisions = Divisions.Where(d => d.Side == side).Select(d => ExportDivision(d.Side, d.Name)).ToImmutableArray(),
        };
    }

    private ImportExport.Mission.Side.Division ExportDivision(Enums.Side side, string division)
    {
        return new ImportExport.Mission.Side.Division
        {
            Name = division,
            Subdivisions = Subdivisions.Where(s => s.Side == side && s.Division == division).Select(s => ExportSubdivision(s.Side,s.Division,s.Name)).ToImmutableArray(),
        };
    }

    private ImportExport.Mission.Side.Division.Subdivision ExportSubdivision(Enums.Side side, string division, string subdivision)
    {
        return new ImportExport.Mission.Side.Division.Subdivision
        {
            Name = subdivision,
            Roles = Roles.Where(r => r.Side == side && r.Division == division && r.Subdivision == subdivision).Select(r => r.MyRole).ToImmutableArray(),
        };
    }
}
