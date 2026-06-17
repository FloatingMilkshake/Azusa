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

        string response = "";

        bool wasFaviconCreated;
        try
        {
#if DEBUG
            string fileName = "test_avatar.png";
#else
            string fileName = "avatar.png";
#endif

            MemoryStream memStream = new(await Setup.Constants.HttpClient.GetByteArrayAsync(ctx.User.AvatarUrl));

            var result = await Setup.Types.ShellCommand.RunAsync($"rclone rcat fs-crypt:/cdn/{fileName}", CancellationToken.None, memStream);

            if (result.ExitCode == 0)
            {
                response += $"Avatar updated successfully!";
            }
            else
            {
                response += $"Avatar update failed with exit code {result.ExitCode}! Error: {result.Error}";
            }

#if DEBUG
            fileName = "test_favicon.png";
#else
            fileName = "favicon.png";
#endif

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

            result = await Setup.Types.ShellCommand.RunAsync($"rclone rcat fs-crypt:/cdn/{fileName}", CancellationToken.None, memStream);

            if (result.ExitCode == 0)
            {
                response += $"\nFavicon updated successfully!";
            }
            else
            {
                response += $"\nFavicon update failed with exit code {result.ExitCode}! Error: {result.Error}";
            }

            File.Delete(tmpAvatarPath);
            File.Delete(tmpFaviconPath);

            var msg = await ctx.GetResponseAsync();
            await msg.ModifyAsync(response);
        }
        catch (Exception e)
        {
            await ctx.RespondAsync($"An unexpected error occured while uploading! `{e.GetType()}: {e.Message}`");
            return;
        }
    }
}
