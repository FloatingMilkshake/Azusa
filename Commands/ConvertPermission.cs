using System.Numerics;

namespace Azusa.Commands;

public class ConvertPermission
{
    [Command("convertpermissioninteger")]
    [TextAlias("convertpermissions", "permissioninteger", "convertperms", "permission", "perms")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task ConvertPermissionIntegerCommand(TextCommandContext ctx, string permissions)
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