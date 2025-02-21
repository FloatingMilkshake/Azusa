namespace Azusa.Commands;

[Command("cdn")]
[AllowedProcessors(typeof(TextCommandProcessor))]
[Description("Manage files uploaded to Amazon S3-compatible cloud storage.")]
public static partial class Cdn
{
    [Command("upload")]
    [Description("Upload a file to Amazon S3-compatible cloud storage.")]
    public static async Task Upload(TextCommandContext ctx,
        [Parameter("name"), Description("The name for the uploaded file.")]
        string name,
        [Parameter("link"), Description("A link to a file to upload.")]
        string link = null,
        [Parameter("file"), Description("A direct file to upload. This will override a link if both are provided!")]
        DiscordAttachment file = null)
    {
        if (file is null && link is null)
        {
            await ctx.RespondAsync("You must provide a link or file to upload!");
            return;
        }
        
        if (name.Contains(' '))
        {
            await ctx.RespondAsync("The name of the file cannot contain spaces! Please try again.");
        }

        if (file is not null) link = file.Url;

        if (link is not null)
        {
            link = link.Replace("<", "");
            link = link.Replace(">", "");
        }

        string fileName;

        // Get file, where 'link' is the URL
        MemoryStream memStream = new(await Program.HttpClient.GetByteArrayAsync(link));

        try
        {
            var bucket = Program.ConfigJson.S3.Bucket;

            // Strip the URL down to just the file name

            // Regex partially taken from https://stackoverflow.com/a/26253039
            var fileNamePattern = FileNamePattern();

            var fileNameAndExtension = fileNamePattern.Match(link ?? "").Value;

            // From here on out we can be sure that 'fileNameAndExtension' is in the format 'example.png'.

            var extension = Path.GetExtension(fileNameAndExtension);
            
            // The user might have included an extension in their desired filename. We should remove it.
            // We should not just match `extension` because the user may have provided a different one!
            // Let's use regex instead.
            var fileExtensionPattern = FileExtensionPattern();
            var userExtension = fileExtensionPattern.Match(name).Value;
            if (userExtension != "")
            {
                name = name.Replace(userExtension, "");
            }

            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            fileName = name switch
            {
                "random" or "generate" => new string(Enumerable.Repeat(chars, 10)
                    .Select(s => s[(new Random()).Next(s.Length)])
                    .ToArray()) + extension,
                "preserve" => fileNameAndExtension,
                _ => name + extension
            };

            var args = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName)
                .WithStreamData(memStream)
                .WithObjectSize(memStream.Length)
                .WithContentType(MimeTypeMap.GetMimeType(extension));

            await Program.Minio.PutObjectAsync(args);
        }
        catch (MinioException e)
        {
            await ctx.RespondAsync($"An API error occured while uploading!```\n{e.GetType()}: {e.Message}\n{e.StackTrace}```");
            return;
        }
        catch (Exception e)
        {
            await ctx.RespondAsync($"An unexpected error occured while uploading!```\n{e.GetType()}: {e.Message}\n{e.StackTrace}```");
            return;
        }

        await ctx.RespondAsync($"Upload successful!\n<{Program.ConfigJson.S3.BaseUrl}/{fileName}>");
    }

    [Command("delete")]
    [Description("Delete a file from Amazon S3-compatible cloud storage.")]
    public static async Task DeleteUpload(TextCommandContext ctx,
        [Parameter("file"), Description("The file to delete.")]
        string fileToDelete)
    {
        fileToDelete = fileToDelete.Replace("<", "").Replace(">", "");

        var fileName = fileToDelete.Replace($"{Program.ConfigJson.S3.BaseUrl}/", "");

        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(Program.ConfigJson.S3.Bucket)
                .WithObject(fileName);

            await Program.Minio.RemoveObjectAsync(args);
        }
        catch (MinioException e)
        {
            await ctx.RespondAsync($"An API error occured while attempting to delete the file!```\n{e.Message}```");
            return;
        }
        catch (Exception e)
        {
            await ctx.RespondAsync($"An unexpected error occured while attempting to delete the file!```\n{e.Message}```");
            return;
        }

        await ctx.RespondAsync("File deleted successfully!\nAttempting to purge Cloudflare cache...");

        var cloudflareUrlPrefix = Program.ConfigJson.S3.UrlPrefix;

        // This code is (mostly) taken from https://github.com/Sankra/cloudflare-cache-purger/blob/master/main.csx#L113.
        // (Note that I originally found it here: https://github.com/Erisa/Lykos/blob/1f32e03/src/Modules/Owner.cs#L232)

        CloudflareContent content = new([cloudflareUrlPrefix + fileName]);
        var cloudflareContentString = JsonConvert.SerializeObject(content);
        try
        {
            using HttpRequestMessage request =
                new(HttpMethod.Delete, $"https://api.cloudflare.com/client/v4/zones/{Program.ConfigJson.S3.ZoneId}/purge_cache/files");
            request.Content = new StringContent(cloudflareContentString, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", $"Bearer {Program.ConfigJson.S3.Token}");

            var response = await Program.HttpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                await ctx.EditResponseAsync($"File deleted successfully!\nCloudflare cache purged!");
            else
                await ctx.EditResponseAsync($"File deleted successfully!\nAn API error occured when purging Cloudflare cache: ```json\n{responseText}```");
        }
        catch (Exception e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"File deleted successfully!\nAn unexpected error occured when purging Cloudflare cache: ```json\n{e.Message}```"));
        }
    }

    [Command("check")]
    [Description("Check whether a file exists in your S3 bucket. Uses the S3 API to avoid caching.")]
    public static async Task CdnPreview(TextCommandContext ctx,
        [Parameter("name"), Description("The name (or link) of the file to check.")]
        string name)
    {
        if (name.Contains(Program.ConfigJson.S3.BaseUrl))
            name = name.Replace(Program.ConfigJson.S3.BaseUrl, "").Trim('/');

        try
        {
            await Program.Minio.GetObjectAsync(new GetObjectArgs().WithBucket(Program.ConfigJson.S3.Bucket)
                .WithObject(name).WithFile(name));
        }
        catch (ObjectNotFoundException)
        {
            await ctx.RespondAsync("That file doesn't exist!");
            return;
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"I ran into an error trying to check for that file! {ex.GetType()}: {ex.Message}");
            return;
        }

        await ctx.RespondAsync("That file exists!");
    }

    [GeneratedRegex(@"[^/\\&\?#]+\.\w*(?=([\?&#].*$|$))")]
    private static partial Regex FileNamePattern();
    
    [GeneratedRegex(@"\.\w*(?=([\?&#].*$|$))")]
    private static partial Regex FileExtensionPattern();
    
    // This code is taken from https://github.com/Sankra/cloudflare-cache-purger/blob/master/main.csx#L197,
    // minus some minor changes.
    // (Note that I originally found it here: https://github.com/Erisa/Lykos/blob/3335c38/src/Modules/Owner.cs#L313)
    private readonly struct CloudflareContent(List<string> urls)
    {
        // ReSharper disable once UnusedMember.Local
        public List<string> Files { get; } = urls;
    }
}