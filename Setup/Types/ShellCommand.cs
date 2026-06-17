namespace Azusa.Setup.Types;

internal static class ShellCommand
{
    internal static async Task<Result> RunAsync(string command, CancellationToken cancellationToken, MemoryStream memoryStream = default)
    {
        var osDescription = RuntimeInformation.OSDescription;
        string fileName;
        string args;
        var escapedArgs = command.Replace("\"", "\\\"");

        if (osDescription.Contains("Windows"))
        {
            fileName = @"C:\Program Files\PowerShell\7\pwsh.exe";
            args = $"-Command \"$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText ; {escapedArgs}\"";
        }
        else
        {
            // Assume Linux if OS is not Windows because I'm too lazy to bother with specific checks right now, might implement that later
            fileName = Environment.GetEnvironmentVariable("SHELL");
            if (!File.Exists(fileName)) fileName = "/bin/sh";

            args = $"-c \"{escapedArgs}\"";
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        };
        startInfo.Environment.Remove("RCLONE_LOG_FILE");
        Process proc = new()
        {
            StartInfo = startInfo
        };

        proc.Start();
        Task<string> stdout;
        Task<string> stderr;
        string result;
        string error;
        try
        {
            if (memoryStream != default)
            {
                await memoryStream.CopyToAsync(proc.StandardInput.BaseStream);
                proc.StandardInput.Close();
            }
            stdout = proc.StandardOutput.ReadToEndAsync(cancellationToken);
            stderr = proc.StandardError.ReadToEndAsync(cancellationToken);

            await proc.WaitForExitAsync(cancellationToken);

            result = await stdout;
            error = await stderr;
        }
        catch (OperationCanceledException)
        {
            result = "The operation was cancelled.";
            error = "";
        }
        if (cancellationToken.IsCancellationRequested)
        {
            proc.Kill();

            // Wait a bit for the process to be killed
            await Task.Delay(5000, CancellationToken.None);
        }

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
