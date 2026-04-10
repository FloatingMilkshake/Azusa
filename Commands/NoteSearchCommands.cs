namespace Azusa.Commands;

internal class NoteSearchCommands
{
    [Command("notesearch")]
    [TextAlias("ns")]
    [Description("Search notes.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task NoteSearchCommandAsync(TextCommandContext ctx,
        [Parameter("query"), Description("The search query."), RemainingText] string query)
    {
        await ctx.RespondAsync("Searching... 0%");
        var responseMessage = await ctx.GetResponseAsync();

        var queryWords = query.Split(' ');
        await responseMessage.ModifyAsync("Searching... 25%");

        var notesCategory = await ctx.Client.GetChannelAsync(1408964457455157301);
        await responseMessage.ModifyAsync("Searching... 50%");

        var noteChannels = (await notesCategory.Guild.GetChannelsAsync()).Where(x => x.ParentId == notesCategory.Id);
        await responseMessage.ModifyAsync("Searching... 75%");

        List<DiscordMessage> matchingMessages = [];

        foreach (var subChannel in noteChannels)
        {
            var messages = await subChannel.GetMessagesAsync().ToListAsync();
            matchingMessages.AddRange(messages.Where(message =>
                queryWords.All(word =>
                    message.Content.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    (message.MessageSnapshots is not null &&
                    message.MessageSnapshots.Any(snapshot => snapshot.Message.Content.Contains(word, StringComparison.OrdinalIgnoreCase))))
            ));
        }
        await responseMessage.ModifyAsync("Searching... 100%");

        string output;
        if (matchingMessages.Count == 0)
        {
            output = "Sorry, I didn't find anything.";
        }
        else if (matchingMessages.Count == 1)
        {
            output = $"This might be what you're looking for... {matchingMessages.First().JumpLink}";
        }
        else
        {
            output = "These might be what you're looking for...";
            foreach (var message in matchingMessages)
            {
                output += $"\n{message.JumpLink}";
            }
        }

        await responseMessage.ModifyAsync(output);
    }
}
