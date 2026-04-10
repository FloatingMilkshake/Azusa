namespace Azusa.Commands;

internal static class UptimeCommands
{
    [Command("uptime")]
    [TextAlias("up")]
    [Description("Check my uptime.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task UptimeCommandAsync(TextCommandContext ctx)
    {
        var uptime = (DateTime.Now - Process.GetCurrentProcess().StartTime);
        await ctx.RespondAsync($"{uptime.Humanize()} ({uptime})");
    }
}
