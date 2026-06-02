namespace Azusa.Events;

internal static class MessageEvents
{
    internal static async Task HandleMessageCreatedEventAsync(DiscordClient client, MessageCreatedEventArgs e)
    {
        await CheckRssFeedArticlesForUpdatesAsync(e);
    }

    private static async Task CheckRssFeedArticlesForUpdatesAsync(MessageCreatedEventArgs e)
    {
        List<string> matches = [
            "node_exporter",
            "forgejo",
            "tubearchivist",
            "jellyfin",
            "monitorss",
            "uptime-kuma",
	    "immich",
            "pocket-id",
            "pi-hole",
            "caddy",
	    "veracrypt"
        ];

        // MonitoRSS only
        if (e.Message.Author.Id != 944784076735414342) return;

        if (matches.Any(m => e.Message.Content.Contains(m, StringComparison.OrdinalIgnoreCase)))
        {
            await Task.Delay(5000); // give the message a bit to load embeds
            await e.Message.ForwardAsync(1409187775005331566);
        }
    }
}
