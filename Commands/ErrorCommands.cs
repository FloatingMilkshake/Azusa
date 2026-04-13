namespace Azusa.Commands;

internal static class ErrorCommands
{
    [Command("error")]
    [TextAlias("err")]
    [Description("Look up an error code with the Microsoft Error Lookup Tool.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task ErrorCommandAsync(TextCommandContext ctx,
        [Parameter("code"), Description("The error code to look up.")] string code)
    {
        var result = (await (await Setup.Constants.HttpClient.GetAsync($"https://err.milkshake.services?code={code}")).Content.ReadAsStringAsync())
            .Replace("<pre>", "").Replace("</pre>", "");

        result = $"```\n{result}\n```";

        var splitResponse = result.SplitForDiscord();

        await ctx.RespondAsync(splitResponse.First());
        foreach (var part in splitResponse.Skip(1))
        {
            await ctx.Channel.SendMessageAsync(part);
        }
    }
}
