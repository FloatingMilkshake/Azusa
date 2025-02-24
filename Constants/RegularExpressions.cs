namespace Azusa.Constants;

public static partial class RegularExpressions
{
    [GeneratedRegex(@"https.*windows-(\d+).*?build[s]?-(?:(\d+(?:-\d+)?)(?:-and-(\d+-\d+)?)*)(?:.+?(?:(canary|dev|beta|release-preview)(?:-and-(canary|dev|beta|release-preview))*)?-channel[s]?.*)?\/")]
    public static partial Regex InsiderUrlPattern();
}