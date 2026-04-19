namespace Azusa.Commands;

internal static class SelectCommands
{
    [Command("Select User")]
    [AllowedProcessors(typeof(UserCommandProcessor))]
    [SlashCommandTypes(DiscordApplicationCommandType.UserContextMenu)]
    [InteractionInstallType(DiscordApplicationIntegrationType.GuildInstall, DiscordApplicationIntegrationType.UserInstall)]
    [InteractionAllowedContexts(DiscordInteractionContextType.BotDM, DiscordInteractionContextType.PrivateChannel, DiscordInteractionContextType.Guild)]
    [RequireApplicationOwner]
    public static async Task SelectUserUserContextMenuCommandAsync(UserCommandContext ctx, DiscordUser user)
    {
        await ctx.DeferResponseAsync(ephemeral: true);
        Setup.State.Caches.Selections[ctx.User.Id] = new Setup.Eval.Selection(user);
        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
            .WithContent($"Selected: {user.Mention}")
            .AsEphemeral(true));
    }

    [Command("Select Message")]
    [AllowedProcessors(typeof(MessageCommandProcessor))]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    [InteractionInstallType(DiscordApplicationIntegrationType.GuildInstall, DiscordApplicationIntegrationType.UserInstall)]
    [InteractionAllowedContexts(DiscordInteractionContextType.BotDM, DiscordInteractionContextType.PrivateChannel, DiscordInteractionContextType.Guild)]
    [RequireApplicationOwner]
    public static async Task SelectMessageMessageCOntextMenuCommandAsync(MessageCommandContext ctx, DiscordMessage message)
    {
        await ctx.DeferResponseAsync(ephemeral: true);
        Setup.State.Caches.Selections[ctx.User.Id] = new Setup.Eval.Selection(message);
        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
            .WithContent($"Selected: {message.JumpLink}")
            .AsEphemeral(true));
    }
}
