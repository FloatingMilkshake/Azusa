namespace Azusa.Commands;

public class Shell
{
    // The idea for this command, and a lot of the code, is taken from Erisa's Lykos. References are linked below.
    // https://github.com/Erisa/Lykos/blob/5f9c17c/src/Modules/Owner.cs#L116-L144
    // https://github.com/Erisa/Lykos/blob/822e9c5/src/Modules/Helpers.cs#L36-L82
    [Command("shell")]
    [TextAlias("sh")]
    [Description("Run a shell command.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task ShellCommand(TextCommandContext ctx,
        [Parameter("command")] [Description("The command to run, including any arguments.")] [RemainingText]
        string command)
    {
        if (Eval.RestrictedTerms.Any(command.Contains))
        {
            await ctx.RespondAsync("Sorry, denying this to be safe.");
            return;
        }
        
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":hourglass:"));

        var cmdResponse = await ShellCommand(command);
        
        try
        {
            await StringHelpers.SplitStringAsync($"{cmdResponse.Output}\n{cmdResponse.Error}", true, ctx: ctx, completionMessage: $"\nFinished with exit code `{cmdResponse.ExitCode}`.");   
        }
        catch
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
            return;
        }
        
        await ctx.Message.DeleteReactionAsync(DiscordEmoji.FromName(ctx.Client, ":hourglass:"), ctx.Client.CurrentUser);
    }

    public static async Task<ShellCommandResponse> ShellCommand(string command)
    {
        var osDescription = RuntimeInformation.OSDescription;
        string fileName;
        string args;
        var escapedArgs = command.Replace("\"", "\\\"");

        if (osDescription.Contains("Windows"))
        {
            fileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
            args = $"-Command \"{escapedArgs} 2>&1\"";
        }
        else
        {
            // Assume Linux if OS is not Windows because I'm too lazy to bother with specific checks right now, might implement that later
            fileName = Environment.GetEnvironmentVariable("SHELL");
            if (!File.Exists(fileName)) fileName = "/bin/sh";

            args = $"-c \"{escapedArgs}\"";
        }

        Process proc = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            }
        };

        proc.Start();
        var result = await proc.StandardOutput.ReadToEndAsync();
        var error = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        return new ShellCommandResponse(proc.ExitCode, result, error);
    }
}

public class ShellCommandResponse(int exitCode, string output, string error)
{
    public ShellCommandResponse() : this(0, null, null)
    {
    }

    public int ExitCode { get; } = exitCode;
    public string Output { get; } = output;
    public string Error { get; } = error;
}