using ArmaBot.Core.Enums;
using System.Collections.Immutable;

namespace ArmaBot.Core.Models;

public sealed class Subdivision
{
    public required string Name { get; init; }

    public required ImmutableArray<Role> Roles { get; init; }
}
