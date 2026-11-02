namespace Azusa.Commands;

[Command("sandbox")]
[Description("Commands for managing sandbox access.")]
[TextAlias("sand", "sb")]
[RequireCatRole]
[AllowedProcessors(typeof(TextCommandProcessor))]
internal static class SandboxCommands
{
    private const ulong SandboxChannelId = 1514497871087407236;
    private const ulong EveryoneRoleId = 799644062973427743;
    private const ulong AzusaRoleId = 1435712630617800717;

    [Command("allow")]
    [Description("Allow access to the sandbox.")]
    [TextAlias("a", "add", "g", "grant")]
    public static async Task SandboxAllowCommandAsync(TextCommandContext ctx, DiscordRole role, string permissions = default)
    {
        if (role.Id == EveryoneRoleId)
        {
            await ctx.RespondAsync(new DiscordMessageBuilder()
                .WithContent("Refusing to edit sandbox permissions for the @everyone role. Please make these changes yourself.")
                .WithAllowedMentions(Mentions.None));
            return;
        }
        if (role.Id == AzusaRoleId)
        {
            await ctx.RespondAsync("Refusing to edit my own permissions. Please make these changes yourself.");
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
                await ctx.RespondAsync("Couldn't parse permission integer!");
                return;
            }

            if (BigInteger.IsNegative(permissionInteger))
            {
                await ctx.RespondAsync("Permission integer cannot be negative!");
                return;
            }
        }

        var sandbox = await ctx.Client.GetChannelAsync(SandboxChannelId);
        await sandbox.AddOverwriteAsync(role, new DiscordPermissions(permissionInteger), reason: $"Sandbox access granted by {ctx.User.Username}");
        await ctx.RespondAsync("Sandbox updated.");

        await Setup.Storage.Redis.HashSetAsync("sandbox", role.Id.ToString(), JsonConvert.SerializeObject(DateTime.UtcNow));
    }

    [Command("deny")]
    [Description("Deny access to the sandbox.")]
    [TextAlias("d", "deny", "r", "remove")]
    public static async Task SandboxDenyCommandAsync(TextCommandContext ctx, DiscordRole role)
    {
        if (role.Id == EveryoneRoleId)
        {
            await ctx.RespondAsync(new DiscordMessageBuilder()
                .WithContent("Refusing to edit sandbox permissions for the @everyone role. Please make these changes yourself.")
                .WithAllowedMentions(Mentions.None));
            return;
        }
        if (role.Id == AzusaRoleId)
        {
            await ctx.RespondAsync("Refusing to edit my own permissions. Please make these changes yourself.");
            return;
        }

        var sandbox = await ctx.Client.GetChannelAsync(SandboxChannelId);
        await sandbox.DeleteOverwriteAsync(role, reason: $"Sandbox access denied by {ctx.User.Username}");
        await ctx.RespondAsync("Sandbox updated.");

        await Setup.Storage.Redis.HashDeleteAsync("sandbox", role.Id.ToString());
    }

    [Command("reset")]
    [Description("Reset the sandbox.")]
    [TextAlias("clear")]
    public static async Task SandboxResetCommandAsync(TextCommandContext ctx)
    {
        var sandbox = await ctx.Client.GetChannelAsync(SandboxChannelId);
        var overwrites = sandbox.PermissionOverwrites.Where(x => x.Type == DiscordOverwriteType.Role && x.Id != AzusaRoleId && x.Id != EveryoneRoleId).ToList();
        foreach (var overwrite in overwrites)
        {
            await sandbox.DeleteOverwriteAsync(await overwrite.GetRoleAsync(), reason: $"Sandbox reset by {ctx.User.Username}");
            await Setup.Storage.Redis.HashDeleteAsync("sandbox", overwrite.Id.ToString());
        }
        await ctx.RespondAsync("Sandbox reset.");
    }
}
