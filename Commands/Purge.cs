namespace Azusa.Commands;

public static class Purge
{
    [Command("purge"), Description("Purge some messages.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequirePermissions(DiscordPermission.ManageMessages)]
    public static async Task PurgeCommand(TextCommandContext ctx, [Parameter("startingMessage"), Description("Where to delete down from. Exclusive.")] ulong startingMessage)
    {
        IReadOnlyList<DiscordMessage> msgs;
        try
        {
            msgs = await ctx.Channel.GetMessagesAfterAsync(startingMessage).ToListAsync();
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex.InnerException is UnauthorizedException)
        {
            await ctx.RespondAsync("I can't read the start message!");
            return;
        }
        catch (Exception ex)
        {
            var response = $"An unknown error occurred:\n```\n{ex.GetType()}: {ex.Message}\n{ex.StackTrace}\n```";
            if (ex.InnerException is not null)
                response += $"```\n{ex.InnerException.GetType()}: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}\n```";
            await ctx.RespondAsync(response);
            return;
        }
        
        if (msgs.Count < 1)
        {
            await ctx.RespondAsync("No messages were found to purge.");
            return;
        }
        
        int numDeleted;
        try
        {
            numDeleted = await ctx.Channel.DeleteMessagesAsync(msgs);
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex.InnerException is UnauthorizedException)
        {
            await ctx.RespondAsync("I don't have permission to delete all of the messages requested!");
            return;
        }
        catch (Exception ex)
        {
            var response = $"An unknown error occurred:\n```\n{ex.GetType()}: {ex.Message}\n{ex.StackTrace}\n```";
            if (ex.InnerException is not null)
                response += $"```\n{ex.InnerException.GetType()}: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}\n```";
            await ctx.RespondAsync(response);
            return;
        }
        
        await ctx.RespondAsync($"Purged {numDeleted} messages!");
    }
}