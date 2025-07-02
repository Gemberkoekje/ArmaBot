using System;

namespace ArmaBotCs.LocalId;

public sealed record LocalId
{
    required public Guid MissionId { get; init; }

    required public string Id { get; init; } // This is the local ID, which is a string representation of the mission ID.
}
