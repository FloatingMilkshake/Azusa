namespace Azusa.Commands;

internal class AnnouncementCommands
{
    [Command("Is this published?")]
    [AllowedProcessors(typeof(MessageCommandProcessor))]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    [InteractionInstallType(DiscordApplicationIntegrationType.GuildInstall, DiscordApplicationIntegrationType.UserInstall)]
    [InteractionAllowedContexts(DiscordInteractionContextType.Guild)]
    public static async Task AnnouncementIsThisPublishedMessageContextMenuCommandAsync(MessageCommandContext ctx, DiscordMessage targetMessage)
    {
        var response = targetMessage.Flags?.HasFlag(DiscordMessageFlags.Crossposted) ?? false
            ? "This message is **published**!"
            : "This message is **not published**!";

        await ctx.RespondAsync(response, ephemeral: true);
    }
}
