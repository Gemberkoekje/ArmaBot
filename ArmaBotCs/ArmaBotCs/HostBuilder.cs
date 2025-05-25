using ArmaBot.Infrastructure.Postgress;
using ArmaBot.Infrastructure.Postgress.Podclaim;
using ArmaBotCs.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmaBotCs;

public static class MyHostBuilder
{
    /// <summary>
    /// Creates a generic application host builder.
    /// </summary>
    /// <param name="args">The arguments passed to the application.</param>
    /// <returns>The host builder.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args, string podId) => Host.CreateDefaultBuilder(args)
       .ConfigureLogging(logging => logging.AddConsole())
    .AddDiscordService(services =>
    {
        var configuration = services.GetRequiredService<IConfiguration>();

        return configuration.GetValue<string>("Discord:Token") ??
               throw new InvalidOperationException(
                   "No bot token has been provided. Set the Discord:Token environment variable to a " +
                   "valid token.");
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<DiscordGatewayClientOptions>(g => g.Intents |= GatewayIntents.MessageContents);
        services.AddSingleton(new PodIdProvider(podId));
        services.AddDiscordCommands(true)
            .AddCommandTree()
            .WithCommandGroup<InfoCommand>();
        services.AddPostgressServices(context.Configuration);
        services.AddHostedService<Worker>();
    });
}
