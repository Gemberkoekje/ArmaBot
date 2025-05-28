using ArmaBot.Core.Models;
using Remora.Discord.API.Objects;
using System;
using System.Collections.Immutable;

namespace ArmaBot.Core.Events;

internal sealed class MissionDataSet
{
    required public string Campaign { get; init; }

    required public string Modset { get; init; }

    required public string Op { get; init; }

    required public DateTime Date { get; init; }

    required public string Description { get; init; }

    required public Channel Channel { get; init; }

    required public User UserToPing { get; init; }

    required public ImmutableArray<Side> Sides { get; init; }
}
