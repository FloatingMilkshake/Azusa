using static Azusa.Commands.Eval;

namespace Azusa.Events
{
    public class InteractionEvents
    {
        public static async Task ComponentInteractionCreated(DiscordClient _, ComponentInteractionCreatedEventArgs e)
        {
            switch (e.Id)
            {
                case "eval-cancel-button":
                    {
                        if (!Cancellations.ContainsKey(e.Message.Id))
                        {
                            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                new DiscordInteractionResponseBuilder().WithContent(
                                    "Unknown task! I can't cancel this, sorry. Are you sure it's still running?").AsEphemeral());
                            return;
                        }

                        if (e.User.Id != e.Message.Reference.Message.Author.Id)
                        {
                            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                new DiscordInteractionResponseBuilder().WithContent(
                                    "Only the person that used this command can cancel it!").AsEphemeral());
                            return;
                        }

                        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

                        await e.Message.ModifyAsync(new DiscordMessageBuilder().WithContent("Working on it...")
                            .AddActionRowComponent(new DiscordActionRowComponent(
                                [new DiscordButtonComponent(DiscordButtonStyle.Danger, "eval-cancel-button", "Cancelling...", true)]
                            )));

                        Cancellations[e.Message.Id].Cancel();

                        break;
                    }
                default:
                    await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent($"Unknown interaction ID `{e.Id}`!").AsEphemeral());
                    break;
            }
        }
    }
}
