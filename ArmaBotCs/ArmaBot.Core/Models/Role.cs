namespace ArmaBot.Core.Models;

internal sealed class Role
{
    required public Enums.Side Side { get; init; }

    required public string Division { get; init; }

    required public string Subdivision { get; init; }

    required public Enums.Role MyRole { get; init; }
}
