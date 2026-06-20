namespace Azusa.Commands;

internal class PermissionCommands
{
    [Command("convertpermissioninteger")]
    [TextAlias("convertpermissions", "permissioninteger", "convertperms", "permission", "perms")]
    [Description("Show the permissions represented by a permission integer.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task ConvertPermissionIntegerCommandAsync(TextCommandContext ctx, string permissions)
    {
        BigInteger permissionInteger;
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

        await ctx.RespondAsync((new DiscordPermissions(permissionInteger)).ToString("name"));
    }
}
