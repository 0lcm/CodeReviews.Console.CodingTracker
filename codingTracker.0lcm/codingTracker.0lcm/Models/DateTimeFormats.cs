
using Microsoft.Extensions.Configuration;

namespace codingTracker._0lcm.Models
{
    internal static class DateTimeFormats
    {
        private static readonly IConfiguration _configuration = Program.configuration;

        internal static readonly string[] HourFormats =
            _configuration.GetSection("TimeFormats:HourFormat").Get<string[]>()
            ?? ["H:mm", "HH:mm"];

        internal static readonly string[] DateFormats =
            _configuration.GetSection("TimeFormats:DateFormats").Get<string[]>()
            ?? ["yyyy-MM-dd", "yyyy-MM-d", "yyyy-M-dd", "yyyy-M-d"];

        internal static readonly string DateIso =
            _configuration["TimeFormats:DateIso"] ?? "yyyy-MM-dd";

        internal static readonly string FullDateWithTimeFormat =
            _configuration["TimeFormats:FullDateWithTimeFormat"] ?? "yyyy-MM-dd HH:mm:ss";
    }
}
