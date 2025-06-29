using ArmaBot.Core.Events;
using Qowaiv.Validation.Abstractions;
using Remora.Rest.Core;
using System.Collections.Immutable;
using System.Linq;

namespace ArmaBot.Core.Models;

/// <summary>
/// Provides response management for the <see cref="Mission"/> aggregate, including handling user responses (RSVPs) and their roles.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// Gets the collection of user responses (RSVPs) currently associated with this mission.
    /// </summary>
    internal ImmutableArray<Response> Responses { get; private set; } = [];

    /// <summary>
    /// Adds or updates a user's response to the mission, including RSVP status and selected roles.
    /// Validates that the side and roles exist in the mission before applying the response.
    /// </summary>
    /// <param name="response">The <see cref="Response"/> object containing the user's RSVP and role selections.</param>
    /// <returns>
    /// A <see cref="Result{Mission}"/> indicating the outcome of the operation, including validation errors if the side or roles do not exist.
    /// </returns>
    public Result<Mission> Respond(Response response)
    {
        if (!Sides.Any(s => s.MySide == response.Side))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Side {response.Side} is not in this mission!"));
        }
        if (!Roles.Any(r => r.Side == response.Side && r.MyRole == response.PrimaryRole))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Primary role {response.PrimaryRole} is not in this mission!"));
        }
        if (response.SecondaryRole != Enums.Role.None && !Roles.Any(r => r.Side == response.Side && r.MyRole == response.SecondaryRole))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Secondary role {response.SecondaryRole} is not in this mission!"));
        }
        if (response.TertiaryRole != Enums.Role.None && !Roles.Any(r => r.Side == response.Side && r.MyRole == response.TertiaryRole))
        {
            return Result.WithMessages<Mission>(ValidationMessage.Error($"Tertiary role {response.TertiaryRole} is not in this mission!"));
        }
        return ApplyEvent(new ResponseUpdated()
        {
            Response = response,
        });
    }

    /// <summary>
    /// Removes a user's response from the mission.
    /// </summary>
    /// <param name="user">The unique identifier of the user whose response should be removed.</param>
    /// <returns>
    /// A <see cref="Result{Mission}"/> indicating the outcome of the operation, including a validation error if the user has not responded.
    /// </returns>
    public Result<Mission> Unrespond(Snowflake user)
    {
        if (!Responses.Any(r => r.User == user))
            return Result.WithMessages<Mission>(ValidationMessage.Error("You have not responded to this mission!"));

        return ApplyEvent(new ResponseRemoved()
        {
            User = user,
        });
    }

    /// <summary>
    /// Handles the <see cref="ResponseUpdated"/> event by adding or updating the user's response in the <see cref="Responses"/> collection.
    /// </summary>
    /// <param name="event">The event containing the updated user response.</param>
    internal void When(ResponseUpdated @event)
    {
        var myResponse = Responses.FirstOrDefault(r => r.User == @event.Response.User);
        if (myResponse != null)
        {
            Responses = Responses.Replace(myResponse, @event.Response);
        }
        else
        {
            Responses = Responses.Add(@event.Response);
        }
    }

    /// <summary>
    /// Handles the <see cref="ResponseRemoved"/> event by removing the user's response from the <see cref="Responses"/> collection.
    /// </summary>
    /// <param name="event">The event containing the user whose response should be removed.</param>
    internal void When(ResponseRemoved @event)
    {
        var myResponse = Responses.FirstOrDefault(r => r.User == @event.User);
        Responses = Responses.Remove(myResponse);
    }
}
