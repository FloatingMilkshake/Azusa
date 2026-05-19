namespace Azusa.Commands;

internal class UpdateAvatarCommands
{
    [Command("updateavatar")]
    [TextAlias("updateavy")]
    [Description("Update your avatar.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task AvyCommandAsync(TextCommandContext ctx)
    {
        await ctx.RespondAsync("Working on it...");

        bool wasFaviconCreated;
        try
        {
            MemoryStream memStream = new(await Setup.Constants.HttpClient.GetByteArrayAsync(ctx.User.AvatarUrl));

            var args = new PutObjectArgs()
                .WithBucket(Setup.State.Process.Configuration.S3.Bucket)
#if DEBUG
                .WithObject("cdn/test_avatar.png")
#else
                .WithObject("cdn/avatar.png")
#endif
                .WithStreamData(memStream)
                .WithObjectSize(memStream.Length)
                .WithContentType("image/png");

            await Setup.State.Process.Minio.PutObjectAsync(args);

            string tmpAvatarPath = RuntimeInformation.OSDescription.Contains("Windows")
                ? $"{Path.GetTempPath()}\\avatar.png"
                : "/tmp/avatar.png";
            string tmpFaviconPath = RuntimeInformation.OSDescription.Contains("Windows")
                ? $"{Path.GetTempPath()}\\favicon.png"
                : "/tmp/favicon.png";

            await File.WriteAllBytesAsync(tmpAvatarPath, await Setup.Constants.HttpClient.GetByteArrayAsync(ctx.User.AvatarUrl));
            var magickResult = await Setup.Types.ShellCommand.RunAsync($"magick {tmpAvatarPath} -resize 192x192 {tmpFaviconPath}", CancellationToken.None);
            wasFaviconCreated = true;

            memStream = new(await File.ReadAllBytesAsync(tmpFaviconPath));
            args = new PutObjectArgs()
                .WithBucket(Setup.State.Process.Configuration.S3.Bucket)
#if DEBUG
                .WithObject("cdn/test_favicon.png")
#else
                .WithObject("cdn/favicon.png")
#endif
                .WithStreamData(memStream)
                .WithObjectSize(memStream.Length)
                .WithContentType("image/png");

            await Setup.State.Process.Minio.PutObjectAsync(args);

            File.Delete(tmpAvatarPath);
            File.Delete(tmpFaviconPath);
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

        var msg = await ctx.GetResponseAsync();
        string response = "Successfully updated avatar!";
        if (!wasFaviconCreated)
            response += "\nFailed to update favicon!";
        await msg.ModifyAsync(response);
    }
}
