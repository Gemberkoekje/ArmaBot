using ArmaBot.Core.Events;
using Qowaiv.Validation.Abstractions;
using System.Collections.Immutable;
using System.Linq;

namespace ArmaBot.Core.Models;

/// <summary>
/// Provides role management for the <see cref="Mission"/> aggregate, including adding and removing roles for specific sides, divisions, and subdivisions.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// Gets the collection of roles currently associated with this mission.
    /// </summary>
    internal ImmutableArray<Role> Roles { get; private set; } = [];

    /// <summary>
    /// Adds a new role to the specified side, division, and subdivision in the mission.
    /// </summary>
    /// <param name="side">The side to which the role will be added.</param>
    /// <param name="divisionName">The name of the division to which the role will be added.</param>
    /// <param name="subdivisionName">The name of the subdivision to which the role will be added.</param>
    /// <param name="role">The role to add.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation errors if the side, division, or subdivision does not exist.
    /// </returns>
    public Result<Mission> AddRole(Enums.Side side, string divisionName, string subdivisionName, Enums.Role role)
    {
        if (!Subdivisions.Any(d => d.Side == side && d.Division == divisionName && d.Name == subdivisionName))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Subdivision {subdivisionName} does not exist."));
        }
        if (!Divisions.Any(d => d.Side == side && d.Name == divisionName))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Division {divisionName} does not exist."));
        }
        if (!Sides.Any(s => s.MySide == side))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Side {side} does not exist."));
        }
        return ApplyEvent(new RoleAdded()
        {
            Side = side,
            DivisionName = divisionName,
            SubdivisionName = subdivisionName,
            Role = role,
        });
    }

    /// <summary>
    /// Removes an existing role from the specified side, division, and subdivision in the mission.
    /// </summary>
    /// <param name="side">The side from which the role will be removed.</param>
    /// <param name="divisionName">The name of the division from which the role will be removed.</param>
    /// <param name="subdivisionName">The name of the subdivision from which the role will be removed.</param>
    /// <param name="role">The role to remove.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation errors if the role is not found.
    /// </returns>
    public Result<Mission> RemoveRole(Enums.Side side, string divisionName, string subdivisionName, Enums.Role role)
    {
        if (!Roles.Any(d => d.Side == side && d.Division == divisionName && d.Subdivision == subdivisionName && d.MyRole == role))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Role {role} not found."));
        }

        return ApplyEvent(new RoleRemoved()
        {
            Side = side,
            DivisionName = divisionName,
            SubdivisionName = subdivisionName,
            Role = role,
        });
    }

    /// <summary>
    /// Handles the <see cref="RoleAdded"/> event by adding the new role to the <see cref="Roles"/> collection.
    /// </summary>
    /// <param name="event">The event containing the side, division, subdivision, and role to add.</param>
    internal void When(RoleAdded @event)
    {
        Roles = Roles.Add(new Role() { Side = @event.Side, Division = @event.DivisionName, Subdivision = @event.SubdivisionName, MyRole = @event.Role });
    }

    /// <summary>
    /// Handles the <see cref="RoleRemoved"/> event by removing the role from the <see cref="Roles"/> collection.
    /// </summary>
    /// <param name="event">The event containing the side, division, subdivision, and role to remove.</param>
    internal void When(RoleRemoved @event)
    {
        Roles = Roles.Remove(Roles.Last(r => r.Side == @event.Side && r.Division == @event.DivisionName && r.Subdivision == @event.SubdivisionName && r.MyRole == @event.Role));
    }
}
