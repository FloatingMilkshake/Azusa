namespace Azusa.Commands;

public class Avy
{
    [Command("avy")]
    [Description("Update your avatar.")]
    [RequireApplicationOwner]
    public static async Task AvyCommand(TextCommandContext ctx)
    {
        await ctx.RespondAsync("Working on it...");

        bool wasFaviconCreated = false;
        try
        {
            MemoryStream memStream = new(await Program.HttpClient.GetByteArrayAsync(ctx.User.AvatarUrl));

            var args = new PutObjectArgs()
                .WithBucket(Program.ConfigJson.S3.Bucket)
                .WithObject("avatar.png")
                .WithStreamData(memStream)
                .WithObjectSize(memStream.Length)
                .WithContentType("image/png");

            await Program.Minio.PutObjectAsync(args);

            string tmpAvatarPath = RuntimeInformation.OSDescription.Contains("Windows")
                ? $"{Path.GetTempPath()}\\avatar.png"
                : "/tmp/avatar.png";
            string tmpFaviconPath = RuntimeInformation.OSDescription.Contains("Windows")
                ? $"{Path.GetTempPath()}\\favicon.png"
                : "/tmp/favicon.png";

            await File.WriteAllBytesAsync(tmpAvatarPath, await Program.HttpClient.GetByteArrayAsync(ctx.User.AvatarUrl));
            var magickResult = await Shell.ShellCommand($"magick {tmpAvatarPath} -resize 192x192 {tmpFaviconPath}", CancellationToken.None);
            wasFaviconCreated = true;

            memStream = new(await File.ReadAllBytesAsync(tmpFaviconPath));
            args = new PutObjectArgs()
                .WithBucket(Program.ConfigJson.S3.Bucket)
                .WithObject("favicon.png")
                .WithStreamData(memStream)
                .WithObjectSize(memStream.Length)
                .WithContentType("image/png");

            await Program.Minio.PutObjectAsync(args);

            File.Delete(tmpAvatarPath);
            File.Delete(tmpFaviconPath);
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

        // This code is (mostly) taken from https://github.com/Sankra/cloudflare-cache-purger/blob/master/main.csx#L113.
        // (Note that I originally found it here: https://github.com/Erisa/Lykos/blob/1f32e03/src/Modules/Owner.cs#L232)

        Cdn.CloudflareContent content = new([Program.ConfigJson.S3.UrlPrefix + "avatar.png", Program.ConfigJson.S3.UrlPrefix + "favicon.png"]);
        var cloudflareContentString = JsonConvert.SerializeObject(content);
        bool wasCloudflareCachePurged = false;
        string cloudflareCachePurgeStatusCode = default;
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Delete, $"https://api.cloudflare.com/client/v4/zones/{Program.ConfigJson.S3.ZoneId}/purge_cache/files");
            request.Content = new StringContent(cloudflareContentString, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", $"Bearer {Program.ConfigJson.S3.Token}");

            var cachePurgeResponse = await Program.HttpClient.SendAsync(request);
            var responseText = await cachePurgeResponse.Content.ReadAsStringAsync();

            if (cachePurgeResponse.IsSuccessStatusCode)
                wasCloudflareCachePurged = true;
            else
                cloudflareCachePurgeStatusCode = $"{(int)cachePurgeResponse.StatusCode}: {cachePurgeResponse.ReasonPhrase}";
        }
        catch (Exception e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"File deleted successfully!\nAn unexpected error occured when purging Cloudflare cache: ```json\n{e.Message}```"));
        }

        var msg = await ctx.GetResponseAsync();
        string response;
        if (wasCloudflareCachePurged)
            response = "Successfully updated avatar and purged Cloudflare cache!";
        else
            response = $"Successfully updated avatar!\nFailed to purge Cloudflare cache: `{cloudflareCachePurgeStatusCode}`";
        if (!wasFaviconCreated)
            response += "\nFailed to update favicon!";
        await msg.ModifyAsync(response);
    }
}
