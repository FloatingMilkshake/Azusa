namespace Azusa.Setup.Types;

internal sealed class ConfigJson
{
    [JsonProperty("s3BaseUrl")] internal string S3BaseUrl { get; set; }

    [JsonProperty("shortLinksBaseUrl")] internal string ShortLinksBaseUrl { get; set; }

    [JsonProperty("grafanaLokiUrl")] internal string GrafanaLokiUrl { get; set; }
}
