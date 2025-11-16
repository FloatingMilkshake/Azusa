using DSharpPlus.EventArgs;

namespace Azusa.Events;

public static class MessageEvents
{
    public static async Task MessageCreated(DiscordClient client, MessageCreatedEventArgs e)
    {
        #region MonitoRSS monitoring
        
        if (e.Channel.Id == 1408962751153569944 && e.Author.Id == 944784076735414342)
        {
            // Log time of latest message from MonitoRSS in feed channel
            await Program.Redis.StringSetAsync("monitorssLastDelivery", $"{JsonConvert.SerializeObject(DateTime.UtcNow)}@{e.Message.JumpLink}");
        }
        
        #endregion MonitoRSS monitoring
        
        #region insider RSS feed parsing for /announcebuild
        
        // ignore self
        if (e.Message.Author?.Id == client.CurrentUser.Id)
            return;
        
        // try to match content with Insider URL pattern
        var insiderUrlPattern = RegularExpressions.InsiderUrlPattern();
        
        // ignore non-matching messages or messages that are not from RSS articles
        if (!insiderUrlPattern.IsMatch(e.Message.Content) || !e.Message.Content.Contains("📰"))
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
        // ReSharper disable once PossibleNullReferenceException; channel cannot be null
        var msg = await e.Message.Channel.SendMessageAsync(command);
        
        // suppress embed
        await Task.Delay(1000);
        await msg.ModifyEmbedSuppressionAsync(true);
        
        if (e.Message.Embeds.Any(x => 
                (x.Description?.Contains("please note", StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Description?.Contains("note:", StringComparison.OrdinalIgnoreCase) ?? false)))
            await msg.CreateReactionAsync(DiscordEmoji.FromName(client, ":bangbang:"));
        
        #endregion insider RSS feed parsing for /announcebuild
    }
}