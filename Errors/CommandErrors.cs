namespace Azusa.Errors;

internal static class CommandErrors
{
    internal static async Task HandleCommandErroredEventAsync(CommandsExtension _, CommandErroredEventArgs e)
    {
        // I don't care about other contexts, this is a text command-only bot
        if (e.Context is not TextCommandContext)
            return;

        var context = e.Context.As<TextCommandContext>();

        var response = $"An unexpected error occurred: `{e.Exception.GetType()}: {e.Exception.Message}`";
        if (e.Exception.InnerException is not null)
        {
            response += $": `{e.Exception.InnerException.GetType()}: {e.Exception.InnerException.Message}`";
        }

        await context.RespondAsync(response);

        Setup.State.Discord.Client.Logger.LogError(e.Exception, "An exception was thrown when executing a command! When {user} used {command}:",
            e.Context.User.Id, e.Context.Command.FullName);
    }
}
