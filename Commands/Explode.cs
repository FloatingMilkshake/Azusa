namespace Azusa.Commands;

public static class Explode
{
    [Command("explode")]
    [Description("boom")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [TextAlias("boom")]
    public static async Task PingCommand(TextCommandContext ctx)
    {
        var rand = new Random();
        var chance = rand.Next(3);
        
        switch (chance)
        {
            case 0:
                await ctx.RespondAsync("kaboom");
                break;
            case 1:
                await ctx.RespondAsync(":boom:");
                break;
            case 2:
                chance = rand.Next(5);
                if (chance == 2)
                    await ctx.RespondAsync("DID SOMEBODY SAY BOOM?");
                else
                    await ctx.RespondAsync("\\*explodes\\*");
                break;
        }
    }
}