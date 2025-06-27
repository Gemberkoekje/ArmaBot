using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.Interfaces;
using ArmaBot.Infrastructure.MartenDb;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qowaiv.DomainModel;
using Remora.Discord.API.Abstractions.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs;

public class ReminderBackgroundTask : BackgroundService, IUpdateReminderBackgroundTask
{
    public List<Reminder> Reminders;
    public IServiceProvider _serviceProvider;

    public ReminderBackgroundTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        using var session = serviceProvider.GetService<IDocumentStore>().QuerySession();
        Reminders = session.Query<Reminder>().ToList();
    }


    public async Task UpdateReminder(Reminder reminder)
    {
        var savedReminder = Reminders.FirstOrDefault(r => r.Id == reminder.Id);
        if (savedReminder != null)
        {
            savedReminder = savedReminder with { Date = reminder.Date};
        }
        else
        {
            Reminders.Add(reminder);
        }
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var dueReminders = Reminders
                .Where(r => r.Date <= now)
                .ToList();

            foreach (var reminder in dueReminders)
            {
                await SendDiscordMessageAsync(reminder, stoppingToken);
                Reminders.Remove(reminder);
                using var scope = _serviceProvider.CreateScope();
                using var session = scope.ServiceProvider.GetService<IDocumentStore>().LightweightSession();
                session.Delete(reminder);
                await session.SaveChangesAsync(stoppingToken);
            }

            // Wait for a short interval before checking again
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task SendDiscordMessageAsync(Reminder reminder, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var _repository = scope.ServiceProvider.GetService<IAggregateRepository<Mission>>();
        var _channelApi = scope.ServiceProvider.GetService<IDiscordRestChannelAPI>();
        var mission = await _repository.LoadAsync(reminder.Id, token);
        var result = await _channelApi.CreateMessageAsync(
            mission.GetMissionData().Channel, // This should be a Snowflake
            $"<@&{mission.GetMissionData().RoleToPing}>⏰ Reminder: {mission.GetMissionData().Op} is due at {reminder.Date:u}",
            ct: token
        );

        if (!result.IsSuccess)
        {
            // Handle error (logging, retry, etc.)
            Console.WriteLine($"Failed to send reminder: {result.Error.Message}");
        }
        else
        {
            Console.WriteLine($"Reminder triggered: {reminder.Id} at {reminder.Date}");
        }
        
    }
}
