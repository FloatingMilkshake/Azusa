namespace Azusa;

internal static class Program
{
    internal static async Task Main()
    {
        #region read config.json
        Setup.State.Process.Configuration = JsonConvert.DeserializeObject<Setup.Types.ConfigJson>(await File.ReadAllTextAsync("config.json"));

        if (Setup.State.Process.Configuration is null)
        {
            Console.WriteLine("config.json is malformed. Please be sure it has all of the required values.");
            Environment.Exit(1);
        }
        #endregion read config.json

        #region set up Minio
        Setup.State.Process.Minio = new MinioClient()
            .WithEndpoint(Setup.State.Process.Configuration.S3.Endpoint)
            .WithCredentials(Setup.State.Process.Configuration.S3.AccessKey, Setup.State.Process.Configuration.S3.SecretKey)
            .WithRegion(Setup.State.Process.Configuration.S3.Region)
            .WithSSL()
            .Build();
        #endregion set up Minio

        #region build Discord client
        var clientBuilder = DiscordClientBuilder.CreateDefault(Setup.State.Process.Configuration.Token, DiscordIntents.All);
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
            Timeout = TimeSpan.FromSeconds(300)
        });
        clientBuilder.ConfigureEventHandlers((builder) =>
        {
            builder.HandleMessageCreated(MessageEvents.HandleMessageCreatedEventAsync)
                   .HandleComponentInteractionCreated(InteractionEvents.HandleComponentInteractionCreatedEventAsync)
                   .HandleGuildMemberAdded(MemberEvents.HandleGuildMemberAddedEventAsync)
                   .HandleGuildMemberRemoved(MemberEvents.HandleGuildMemberRemovedEventAsync)
                   .HandleGuildMemberUpdated(MemberEvents.HandleGuildMemberUpdatedEventAsync);
        });
        clientBuilder.UseCommands((_, extension) =>
        {
            var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                t.IsClass && t.Namespace is not null && t.Namespace.Contains("Azusa.Commands") &&
                !t.IsNested && t != typeof(Commands.SelectCommands)).ToList();
            extension.AddCommands(commandTypes, 799644062973427743);
            extension.AddCommands(typeof(Commands.SelectCommands));

            extension.CommandErrored += Errors.CommandErrors.HandleCommandErroredEventAsync;

            extension.AddCheck<RequireSecretRoleContextCheck>();
            extension.AddCheck<RequireCatRoleContextCheck>();

            TextCommandProcessor textCommandProcessor = new(new TextCommandConfiguration
            {
#if DEBUG
                PrefixResolver = new DefaultPrefixResolver(true, "azd").ResolvePrefixAsync
#else
                PrefixResolver = new DefaultPrefixResolver(true, "a!", "azusa", "azu", "az").ResolvePrefixAsync
#endif
            });
            extension.AddProcessor(textCommandProcessor);
        }, new CommandsConfiguration
        {
            UseDefaultCommandErrorHandler = false,
        });
        Setup.State.Discord.Client = clientBuilder.Build();
        #endregion build Discord client

        await Setup.State.Discord.Client.ConnectAsync();

        Setup.Constants.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Azusa (https://github.com/FloatingMilkshake/Azusa)");

        await Task.Run(async () => Tasks.CleanupTasks.ExecuteAsync());

        await Task.Delay(Timeout.InfiniteTimeSpan);
    }
}
