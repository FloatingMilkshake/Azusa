namespace Azusa.Setup;

internal static class Types
{
    internal static class ShellCommand
    {
        internal static async Task<Setup.Types.ShellCommand.Result> RunAsync(string command, CancellationToken cancellationToken)
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

            return new Setup.Types.ShellCommand.Result(proc.ExitCode, result, error);
        }

        internal class Result
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

            private Result() { }
        }
    }

    // This code is taken from https://github.com/Sankra/cloudflare-cache-purger/blob/master/main.csx#L197,
    // minus some minor changes.
    // (Note that I originally found it here: https://github.com/Erisa/Lykos/blob/3335c38/src/Modules/Owner.cs#L313)
    internal readonly struct CloudflareContent(List<string> urls)
    {
        internal List<string> Files { get; } = urls;
    }

    internal static class Apis
    {
        internal static class ShortLinksApi
        {
            internal class ShortLinksApiResponse
            {
                [JsonProperty("items")]
                internal List<Item> Items { get; set; }

                private ShortLinksApiResponse() { }
            }

            internal class Item
            {
                [JsonProperty("key")]
                internal string Key { get; set; }

                [JsonProperty("value")]
                internal string Value { get; set; }

                private Item() { }
            }
        }
    }
}
