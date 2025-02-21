namespace Azusa.Commands;

public class PingCommands
{
    [Command("ping")]
    [Description("Pong!")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public async Task Ping(TextCommandContext ctx)
    {
        await ctx.RespondAsync("Ping!");
        var msg = await ctx.GetResponseAsync();
        var rtt = (msg.Id - ctx.Message.Id) >> 22;
        var ping = ctx.Client.GetConnectionLatency(0).TotalMilliseconds;
        
        await ctx.EditResponseAsync($"Pong! Latency `{ping}` RTT `{rtt}`");
    }
}