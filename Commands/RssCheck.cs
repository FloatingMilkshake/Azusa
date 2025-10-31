namespace Azusa.Commands;

public class RssCheck
{
    [Command("rsscheck")]
    [Description("Check the last MonitoRSS delivery time.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [TextAlias("checkrss")]
    public static async Task RssCheckCommand(TextCommandContext ctx)
    {
        var storedLastDelivery = await Program.Redis.StringGetAsync("monitorssLastDelivery");
        if (!storedLastDelivery.HasValue)
        {
            await ctx.RespondAsync("unknown");
            return;
        }
        
        try
        {
            var lastDeliveryTime = JsonConvert.DeserializeObject<DateTime>(storedLastDelivery);
            await ctx.RespondAsync($"{lastDeliveryTime:o}; {lastDeliveryTime.Humanize()}");
        }
        catch
        {
            await ctx.RespondAsync($"failed to deserialize...\n```\n{storedLastDelivery}\n```");
            return;
        }
    }
}