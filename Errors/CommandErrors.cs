using Azusa.Attributes;
using DSharpPlus.Commands.Exceptions;

namespace Azusa.Errors;

internal static class CommandErrors
{
    internal static async Task HandleCommandErroredEventAsync(CommandsExtension _, CommandErroredEventArgs e)
    {
        // I don't care about other contexts, this is a text command-only bot
        if (e.Context is not TextCommandContext)
            return;

        var context = e.Context.As<TextCommandContext>();

        if (e.Exception is ChecksFailedException checksFailedException &&
            checksFailedException.Errors.Any(x => x.ContextCheckAttribute is RequireApplicationOwnerAttribute or RequirePermissionsAttribute or RequireSecretRoleAttribute))
        {
            await context.RespondAsync("Sorry, you aren't allowed to use this command! If you think you should be able to, please ask Milkshake.");
            return;
        }

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
