using ArmaBot.Core.Enums;

namespace ArmaBot.Core.Events;

internal sealed class DivisionRemoved
{
    required public Side Side { get; init; }

    required public string Name { get; init; }
}
