using ArmaBot.Core.Models;

namespace ArmaBot.Core.Events;

internal sealed class ResponseUpdated
{
    required public Response Response { get; init; }
}
