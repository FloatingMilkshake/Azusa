namespace Azusa.Setup.Types;

internal static class ShellCommand
{
    internal static async Task<Result> RunAsync(string command, CancellationToken cancellationToken)
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

        // Wait a bit for the process to be killed
        await Task.Delay(5000, CancellationToken.None);

        return new Result(proc.ExitCode, result, error);
    }

    internal sealed class Result
    {
        internal int ExitCode { get; private set; }
        internal string Output { get; private set; }
        internal string Error { get; private set; }

        internal Result(int exitCode, string output, string error)
        {
            ExitCode = exitCode;
            Output = output;
            Error = error;
        }

        internal Result(int exitCode, string output)
        {
            ExitCode = exitCode;
            Output = output;
            Error = default;
        }
    }
}
