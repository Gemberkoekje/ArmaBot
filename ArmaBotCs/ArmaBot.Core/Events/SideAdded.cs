using ArmaBot.Core.Enums;

namespace ArmaBot.Core.Events;

internal sealed class SideAdded
{
    required public Side Side { get; init; }
}
