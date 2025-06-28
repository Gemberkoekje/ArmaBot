using Remora.Rest.Core;

namespace ArmaBot.Core.Events;

public class ResponseRemoved
{
    public Snowflake User { get; init; }
}
