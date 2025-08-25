namespace Azusa.Commands;

public static class Uptime
{
    [Command("uptime")]
    [Description("Check my uptime.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task UptimeCommand(TextCommandContext ctx)
    {
        await ctx.RespondAsync((DateTime.Now - Process.GetCurrentProcess().StartTime).ToString());
    }
}