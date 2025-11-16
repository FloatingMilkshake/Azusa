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
            var split = storedLastDelivery.ToString().Split("@");
            var lastDeliveryTime = JsonConvert.DeserializeObject<DateTime>(split.First());
            var msgLink = split.First() == split.Last() ? "" : $": {split.Last()}";
            await ctx.RespondAsync($"{lastDeliveryTime:o}; {lastDeliveryTime.Humanize()}{msgLink}");
        }
        catch
        {
            await ctx.RespondAsync($"failed to deserialize...\n```\n{storedLastDelivery}\n```");
            return;
        }
    }
}