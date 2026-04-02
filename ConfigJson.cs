namespace Azusa;

public class ConfigJson
{
    [JsonProperty("token")]
    public string Token { get; private set; }

    [JsonProperty("canvas")]
    public Canvas Canvas { get; set; }

    [JsonProperty("s3")]
    public S3 S3 { get; set; }

    [JsonProperty("shortLinks")]
    public ShortLinks ShortLinks { get; set; }
}

public class Canvas
{
    [JsonProperty("apiToken")]
    public string ApiToken { get; private set; }
    
    [JsonProperty("domain")]
    public string Domain { get; private set; }
}

public class S3
{
    [JsonProperty("accessKey")]
    public string AccessKey { get; set; }

    [JsonProperty("baseUrl")]
    public string BaseUrl { get; set; }

    [JsonProperty("bucket")]
    public string Bucket { get; set; }

    [JsonProperty("endpoint")]
    public string Endpoint { get; set; }

    [JsonProperty("region")]
    public string Region { get; set; }

    [JsonProperty("secretKey")]
    public string SecretKey { get; set; }

    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("urlPrefix")]
    public string UrlPrefix { get; set; }

    [JsonProperty("zoneId")]
    public string ZoneId { get; set; }
}

public class ShortLinks
{
    [JsonProperty("baseUrl")]
    public string BaseUrl { get; set; }

    [JsonProperty("secret")]
    public string Secret { get; set; }
}