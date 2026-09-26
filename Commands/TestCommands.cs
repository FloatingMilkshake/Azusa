namespace Azusa.Commands;

public class TestCommands
{
    [Command("test")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task TestCommandAsync(TextCommandContext ctx)
    {
        await ctx.RespondAsync("meow");
    }
}
