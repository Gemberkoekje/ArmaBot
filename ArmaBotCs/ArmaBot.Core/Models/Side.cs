using System.Collections.Immutable;

namespace ArmaBot.Core.Models;

internal sealed class Side
{
    required public Enums.Side MySide { get; init; }

    public ImmutableArray<Division> Divisions { get; init; }
}
