using ArmaBot.Core.Events;
using Qowaiv.Validation.Abstractions;
using Remora.Rest.Core;
using System.Collections.Immutable;
using System.Linq;

namespace ArmaBot.Core.Models;

public partial class Mission
{
    internal ImmutableArray<Response> Responses { get; private set; } = [];

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

    public Result<Mission> Unrespond(Snowflake user)
    {
        if (!Responses.Any(r => r.User == user))
            return Result.WithMessages<Mission>(ValidationMessage.Error("You have not responded to this mission!"));

        return ApplyEvent(new ResponseRemoved()
        {
            User = user,
        });
    }

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

    internal void When(ResponseRemoved @event)
    {
        var myResponse = Responses.FirstOrDefault(r => r.User == @event.User);
        Responses = Responses.Remove(myResponse);
    }
}