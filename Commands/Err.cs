namespace Azusa.Commands;

public class Err
{
    [Command("err")]
    [Description("Look up a Microsoft error code with the Microsoft Error Lookup Tool.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    public static async Task ErrCmd(TextCommandContext ctx, [Description("The code to look up.")] string code)
    {
        await ctx.RespondAsync("Working on it...");
        
        // Shoot a wake on LAN packet to the Windows machine to ensure it is awake
        
        // Parse MAC address to byte array
        byte[] mac = Program.ConfigJson.Err.MacAddress.Split(':')
            .Select(b => Convert.ToByte(b, 16))
            .ToArray();

        // Create the magic packet
        byte[] packet = new byte[102];
        for (int i = 0; i < 6; i++) packet[i] = 0xFF;
        for (int i = 6; i < 102; i++) packet[i] = mac[i % mac.Length];

        // Send the magic packet
        using var client = new UdpClient();
        client.Connect(Program.ConfigJson.Err.IpAddress, Program.ConfigJson.Err.Port);
        await client.SendAsync(packet, packet.Length);
        
        // Sanitize input
        code = code.Replace("\"", "").Replace(";", "").Replace("&", "").Replace("|", "").Replace("&&", "").Replace("||", "");
        
        // SSH into the Windows machine and run the tool
        var cmd = $"ssh -o ConnectTimeout=30 {Program.ConfigJson.Err.SshUsername}@{Program.ConfigJson.Err.SshHost} \"$PSStyle.OutputRendering = \"PlainText\"; C:\\err.exe {code} 2>&1 | Out-String\"";
        var result = await Shell.ShellCommand(cmd);
        
        var response = GetErrorMessage(result);
        
        var outMsg = new DiscordMessageBuilder();
        if (response.Length < 2000)
        {
            outMsg.Content = response;
        }
        else
        {
            if (response.Length < 3980)
                outMsg.AddEmbed(new DiscordEmbedBuilder().WithDescription(response));
            else
                outMsg.AddEmbed(new DiscordEmbedBuilder().WithDescription($"{response[..3980]}\ntruncated...\n```"));
        }
        
        await ctx.EditResponseAsync(outMsg);
    }
    
    private static string GetErrorMessage(ShellCommandResponse result)
    {
        // Success; exit code 1 = msft error lookup tool couldn't find the error code
        if (result.ExitCode is 0 or 1)
            return $"```\n{result.Output}\n```";

        // Failure; show more detail to bot owners
        return $"Error lookup failed with exit code `{result.ExitCode}`: {(string.IsNullOrWhiteSpace(result.Output) ? "[no output]" : "\n```\n{result.Output}\n```")}";

    }
}