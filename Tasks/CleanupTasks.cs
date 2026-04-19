namespace Azusa.Tasks;

internal static class CleanupTasks
{
    internal static async Task ExecuteAsync()
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromMinutes(30));
            CleanUpEvalSelections();
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
}
