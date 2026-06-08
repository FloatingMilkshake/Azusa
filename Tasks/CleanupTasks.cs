namespace Azusa.Tasks;

internal static class CleanupTasks
{
    internal static async Task ExecuteAsync()
    {
        while (true)
        {
            CleanUpEvalSelections();
            await CleanUpSandbox();
            await Task.Delay(TimeSpan.FromMinutes(30));
        }
    }

    private static void CleanUpEvalSelections()
    {
        foreach (var selection in Setup.State.Caches.Selections)
        {
            if (selection.Value.Timestamp < DateTime.UtcNow.AddMinutes(-30))
            {
                Setup.State.Caches.Selections.Remove(selection.Key);
            }
        }
    }

    private static async Task CleanUpSandbox()
    {
        var sandboxChannel = await Setup.State.Discord.Client.GetChannelAsync(1493427927830761664);
        var overwrites = sandboxChannel.PermissionOverwrites.Where(x => x.Type == DiscordOverwriteType.Role);
        foreach (var overwrite in overwrites)
        {
            var setTime = await Setup.Storage.Redis.HashGetAsync("sandbox", overwrite.Id);
            if (setTime.HasValue && (JsonConvert.DeserializeObject<DateTime>(setTime)) < (DateTime.UtcNow.AddHours(-1)))
            {
                await sandboxChannel.DeleteOverwriteAsync(await overwrite.GetRoleAsync(), "Cleaning up sandbox");
                await Setup.Storage.Redis.HashDeleteAsync("sandbox", overwrite.Id);
            }
        }
    }
}
