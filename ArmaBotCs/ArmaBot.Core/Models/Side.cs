using System.Collections.Generic;
using System.Collections.Immutable;

namespace ArmaBot.Core.Models;

public sealed class Side
{
    public required Enums.Side MySide { get; init; }

    public ImmutableArray<Division> Divisions { get; init; }
}
