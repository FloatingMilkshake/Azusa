namespace Azusa.Commands;

[Command("haste")]
[AllowedProcessors(typeof(TextCommandProcessor))]
[Description("Commands for managing Hastebin content.")]
public class HasteCommands
{
    private static readonly string HasteApiEndpoint = "https://api.cloudflare.com/client/v4/accounts/{0}/storage/kv/namespaces/{1}/values/documents:{2}";
    
    [Command("create")]
    [Description("Create a new paste.")]
    public static async Task HasteCreate(TextCommandContext ctx,
        [Parameter("content"), Description("The content of the new paste.")] string content,
        [Parameter("key"), Description("The name of the key for the new paste. Accepts formats \"documents:abc\" or \"abc\".")] string key = "")
    {
        if (string.IsNullOrWhiteSpace(key))
            key = GenerateKey();
        else
            key = key.Replace("documents:", "");
        
        var hasteUrl = string.Format(HasteApiEndpoint, Program.ConfigJson.Hastebin.AccountId, Program.ConfigJson.Hastebin.NamespaceId, key);
        var request = new HttpRequestMessage(HttpMethod.Put, hasteUrl);
        request.Content = new StringContent(content);
        request.Headers.Add("Authorization", $"Bearer {Program.ConfigJson.Hastebin.Token}");
        
        var response = await Program.HttpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            await ctx.RespondAsync($"Successfully created paste: {Program.ConfigJson.Hastebin.Url}/{key}");
        else
            await ctx.RespondAsync($"Failed to create paste! Cloudflare API returned code {response.StatusCode}.");
    }
    
    [Command("delete")]
    [Description("Delete a paste.")]
    public static async Task HasteDelete(TextCommandContext ctx,
        [Parameter("key"), Description("The key of the paste to delete. Accepts formats \"documents:abc\" or \"abc\".")] string key)
    {
        key = key.Replace("documents:", "");
        key = key.Replace($"{Program.ConfigJson.Hastebin.Url.Trim('/') + "/"}", "");
        
        var hasteUrl = string.Format(HasteApiEndpoint, Program.ConfigJson.Hastebin.AccountId, Program.ConfigJson.Hastebin.NamespaceId, key);
        var request = new HttpRequestMessage(HttpMethod.Delete, hasteUrl);
        request.Headers.Add("Authorization", $"Bearer {Program.ConfigJson.Hastebin.Token}");
        
        var response = await Program.HttpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            await ctx.RespondAsync($"Successfully deleted paste: {Program.ConfigJson.Hastebin.Url}/{key}");
        else
            await ctx.RespondAsync($"Failed to delete paste! Cloudflare API returned code {response.StatusCode}.");
    }
    
    // https://github.com/FloatingMilkshake/starbin/blob/aa4726b/functions/documents/index.ts#L27-L44
    private static string GenerateKey()
    {
        Random random = new();
        const string vowels = "aeiou";
        const string consonants = "bcdfghjklmnpqrstvwxyz";
        const int size = 6;
        
        var key = "";
        var start = random.Next(2);
        for (var i = 0; i < size; i++)
        {
            key += i % 2 == start ? consonants[random.Next(consonants.Length)] : vowels[random.Next(vowels.Length)];
        }
        
        return key;
    }
}