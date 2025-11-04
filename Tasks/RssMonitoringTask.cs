namespace Azusa.Tasks;

public class RssMonitoringTask
{
    public static bool TaskDisabled = false;
    public static bool PushDisabled = false;
    
    public static async Task ExecuteAsync()
    {
#if DEBUG
        PushDisabled = true;
#endif
        
        while (true)
        {
            if (TaskDisabled)
                return;
            await CheckRssDeliveryTimeAsync();
            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    }
    
    public static async Task CheckRssDeliveryTimeAsync()
    {
        Program.Discord.Logger.LogDebug("Checking last RSS delivery time");
        
        try
        {
            var lastDeliveryTimeString = await Program.Redis.StringGetAsync("monitorssLastDelivery");
            if (!lastDeliveryTimeString.HasValue)
            {
                var now = DateTime.UtcNow;
                lastDeliveryTimeString = JsonConvert.SerializeObject(now);
                await Program.Redis.StringSetAsync("monitorssLastDelivery", JsonConvert.SerializeObject(now));
            }
            var lastDeliveryTime = JsonConvert.DeserializeObject<DateTime>(lastDeliveryTimeString);
            
            // FIRING
            if (lastDeliveryTime < DateTime.UtcNow.AddHours(-24))
            {
                if (PushDisabled)
                {
                    Program.Discord.Logger.LogWarning("[FIRING] Bad RSS delivery time; push disabled");
                }
                else
                {
                    Program.Discord.Logger.LogWarning("[FIRING] Bad RSS delivery time");
                    using var request = new HttpRequestMessage(HttpMethod.Get, "https://uptime.floatingmilkshake.com/api/push/TWb2yhcte8Y11T3flOBiJ2l3LxThXMEL?status=up&msg=No%20articles%20delivered%20for%2024%20hours");
                    request.Headers.Add("CF-Access-Client-Id", Program.ConfigJson.UptimeKumaServiceToken.ClientId);
                    request.Headers.Add("CF-Access-Client-Secret",  Program.ConfigJson.UptimeKumaServiceToken.ClientSecret);
                    await Program.HttpClient.SendAsync(request);
                }
            }
            // OK
            else
            {
                Program.Discord.Logger.LogDebug("[OK] Good RSS delivery time");
            }
        }
        catch (Exception ex)
        {
            try
            {
                var channel = await Program.Discord.GetChannelAsync(1409289579139305573);
                await channel.SendMessageAsync($"<@455432936339144705> failed to execute MonitoRSS monitoring task!\n```\n{ex.GetType()}: {ex.Message}\n{ex.StackTrace}\n```");
            }
            catch
            {
                // whatever
            }
            finally
            {
                Program.Discord.Logger.LogError("Failed to execute MonitoRSS monitoring task!\n{exType}: {exMessage}\n{exStackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
            }
            TaskDisabled = true;
        }
    }
}