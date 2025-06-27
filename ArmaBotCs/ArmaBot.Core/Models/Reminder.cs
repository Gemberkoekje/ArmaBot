using Remora.Rest.Core;
using System;

namespace ArmaBot.Core.Models;

public record Reminder
{
    public Guid Id { get; init; }

    public DateTime Date { get; init; }
}
