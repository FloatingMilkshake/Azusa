namespace Azusa.Commands;

[Command("sandbox")]
[Description("Commands for managing sandbox access.")]
[TextAlias("sand", "sb")]
[RequireCatRole]
[AllowedProcessors(typeof(TextCommandProcessor))]
internal static class SandboxCommands
{
    const ulong SandboxChannelId = 1493427927830761664;

    [Command("allow")]
    [Description("Allow access to the sandbox.")]
    [TextAlias("a", "add", "g", "grant")]
    public static async Task SandboxAllowCommandAsync(TextCommandContext ctx, DiscordRole role, string permissions = default)
    {
        BigInteger permissionInteger;
        if (string.IsNullOrWhiteSpace(permissions))
        {
            permissionInteger = 1024;
        }
        else
        {
            try
            {
                permissionInteger = BigInteger.Parse(permissions);
            }
            catch (FormatException)
            {
                await ctx.RespondAsync("couldn't parse permission integer");
                return;
            }

            if (BigInteger.IsNegative(permissionInteger))
            {
                await ctx.RespondAsync("permission integer cannot be negative");
                return;
            }
        }

        var sandbox = await ctx.Client.GetChannelAsync(SandboxChannelId);
        await sandbox.AddOverwriteAsync(role, new DiscordPermissions(permissionInteger), reason: $"Sandbox access granted by {ctx.User.Username}");
        await ctx.RespondAsync("Done!");

        await Setup.Storage.Redis.HashSetAsync("sandbox", role.Id.ToString(), JsonConvert.SerializeObject(DateTime.UtcNow));
    }

    [Command("deny")]
    [Description("Deny access to the sandbox.")]
    [TextAlias("d", "deny", "r", "remove")]
    public static async Task SandboxDenyCommandAsync(TextCommandContext ctx, DiscordRole role)
    {
        var sandbox = await ctx.Client.GetChannelAsync(SandboxChannelId);
        await sandbox.DeleteOverwriteAsync(role, reason: $"Sandbox access denied by {ctx.User.Username}");
        await ctx.RespondAsync("Done!");
    }
}
