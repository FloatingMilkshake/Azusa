namespace Azusa.Commands;

public static class Err
{
    [Command("err")]
    [Description("Look up an error code with the Microsoft Error Lookup Tool.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task ErrCommand(TextCommandContext ctx,
        [Parameter("code"), Description("The error code to look up.")] string code)
    {
        var result = (await (await Program.HttpClient.GetAsync($"https://err.milkshake.services?code={code}")).Content.ReadAsStringAsync())
            .Replace("<pre>", "").Replace("</pre>", "");

        result = $"```\n{result}\n```";

        await Helpers.StringHelpers.SplitStringAsync(result, true, ctx: ctx);
    }
}
