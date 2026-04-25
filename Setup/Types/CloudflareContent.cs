namespace Azusa.Setup.Types;

// This code is taken from https://github.com/Sankra/cloudflare-cache-purger/blob/master/main.csx#L197,
// minus some minor changes.
// (Note that I originally found it here: https://github.com/Erisa/Lykos/blob/3335c38/src/Modules/Owner.cs#L313)
internal readonly struct CloudflareContent(List<string> urls)
{
    internal List<string> Files { get; } = urls;
}
