namespace Azusa.Setup;

internal class Constants
{
    internal static readonly HttpClient HttpClient = new();
    internal static readonly List<ulong> PanicAuthorizedUsers = [455432936339144705, 208935109485789184, 455428041586376729, 573984492713279512];
    internal static readonly string CanvasApiPath = $"https://{Setup.Configuration.ConfigJson.Canvas.Domain}/api/v1/planner/items?per_page=100";
    internal class RegularExpressions
    {
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        internal static readonly Regex WindowsInsiderBlogUrlPattern = new(@"https:\/\/blogs\.windows\.com\/windows-insider\/.+windows-11.+build[s]?-(?:(\d+(?:-\d+)?)(?:-and-(\d+-\d+)?){0,1})(?:.+?(?:(canary|dev|beta|release-preview)(?:(?:-and)?-(canary|dev|beta|release-preview))*)?-channel[s]?.*)?\/");
        internal static readonly Regex CanvasApiLinkHeaderNextUrlPattern = new(@"<(https:\/\/[^<]+)>; rel=""next""");
        internal static readonly Regex CdnFileNamePattern = new(@"[^/\\&\?#]+\.\w*(?=([\?&#].*$|$))");
        internal static readonly Regex CdnFileExtensionPattern = new(@"\.\w*(?=([\?&#].*$|$))");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
    }
}
