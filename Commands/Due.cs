namespace Azusa.Commands;

public static class Due
{
    [Command("due"), Description("Get due assignments from Canvas.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task DueCommand(TextCommandContext ctx, [Parameter("filter"), Description("The time to filter by."), RemainingText] string filter)
    {
        await ctx.RespondAsync("Working on it...");
        
        var request = new HttpRequestMessage(HttpMethod.Get, Program.ConfigJson.Canvas.CanvasDomain);
        request.Headers.Add("X-Filter", filter);
        request.Headers.Add("CF-Access-Client-Id", Program.ConfigJson.Canvas.CloudflareAccessClientId);
        request.Headers.Add("CF-Access-Client-Secret", Program.ConfigJson.Canvas.CloudflareAccessClientSecret);
        
        var response = await Program.HttpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            if (data.Contains("<title>Sign in ・ Cloudflare Access</title>"))
            {
                await ctx.EditResponseAsync("Blocked by Cloudflare Access! Check your Service Token.");
                return;
            }
            await ctx.EditResponseAsync(data);
        }
        else
        {
            await ctx.EditResponseAsync($"`{(int)response.StatusCode} {response.ReasonPhrase}`");
        }
    }
}