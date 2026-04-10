namespace Azusa.Commands;

internal class MimeTypeCommands
{
    [Command("mimetype")]
    [TextAlias("mime", "type")]
    [Description("Check the mime type of a file online.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task MimeTypeCommandAsync(CommandContext ctx,
        [Parameter("file")]
        [Description("The path to the file to check the mime type for.")] string file)
    {
        file = file.Replace("<", "").Replace(">", "");
        
        var response = await Setup.Constants.HttpClient.GetAsync(file);
        if (response.IsSuccessStatusCode)
            await ctx.RespondAsync(response.Content.Headers.ContentType?.ToString() ?? "none");
        else
            await ctx.RespondAsync($"{Convert.ToInt32(response.StatusCode)} {response.ReasonPhrase}");
    }
}
