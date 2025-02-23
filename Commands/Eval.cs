namespace Azusa.Commands;

public static class Eval
{
    private static readonly List<string> RestrictedTerms = ["poweroff", "shutdown", "reboot", "halt"];

    private static readonly string[] EvalImports =
    [
        "System", "System.Collections.Generic", "System.Linq",
        "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Commands",
        "DSharpPlus.Entities", "Microsoft.Extensions.Logging", "Newtonsoft.Json",
        Assembly.GetExecutingAssembly().GetName().Name
    ];

    [Command("eval")]
    [Description("Evaluate C# code.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task EvalCommand(TextCommandContext ctx, [Parameter("code")] [Description("The code to evaluate.")] [RemainingText] string code)
    {
        if (RestrictedTerms.Any(code.Contains))
        {
            await ctx.RespondAsync("Sorry, denying this to be safe.");
            return;
        }
        
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":hourglass:"));

        try
        {
            Globals globals = new(Program.Discord, ctx);

            var scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions.WithImports(EvalImports);
            scriptOptions = scriptOptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var script = CSharpScript.Create(code, scriptOptions, typeof(Globals));
            script.Compile();
            var result = await script.RunAsync(globals).ConfigureAwait(false);

            if (result?.ReturnValue is null)
            {
                await ctx.RespondAsync("null");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                {
                    // Isn't null, so it has to be whitespace
                    await ctx.RespondAsync($"\"{result.ReturnValue}\"");

                    return;
                }

                await StringHelpers.SplitStringAsync(result.ReturnValue.ToString(), true, ctx: ctx);
            }
        }
        catch (Exception e)
        {
            try
            {
                await ctx.RespondAsync(e.GetType() + ": " + e.Message);
            }
            catch
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
                return;
            }
        }
        
        await ctx.Message.DeleteReactionAsync(DiscordEmoji.FromName(ctx.Client, ":hourglass:"), ctx.Client.CurrentUser);
    }
}

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
public class Globals
{
    public Globals(DiscordClient client, TextCommandContext ctx)
    {
        Context = ctx;
        Client = client;
        Message = ctx.Message;
        Channel = ctx.Channel;
        Guild = ctx.Guild;
        User = ctx.User;
        if (Guild is not null) Member = Guild.GetMemberAsync(User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public DiscordClient Client { get; set; }
    public DiscordMessage Message { get; set; }
    public DiscordChannel Channel { get; set; }
    public DiscordGuild Guild { get; set; }
    public DiscordUser User { get; set; }
    public DiscordMember Member { get; set; }
    public TextCommandContext Context { get; set; }
}