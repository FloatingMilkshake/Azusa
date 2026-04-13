namespace Azusa.Commands;

internal class ShellCommands
{
    // The idea for this command, and a lot of the code, is taken from Erisa's Lykos. References are linked below.
    // https://github.com/Erisa/Lykos/blob/5f9c17c/src/Modules/Owner.cs#L116-L144
    // https://github.com/Erisa/Lykos/blob/822e9c5/src/Modules/Helpers.cs#L36-L82
    [Command("shell")]
    [TextAlias("sh")]
    [Description("Run a shell command.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task ShellCommandAsync(TextCommandContext ctx,
        [Parameter("command")] [Description("The command to run, including any arguments.")] [RemainingText]
        string command)
    {
        await ctx.RespondAsync(new DiscordMessageBuilder().WithContent("Working on it...")
            .AddActionRowComponent(new DiscordActionRowComponent(
                [new DiscordButtonComponent(DiscordButtonStyle.Danger, "button-callback-eval-cancel", "Cancel")]
            ))
        );

        var msg = await ctx.GetResponseAsync();
        Setup.State.Caches.CancellationTokens.Add(msg.Id, new CancellationTokenSource());
        var cancellationToken = Setup.State.Caches.CancellationTokens[msg.Id].Token;

        var cmdResponse = await Setup.Types.ShellCommand.RunAsync(command, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("The operation was cancelled."));
            Setup.State.Caches.CancellationTokens.Remove(msg.Id);
            return;
        }

        var splitOutput = $"```\n{cmdResponse.Output}\n{cmdResponse.Error}\n```".SplitForDiscord();

        foreach (var part in splitOutput)
        {
            await ctx.Channel.SendMessageAsync(part);
        }
        await msg.ModifyAsync(new DiscordMessageBuilder().WithContent($"\nFinished with exit code `{cmdResponse.ExitCode}`."));

        Setup.State.Caches.CancellationTokens.Remove(msg.Id);
    }
}
