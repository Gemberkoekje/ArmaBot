using ArmaBot.Core.Enums;

namespace ArmaBot.Core.Events;

internal sealed class SideRemoved
{
    required public Side Side { get; init; }
}
