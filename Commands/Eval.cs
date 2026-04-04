namespace Azusa.Commands;

public static class Eval
{
    public static readonly string[] EvalImports =
    [
        "System", "System.Collections.Generic", "System.Linq",
        "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Commands",
        "DSharpPlus.Entities", "Microsoft.Extensions.Logging", "Newtonsoft.Json",
        Assembly.GetExecutingAssembly().GetName().Name
    ];

    public static readonly Dictionary<ulong, CancellationTokenSource> Cancellations = new();

    [Command("eval")]
    [Description("Evaluate C# code.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task EvalCommand(TextCommandContext ctx, [Parameter("code")] [Description("The code to evaluate.")] [RemainingText] string code)
    {
        CancellationToken cancellationToken = default;

        var builder = new DiscordMessageBuilder().WithContent("Working on it...");

        await ctx.RespondAsync(builder);
        var msg = await ctx.GetResponseAsync();

        try
        {
            var scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions.WithImports(EvalImports);
            scriptOptions = scriptOptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var script = CSharpScript.Create(code, scriptOptions, typeof(Globals));

            // Only offer the option to cancel if the code being evaluated supports it.
            if (code.Contains("CToken"))
            {
                builder.AddActionRowComponent(new DiscordActionRowComponent(
                    [new DiscordButtonComponent(DiscordButtonStyle.Danger, "eval-cancel-button", "Cancel")]
                ));

                Cancellations.Add(msg.Id, new CancellationTokenSource());
                cancellationToken = Cancellations[msg.Id].Token;

                await msg.ModifyAsync(builder);
            }

            var result = await script.RunAsync(new Globals(Program.Discord, ctx, cancellationToken), cancellationToken).ConfigureAwait(false);

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

                var splitOutput = await StringHelpers.SplitStringAsync(result.ReturnValue.ToString());

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

        Cancellations.Remove(msg.Id);
    }
}

public class Globals
{
    public Globals(DiscordClient client, TextCommandContext ctx, CancellationToken cancellationToken)
    {
        Context = ctx;
        Client = client;
        Message = ctx.Message;
        Channel = ctx.Channel;
        Guild = ctx.Guild;
        User = ctx.User;
        if (Guild is not null) Member = Guild.GetMemberAsync(User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
        CToken = cancellationToken;
    }

    public DiscordClient Client { get; set; }
    public DiscordMessage Message { get; set; }
    public DiscordChannel Channel { get; set; }
    public DiscordGuild Guild { get; set; }
    public DiscordUser User { get; set; }
    public DiscordMember Member { get; set; }
    public TextCommandContext Context { get; set; }
    public CancellationToken CToken { get; set; }
}