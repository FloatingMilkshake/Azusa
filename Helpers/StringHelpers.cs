namespace Azusa.Helpers;

public static class StringHelpers
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static async Task<List<string>> SplitStringAsync(string input, bool respond = false, int maxLength = 1980, CommandContext ctx = null, string completionMessage = null)
    {
        List<string> split = [];
        
        if (input.Length > maxLength)
        {
            // Split into multiple messages
            for (int i = 0; i < input.Length; i += maxLength)
            {
                // If the output was meant to be in a code block (beginning of complete output string begins with ```)
                // then put each output segment into a code block
                        
                var length = Math.Min(maxLength, input.Length - i);
                var segment = input.Substring(i, length);
                var codeBlockRegex = new Regex("```.*$");
                if (codeBlockRegex.IsMatch(input.Split('\n')[0]))
                {
                    if (i == 0)
                        segment = $"{segment}\n```";
                    else if (i + maxLength > input.Length)
                        segment = $"{codeBlockRegex.Match(input.Split('\n')[0])}\n{segment}";
                    else
                        segment = $"{codeBlockRegex.Match(input.Split('\n')[0])}\n{segment}```";
                }
                        
                split.Add(segment);
            }
        }
        else
        {
            split.Add(input);
        }

        if (!respond)
            return split;
        
        if (ctx is null)
            throw new ArgumentException("SplitStringAsync cannot respond to a null CommandContext. Please provide a CommandContext.");
            
        if (split.Count == 1)
        {
            await ctx.RespondAsync(split.First().Trim() + $" {completionMessage}");
        }
        else
        {
            await ctx.RespondAsync(split.First());
            foreach (var message in split.Skip(1))
                await ctx.Channel.SendMessageAsync(message);
            if (completionMessage is not null)
                await ctx.RespondAsync(completionMessage);
        }
        return [];
    }
}