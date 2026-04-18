namespace Azusa.Events;

internal static class MemberEvents
{
    private const ulong MemberLogGuildId = 799644062973427743;
    private const ulong MemberLogChannelId = 1495157743068381435;

    internal static async Task HandleGuildMemberAddedEventAsync(DiscordClient _, GuildMemberAddedEventArgs e)
    {
        if (e.Guild.Id != MemberLogGuildId)
            return;

        await SendMemberLogMessageAsync($"{e.Member.Mention} joined.");
    }

    internal static async Task HandleGuildMemberRemovedEventAsync(DiscordClient _, GuildMemberRemovedEventArgs e)
    {
        if (e.Guild.Id != MemberLogGuildId)
            return;

        await SendMemberLogMessageAsync($"{e.Member.Mention} left.");
    }

    internal static async Task HandleGuildMemberUpdatedEventAsync(DiscordClient _, GuildMemberUpdatedEventArgs e)
    {
        if (e.Guild.Id != MemberLogGuildId)
            return;

        if (e.NicknameBefore != e.NicknameAfter)
        {
            await SendMemberLogMessageAsync($"{e.Member.Mention} changed nicknames." +
                $"\n**Before:** {e.NicknameBefore}" +
                $"\n**After:** {e.NicknameAfter}");
        }

        if (!e.RolesBefore.SequenceEqual(e.RolesAfter))
        {
            await SendMemberLogMessageAsync($"{e.Member.Mention} changed roles." +
                $"\n**Before:** {GetRoleMentions(e.RolesBefore)}" +
                $"\n**After:** {GetRoleMentions(e.RolesAfter)}");
        }
    }

    private static string GetRoleMentions(IEnumerable<DiscordRole> roles)
    {
        return string.Join(", ", roles.OrderByDescending(r => r.Position).Select(r => r.Mention));
    }

    private static async Task SendMemberLogMessageAsync(string logMessage)
    {
        var memberLogChannel = await Setup.State.Discord.Client.GetChannelAsync(MemberLogChannelId);
        await memberLogChannel.SendMessageAsync(new DiscordMessageBuilder()
            .WithContent(logMessage)
            .WithAllowedMentions(Mentions.None));
    }
}
