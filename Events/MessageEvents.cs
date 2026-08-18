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
            "jellyfin",
            "monitorss",
            "uptime-kuma",
            "pocket-id",
            "technitium",
            "caddy",
            "anubis",
            "immich",
	        "wivrn"
        ];

        if ((e.Message.Author.Id == 944784076735414342 || e.Message.WebhookMessage.Value)
            && e.Channel.Id == 1408962751153569944
            && (matches.Any(m => e.Message.Content.Contains(m, StringComparison.OrdinalIgnoreCase))
            || matches.Any(m => e.Message.Embeds.Any(e => e.Title.Contains(m, StringComparison.OrdinalIgnoreCase)))))
        {
            await Task.Delay(5000); // give the message a bit to load embeds
            await e.Message.ForwardAsync(1409187775005331566);
        }
    }
}
