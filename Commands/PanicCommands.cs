namespace Azusa.Commands;

internal class PanicCommands
{
    [Command("panic")]
    [Description("You know what this is for.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [Secret]
    public static async Task PanicCommandAsync(TextCommandContext ctx,
        [Parameter("who"), Description("You should ignore this if you're not milkshake.")] string who = "")
    {
        var lastPanic = JsonConvert.DeserializeObject<DateTime?>((await Setup.Storage.Redis.StringGetAsync("lastPanic")).ToString() ?? "");
        if (lastPanic is not null && lastPanic > DateTime.UtcNow.AddMinutes(-5))
        {
            await ctx.RespondAsync("Sorry, but this can only be used once every 5 minutes.");
            return;
        }

        if (who == "cf" && (ctx.User.Id != 455432936339144705 || ctx.User.Id != 208935109485789184))
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://ntfy.sh/mistralton_pager_alerts");
            request.Headers.Add("Title", "Please check Discord ASAP");
            request.Headers.Add("Priority", "urgent");
            request.Headers.Add("Tags", "warning");
            request.Content = new StringContent("Check in please");
            await Setup.Constants.HttpClient.SendAsync(request);
        }
        else
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://uptime.milkshake.services/api/push/5ccY6HAwapl41NuIaKEmPcyODh9a6oaM?msg=PANIC%20from%20bot%20by%20{ctx.User.Username}");
            await Setup.Constants.HttpClient.SendAsync(request);
        }

        await Setup.Storage.Redis.StringSetAsync("lastPanic", JsonConvert.SerializeObject(DateTime.UtcNow));

        await ctx.RespondAsync("Done.");
    }
}
