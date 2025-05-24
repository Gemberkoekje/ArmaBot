using ArmaBotCs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.Commands.Services;

var host = MyHostBuilder.CreateHostBuilder(args)
    .UseConsoleLifetime()
    .Build();

var slashService = host.Services.GetRequiredService<SlashService>();
var updateSlash = await slashService.UpdateSlashCommandsAsync();
var log = host.Services.GetRequiredService<ILogger<Program>>();
if (!updateSlash.IsSuccess)
{
    log.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Error.Message);
}

await host.RunAsync();
