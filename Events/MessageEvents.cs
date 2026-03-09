namespace Azusa.Events;

public static class MessageEvents
{
    public static async Task MessageCreated(DiscordClient client, MessageCreatedEventArgs e)
    {
        await ParseWindowsInsidersRssAsync(client, e);

        await CheckRssFeedArticlesForUpdatesAsync(client, e);
    }

    private static async Task ParseWindowsInsidersRssAsync(DiscordClient client, MessageCreatedEventArgs e)
    {
        // ignore self
        if (e.Message.Author?.Id == client.CurrentUser.Id)
            return;

        // try to match content with Insider URL pattern
        var insiderUrlPattern = RegularExpressions.InsiderUrlPattern();

        // ignore non-matching messages or messages that are not from MonitoRSS
        if (!insiderUrlPattern.IsMatch(e.Message.Content) || e.Message.Author.Id != 944784076735414342)
            return;

        var insiderUrlMatch = insiderUrlPattern.Match(e.Message.Content);
        var buildNumber1 = insiderUrlMatch.Groups[1].Value;
        var buildNumber2 = insiderUrlMatch.Groups[2].Value;
        var channel1 = insiderUrlMatch.Groups[3].Value;
        var channel2 = insiderUrlMatch.Groups[4].Value;

        // format channel names correctly
        // canary -> Canary Channel
        // dev -> Dev Channel
        // beta -> Beta Channel
        // release-preview -> Release Preview Channel

        channel1 = channel1 switch
        {
            "canary" => "Canary Channel",
            "dev" => "Dev Channel",
            "beta" => "Beta Channel",
            "release-preview" => "Release Preview Channel",
            _ => string.Empty
        };

        channel2 = channel2 switch
        {
            "canary" => "Canary Channel",
            "dev" => "Dev Channel",
            "beta" => "Beta Channel",
            "release-preview" => "Release Preview Channel",
            _ => string.Empty
        };

        // assemble /announcebuild command
        // format is: /announcebuild windows_version:WINDOWS_VERSION build_number:BUILD_NUMBER blog_link:BLOG_LINK insider_role1:FIRST_ROLE insider_role2:SECOND_ROLE
        // insider_role2 is optional
        // if two build numbers are present, pick the higher one
        // if a build number contains a hyphen, replace it with a dot (ex. 22635-3430 -> 22635.3430)

        var buildNumber = buildNumber2 == string.Empty
            ? buildNumber1
            : string.Compare(buildNumber1, buildNumber2, StringComparison.Ordinal) > 0
                ? buildNumber1
                : buildNumber2;
        buildNumber = buildNumber.Replace('-', '.');

        var blogLink = insiderUrlMatch.ToString();

        var command = $"/announcebuild build_number:{buildNumber} blog_link:{blogLink} insider_role1:{channel1}";
        if (channel2 != string.Empty) command += $" insider_role2:{channel2}";

        // send command to channel
        var msg = await e.Message.Channel.SendMessageAsync(command);

        // suppress embed
        await Task.Delay(1000);
        await msg.ModifyEmbedSuppressionAsync(true);

        if (e.Message.Embeds.Any(x =>
                (x.Description?.Contains("please note", StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Description?.Contains("note:", StringComparison.OrdinalIgnoreCase) ?? false)))
            await msg.CreateReactionAsync(DiscordEmoji.FromName(client, ":bangbang:"));
    }

    private static async Task CheckRssFeedArticlesForUpdatesAsync(DiscordClient client, MessageCreatedEventArgs e)
    {
        List<string> matches = [
            "node_exporter",
            "forgejo",
            "timvisee/send",
            "kotx/aster",
            "tubearchivist",
            "jellyfin",
            "worker-links",
            "monitorss",
            "discord-oidc-worker",
            "uptime-kuma",
            "Erisa/starbin",
            "cloudflared",
	        "immich",
            "pocket-id"
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