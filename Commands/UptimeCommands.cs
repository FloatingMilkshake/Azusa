namespace Azusa.Commands;

internal static class UptimeCommands
{
    [Command("uptime")]
    [TextAlias("up")]
    [Description("Check my uptime.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task UptimeCommandAsync(TextCommandContext ctx)
    {
        await ctx.RespondAsync((DateTime.Now - Process.GetCurrentProcess().StartTime).ToString());
    }
}
