namespace Azusa.Commands;

public class Panic
{
    public static readonly List<ulong> authorized = [455432936339144705, 208935109485789184, 455428041586376729, 573984492713279512];

    [Command("panic"), Description("You know what this is for.")]
    public static async Task PanicCommand(TextCommandContext ctx, [Parameter("who"), Description("You should ignore this if you're not milkshake.")] string who = "")
    {
        if (!authorized.Contains(ctx.User.Id))
        {
            await ctx.RespondAsync("Sorry, you can't use this.");
            return;
        }

        var lastPanic = JsonConvert.DeserializeObject<DateTime?>((await Program.Redis.StringGetAsync("lastPanic")).ToString() ?? "");
        if (lastPanic is not null && lastPanic > DateTime.UtcNow.AddMinutes(-5))
        {
            await ctx.RespondAsync("Sorry, but this can only be used once every 5 minutes.");
            return;
        }

        if ((ctx.User.Id == 455432936339144705 || ctx.User.Id == 208935109485789184) && who == "cf")
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://ntfy.sh/mistralton_pager_alerts");
            request.Headers.Add("Title", "Please check Discord ASAP");
            request.Headers.Add("Priority", "urgent");
            request.Headers.Add("Tags", "warning");
            request.Content = new StringContent("Check in please");
            await Program.HttpClient.SendAsync(request);
        }
        else
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://uptime.floatingmilkshake.com/api/push/5ccY6HAwapl41NuIaKEmPcyODh9a6oaM?msg=PANIC%20from%20bot%20by%20{ctx.User.Username}");
            await Program.HttpClient.SendAsync(request);
        }

        await Program.Redis.StringSetAsync("lastPanic", JsonConvert.SerializeObject(DateTime.UtcNow));

        await ctx.RespondAsync("Done.");
    }
}
