namespace Azusa.Setup.Types;

internal sealed class ConfigJson
{
    [JsonProperty("token")] internal string Token { get; private set; }

    [JsonProperty("s3")] internal S3Configuration S3 { get; set; }

    [JsonProperty("shortLinks")] internal ShortLinksConfiguration ShortLinks { get; set; }

    internal class S3Configuration
    {
        [JsonProperty("baseUrl")] internal string BaseUrl { get; set; }
    }

    internal class ShortLinksConfiguration
    {
        [JsonProperty("baseUrl")] internal string BaseUrl { get; set; }

        [JsonProperty("secret")] internal string Secret { get; set; }
    }
}
