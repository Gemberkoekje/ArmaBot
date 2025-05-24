using ArmaBot.Core.Models;
using Remora.Discord.API.Objects;
using System;
using System.Collections.Immutable;

namespace ArmaBot.Core.Events;

public sealed class MissionDataSet
{
    public required string Campaign { get; init; }

    public required string Modset { get; init; }

    public required string Op { get; init; }

    public required DateTime Date { get; init; }

    public required string Description { get; init; }

    public required Channel Channel { get; init; }

    public required User UserToPing { get; init; }

    public required ImmutableArray<Side> Sides { get; init; }
}
