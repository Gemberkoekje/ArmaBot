using ArmaBot.Core.Enums;
using System.Collections.Immutable;

namespace ArmaBot.Core.Models;

internal sealed class Subdivision
{
    required public string Name { get; init; }

    required public ImmutableArray<Role> Roles { get; init; }
}
