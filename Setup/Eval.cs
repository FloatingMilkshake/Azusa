namespace Azusa.Setup;

public class Eval
{
    internal static readonly string[] Imports =
    [
        "System", "System.Collections.Generic", "System.Linq",
        "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Commands",
        "DSharpPlus.Entities", "Microsoft.Extensions.Logging", "Newtonsoft.Json",
        Assembly.GetExecutingAssembly().GetName().Name
    ];

    public class Globals
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
        }

        public DiscordClient Client { get; set; }
        public DiscordMessage Message { get; set; }
        public DiscordChannel Channel { get; set; }
        public DiscordGuild Guild { get; set; }
        public DiscordUser User { get; set; }
        public DiscordMember Member { get; set; }
        public TextCommandContext Context { get; set; }
        public CancellationToken CToken { get; set; }
    }
}
