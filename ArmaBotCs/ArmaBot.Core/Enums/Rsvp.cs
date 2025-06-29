namespace ArmaBot.Core.Enums;

/// <summary>
/// Represents the RSVP (Répondez s'il vous plaît) status for a mission or event.
/// </summary>
public enum Rsvp
{
    /// <summary>
    /// No RSVP response has been given.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates a positive RSVP; the participant will attend the event.
    /// </summary>
    Yes = 1,

    /// <summary>
    /// Indicates an uncertain RSVP; the participant might attend the event.
    /// </summary>
    Maybe = 2,
}
