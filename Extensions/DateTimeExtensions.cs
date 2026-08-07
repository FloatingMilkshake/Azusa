internal static class DateTimeExtensions
{
    extension(DateTime dateTime)
    {
        internal string Humanize()
        {
            TimeSpan diff = DateTime.UtcNow - dateTime;
            string relative = diff.Duration().Humanize();
            return diff >= TimeSpan.Zero ? $"{relative} ago" : $"in {relative}";
        }
    }
}
