using ArmaBot.Core.Enums;

namespace ArmaBot.Core.Events;

internal sealed class RoleAdded
{
    required public Side Side { get; init; }

    required public string DivisionName { get; init; }

    required public string SubdivisionName { get; init; }

    required public Role Role { get; init; }
}
