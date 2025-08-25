namespace Azusa.Commands;

public static class WakeUp
{
    [Command("wakeup")]
    [Description("wake up wake up wake up wake up")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [TextAlias("wake", "wol", "w")]
    public static async Task WakeUpCommand(TextCommandContext ctx)
    {
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
        
        await ctx.RespondAsync("Alright, trying...");
        
        var response = await ctx.GetResponseAsync();
        
        // Ping to see if it woke up
        var cmdOut = await Shell.ShellCommand($"ping {Program.ConfigJson.Err.SshHost} -c 10");
        
        if (cmdOut.Output.Contains("64 bytes from"))
        {
            await response.ModifyAsync("Alright, trying... it worked!");
            return;
        }
        else
        {
            await response.ModifyAsync($"Alright, trying... it didn't work!\nExited `{cmdOut.ExitCode}`: {cmdOut.Error.Trim()}");
        }
    }
}