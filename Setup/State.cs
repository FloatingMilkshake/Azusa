namespace Azusa.Setup;

internal static class State
{
    internal static class Discord
    {
        internal static DiscordClient Client;
    }

    internal static class Caches
    {
        public static readonly Dictionary<ulong, CancellationTokenSource> CancellationTokens = [];
    }

    internal static IMinioClient Minio = new MinioClient()
            .WithEndpoint(Setup.Configuration.ConfigJson.S3.Endpoint)
            .WithCredentials(Setup.Configuration.ConfigJson.S3.AccessKey, Setup.Configuration.ConfigJson.S3.SecretKey)
            .WithRegion(Setup.Configuration.ConfigJson.S3.Region)
            .WithSSL()
            .Build();
}
