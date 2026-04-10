namespace Azusa.Setup;

internal class Types
{
    internal class ShellCommandResponse(int exitCode, string output, string error)
    {
        internal int ExitCode { get; } = exitCode;
        internal string Output { get; } = output;
        internal string Error { get; } = error;
    }

    // This code is taken from https://github.com/Sankra/cloudflare-cache-purger/blob/master/main.csx#L197,
    // minus some minor changes.
    // (Note that I originally found it here: https://github.com/Erisa/Lykos/blob/3335c38/src/Modules/Owner.cs#L313)
    internal readonly struct CloudflareContent(List<string> urls)
    {
        internal List<string> Files { get; } = urls;
    }

    internal class Apis
    {
        internal class ShortLinksApi
        {
            internal class ShortLinksApiResponse
            {
                [JsonProperty("items")]
                internal List<Item> Items { get; set; }
            }

            internal class Item
            {
                [JsonProperty("key")]
                internal string Key { get; set; }

                [JsonProperty("value")]
                internal string Value { get; set; }
            }
        }
    }
}
