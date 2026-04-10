namespace Azusa.Commands;

internal static class PingCommands
{
    [Command("ping")]
    [TextAlias("pingf")]
    [Description("Pong!")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task PingCommandAsync(TextCommandContext ctx)
    {
        await ctx.RespondAsync("Ping!");
        var msg = await ctx.GetResponseAsync();

        var rtt = (msg?.Id - ctx.Message.Id) >> 22 ?? 0;
        var ping = ctx.Client.GetConnectionLatency(0).TotalMilliseconds;

        string redisLatency;
        try
        {
            redisLatency = $"{(await Setup.Storage.Redis.PingAsync()).TotalMilliseconds.ToString()}ms";
        }
        catch
        {
            redisLatency = "Unreachable!";
        }
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Pong!\n"
            + $"Websocket `{ping}ms`\n"
            + $"RTT `{rtt}ms`\n"
            + $"Redis `{redisLatency}`"));
    }
}
