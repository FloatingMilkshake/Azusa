namespace Azusa.Events;

internal static class InteractionEvents
{
    internal static async Task HandleComponentInteractionCreatedEventAsync(DiscordClient _, ComponentInteractionCreatedEventArgs e)
    {
        switch (e.Id)
        {
            case "button-callback-eval-cancel":
                {
                    await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

                    bool isShell = e.Message.Content == "Executing...";

                    if (!Setup.State.Caches.CancellationTokens.TryGetValue(e.Message.Id, out CancellationTokenSource cancellationTokenSource))
                    {
                        await e.Message.ModifyAsync(new DiscordMessageBuilder().WithContent(isShell ? "Executing..." : "Evaluating...")
                        .AddActionRowComponent(new DiscordActionRowComponent(
                            [new DiscordButtonComponent(DiscordButtonStyle.Danger, "button-callback-eval-cancel", "Can't cancel (CToken missing!)", true)]
                        )));
                        return;
                    }

                    if (e.User.Id != e.Message.Reference.Message.Author.Id)
                    {
                        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().WithContent(
                                "Only the person that used this command can cancel it!").AsEphemeral(true));
                        return;
                    }

                    await e.Message.ModifyAsync(new DiscordMessageBuilder().WithContent(isShell ? "Executing..." : "Evaluating...")
                        .AddActionRowComponent(new DiscordActionRowComponent(
                            [new DiscordButtonComponent(DiscordButtonStyle.Danger, "button-callback-eval-cancel", "Cancelling...", true)]
                        )));

                    cancellationTokenSource.Cancel();

                    break;
                }
            default:
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Unknown interaction ID `{e.Id}`!").AsEphemeral(true));
                break;
        }
    }
}
