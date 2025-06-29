using ArmaBotCs.Helpers;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ArmaBotCs.Commands;

internal sealed class InfoCommand(FeedbackService feedback) : CommandGroup
{
    [Command("info")]
    [Ephemeral]
    [Description("Displays information about the bot.")]
    public async Task<IResult> HandleInfoCommand()
    {
        var commands = CommandReflectionHelper.GetAllCommands();
        var infoText = string.Join("\n", commands.Select(c =>
            $"**/{c.Command}**: {c.Description ?? "No description."}"));

        var embed = new Embed(
            Colour: feedback.Theme.Secondary,
            Title: "Available Commands",
            Fields: new[] { new EmbedField(string.Empty, infoText) });

        // Send the response using the Interaction API
        return (Result)await feedback.SendContextualEmbedAsync(embed, ct: CancellationToken);
    }
}
