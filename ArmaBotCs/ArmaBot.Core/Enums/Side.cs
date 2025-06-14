namespace ArmaBot.Core.Enums;

/// <summary>
/// Represents the possible sides/factions in Arma 3 missions.
/// </summary>
public enum Side
{
    /// <summary>
    /// No side assigned.
    /// </summary>
    None = 0,

    /// <summary>
    /// BLUFOR (Blue Force): Typically represents NATO or friendly forces.
    /// </summary>
    Blufor = 1,

    /// <summary>
    /// OPFOR (Opposing Force): Typically represents enemy or hostile forces.
    /// </summary>
    Opfor = 2,

    /// <summary>
    /// Independent: Represents factions that are neither BLUFOR nor OPFOR, such as guerrillas or resistance.
    /// </summary>
    Independent = 3,

    /// <summary>
    /// Civilian: Represents non-combatant or neutral parties.
    /// </summary>
    Civilian = 4,
}
