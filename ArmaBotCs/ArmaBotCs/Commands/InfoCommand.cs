using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ArmaBotCs.Commands;

public sealed class InfoCommand : CommandGroup
{
    private readonly FeedbackService _feedback;

    public InfoCommand(FeedbackService feedback)
    {
        _feedback = feedback;
    }

    [Command("info")]
    [Description("Displays information about the bot.")]
    public async Task<IResult> HandleInfoCommand()
    {
        const string response = "Hello! I am ArmaBot, your mission management assistant. Use `/missioncreate` to create a mission!";

        var embed = new Embed(
            Colour: _feedback.Theme.Secondary,
            Title: "Armabot CS!",
            Fields: new[]
            {
                new EmbedField(string.Empty, response),
            });

        // Send the response using the Interaction API
        return (Result)await _feedback.SendContextualEmbedAsync(embed, ct: CancellationToken);
    }
}
