namespace Azusa.Setup;

public static class Eval
{
    internal static readonly string[] Imports =
    [
        "System", "System.Collections.Generic", "System.Linq",
        "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Commands",
        "DSharpPlus.Entities", "Microsoft.Extensions.Logging", "Newtonsoft.Json",
        "Azusa", "Azusa.Setup.Eval", "Azusa.Setup.Eval.Utilities"
    ];

    public sealed class Selection
    {
        public DiscordUser User { get; private set; }
        public DiscordMessage Message { get; private set; }
        public DateTime Timestamp { get; private set; }

        internal Selection(DiscordUser user)
        {
            User = user;
            Message = default;
            Timestamp = DateTime.UtcNow;
        }

        internal Selection(DiscordMessage message)
        {
            User = message.Author;
            Message = message;
            Timestamp = DateTime.UtcNow;
        }
    }

    public sealed class Globals
    {
        internal Globals(DiscordClient client, TextCommandContext ctx, CancellationToken cancellationToken)
        {
            Context = ctx;
            Client = client;
            Message = ctx.Message;
            Channel = ctx.Channel;
            Guild = ctx.Guild;
            User = ctx.User;
            if (Guild is not null) Member = Guild.GetMemberAsync(User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            CToken = cancellationToken;
            Selection = Setup.State.Caches.Selections.GetValueOrDefault(ctx.User.Id);
        }

        public DiscordClient Client { get; private set; }
        public DiscordMessage Message { get; private set; }
        public DiscordChannel Channel { get; private set; }
        public DiscordGuild Guild { get; private set; }
        public DiscordUser User { get; private set; }
        public DiscordMember Member { get; private set; }
        public TextCommandContext Context { get; private set; }
        public CancellationToken CToken { get; private set; }
        public Selection Selection { get; private set; }
    }

    public static class Utilities
    {
        public static string Jsonify(object input)
        {
            if (input is null)
                return null;
            return $"```json\n{JsonConvert.SerializeObject(input, Formatting.Indented)}\n```";
        }
    }
}
