namespace Azusa.Commands;

internal static class PurgeCommands
{
    [Command("purge")]
    [TextAlias("delete", "clear", "del")]
    [Description("Purge some messages.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequirePermissions(DiscordPermission.ManageMessages)]
    public static async Task PurgeCommandAsync(TextCommandContext ctx,
        [Parameter("startingMessage")] [Description("Where to delete down from. Exclusive.")] ulong startingMessageOrCount)
    {
        await ctx.Message.DeleteAsync();
        
        IReadOnlyList<DiscordMessage> msgs;
        
        if (startingMessageOrCount <= 100)
        {
            msgs = await ctx.Channel.GetMessagesAsync(Convert.ToInt32(startingMessageOrCount)).ToListAsync();
        }
        else
        {
            try
            {
                var startingMessage = await ctx.Channel.GetMessageAsync(startingMessageOrCount);
                msgs = await ctx.Channel.GetMessagesAfterAsync(startingMessage.Id, int.MaxValue).ToListAsync();
            }
            catch (Exception ex) when (ex is UnauthorizedException or NotFoundException || ex.InnerException is UnauthorizedException or NotFoundException)
            {
                await ctx.RespondAsync("You entered an invalid message ID, or asked me to delete too many messages (the limit is 100)! Try again.");
                return;
            }
            catch (Exception ex)
            {
                var response = $"An unknown error occurred: `{ex.GetType()}: {ex.Message}`";
                if (ex.InnerException is not null)
                    response += $": `{ex.InnerException.GetType()}: {ex.InnerException.Message}`";
                await ctx.RespondAsync(response);
                return;
            }
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
            var response = $"An unknown error occurred: `{ex.GetType()}: {ex.Message}`";
            if (ex.InnerException is not null)
                response += $": `{ex.InnerException.GetType()}: {ex.InnerException.Message}`";
            await ctx.RespondAsync(response);
            return;
        }

        await ctx.RespondAsync($"Purged {numDeleted} messages!");
        await Task.Delay(5000);
        var msg = await ctx.GetResponseAsync();
        if (msg is not null)
            await msg.DeleteAsync();
    }
}
