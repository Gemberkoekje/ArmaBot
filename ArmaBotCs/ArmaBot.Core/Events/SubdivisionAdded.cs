using ArmaBot.Core.Enums;

namespace ArmaBot.Core.Events;

internal sealed class SubdivisionAdded
{
    required public Side Side { get; init; }

    required public string DivisionName { get; init; }

    required public string Name { get; init; }
}
