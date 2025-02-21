namespace Azusa.Commands;

public static class Ping
{
    [Command("ping")]
    [Description("Pong!")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task PingCommand(TextCommandContext ctx)
    {
        await ctx.RespondAsync("Ping!");
        var msg = await ctx.GetResponseAsync();
        
        var rtt = ((msg?.Id - ctx.Message.Id) >> 22) ?? 0;
        var ping = ctx.Client.GetConnectionLatency(0).TotalMilliseconds;
        
        await ctx.EditResponseAsync($"Pong! Latency `{ping}` RTT `{rtt}`");
    }
}