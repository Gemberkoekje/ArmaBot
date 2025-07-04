namespace ArmaBot.Core.Enums;

/// <summary>
/// Represents the RSVP (R�pondez s'il vous pla�t) status for a mission or event.
/// </summary>
public enum Rsvp
{
    /// <summary>
    /// Indicates a negative RSVP; the participant will not attend the event.
    /// </summary>
    No = 0,

    /// <summary>
    /// Indicates a positive RSVP; the participant will attend the event.
    /// </summary>
    Yes = 1,

    /// <summary>
    /// Indicates an uncertain RSVP; the participant might attend the event.
    /// </summary>
    Maybe = 2,
}
