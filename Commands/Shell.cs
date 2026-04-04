using static Azusa.Commands.Eval;

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
        await ctx.RespondAsync(new DiscordMessageBuilder().WithContent("Working on it...")
            .AddActionRowComponent(new DiscordActionRowComponent(
                [new DiscordButtonComponent(DiscordButtonStyle.Danger, "eval-cancel-button", "Cancel")]
            ))
        );

        var msg = await ctx.GetResponseAsync();
        Cancellations.Add(msg.Id, new CancellationTokenSource());
        var cancellationToken = Cancellations[msg.Id].Token;

        var cmdResponse = await ShellCommand(command, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("The operation was cancelled."));
            Cancellations.Remove(msg.Id);
            return;
        }

        var splitOutput = await StringHelpers.SplitStringAsync($"```\n{cmdResponse.Output}\n{cmdResponse.Error}\n```");

        foreach (var part in splitOutput)
        {
            await ctx.Channel.SendMessageAsync(part);
        }
        await msg.ModifyAsync(new DiscordMessageBuilder().WithContent($"\nFinished with exit code `{cmdResponse.ExitCode}`."));

        Cancellations.Remove(msg.Id);
    }

    public static async Task<ShellCommandResponse> ShellCommand(string command, CancellationToken cancellationToken)
    {
        var osDescription = RuntimeInformation.OSDescription;
        string fileName;
        string args;
        var escapedArgs = command.Replace("\"", "\\\"");

        if (osDescription.Contains("Windows"))
        {
            fileName = @"C:\Program Files\PowerShell\7\pwsh.exe";
            args = $"-Command \"$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText ; {escapedArgs} 2>&1\"";
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
        string result;
        string error;
        try
        {
            result = await proc.StandardOutput.ReadToEndAsync(cancellationToken);
            error = await proc.StandardOutput.ReadToEndAsync(cancellationToken);
            await proc.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            result = "The operation was cancelled.";
            error = "";
        }
        if (cancellationToken.IsCancellationRequested)
            proc.Kill();

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