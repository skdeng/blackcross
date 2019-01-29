using System;

namespace BlackCross.Core.Extensions
{
    public static class DateTimeExtension
    {
        public static double ToUnixTimestamp(this DateTime datetime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(datetime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
