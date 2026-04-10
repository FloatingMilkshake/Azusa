namespace Azusa;

internal class ConfigJson
{
    [JsonProperty("token")] internal string Token { get; private set; }

    [JsonProperty("canvas")] internal Canvas Canvas { get; set; }

    [JsonProperty("s3")] internal S3 S3 { get; set; }

    [JsonProperty("shortLinks")] internal ShortLinks ShortLinks { get; set; }
}

internal class Canvas
{
    [JsonProperty("apiToken")] internal string ApiToken { get; private set; }
    
    [JsonProperty("domain")] internal string Domain { get; private set; }
}

internal class S3
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

internal class ShortLinks
{
    [JsonProperty("baseUrl")] internal string BaseUrl { get; set; }

    [JsonProperty("secret")] internal string Secret { get; set; }
}
