namespace Azusa.Helpers;

public class CommandHelpers
{
    public static async Task FailOnMissingInfo(TextCommandContext ctx)
    {
        await ctx.RespondAsync("This command is disabled! Please make sure you have provided values for all of the necessary keys in the config file.");
    }
}