namespace Azusa.Commands;

public class WhoAreYou
{
    [Command("whoareyou")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task WhoAreYouCommand(TextCommandContext ctx)
    {
#if DEBUG
        await ctx.RespondAsync("dev");
#else
        await ctx.RespondAsync("prod");
#endif
    }
}
