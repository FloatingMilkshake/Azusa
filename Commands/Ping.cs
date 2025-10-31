namespace Azusa.Commands;

public static class Ping
{
    [Command("ping")]
    [Description("Pong!")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [TextAlias("pingf")]
    public static async Task PingCommand(TextCommandContext ctx)
    {
        await ctx.RespondAsync("Ping!");
        var msg = await ctx.GetResponseAsync();

        var rtt = (msg?.Id - ctx.Message.Id) >> 22 ?? 0;
        var ping = ctx.Client.GetConnectionLatency(0).TotalMilliseconds;
        
        var dbPing = await CheckDatabaseLatencyAsync();
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Pong!\n"
            + $"Websocket `{ping}ms`\n"
            + $"RTT `{rtt}ms`\n"
            + $"Redis `{dbPing}ms`\n"));
    }
    
    private static async Task<string> CheckDatabaseLatencyAsync()
    {
        try
        {
            return (await Program.Redis.PingAsync()).TotalMilliseconds.ToString();
        }
        catch
        {
            return "Unreachable!";
        }
    }
}
