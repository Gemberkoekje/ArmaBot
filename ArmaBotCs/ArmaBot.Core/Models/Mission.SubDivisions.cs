using ArmaBot.Core.Events;
using Qowaiv.Validation.Abstractions;
using System.Collections.Immutable;
using System.Linq;

namespace ArmaBot.Core.Models;

/// <summary>
/// Provides subdivision management for the <see cref="Mission"/> aggregate, including adding and removing subdivisions for specific sides and divisions.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// Gets the collection of subdivisions currently associated with this mission.
    /// </summary>
    internal ImmutableArray<Subdivision> Subdivisions { get; private set; } = [];

    /// <summary>
    /// Adds a new subdivision to the specified side and division in the mission.
    /// </summary>
    /// <param name="side">The side to which the subdivision will be added.</param>
    /// <param name="divisionName">The name of the division to which the subdivision will be added.</param>
    /// <param name="subdivisionName">The name of the subdivision to add.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation errors if the subdivision already exists, or if the side or division does not exist.
    /// </returns>
    public Result<Mission> AddSubdivision(Enums.Side side, string divisionName, string subdivisionName)
    {
        if (!Divisions.Any(d => d.Side == side && d.Name == divisionName))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Division {divisionName} does not exist."));
        }
        if (!Sides.Any(s => s.MySide == side))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Side {side} does not exist."));
        }
        if (!Subdivisions.Any(d => d.Side == side && d.Division == divisionName && d.Name == subdivisionName))
        {
            return ApplyEvent(new SubdivisionAdded()
            {
                Side = side,
                DivisionName = divisionName,
                Name = subdivisionName,
            });
        }
        return this;
    }

    /// <summary>
    /// Removes an existing subdivision from the specified side and division in the mission.
    /// </summary>
    /// <param name="side">The side from which the subdivision will be removed.</param>
    /// <param name="divisionName">The name of the division from which the subdivision will be removed.</param>
    /// <param name="subdivisionName">The name of the subdivision to remove.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation errors if the subdivision does not exist.
    /// </returns>
    public Result<Mission> RemoveSubdivision(Enums.Side side, string divisionName, string subdivisionName)
    {
        if (!Subdivisions.Any(d => d.Side == side && d.Division == divisionName && d.Name == subdivisionName))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Subdivision {subdivisionName} does not exist."));
        }
        return ApplyEvent(new SubdivisionRemoved()
        {
            Side = side,
            DivisionName = divisionName,
            Name = subdivisionName,
        });
    }

    /// <summary>
    /// Handles the <see cref="SubdivisionAdded"/> event by adding the new subdivision to the <see cref="Subdivisions"/> collection.
    /// </summary>
    /// <param name="event">The event containing the side, division, and subdivision name to add.</param>
    internal void When(SubdivisionAdded @event)
    {
        Subdivisions = Subdivisions.Add(new Subdivision() { Side = @event.Side, Division = @event.DivisionName, Name = @event.Name });
    }

    /// <summary>
    /// Handles the <see cref="SubdivisionRemoved"/> event by removing the subdivision from the <see cref="Subdivisions"/> collection.
    /// </summary>
    /// <param name="event">The event containing the side, division, and subdivision name to remove.</param>
    internal void When(SubdivisionRemoved @event)
    {
        Subdivisions = Subdivisions.Remove(Subdivisions.Single(s => s.Division == @event.DivisionName && s.Side == @event.Side && s.Name == @event.Name));
        Roles = Roles.RemoveAll(r => r.Side == @event.Side && r.Division == @event.DivisionName && r.Subdivision == @event.Name);
    }
}
