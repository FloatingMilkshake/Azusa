namespace Azusa.Commands;

public class MimeType
{
    [Command("mimetype")]
    [Description("Check the mime type of a file online.")]
    [TextAlias("mime", "type")]
    public static async Task MimeTypeCommand(CommandContext ctx,
        [Parameter("file")]
        [Description("The path to the file to check the mime type for.")] string file)
    {
        file = file.Replace("<", "").Replace(">", "");
        
        var response = await Program.HttpClient.GetAsync(file);
        if (response.IsSuccessStatusCode)
            await ctx.RespondAsync(response.Content.Headers.ContentType?.ToString() ?? "none");
        else
            await ctx.RespondAsync($"{Convert.ToInt32(response.StatusCode)} {response.ReasonPhrase}");
    }
}