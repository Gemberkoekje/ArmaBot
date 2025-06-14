namespace ArmaBot.Core.Enums;

/// <summary>
/// Represents the various roles available in Arma 3 missions.
/// </summary>
public enum Role
{
    /// <summary>
    /// No specific role assigned.
    /// </summary>
    None = 0,

    /// <summary>
    /// Squad Leader: Commands a squad and coordinates its actions.
    /// </summary>
    SquadLeader = 1,

    /// <summary>
    /// Team Leader: Leads a fireteam within a squad.
    /// </summary>
    TeamLeader = 2,

    /// <summary>
    /// Radio Telephone Operator: Handles long-range communications.
    /// </summary>
    RTO = 3,

    /// <summary>
    /// Forward Observer / JTAC: Coordinates indirect fire and air support.
    /// </summary>
    FO_JTAC = 4,

    /// <summary>
    /// Medic: Provides medical assistance and revives teammates.
    /// </summary>
    Medic = 5,

    /// <summary>
    /// Engineer: Repairs vehicles, defuses explosives, and constructs fortifications.
    /// </summary>
    Engineer = 6,

    /// <summary>
    /// CBRN Specialist: Handles chemical, biological, radiological, and nuclear threats.
    /// </summary>
    CBRNSpecialist = 7,

    /// <summary>
    /// Automatic Rifleman: Provides suppressive fire with a light machine gun.
    /// </summary>
    AutoRifleman = 8,

    /// <summary>
    /// Anti-Tank: Equipped to destroy armored vehicles.
    /// </summary>
    AntiTank = 9,

    /// <summary>
    /// Grenadier: Uses grenade launchers to provide explosive support.
    /// </summary>
    Grenadier = 10,

    /// <summary>
    /// Sapper: Specializes in explosives and demolition.
    /// </summary>
    Sapper = 11,

    /// <summary>
    /// Anti-Air: Equipped to engage and destroy aircraft.
    /// </summary>
    AntiAir = 12,

    /// <summary>
    /// Marksman: Provides accurate long-range fire support.
    /// </summary>
    Marksman = 13,

    /// <summary>
    /// Sniper: Specializes in long-range precision shooting and reconnaissance.
    /// </summary>
    Sniper = 14,

    /// <summary>
    /// Rifleman (AT): Rifleman equipped with anti-tank weaponry.
    /// </summary>
    RiflemanAT = 15,

    /// <summary>
    /// Rifleman: Standard infantry role.
    /// </summary>
    Rifleman = 16,

    /// <summary>
    /// Gunner: Operates vehicle or static weapons.
    /// </summary>
    Gunner = 17,

    /// <summary>
    /// Assistant: Assists a specialist, such as a gunner or anti-tank operator.
    /// </summary>
    Assistant = 18,

    /// <summary>
    /// UAV Operator: Controls unmanned aerial vehicles for reconnaissance or support.
    /// </summary>
    UAVOperator = 19,

    /// <summary>
    /// Commander: Overall leader, often in charge of multiple squads or vehicles.
    /// </summary>
    Commander = 20,

    /// <summary>
    /// Driver: Operates ground vehicles.
    /// </summary>
    Driver = 21,

    /// <summary>
    /// Pilot: Flies helicopters or fixed-wing aircraft.
    /// </summary>
    Pilot = 22,

    /// <summary>
    /// Co-Pilot: Assists the pilot, may operate systems or take control if needed.
    /// </summary>
    CoPilot = 23,

    /// <summary>
    /// Indirect Fire Specialist: Operates mortars or artillery.
    /// </summary>
    IndirectFireSpecialist = 24,

    /// <summary>
    /// VIP: Very Important Person, often a mission objective or protected individual.
    /// </summary>
    VIP = 25,
}
