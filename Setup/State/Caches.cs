namespace Azusa.Setup.State;

internal static class Caches
{
    internal static readonly Dictionary<ulong, CancellationTokenSource> CancellationTokens = [];
    internal static readonly Dictionary<ulong, Eval.Selection> Selections = [];
}
