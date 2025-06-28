using System;

namespace ArmaBotCs.Reminders;

public record Reminder
{
    public Guid Id { get; init; }

    public DateTime Date { get; init; }
}
