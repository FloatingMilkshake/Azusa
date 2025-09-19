namespace Azusa;

public class ConfigJson
{
    [JsonProperty("token")]
    public string Token { get; private set; }

    [JsonProperty("canvas")]
    public Canvas Canvas { get; set; }
    
    [JsonProperty("tailnetName")]
    public string TailnetName { get; private set; }
    
    [JsonProperty("err")]
    public Err Err { get; set; }
    
    [JsonProperty("wakeOnLan")]
    public WakeOnLan WakeOnLan { get; set; }

    [JsonProperty("hastebin")]
    public Hastebin Hastebin { get; set; }

    [JsonProperty("s3")]
    public S3 S3 { get; set; }

    [JsonProperty("workerLinks")]
    public WorkerLinks WorkerLinks { get; set; }
}

public class Canvas
{
    [JsonProperty("apiToken")]
    public string ApiToken { get; private set; }
    
    [JsonProperty("domain")]
    public string Domain { get; private set; }
}

public class Err
{
    [JsonProperty("sshHost")]
    public string SshHost { get; private set; }
    
    [JsonProperty("sshUsername")]
    public string SshUsername { get; private set; }
}

public class WakeOnLan
{
    [JsonProperty("relayHost")]
    public string RelayHost { get; private set; }
    
    [JsonProperty("relayUsername")]
    public string RelayUsername { get; private set; }
    
    [JsonProperty("targetMacAddress")]
    public string TargetMacAddress { get; private set; }
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