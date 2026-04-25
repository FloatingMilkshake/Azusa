namespace Azusa.Setup.Types;

internal sealed class ConfigJson
{
    [JsonProperty("token")] internal string Token { get; private set; }

    [JsonProperty("s3")] internal S3Configuration S3 { get; set; }

    [JsonProperty("shortLinks")] internal ShortLinksConfiguration ShortLinks { get; set; }

    internal class S3Configuration
    {
        [JsonProperty("accessKey")] internal string AccessKey { get; set; }

        [JsonProperty("baseUrl")] internal string BaseUrl { get; set; }

        [JsonProperty("bucket")] internal string Bucket { get; set; }

        [JsonProperty("endpoint")] internal string Endpoint { get; set; }

        [JsonProperty("region")] internal string Region { get; set; }

        [JsonProperty("secretKey")] internal string SecretKey { get; set; }

        [JsonProperty("token")] internal string Token { get; set; }

        [JsonProperty("urlPrefix")] internal string UrlPrefix { get; set; }

        [JsonProperty("zoneId")] internal string ZoneId { get; set; }
    }

    internal class ShortLinksConfiguration
    {
        [JsonProperty("baseUrl")] internal string BaseUrl { get; set; }

        [JsonProperty("secret")] internal string Secret { get; set; }
    }
}
