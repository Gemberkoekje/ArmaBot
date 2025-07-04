namespace ArmaBot.Core.Models;

internal sealed class Subdivision
{
    required public Enums.Side Side { get; init; }

    required public string Division { get; init; }

    required public string Name { get; init; }
}
