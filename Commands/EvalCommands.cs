namespace Azusa.Commands;

internal static class EvalCommands
{
    [Command("eval")]
    [Description("Evaluate C# code.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task EvalCommandAsync(TextCommandContext ctx, [Parameter("code")] [Description("The code to evaluate.")] [RemainingText] string code)
    {
        CancellationToken cancellationToken = default;

        var builder = new DiscordMessageBuilder().WithContent("Working on it...");

        await ctx.RespondAsync(builder);
        var msg = await ctx.GetResponseAsync();

        try
        {
            var scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions.WithImports(Setup.Eval.Imports);
            scriptOptions = scriptOptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var script = CSharpScript.Create(code, scriptOptions, typeof(Setup.Eval.Globals));

            // Only offer the option to cancel if the code being evaluated supports it.
            if (code.Contains("CToken"))
            {
                builder.AddActionRowComponent(new DiscordActionRowComponent(
                    [new DiscordButtonComponent(DiscordButtonStyle.Danger, "button-callback-eval-cancel", "Cancel")]
                ));

                Setup.State.Caches.CancellationTokens.Add(msg.Id, new CancellationTokenSource());
                cancellationToken = Setup.State.Caches.CancellationTokens[msg.Id].Token;

                await msg.ModifyAsync(builder);
            }

            var result = await script.RunAsync(new Setup.Eval.Globals(Setup.State.Discord.Client, ctx, cancellationToken), cancellationToken)
                .ConfigureAwait(false);

            if (result?.ReturnValue is null)
            {
                await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("null"));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                {
                    // Isn't null, so it has to be whitespace
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithContent($"\"{result.ReturnValue}\""));
                    return;
                }

                var splitOutput = result.ReturnValue.ToString().SplitForDiscord();

                foreach (var part in splitOutput)
                {
                    await ctx.Channel.SendMessageAsync(part);
                }

                if (cancellationToken.IsCancellationRequested)
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("The operation was cancelled."));
                else
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("Done!"));
            }
        }
        catch (Exception e)
        {
            if (cancellationToken.IsCancellationRequested)
                await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("The operation was cancelled."));
            else
                await msg.ModifyAsync(new DiscordMessageBuilder().WithContent(e.GetType() + ": " + e.Message));
        }

        Setup.State.Caches.CancellationTokens.Remove(msg.Id);
    }
}
