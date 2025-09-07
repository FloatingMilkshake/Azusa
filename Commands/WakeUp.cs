namespace Azusa.Commands;

public static class WakeUp
{
    [Command("wakeup")]
    [Description("wake up wake up wake up wake up")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [TextAlias("wake", "wol", "w")]
    [RequireApplicationOwner]
    public static async Task WakeUpCommand(TextCommandContext ctx)
    {
        await ctx.RespondAsync("Alright, trying...");
        
        var wakeResult = await DoWakeup();
        
        var response = await ctx.GetResponseAsync();
        
        if (wakeResult.ExitCode == 0)
        {
            // Ping to see if it woke up
            var command = $"ping -c 10 {Program.ConfigJson.Err.SshHost}.{Program.ConfigJson.TailnetName}";
            var pingResult = await Shell.ShellCommand(command);
        
            if (pingResult.Output.Contains("64 bytes from"))
                await response.ModifyAsync("Alright, trying... it worked!");
            else
                await response.ModifyAsync($"Alright, trying... it didn't work!\nExited `{pingResult.ExitCode}`: {pingResult.Error.Trim()}");
        }
        else
        {
            await response.ModifyAsync($"Alright, trying... it didn't work!\nExited `{wakeResult.ExitCode}`: {wakeResult.Error.Trim()}");
        }
    }
    
    internal static async Task<ShellCommandResponse> DoWakeup()
    {
        return await Shell.ShellCommand($"ssh -o IdentityAgent=none {Program.ConfigJson.WakeOnLan.RelayUsername}@{Program.ConfigJson.WakeOnLan.RelayHost}.{Program.ConfigJson.TailnetName} \"wakeonlan {Program.ConfigJson.WakeOnLan.TargetMacAddress}\"");
    }
}