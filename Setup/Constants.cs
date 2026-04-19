namespace Azusa.Setup;

internal static class Constants
{
    internal static readonly HttpClient HttpClient = new();
    internal static readonly List<ulong> PanicAuthorizedUsers = [455432936339144705, 208935109485789184, 455428041586376729, 573984492713279512];

    internal class RegularExpressions
    {
        internal static readonly Regex CdnFileNamePattern = new(@"[^/\\&\?#]+\.\w*(?=([\?&#].*$|$))");
        internal static readonly Regex CdnFileExtensionPattern = new(@"\.\w*(?=([\?&#].*$|$))");
    }
}
