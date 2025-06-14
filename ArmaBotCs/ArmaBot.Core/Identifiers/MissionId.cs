using Qowaiv.Customization;
using System;

namespace ArmaBot.Core.Identifiers;

/// <summary>
/// Represents a strongly-typed unique identifier for a mission.
/// </summary>
/// <remarks>
/// This struct wraps a <see cref="Guid"/> to provide type safety and clarity when referencing missions.
/// </remarks>
[Id<GuidBehavior, Guid>]
public readonly partial struct MissionId { }
