namespace Azusa.Commands;

public static class Eval
{
    private static readonly List<string> RestrictedTerms = ["poweroff", "shutdown", "reboot", "halt"];
    public static readonly string[] EvalImports = ["System", "System.Collections.Generic", "System.Linq",
        "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Commands",
        "DSharpPlus.Entities", "Microsoft.Extensions.Logging", "Newtonsoft.Json",
        Assembly.GetExecutingAssembly().GetName().Name];
    
    [Command("eval"), Description("Evaluate C# code.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task EvalCommand(TextCommandContext ctx, [Parameter("code"), Description("The code to evaluate."), RemainingText] string code)
    {
        if (RestrictedTerms.Any(code.Contains))
        {
            await ctx.RespondAsync("Sorry, denying this to be safe.");
            return;
        }

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
                
                // Content is too long for Discord
                if (result.ReturnValue.ToString()!.Length > 1900)
                {
                    Program.Discord.Logger.LogInformation(Program.BotEventId, "Eval result (too long for Discord): {result}", result.ReturnValue);
                    
                    await ctx.RespondAsync("Done, but the result was too long to post here! Logged to console instead.");
                    return;
                }
                
                // Respond in channel if content length within Discord character limit
                await ctx.RespondAsync(HideSensitiveInfo(result.ReturnValue.ToString()) ?? "null");
            }
        }
        catch (Exception e)
        {
            await ctx.RespondAsync(e.GetType() + ": " + e.Message);
        }
    }
    
    private static string HideSensitiveInfo(string input)
    {
        const string redacted = "[redacted]";
        var output = input.Replace(Program.ConfigJson.Token, redacted);

        return output;
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