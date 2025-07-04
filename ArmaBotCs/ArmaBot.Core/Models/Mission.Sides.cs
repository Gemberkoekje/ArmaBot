using ArmaBot.Core.Events;
using Qowaiv.Validation.Abstractions;
using System.Collections.Immutable;
using System.Linq;

namespace ArmaBot.Core.Models;

/// <summary>
/// Provides side management for the <see cref="Mission"/> aggregate, including adding and removing sides (factions) for the mission.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// Gets the collection of sides (factions) currently associated with this mission.
    /// </summary>
    internal ImmutableArray<Side> Sides { get; private set; } = [];

    /// <summary>
    /// Adds a new side (faction) to the mission.
    /// </summary>
    /// <param name="sideData">The side to add (e.g., BLUFOR, OPFOR, Independent, Civilian).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation errors if the side already exists.
    /// </returns>
    public Result<Mission> AddSide(Enums.Side sideData)
    {
        if (!Sides.Any(s => s.MySide == sideData))
        {
            return ApplyEvent(new SideAdded()
            {
                Side = sideData,
            });
        }
        return this;
    }

    /// <summary>
    /// Removes an existing side (faction) from the mission.
    /// </summary>
    /// <param name="sideData">The side to remove.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation errors if the side does not exist.</returns>
    public Result<Mission> RemoveSide(Enums.Side sideData)
    {
        if (!Sides.Any(s => s.MySide == sideData))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Side {sideData} does not exist."));
        }
        return ApplyEvent(new SideRemoved()
        {
            Side = sideData,
        });
    }

    /// <summary>
    /// Handles the <see cref="SideAdded"/> event by adding the new side to the <see cref="Sides"/> collection.
    /// </summary>
    /// <param name="event">The event containing the side to add.</param>
    internal void When(SideAdded @event)
    {
        Sides = Sides.Add(new Side() { MySide = @event.Side });
    }

    /// <summary>
    /// Handles the <see cref="SideRemoved"/> event by removing the side from the <see cref="Sides"/> collection.
    /// </summary>
    /// <param name="event">The event containing the side to remove.</param>
    internal void When(SideRemoved @event)
    {
        Sides = Sides.Remove(Sides.Single(s => s.MySide == @event.Side));
        Divisions = Divisions.RemoveAll(r => r.Side == @event.Side);
        Subdivisions = Subdivisions.RemoveAll(r => r.Side == @event.Side);
        Roles = Roles.RemoveAll(r => r.Side == @event.Side);
    }
}
