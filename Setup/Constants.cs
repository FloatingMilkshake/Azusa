namespace Azusa.Setup;

internal static class Constants
{
    internal static readonly HttpClient HttpClient = new();

    internal static class RegularExpressions
    {
        internal static readonly Regex CdnFileNamePattern = new(@"[^/\\&\?#]+\.\w*(?=([\?&#].*$|$))");
        internal static readonly Regex CdnFileExtensionPattern = new(@"\.\w*(?=([\?&#].*$|$))");
    }
}
