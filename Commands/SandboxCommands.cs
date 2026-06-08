namespace Azusa.Commands;

[Command("sandbox")]
[Description("Commands for managing sandbox access.")]
[TextAlias("sand", "sb")]
[RequireCatRole]
[AllowedProcessors(typeof(TextCommandProcessor))]
internal static class SandboxCommands
{
    const ulong SandboxChannelId = 1493427927830761664;
    private static readonly List<ulong> ProtectedRoles = [799644062973427743, 1494908968970096791];

    [Command("allow")]
    [Description("Allow access to the sandbox.")]
    [TextAlias("a", "add", "g", "grant")]
    public static async Task SandboxAllowCommandAsync(TextCommandContext ctx, DiscordRole role, string permissions = default)
    {
        if (ProtectedRoles.Contains(role.Id))
        {
            await ctx.RespondAsync("sandbox permissions for that role cannot be edited");
            return;
        }

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
        if (ProtectedRoles.Contains(role.Id))
        {
            await ctx.RespondAsync("sandbox permissions for that role cannot be edited");
            return;
        }

        var sandbox = await ctx.Client.GetChannelAsync(SandboxChannelId);
        await sandbox.DeleteOverwriteAsync(role, reason: $"Sandbox access denied by {ctx.User.Username}");
        await ctx.RespondAsync("Done!");

        await Setup.Storage.Redis.HashDeleteAsync("sandbox", role.Id.ToString());
    }

    [Command("reset")]
    [Description("Reset the sandbox.")]
    [TextAlias("clear")]
    public static async Task SandboxResetCommandAsync(TextCommandContext ctx)
    {
        var sandbox = await ctx.Client.GetChannelAsync(SandboxChannelId);
        var overwrites = sandbox.PermissionOverwrites.Where(
            x => x.Type == DiscordOverwriteType.Role
                && !ProtectedRoles.Contains(x.Id)
        ).ToList();
        foreach (var overwrite in overwrites)
        {
            await sandbox.DeleteOverwriteAsync(await overwrite.GetRoleAsync(), reason: $"Sandbox reset by {ctx.User.Username}");
            await Setup.Storage.Redis.HashDeleteAsync("sandbox", overwrite.Id.ToString());
        }
        await ctx.RespondAsync("Done!");
    }
}
