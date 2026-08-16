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

        #region set up logging
        var logConfig = new LoggerConfiguration().WriteTo.Console(theme: AnsiConsoleTheme.Sixteen).MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Error);
#if DEBUG
        logConfig.MinimumLevel.Debug();
#else
        logConfig.MinimumLevel.Debug();
#endif

        if (Setup.State.Process.Configuration.GrafanaLokiUrl is not null)
        {
            var discordBot = "azusa";
#if DEBUG
            discordBot = "azusa_dev";
#endif
            logConfig.WriteTo.GrafanaLoki(Setup.State.Process.Configuration.GrafanaLokiUrl, [new LokiLabel { Key = "discord_bot", Value = discordBot }]);
        }

        Log.Logger = logConfig.CreateLogger();
        #endregion set up logging

        #region build Discord client
        var clientBuilder = DiscordClientBuilder.CreateDefault(Environment.GetEnvironmentVariable("BOT_TOKEN"), DiscordIntents.All);
        clientBuilder.ConfigureLogging(config =>
        {
            config.AddSerilog();
        });
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
            extension.AddCommands(commandTypes);
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
