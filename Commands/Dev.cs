namespace Azusa.Commands;

public static class Dev
{
    private static readonly string HasteUrl = $"https://api.cloudflare.com/client/v4/accounts/{Program.ConfigJson.Hastebin.AccountId}/storage/kv/namespaces/{Program.ConfigJson.Hastebin.NamespaceId}/values/documents:{"azusaDevModeEnabled"}";

    [Command("dev")]
    [Description("Toggle the state of dev mode. Prod will disconnect from Discord for the duration of dev mode.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task DevCommand(TextCommandContext ctx, [Parameter("state")] [Description("The new state of dev mode to set. Use `check` to check state.")] string devModeEnabled)
    {
        if (devModeEnabled is "on" or "enable" or "true" or "yes" or "y")
        {
            var request = new HttpRequestMessage(HttpMethod.Put, HasteUrl);
            request.Content = new StringContent("true");
            request.Headers.Add("Authorization", $"Bearer {Program.ConfigJson.Hastebin.Token}");

            string outMsg;
            var response = await Program.HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                outMsg = "Dev mode enabled.";
            }
            else
            {
                await ctx.RespondAsync($"Failed to set dev mode state! Cloudflare API returned code {response.StatusCode} when setting KV value.");
                return;
            }

#if !DEBUG
            outMsg += " Restarting...";
#endif
            await ctx.RespondAsync(outMsg);
#if !DEBUG
            Environment.Exit(1);
#endif
        }
        else if (devModeEnabled == "check")
        {
            var enabled = await (await Program.HttpClient.GetAsync("https://haste.floatingmilkshake.com/raw/azusaDevModeEnabled")).Content.ReadAsStringAsync() == "true";
            if (enabled)
                await ctx.RespondAsync("Dev mode is enabled!");
            else
                await ctx.RespondAsync("Dev mode is disabled!");
        }
        else
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, HasteUrl);
            request.Headers.Add("Authorization", $"Bearer {Program.ConfigJson.Hastebin.Token}");

            string outMsg;
            var response = await Program.HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                outMsg = "Dev mode disabled.";
            }
            else
            {
                await ctx.RespondAsync($"Failed to set dev mode state! Cloudflare API returned code {response.StatusCode} when setting KV value.");
                return;
            }

#if DEBUG
            outMsg += " This instance is running in dev mode already. It will not be stopped! Please stop it manually to avoid conflicts.";
#endif
            await ctx.RespondAsync(outMsg);
        }
    }
}