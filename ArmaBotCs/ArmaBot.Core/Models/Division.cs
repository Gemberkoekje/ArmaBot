using System.Collections.Generic;
using System.Collections.Immutable;

namespace ArmaBot.Core.Models;

public sealed class Division
{
    public required string Name { get; init; }

    public ImmutableArray<Subdivision> Subdivisions { get; init; }
}
