using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Trees;

namespace Azusa.Commands;

internal static class HelpCommands
{
    // Most of this is taken from DSharpPlus.CommandsNext and adapted to fit here.
    // https://github.com/DSharpPlus/DSharpPlus/blob/1c1aa15/DSharpPlus.CommandsNext/CommandsNextExtension.cs#L829
    [Command("help")]
    [Description("Displays command help.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task HelpCommandAsync(TextCommandContext ctx, [Description("Command to provide help for.")] [RemainingText] string command = "")
    {
        var commandSplit = command.Split(' ');

        DiscordEmbedBuilder helpEmbed = new()
        {
            Title = "Help",
            Color = new DiscordColor("#0080ff")
        };

        var cmds = ctx.Extension.Commands.Values.Where(cmd =>
            cmd.Attributes.Any(attr => attr is AllowedProcessorsAttribute apAttr
                                       && apAttr.Processors.Contains(typeof(TextCommandProcessor))));

        if (commandSplit.Length != 0 && commandSplit[0] != "")
        {
            Command cmd = null;
            var searchIn = cmds;
            for (var i = 0; i < commandSplit.Length; i++)
            {
                if (searchIn is null)
                {
                    cmd = null;
                    break;
                }

                var comparison = StringComparison.InvariantCultureIgnoreCase;
                var comparer = StringComparer.InvariantCultureIgnoreCase;
                cmd = searchIn.FirstOrDefault(xc => xc.Name.Equals(commandSplit[i], comparison) || ((xc.Attributes.FirstOrDefault(x => x is TextAliasAttribute) as TextAliasAttribute)?.Aliases.Contains(commandSplit[i], comparer) ?? false));

                if (cmd is null)
                    break;

                // Only run checks on the last command in the chain.
                // So if we are looking at a command group here, only run checks against the actual command,
                // not the group(s) it's under.
                if (i == commandSplit.Length - 1)
                {
                    IEnumerable<ContextCheckAttribute> failedChecks = CheckPermissions(ctx, cmd).ToList();
                    if (failedChecks.Any())
                        return;
                }

                searchIn = cmd.Subcommands.Any() ? cmd.Subcommands : null;
            }

            if (cmd is null)
                throw new CommandNotFoundException(string.Join(" ", commandSplit));

            helpEmbed.Description = $"`{cmd.Name}`: {cmd.Description ?? "No description provided."}";

            if (cmd.Subcommands.Count > 0 && cmd.Subcommands.Any(subCommand => subCommand.Attributes.Any(attr => attr is DefaultGroupCommandAttribute)))
                helpEmbed.Description += "\n\nThis group can be executed as a standalone command.";

            var aliases = cmd.Method?.GetCustomAttributes<TextAliasAttribute>().FirstOrDefault()?.Aliases ?? (cmd.Attributes.FirstOrDefault(x => x is TextAliasAttribute) as TextAliasAttribute)?.Aliases;
            if (aliases is not null && (aliases.Length > 1 || (aliases.Length == 1 && aliases[0] != cmd.Name)))
            {
                var aliasStr = "";
                foreach (var alias in aliases)
                {
                    if (alias == cmd.Name)
                        continue;

                    aliasStr += $"`{alias}`, ";
                }

                aliasStr = aliasStr.TrimEnd(',', ' ');
                helpEmbed.AddField("Aliases", aliasStr);
            }

            var arguments = cmd.Method?.GetParameters();
            if (arguments is not null && arguments.Length > 0)
            {
                var argumentsStr = $"`{cmd.Name}";
                foreach (var arg in arguments)
                {
                    if (arg.ParameterType == typeof(CommandContext) || arg.ParameterType.IsSubclassOf(typeof(CommandContext)))
                        continue;

                    var isCatchAll = arg.GetCustomAttribute<RemainingTextAttribute>() != null;
                    argumentsStr += $"{(arg.IsOptional || isCatchAll ? " [" : " <")}{arg.Name}{(isCatchAll ? "..." : "")}{(arg.IsOptional || isCatchAll ? "]" : ">")}";
                }

                argumentsStr += "`\n";

                foreach (var arg in arguments)
                {
                    if (arg.ParameterType == typeof(CommandContext) || arg.ParameterType.IsSubclassOf(typeof(CommandContext)))
                        continue;

                    argumentsStr += $"`{arg.Name} ({arg.ParameterType.Name})`: {arg.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "No description provided."}\n";
                }

                helpEmbed.AddField("Arguments", argumentsStr.Trim());
            }
            //helpBuilder.WithCommand(cmd);

            if (cmd.Subcommands.Any())
            {
                var subCommands = cmd.Subcommands.OrderBy(x => x.Name).ToList();
                var cmdList = "";
                foreach (var subCommand in subCommands)
                    cmdList += $"`{subCommand.Name}`, ";
                helpEmbed.AddField("Subcommands", cmdList.TrimEnd(',', ' '));
                //helpBuilder.WithSubcommands(eligibleCommands.OrderBy(xc => xc.Name));
            }

            if (ctx.Guild is not null && ctx.Guild.Id == 799644062973427743)
                helpEmbed.WithFooter(text: $"Who can use this: {GetCommandPermissions(cmd)}");
        }
        else
        {
            var commandsToSearch = cmds;
            List<Command> eligibleCommands = [];
            foreach (var sc in commandsToSearch)
            {
                var executionChecks = sc.Attributes.Where(x => x is ContextCheckAttribute);

                if (!executionChecks.Any())
                {
                    eligibleCommands.Add(sc);
                    continue;
                }

                var candidateFailedChecks = CheckPermissions(ctx, sc);
                if (!candidateFailedChecks.Any())
                    eligibleCommands.Add(sc);
            }

            if (eligibleCommands.Count != 0)
            {
                eligibleCommands = eligibleCommands.OrderBy(x => x.Name).ToList();
                var cmdList = "";
                foreach (var eligibleCommand in eligibleCommands)
                    cmdList += $"`{eligibleCommand.Name}`, ";
                helpEmbed.AddField("Commands", cmdList.TrimEnd(',', ' '));
                helpEmbed.Description = "Listing all top-level commands and groups. Specify a command to see more information.";
                //helpBuilder.WithSubcommands(eligibleCommands.OrderBy(xc => xc.Name));
            }
        }

        var builder = new DiscordMessageBuilder().AddEmbed(helpEmbed);

        await ctx.RespondAsync(builder);
    }

    private static List<ContextCheckAttribute> CheckPermissions(TextCommandContext ctx, Command command)
    {
        if (Setup.State.Discord.Client.CurrentApplication.Owners.Contains(ctx.User))
            return [];

        var contextChecks = command.Attributes.Where(x => x is ContextCheckAttribute);
        var failedChecks = new List<ContextCheckAttribute>();

        foreach (var check in contextChecks)
        {
            if (check is RequirePermissionsAttribute requirePermissionsAttribute)
                if (ctx.Member is null || ctx.Guild is null
                                       || !ctx.Channel.PermissionsFor(ctx.Member).HasAllPermissions(requirePermissionsAttribute.UserPermissions)
                                       || !ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember).HasAllPermissions(requirePermissionsAttribute.BotPermissions))
                    failedChecks.Add(requirePermissionsAttribute);

            if (check is RequireApplicationOwnerAttribute requireApplicationOwnerAttribute
                && !Setup.State.Discord.Client.CurrentApplication.Owners.Contains(ctx.User))
            {
                failedChecks.Add(requireApplicationOwnerAttribute);
            }

            if (check is RequireCatRoleAttribute requireCatRoleAttribute
                && (ctx.Guild is null || ctx.Member.Roles.All(x => x.Name != "🐱")))
            {
                failedChecks.Add(requireCatRoleAttribute);
            }

            if (check is RequireSecretRoleAttribute requireSecretRoleAttribute
                && (ctx.Guild is null || ctx.Member.Roles.All(x => x.Name != "㊙️")))
            {
                failedChecks.Add(requireSecretRoleAttribute);
            }
        }

        return failedChecks;
    }

    private static string GetCommandPermissions(Command command)
    {
        var requireOwnerAttribute = command.Attributes.FirstOrDefault(x => x is RequireApplicationOwnerAttribute);
        var requireCatRoleAttribute = command.Attributes.FirstOrDefault(x => x is RequireCatRoleAttribute);
        var requireSecretRoleAttribute = command.Attributes.FirstOrDefault(x => x is RequireSecretRoleAttribute);

        if (requireOwnerAttribute != default)
            return "Only Milkshake";

        if (requireCatRoleAttribute != default)
            return "🐱";

        if (requireSecretRoleAttribute != default)
            return "㊙️";

        return "Anyone";
    }
}
