using Remora.Rest.Core;

namespace ArmaBot.Core.Models;

public record Response
{
    public Snowflake User { get; init; }

    public Enums.Rsvp Rsvp { get; init; }

    public Enums.Side Side { get; init; }

    public Enums.Role PrimaryRole { get; init; }

    public Enums.Role SecondaryRole { get; init; }

    public Enums.Role TertiaryRole { get; init; }
}
