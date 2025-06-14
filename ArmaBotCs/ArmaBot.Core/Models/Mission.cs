using ArmaBot.Core.Identifiers;
using ArmaBot.Core.Validators;
using FluentValidation;
using Qowaiv.DomainModel;
using Qowaiv.Validation.Abstractions;
using System.Collections.Generic;

namespace ArmaBot.Core.Models;

/// <summary>
/// Represents an Arma 3 mission aggregate, including its unique identifier and validation logic.
/// </summary>
public partial class Mission : Aggregate<Mission, MissionId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Mission"/> class with a new unique mission identifier.
    /// </summary>
    public Mission() : this(MissionId.Next())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mission"/> class with the specified mission identifier.
    /// </summary>
    /// <param name="aggregateId">The unique identifier for the mission aggregate.</param>
    public Mission(MissionId aggregateId) : base(aggregateId, new MissionValidator())
    {
    }

    public Result<Mission> ApplyEvents(IEnumerable<object> events)
    {
        return Apply(events);
    }
}
