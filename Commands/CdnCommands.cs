namespace Azusa.Commands;

[Command("cdn")]
[Description("Manage files uploaded to object storage.")]
[AllowedProcessors(typeof(TextCommandProcessor))]
[RequireApplicationOwner]
internal static class CdnCommands
{
    [Command("upload")]
    [Description("Upload a file to object storage. An uploaded file attachment will override the `link` argument!")]
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

            var result = await Setup.Types.ShellCommand.RunAsync($"rclone -vv rcat fs-crypt:/cdn/{fileName}", CancellationToken.None, memStream);

            if (result.ExitCode == 0)
            {
                await ctx.RespondAsync($"Upload successful!\n<{Setup.State.Process.Configuration.S3BaseUrl}/{fileName}>");
            }
            else
            {
                await ctx.RespondAsync($"Upload failed with exit code {result.ExitCode}! Error: {result.Error}");
            }
        }
        catch (Exception e)
        {
            await ctx.RespondAsync($"An unexpected error occured while uploading! `{e.GetType()}: {e.Message}`");
            return;
        }
    }

    [Command("delete")]
    [Description("Delete a file from object storage.")]
    [TextAlias("del", "d")]
    public static async Task CdnDeleteCommandAsync(TextCommandContext ctx,
        [Parameter("file")] [Description("The file to delete.")]
        string fileToDelete)
    {
        fileToDelete = fileToDelete.Replace("<", "").Replace(">", "");

        var fileName = fileToDelete.Replace($"{Setup.State.Process.Configuration.S3BaseUrl}/", "");

        try
        {
            var result = await Setup.Types.ShellCommand.RunAsync($"rclone deletefile fs-crypt:/cdn/{fileName}", CancellationToken.None);

            if (result.ExitCode == 0)
            {
                await ctx.RespondAsync("File deleted successfully!");
            }
            else
            {
                await ctx.RespondAsync($"Deletion failed with exit code {result.ExitCode}! Error: {result.Error}");
            }
        }
        catch (Exception e)
        {
            await ctx.RespondAsync($"An unexpected error occured while attempting to delete the file!```\n{e.Message}```");
            return;
        }
    }

    [Command("check")]
    [Description("Check whether a file exists.")]
    [TextAlias("c")]
    public static async Task CdnPreviewCommandAsync(TextCommandContext ctx,
        [Parameter("name")] [Description("The name of the file to check.")]
        string name)
    {
        name = name.Replace(Setup.State.Process.Configuration.S3BaseUrl, "").Trim('/');

        try
        {
            var result = await Setup.Types.ShellCommand.RunAsync($"rclone ls fs-crypt:/cdn/{name}", CancellationToken.None);

            if (result.ExitCode == 0)
            {
                if (string.IsNullOrWhiteSpace(result.Output))
                {
                    await ctx.RespondAsync("That file doesn't exist!");
                }
                else
                {
                    await ctx.RespondAsync("That file exists!");
                }
            }
            else
            {
                await ctx.RespondAsync($"Check failed with exit code {result.ExitCode}! Error: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"I ran into an error trying to check for that file! {ex.GetType()}: {ex.Message}");
            return;
        }
    }
}
