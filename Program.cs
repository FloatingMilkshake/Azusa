namespace Azusa;

public static class Program
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
    // ReSharper disable InconsistentNaming
    internal static EventId BotEventId { get; } = new(1000, "Azusa");
    internal static ConfigJson ConfigJson;
    internal static DiscordClient Discord;
    internal static readonly HttpClient HttpClient = new();
    internal static MinioClient Minio;
#if DEBUG
    internal static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
#else
    internal static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("redis");
#endif
    public static readonly IDatabase Redis = redis.GetDatabase();
    // ReSharper enable InconsistentNaming
#pragma warning restore CA2211 // Non-constant fields should not be visible

    internal static async Task Main()
    {
        // Read config.json
        string json;
        await using (var fs = File.OpenRead("config.json"))
        using (StreamReader sr = new(fs, new UTF8Encoding(false)))
        {
            json = await sr.ReadToEndAsync();
        }

        ConfigJson = JsonConvert.DeserializeObject<ConfigJson>(json);

        if (ConfigJson is null)
        {
            Discord.Logger.LogCritical(
                // ReSharper disable once LogMessageIsSentenceProblem
                "config.json is malformed. Please be sure it has all of the required values.");
            Environment.Exit(1);
        }

        // Check for dev mode
#if !DEBUG
        var devMode = (await (await HttpClient.GetAsync("https://haste.floatingmilkshake.com/raw/azusaDevModeEnabled")).Content.ReadAsStringAsync() == "true");
        while (devMode)
        {
            await Task.Delay(10000);
            devMode = (await (await HttpClient.GetAsync("https://haste.floatingmilkshake.com/raw/azusaDevModeEnabled")).Content.ReadAsStringAsync() == "true");
        }
#endif

        Minio = new MinioClient()
            .WithEndpoint(ConfigJson.S3.Endpoint)
            .WithCredentials(ConfigJson.S3.AccessKey, ConfigJson.S3.SecretKey)
            .WithRegion(ConfigJson.S3.Region)
            .WithSSL();

        var clientBuilder = DiscordClientBuilder.CreateDefault(ConfigJson.Token, DiscordIntents.All);
#if DEBUG
        clientBuilder.SetLogLevel(LogLevel.Debug);
#else
        clientBuilder.SetLogLevel(LogLevel.Information);
#endif
        clientBuilder.ConfigureExtraFeatures(config =>
        {
            config.LogUnknownEvents = false;
            config.LogUnknownAuditlogs = false;
        });
        clientBuilder.UseInteractivity(new InteractivityConfiguration
        {
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromSeconds(30)
        });
        clientBuilder.ConfigureEventHandlers((builder) =>
        {
            builder.HandleMessageCreated(MessageEvents.MessageCreated);
        });
        clientBuilder.UseCommands((_, extension) =>
        {
            // Register commands
            var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                t.IsClass && t.Namespace is not null && t.Namespace.Contains("Azusa.Commands") &&
                !t.IsNested).ToList();

            extension.AddCommands(commandTypes, 799644062973427743);

            TextCommandProcessor textCommandProcessor = new(new TextCommandConfiguration
            {
                PrefixResolver = new DefaultPrefixResolver(true, "a!", "azusa", "azu", "az").ResolvePrefixAsync
            });
            extension.AddProcessor(textCommandProcessor);
        });

        // Build the client
        Discord = clientBuilder.Build();

        // Connect
        await Discord.ConnectAsync();
        
        // Run RSS monitoring task
        await Tasks.RssMonitoringTask.ExecuteAsync();
        
        await Task.Delay(Timeout.InfiniteTimeSpan);
    }
}
