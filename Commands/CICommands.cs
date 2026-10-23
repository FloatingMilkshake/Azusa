namespace Azusa.Commands;

[Command("ci")]
[Description("Manage CI runners.")]
[AllowedProcessors(typeof(TextCommandProcessor))]
[RequireApplicationOwner]
internal static class CICommands
{
    [Command("status")]
    [Description("Get the current state of things.")]
    public static async Task CIQueryCommandAsync(TextCommandContext ctx)
    {
        await ctx.RespondAsync("Working on it...");

        string response = "**Pool State**\n";
        var queryResult = await Setup.Types.ShellCommand.RunAsync("ssh -o UserKnownHostsFile=/app/known_hosts -i id_ed25519 toggle-ctl@mizuki query", CancellationToken.None);
        response += queryResult.Output.Trim() + "\n";
        response += "\n**Timer State**\n";
        var timerResult = await Setup.Types.ShellCommand.RunAsync("ssh -o UserKnownHostsFile=/app/known_hosts -i id_ed25519 toggle-ctl@mizuki timer", CancellationToken.None);
        response += timerResult.Output.Trim();
        
        await ctx.EditResponseAsync(queryResult.ExitCode != 0 || timerResult.ExitCode != 0
            ? $"Failed to check status. Query check exited {queryResult.ExitCode}, timer check exited {timerResult.ExitCode}."
            : response);
    }

    [Command("override")]
    [Description("Override the pool timer and enable large runners.")]
    public static async Task CIOverrideCommandAsync(TextCommandContext ctx, int hours = 0)
    {
        await ctx.RespondAsync("Working on it...");

        if (hours == 0)
        {
            await Setup.Types.ShellCommand.RunAsync("ssh -o UserKnownHostsFile=/app/known_hosts -i id_ed25519 toggle-ctl@mizuki stay-up", CancellationToken.None);
        }
        else
        {
            await Setup.Types.ShellCommand.RunAsync($"ssh -o UserKnownHostsFile=/app/known_hosts -i id_ed25519 toggle-ctl@mizuki boost:{hours}", CancellationToken.None);
        }

        await ctx.EditResponseAsync(hours == 0
            ? "Overridden persistently. Remember to reset later."
            : $"Overridden for {hours} hour{(hours > 1 ? "s" : "")}.");
    }

    [Command("reset")]
    [Description("Reset the override and automatically manage runners.")]
    public static async Task CIResetCommandAsync(TextCommandContext ctx)
    {
        await Setup.Types.ShellCommand.RunAsync("ssh -o UserKnownHostsFile=/app/known_hosts -i id_ed25519 toggle-ctl@mizuki auto", CancellationToken.None);

        await ctx.RespondAsync("Reset!");
    }
}
