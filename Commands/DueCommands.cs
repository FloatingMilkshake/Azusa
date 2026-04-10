namespace Azusa.Commands;

internal static class DueCommands
{
    [Command("due")]
    [Description("Get due assignments from Canvas.")]
    [AllowedProcessors(typeof(TextCommandProcessor))]
    [RequireApplicationOwner]
    public static async Task DueCommandAsync(TextCommandContext ctx,
        [Parameter("filter")] [Description("The date to filter to. Relative or absolute. Defaults to 5 days.")] [RemainingText] string filter = "5d")
    {
        var now = DateTime.Now;
        var date = ParseFilter(filter);
        if (date is null)
        {
            await ctx.RespondAsync("Couldn't parse your filter! Try again.");
            return;
        }
        
        await ctx.RespondAsync("Getting data from Canvas...");
        var response = await ctx.GetResponseAsync();
        var pages = await GetAllPagesAsync(date.Value);
        await response.ModifyAsync("Processing...");
        var items = FilterItems(pages);
        var timePeriodStr = TimeSpan.FromSeconds(Math.Round((now - date.Value).TotalSeconds)).Humanize();
        await response.ModifyAsync(new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithTitle($"{items.Count} assignment{(items.Count == 1 ? "" : "s")} due")
                .WithDescription(SortItemsForDisplay(items))
                .WithColor(new DiscordColor("#E95434"))
                .WithFooter(items.Count > 0
                        ? $"Here's everything unsubmitted with a due date set within the {(date.Value < now ? "last" : "next")} {timePeriodStr}!"
                        : $"There are no unsubmitted assignments with a due date set within the {(date.Value < now ? "last" : "next")} {timePeriodStr}!")));
    }
    
    /// <summary>
    /// Parse the provided filter as a DateTime.
    /// </summary>
    /// <param name="filter">The filter to parse. Can be a relative or absolute date.</param>
    /// <returns>The filter expressed as a DateTime. Null if failed to parse.</returns>
    private static DateTime? ParseFilter(string filter)
    {
        // Try parsing as relative with HumanDateParser first, and fall back to parsing as absolute with DateTime
        DateTime date;
        try
        {
            if (filter.Contains('-'))
                filter = $"{filter} ago".Replace("-", "");
            date = HumanDateParser.HumanDateParser.Parse(filter);
        }
        catch (HumanDateParser.ParseException)
        {
            try
            {
                date = DateTime.Parse(filter);
            }
            catch (FormatException)
            {
                return null;
            }
        }
        return date;
    }
    
    /// <summary>
    /// Gets all pages of content (assignments, announcements, etc.) from the Canvas API that are listed in the planner on or before the date provided.
    /// </summary>
    /// <param name="date">The date to use as a filter. Only gets content listed in the planner on or before this date.</param>
    /// <returns>A list containing pages of content from the Canvas API.</returns>
    private static async Task<List<string>> GetAllPagesAsync(DateTime date)
    {
        List<string> pages = [];
        
        // Get first page first
        var (firstPageData, firstPageContent) = await GetSinglePageAsync(date);
        pages.Add(firstPageContent);
        var nextPageUrl = GetNextPageFromHeader(firstPageData);
        if (Setup.State.Discord.Client.Logger.IsEnabled(LogLevel.Debug))
            Setup.State.Discord.Client.Logger.LogDebug("Due: Got page 1, {nextPage}", !string.IsNullOrEmpty(nextPageUrl) ? "found next page" : "no next page");
        
        while (!string.IsNullOrEmpty(nextPageUrl))
        {
            var (pageData, pageContent) = await GetSinglePageAsync(date, nextPageUrl);
            pages.Add(pageContent);
            nextPageUrl = GetNextPageFromHeader(pageData);
            if (Setup.State.Discord.Client.Logger.IsEnabled(LogLevel.Debug))
                Setup.State.Discord.Client.Logger.LogDebug("Due: Got page {pageNumber}, {nextPage}", pages.Count, !string.IsNullOrEmpty(nextPageUrl) ? "found next page" : "no next page");
        }
        
        return pages;
    }
    
    /// <summary>
    /// Gets a single page of content (assignments, announcements, etc.) from the Canvas API that are listed in the planner on or before the date provided.
    /// </summary>
    /// <param name="date">The date to use as a filter. Only gets content listed in the planner on or before this date.</param>
    /// <param name="page">The URL of a single page to fetch.</param>
    /// <returns>A single page of content from the Canvas API.</returns>
    private static async Task<(HttpResponseMessage, string)> GetSinglePageAsync(DateTime date, string page = null)
    {
        string url;
        if (page is null)
            url = Setup.Constants.CanvasApiPath;
        else
            url = page;
        
        var now = DateTime.Now;
        if (date < now)
            url += $"&start_date={date:yyyy-MM-dd}&end_date={now:yyyy-MM-dd}";
        else
            url += $"&start_date={now:yyyy-MM-dd}&end_date={date:yyyy-MM-dd}";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Bearer {Setup.Configuration.ConfigJson.Canvas.ApiToken}");
        var response = await Setup.Constants.HttpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch from the Canvas API! {Convert.ToInt32(response.StatusCode)}: {response.ReasonPhrase}");
        
        return (response, await response.Content.ReadAsStringAsync());
    }
    
    /// <summary>
    /// Gets the next page of items from the `link` header in the provided API response.
    /// </summary>
    /// <param name="response">The API response to get the next page from.</param>
    /// <returns>The URL to the next page.</returns>
    private static string GetNextPageFromHeader(HttpResponseMessage response)
    {
        // 'link' header contains a "next" URL for the next page
        // looking for:
        // <https://blah>; rel="next"
        //  ^^^^^^^^^^^^
        
        var linkHeader = response.Headers.FirstOrDefault(x => x.Key.Equals("link", StringComparison.OrdinalIgnoreCase));
        if (linkHeader.Value is null)
            return string.Empty;
        var linkHeaderValue = linkHeader.Value.FirstOrDefault();
        var nextLinkMatch = Setup.Constants.RegularExpressions.CanvasApiLinkHeaderNextUrlPattern.Match(linkHeaderValue);
        return nextLinkMatch.Groups[1].Value;
    }
    
    /// <summary>
    /// Filters pages to return a list of items of type 'assignment' or 'quiz'.
    /// </summary>
    /// <param name="pages">The list of pages to filter.</param>
    /// <returns>The filtered list of items.</returns>
    private static List<string> FilterItems(List<string> pages)
    {
        List<string> filteredItems = [];
        
        foreach (var rawPage in pages)
        {
            var page = JArray.Parse(rawPage);
            foreach (var item in page)
            {
                if ((item["plannable_type"]?.Value<string>() == "assignment" || item["plannable_type"]?.Value<string>() == "quiz")
                    && (!(bool)item["submissions"]?["submitted"]) && (!(bool)item["submissions"]?["graded"]))
                {
                    filteredItems.Add(item.ToString());
                }
            }
        }
        
        return filteredItems;
    }
    
    /// <summary>
    /// Groups items by course, then sorts by date. Formats as a string for display.
    /// </summary>
    /// <param name="items">The list of items to sort.</param>
    /// <returns>The sorted list of items.</returns>
    private static string SortItemsForDisplay(List<string> items)
    {
        // Organize items by course
        // <name of course, [items]>
        Dictionary<string, List<JObject>> itemsByCourse = [];
        
        // Group by course
        foreach (var item in items.Select(JObject.Parse))
        {
            if (itemsByCourse.ContainsKey(item["context_name"]!.ToString()))
                itemsByCourse[item["context_name"]!.ToString()].Add(item);
            else
                itemsByCourse[item["context_name"]!.ToString()] = [item];
        }
        
        // Sort within groups by date (oldest to newest / due soonest to latest)
        itemsByCourse = itemsByCourse.ToDictionary(
            x => x.Key,
            x => x.Value.OrderBy(item => (DateTime?)item["plannable"]?["due_at"]).ToList()
        );
        
        // Take the sorted list and create pretty output strings for display; group by course, already sorted by date
        
        string output = "";
        
        foreach (var course in itemsByCourse)
        {
            var courseTitle = course.Key;
            var courseItems = course.Value;
            
            output += $"__{courseTitle}:__\n";
            foreach (var item in courseItems)
            {
                var dueDate = ((DateTime?)(item["plannable"]?["due_at"]));
                var dueDateStr = dueDate.HasValue
                    ? dueDate.Value.Year == DateTime.Now.Year
                        ? dueDate.Value.ToLocalTime().ToString("MM/dd @ hh:mmtt").ToLower()
                        : dueDate.Value.ToLocalTime().ToString("MM/dd/yy @ hh:mmtt").ToLower()
                    : "[error]";
                output += $"- **{StringHelpers.Truncate(item["plannable"]?["title"]?.ToString(), 30)}**, due {dueDateStr}\n";
            }
        }
        
        return output;
    }
}
