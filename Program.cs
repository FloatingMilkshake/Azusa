namespace Azusa;

internal static class Program
{
    internal static async Task Main()
    {
        // Read config.json
        Setup.Configuration.ConfigJson = JsonConvert.DeserializeObject<ConfigJson>(await File.ReadAllTextAsync("config.json"));

        if (Setup.Configuration.ConfigJson is null)
        {
            Console.WriteLine("config.json is malformed. Please be sure it has all of the required values.");
            Environment.Exit(1);
        }

        var clientBuilder = DiscordClientBuilder.CreateDefault(Setup.Configuration.ConfigJson.Token, DiscordIntents.All);
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
            // Register commands
            var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                t.IsClass && t.Namespace is not null && t.Namespace.Contains("Azusa.Commands") &&
                !t.IsNested).ToList();

            extension.CommandErrored += Errors.CommandErrors.HandleCommandErroredEventAsync;

            extension.AddCommands(commandTypes, 799644062973427743);

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

        await Setup.State.Discord.Client.ConnectAsync();

        Setup.Constants.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Azusa (contact https://floatingmilkshake.com)");

        await Task.Delay(Timeout.InfiniteTimeSpan);
    }
}
