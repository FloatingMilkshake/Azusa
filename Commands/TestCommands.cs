namespace Azusa.Commands;

public class TestCommands
{
    [Command("suppress")]
    public static async Task EnableEmbedSuppressionCommandAsync(TextCommandContext ctx, ulong messageId)
    {
        var msg = await ctx.Channel.GetMessageAsync(messageId);
        await msg.ModifyEmbedSuppressionAsync(true);
        await ctx.RespondAsync("suppressed");
    }

    [Command("unsuppress")]
    public static async Task DisableEmbedSuppressionCommandAsync(TextCommandContext ctx, ulong messageId)
    {
        var msg = await ctx.Channel.GetMessageAsync(messageId);
        await msg.ModifyEmbedSuppressionAsync(false);
        await ctx.RespondAsync("unsuppressed");
    }
}
