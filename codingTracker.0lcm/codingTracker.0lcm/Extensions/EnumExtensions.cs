namespace codingTracker._0lcm.Extensions
{
    internal static class EnumExtensions
    {
        internal static string ToDisplayString(this Enum value)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                value.ToString(),
                "([a-z])([A-Z])",
                "$1 $2"
                );
        }
    }
}
