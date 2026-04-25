namespace Azusa.Commands;

[Command("link")]
[Description("Set, update, or delete a short link.")]
[AllowedProcessors(typeof(TextCommandProcessor))]
[RequireApplicationOwner]
internal static class LinkCommands
{
    [Command("get")]
    [TextAlias("check")]
    [Description("Get the URL for a short link.")]
    public static async Task LinkGetCommandAsync(TextCommandContext ctx,
        [Parameter("key"), Description("The key for the link to get.")]
        string key)
    {
        if (Setup.State.Process.Configuration.ShortLinks.BaseUrl is null)
        {
            await ctx.RespondAsync("Error: No base URL provided! Make sure the baseUrl field under shortLinks in your config.json file is set.");
            return;
        }

        if (key[0] != '/') key = $"/{key}";

        var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });

        var request = new HttpRequestMessage(HttpMethod.Get, $"{Setup.State.Process.Configuration.ShortLinks.BaseUrl}{key}");
        foreach (var header in Setup.Constants.HttpClient.DefaultRequestHeaders)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        var response = await httpClient.SendAsync(request);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
        {
            await ctx.RespondAsync($"`{key}` redirects to {response.Headers.Location.OriginalString}");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await ctx.RespondAsync($"`{key}` doesn't exist!");
        }
        else
        {
            await ctx.RespondAsync($"Got response `{$"{(int)response.StatusCode}: {response.ReasonPhrase}"}`..." +
                $"\n```json\n{await response.Content.ReadAsStringAsync()}\n```");
        }
    }
    
    [Command("set")]
    [Description("Set or update a short link.")]
    [TextAlias("create", "add", "s", "c", "a")]
    public static async Task LinkSetCommandAsync(TextCommandContext ctx,
        [Parameter("key")] [Description("Set a custom key for the short link.")]
        string key,
        [Parameter("url")] [Description("The URL the short link should point to.")]
        string url)
    {
        if (url.Contains('<')) url = url.Replace("<", "");

        if (url.Contains('>')) url = url.Replace(">", "");

        if (key[0] == '/' && key.Length > 1) key = key[1..];

        if (Setup.State.Process.Configuration.ShortLinks.BaseUrl is null)
        {
            await ctx.RespondAsync("Error: No base URL provided! Make sure the baseUrl field under shortLinks in your config.json file is set.");
            return;
        }

        if (Setup.State.Process.Configuration.ShortLinks.Secret is null)
        {
            await ctx.RespondAsync("Error: No secret provided! Make sure the secret field under shortLinks in your config.json file is set.");
            return;
        }

        var request = key is "random" or "rand"
            ? new HttpRequestMessage(HttpMethod.Post, Setup.State.Process.Configuration.ShortLinks.BaseUrl)
            : key[0] == '/'
                ? new HttpRequestMessage(HttpMethod.Put, $"{Setup.State.Process.Configuration.ShortLinks.BaseUrl}{key}")
                : new HttpRequestMessage(HttpMethod.Put, $"{Setup.State.Process.Configuration.ShortLinks.BaseUrl}/{key}");

        request.Headers.Add("Authorization", Setup.State.Process.Configuration.ShortLinks.Secret);
        request.Headers.Add("URL", url);

        HttpResponseMessage response;
        try
        {
            response = await Setup.Constants.HttpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"An exception occurred while trying to send the request! `{ex.GetType()}: {ex.Message}`");
            return;
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var status = $"{(int)response.StatusCode}: {response.ReasonPhrase}";

        if (response.IsSuccessStatusCode)
        {
            await ctx.RespondAsync("OK!");
        }
        else if (responseText.Length > 1900)
        {
            await ctx.RespondAsync($"Got response `{status}`...but the full response is too long to post here! I didn't feel like making this spit out the entire output. Have fun working this out.");
            return;
        }
        else await ctx.RespondAsync($"Failed with response `{status}`...\n```json\n{responseText}\n```");
    }

    [Command("delete")]
    [Description("Delete a short link.")]
    [TextAlias("del", "d")]
    public static async Task LinkDeleteCommandAsync(TextCommandContext ctx,
        [Parameter("link")] [Description("The key or URL of the short link to delete.")]
        string url)
    {
        if (url[0] == '/') url = url[1..];

        var baseUrl = Setup.State.Process.Configuration.ShortLinks.BaseUrl;
        if (!url.Contains(baseUrl)) url = $"{baseUrl}/{url}";

        if (Setup.State.Process.Configuration.ShortLinks.Secret is null)
        {
            await ctx.RespondAsync("Error: No secret provided! Make sure the secret field under shortLinks in your config.json file is set.");
            return;
        }

        HttpRequestMessage request = new(HttpMethod.Delete, url);
        request.Headers.Add("Authorization", Setup.State.Process.Configuration.ShortLinks.Secret);

        HttpResponseMessage response;
        try
        {
            response = await Setup.Constants.HttpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"An exception occurred while trying to send the request! `{ex.GetType()}: {ex.Message}`");
            return;
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var status = $"{(int)response.StatusCode}: {response.ReasonPhrase}";

        if (response.IsSuccessStatusCode)
        {
            await ctx.RespondAsync("OK!");
        }
        else if (responseText.Length > 1900)
        {
            await ctx.RespondAsync($"Got response `{status}`...but the full response is too long to post here! I didn't feel like making this spit out the entire output. Have fun working this out.");
            return;
        }
        else await ctx.RespondAsync($"Failed with response `{status}`...\n```json\n{responseText}\n```");
    }

    [Command("list")]
    [Description("List all short links.")]
    [TextAlias("l", "all")]
    public static async Task LinkListCommandAsync(TextCommandContext ctx,
        [Parameter("match_keys")] [Description("Optionally filter by key.")]
        string keyFilter = "",
        [Parameter("match_values")] [Description("Optionally filter by value.")]
        string valueFilter = "")
    {
        await ctx.RespondAsync("Working on it...");
    
        HttpRequestMessage request = new(HttpMethod.Get, Setup.State.Process.Configuration.ShortLinks.BaseUrl);
        request.Headers.Add("Authorization", Setup.State.Process.Configuration.ShortLinks.Secret);
    
        var response = await Setup.Constants.HttpClient.SendAsync(request);
    
        var responseText = await response.Content.ReadAsStringAsync();

        var items = (JsonConvert.DeserializeObject<ShortLinksApiResponse>(responseText)).Items;

        DiscordEmbedBuilder embed = new()
        {
            Title = string.IsNullOrWhiteSpace(keyFilter) && string.IsNullOrWhiteSpace(valueFilter)
                ? "Link List"
                : "Matching Links"
        };
    
        if (items.Count == 0)
        {
            embed.Description = "No links matched the specified filters.";
            embed.Color = DiscordColor.Red;
    
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            return;
        }

        string kvList = "";
        foreach (var item in items)
        {
            kvList += $"**{item.Key}**: {item.Value}\n\n";
        }

        var pages = InteractivityExtension.GeneratePagesInEmbed(kvList, SplitType.Line, embed).ToList();
    
        var leftSkip = new DiscordButtonComponent(DiscordButtonStyle.Primary, "leftskip", "<<<");
        var left = new DiscordButtonComponent(DiscordButtonStyle.Primary, "left", "<");
        var right = new DiscordButtonComponent(DiscordButtonStyle.Primary, "right", ">");
        var rightSkip = new DiscordButtonComponent(DiscordButtonStyle.Primary, "rightskip", ">>>");
        var stop = new DiscordButtonComponent(DiscordButtonStyle.Danger, "stop", "Stop");
    
        if (pages.Count > 1)
            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages,
                new PaginationButtons
                { SkipLeft = leftSkip, Left = left, Right = right, SkipRight = rightSkip, Stop = stop },
                deletion: ButtonPaginationBehavior.DeleteMessage);
        else
            await ctx.Channel.SendMessageAsync(embed.WithDescription(kvList));
    }

    private class ShortLinksApiResponse
    {
        [JsonProperty("items")]
        internal List<ShortLink> Items { get; set; }
    }

    internal class ShortLink
    {
        [JsonProperty("key")]
        internal string Key { get; set; }

        [JsonProperty("value")]
        internal string Value { get; set; }
    }
}
