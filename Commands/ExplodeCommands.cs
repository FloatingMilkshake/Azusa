namespace Azusa.Commands;

internal static class ExplodeCommands
{
    [Command("explode")]
    [TextAlias("boom", "kaboom", "kaplode", "didsomebodysayboom")]
    [Description("boom")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task ExplodeCommandAsync(TextCommandContext ctx)
    {
        var rand = new Random();
        var chance = rand.Next(4);
        
        switch (chance)
        {
            case 0:
                await ctx.RespondAsync("kaboom");
                break;
            case 1:
                await ctx.RespondAsync(":boom:");
                break;
            case 2:
                await ctx.RespondAsync("<:cat:1230731441344610374>");
                break;
            case 3:
                chance = rand.Next(5);
                if (chance == 2)
                    await ctx.RespondAsync("DID SOMEBODY SAY BOOM?");
                else
                    await ctx.RespondAsync("\\*explodes\\*");
                break;
        }
    }
}
