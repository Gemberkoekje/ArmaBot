using ArmaBot.Core.Validators;
using Qowaiv.DomainModel;
using Qowaiv.Validation.Abstractions;
using System;
using System.Collections.Generic;

namespace ArmaBot.Core.Models;

/// <summary>
/// Represents an Arma 3 mission aggregate, including its unique identifier and validation logic.
/// </summary>
public sealed partial class Mission : Aggregate<Mission, Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Mission"/> class with a new unique mission identifier.
    /// </summary>
    public Mission() : this(Guid.NewGuid())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mission"/> class with the specified mission identifier.
    /// </summary>
    /// <param name="aggregateId">The unique identifier for the mission aggregate.</param>
    public Mission(Guid aggregateId) : base(aggregateId, new MissionValidator())
    {
    }

    /// <summary>
    /// Applies a sequence of events to the mission aggregate and returns the resulting state.
    /// </summary>
    /// <param name="events">The collection of events to apply to the mission.</param>
    /// <returns>
    /// A <see cref="Result{Mission}"/> representing the outcome of applying the events, including validation results.
    /// </returns>
    public Result<Mission> ApplyMissionEvents(IEnumerable<object> events)
    {
        return Apply(events);
    }

    /// <summary>
    /// Marks all events in the buffer as committed, indicating they have been processed and persisted.
    /// </summary>
    public void MarkAllAsCommitted()
    {
        Buffer = Buffer.MarkAllAsCommitted();
    }
}
