using System.Collections.Immutable;

namespace ArmaBot.Core.Models;

internal sealed class Division
{
    required public string Name { get; init; }

    public ImmutableArray<Subdivision> Subdivisions { get; init; }
}
