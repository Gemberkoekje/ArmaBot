#nullable enable
using Remora.Rest.Core;
using System;

namespace ArmaBot.Core.Events;

internal sealed class MissionDataSet
{
    required public string? Campaign { get; init; }

    required public string? Modset { get; init; }

    required public string? Op { get; init; }

    required public DateTime? Date { get; init; }

    required public string? Description { get; init; }

    required public Snowflake? Channel { get; init; }

    required public Snowflake? Guild { get; init; }

    required public Snowflake? RoleToPing { get; init; }
}
