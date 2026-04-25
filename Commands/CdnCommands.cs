namespace Azusa.Commands;

[Command("cdn")]
[Description("Manage files uploaded to R2.")]
[AllowedProcessors(typeof(TextCommandProcessor))]
[RequireApplicationOwner]
internal static class CdnCommands
{
    [Command("upload")]
    [Description("Upload a file to R2. An uploaded file attachment will override the `link` argument!")]
    [TextAlias("up", "u")]
    public static async Task CdnUploadCommandAsync(TextCommandContext ctx,
        [Parameter("name")] [Description("The name for the uploaded file.")]
        string name,
        [Parameter("link")] [Description("A link to a file to upload.")]
        string link = default)
    {
        DiscordAttachment file = default;
        if (ctx.Message.Attachments.Count > 0)
            file = ctx.Message.Attachments[0];
        
        if (file == default && link == default)
        {
            await ctx.RespondAsync("You must provide a link or file to upload!");
            return;
        }

        if (name.Contains(' '))
            await ctx.RespondAsync("The name of the file cannot contain spaces! Please try again.");

        if (file is not null) link = file.Url;

        if (link is not null)
        {
            link = link.Replace("<", "");
            link = link.Replace(">", "");
        }

        string fileName;

        // Get file, where 'link' is the URL
        MemoryStream memStream = new(await Setup.Constants.HttpClient.GetByteArrayAsync(link));

        try
        {
            var bucket = Setup.State.Process.Configuration.S3.Bucket;

            // Strip the URL down to just the file name

            var fileNameAndExtension = Setup.Constants.RegularExpressions.CdnFileNamePattern.Match(link ?? "").Value;

            // From here on out we can be sure that 'fileNameAndExtension' is in the format 'example.png'.

            var extension = Path.GetExtension(fileNameAndExtension);

            // The user might have included an extension in their desired filename. We should remove it.
            // We should not just match `extension` because the user may have provided a different one!
            // Let's use regex instead.
            var userExtension = Setup.Constants.RegularExpressions.CdnFileExtensionPattern.Match(name).Value;
            if (userExtension != "")
                name = name.Replace(userExtension, "");

            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            fileName = name switch
            {
                "random" or "generate" => new string(Enumerable.Repeat(chars, 10)
                    .Select(s => s[new Random().Next(s.Length)])
                    .ToArray()) + extension,
                "preserve" => fileNameAndExtension,
                _ => name + extension
            };
            
            var mimeType = MimeTypeMap.GetMimeType(extension);
            if (mimeType == "application/octet-stream")
                mimeType = null;

            var args = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName)
                .WithStreamData(memStream)
                .WithObjectSize(memStream.Length)
                .WithContentType(mimeType);

            await Setup.State.Process.Minio.PutObjectAsync(args);
        }
        catch (MinioException e)
        {
            await ctx.RespondAsync($"An API error occured while uploading! `{e.GetType()}: {e.Message}`");
            return;
        }
        catch (Exception e)
        {
            await ctx.RespondAsync($"An unexpected error occured while uploading! `{e.GetType()}: {e.Message}`");
            return;
        }

        await ctx.RespondAsync($"Upload successful!\n<{Setup.State.Process.Configuration.S3.BaseUrl}/{fileName}>");
    }

    [Command("delete")]
    [Description("Delete a file from R2.")]
    [TextAlias("del", "d")]
    public static async Task CdnDeleteCommandAsync(TextCommandContext ctx,
        [Parameter("file")] [Description("The file to delete.")]
        string fileToDelete)
    {
        fileToDelete = fileToDelete.Replace("<", "").Replace(">", "");

        var fileName = fileToDelete.Replace($"{Setup.State.Process.Configuration.S3.BaseUrl}/", "");

        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(Setup.State.Process.Configuration.S3.Bucket)
                .WithObject(fileName);

            await Setup.State.Process.Minio.RemoveObjectAsync(args);
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

        var cloudflareUrlPrefix = Setup.State.Process.Configuration.S3.UrlPrefix;

        // This code is (mostly) taken from https://github.com/Sankra/cloudflare-cache-purger/blob/master/main.csx#L113.
        // (Note that I originally found it here: https://github.com/Erisa/Lykos/blob/1f32e03/src/Modules/Owner.cs#L232)

        Setup.Types.CloudflareContent content = new([cloudflareUrlPrefix + fileName]);
        var cloudflareContentString = JsonConvert.SerializeObject(content);
        try
        {
            using HttpRequestMessage request =
                new(HttpMethod.Delete, $"https://api.cloudflare.com/client/v4/zones/{Setup.State.Process.Configuration.S3.ZoneId}/purge_cache/files");
            request.Content = new StringContent(cloudflareContentString, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", $"Bearer {Setup.State.Process.Configuration.S3.Token}");

            var response = await Setup.Constants.HttpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                await ctx.EditResponseAsync("File deleted successfully!\nCloudflare cache purged!");
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
    [Description("Check whether a file exists in the R2 bucket. Uses the R2 API to avoid caching.")]
    [TextAlias("c")]
    public static async Task CdnPreviewCommandAsync(TextCommandContext ctx,
        [Parameter("name")] [Description("The name (or link) of the file to check.")]
        string name)
    {
        if (name.Contains(Setup.State.Process.Configuration.S3.BaseUrl))
            name = name.Replace(Setup.State.Process.Configuration.S3.BaseUrl, "").Trim('/');

        try
        {
            await Setup.State.Process.Minio.GetObjectAsync(new GetObjectArgs().WithBucket(Setup.State.Process.Configuration.S3.Bucket)
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
}
