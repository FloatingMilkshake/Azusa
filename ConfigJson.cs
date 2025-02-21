namespace Azusa;

public class ConfigJson
{
    [JsonProperty("token")]
    public string Token { get; private set; }
    
    [JsonProperty("canvas")]
    public Canvas Canvas { get; set; }
    
    [JsonProperty("hastebin")]
    public Hastebin Hastebin { get; set; }
    
    [JsonProperty("s3")]
    public S3 S3 { get; set; }
    
    [JsonProperty("workerLinks")]
    public WorkerLinks WorkerLinks { get; set; }
}

public class Canvas
{
    [JsonProperty("canvasDomain")]
    public string CanvasDomain { get; set; }
    
    [JsonProperty("cloudflareAccessClientId")]
    public string CloudflareAccessClientId { get; set; }
    
    [JsonProperty("cloudflareAccessClientSecret")]
    public string CloudflareAccessClientSecret { get; set; }
}

public class Hastebin
{
    [JsonProperty("accountId")]
    public string AccountId { get; set; }
    
    [JsonProperty("namespaceId")]
    public string NamespaceId { get; set; }
    
    [JsonProperty("token")]
    public string Token { get; set; }
    
    [JsonProperty("url")]
    public string Url { get; set; }
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

public class WorkerLinks
{
    [JsonProperty("accountId")]
    public string AccountId { get; set; }
    
    [JsonProperty("apiKey")]
    public string ApiKey { get; set; }
    
    [JsonProperty("baseUrl")]
    public string BaseUrl { get; set; }
    
    [JsonProperty("email")]
    public string Email { get; set; }
    
    [JsonProperty("namespaceId")]
    public string NamespaceId { get; set; }
    
    [JsonProperty("secret")]
    public string Secret { get; set; }
}