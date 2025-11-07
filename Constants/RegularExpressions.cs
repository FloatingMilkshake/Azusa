namespace Azusa.Constants;

public static partial class RegularExpressions
{
    [GeneratedRegex(@"https:\/\/blogs\.windows\.com\/windows-insider\/.+windows-11.+build[s]?-(?:(\d+(?:-\d+)?)(?:-and-(\d+-\d+)?){0,1})(?:.+?(?:(canary|dev|beta|release-preview)(?:(?:-and)?-(canary|dev|beta|release-preview))*)?-channel[s]?.*)?\/")]
    public static partial Regex InsiderUrlPattern();
}