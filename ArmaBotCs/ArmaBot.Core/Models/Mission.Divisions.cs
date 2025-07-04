using ArmaBot.Core.Events;
using Qowaiv.Validation.Abstractions;
using System.Collections.Immutable;
using System.Linq;

namespace ArmaBot.Core.Models;

/// <summary>
/// Provides division management for the <see cref="Mission"/> aggregate, including adding and removing divisions for specific sides.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// Gets the collection of divisions currently associated with this mission.
    /// </summary>
    internal ImmutableArray<Division> Divisions { get; private set; } = [];

    /// <summary>
    /// Adds a new division to the specified side in the mission.
    /// </summary>
    /// <param name="side">The side to which the division will be added.</param>
    /// <param name="divisionName">The name of the division to add.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation errors if the division already exists or the side does not exist.
    /// </returns>
    public Result<Mission> AddDivision(Enums.Side side, string divisionName)
    {
        if (!Sides.Any(s => s.MySide == side))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Side {side} does not exist."));
        }
        if (!Divisions.Any(d => d.Side == side && d.Name == divisionName))
        {
            return ApplyEvent(new DivisionAdded()
            {
                Side = side,
                Name = divisionName,
            });
        }
        return this;
    }

    /// <summary>
    /// Removes an existing division from the specified side in the mission.
    /// </summary>
    /// <param name="side">The side from which the division will be removed.</param>
    /// <param name="divisionName">The name of the division to remove.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation errors if the division does not exist.
    /// </returns>
    public Result<Mission> RemoveDivision(Enums.Side side, string divisionName)
    {
        if (!Divisions.Any(d => d.Side == side && d.Name == divisionName))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Division {divisionName} does not exist."));
        }
        return ApplyEvent(new DivisionRemoved()
        {
            Side = side,
            Name = divisionName,
        });
    }

    /// <summary>
    /// Handles the <see cref="DivisionAdded"/> event by adding the new division to the <see cref="Divisions"/> collection.
    /// </summary>
    /// <param name="event">The event containing the side and division name to add.</param>
    internal void When(DivisionAdded @event)
    {
        Divisions = Divisions.Add(new Division() { Side = @event.Side, Name = @event.Name });
    }

    /// <summary>
    /// Handles the <see cref="DivisionRemoved"/> event by removing the division from the <see cref="Divisions"/> collection.
    /// </summary>
    /// <param name="event">The event containing the side and division name to remove.</param>
    internal void When(DivisionRemoved @event)
    {
        Divisions = Divisions.Remove(Divisions.Single(d => d.Side == @event.Side && d.Name == @event.Name));
        Subdivisions = Subdivisions.RemoveAll(r => r.Side == @event.Side && r.Division == @event.Name);
        Roles = Roles.RemoveAll(r => r.Side == @event.Side && r.Division == @event.Name);
    }
}
