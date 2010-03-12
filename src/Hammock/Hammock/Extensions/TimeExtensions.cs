using System;

namespace Hammock.Extensions
{
    internal static class TimeExtensions
    {
        public static TimeSpan Ticks(this long value)
        {
            return TimeSpan.FromTicks(value);
        }

        // todo refactor this for accuracy
        public static TimeSpan Years(this int value)
        {
            return TimeSpan.FromDays(value * 365);
        }

        // todo refactor this for accuracy
        public static TimeSpan Months(this int value)
        {
            return TimeSpan.FromDays(value * 30);
        }

        public static TimeSpan Weeks(this int value)
        {
            return TimeSpan.FromDays(value * 7);
        }

        public static TimeSpan Days(this int days)
        {
            return new TimeSpan(days, 0, 0, 0);
        }

        public static TimeSpan Day(this int days)
        {
            return new TimeSpan(days, 0, 0, 0);
        }

        public static TimeSpan Hours(this int hours)
        {
            return new TimeSpan(0, hours, 0, 0);
        }

        public static TimeSpan Hour(this int hours)
        {
            return new TimeSpan(0, hours, 0, 0);
        }

        public static TimeSpan Minutes(this int minutes)
        {
            return new TimeSpan(0, 0, minutes, 0);
        }

        public static TimeSpan Minute(this int minutes)
        {
            return new TimeSpan(0, 0, minutes, 0);
        }

        public static TimeSpan Second(this int seconds)
        {
            return new TimeSpan(0, 0, 0, seconds);
        }

        public static TimeSpan Seconds(this int seconds)
        {
            return new TimeSpan(0, 0, 0, seconds);
        }

        public static TimeSpan Millisecond(this int milliseconds)
        {
            return new TimeSpan(0, 0, 0, 0, milliseconds);
        }

        public static TimeSpan Milliseconds(this int milliseconds)
        {
            return new TimeSpan(0, 0, 0, 0, milliseconds);
        }

        public static DateTime Ago(this TimeSpan value)
        {
            return DateTime.UtcNow.Add(value.Negate());
        }

        public static DateTime FromNow(this TimeSpan value)
        {
            return new DateTime((DateTime.Now + value).Ticks);
            //return new DateTime((DateTime.UtcNow + value).Ticks);
        }

        public static DateTime FromUnixTime(this long seconds)
        {
            var time = new DateTime(1970, 1, 1);
            time = time.AddSeconds(seconds);

            return time.ToLocalTime();
        }

        public static long ToUnixTime(this DateTime dateTime)
        {
            var timeSpan = (dateTime - new DateTime(1970, 1, 1));
            var timestamp = (long)timeSpan.TotalSeconds;

            return timestamp;
        }
    }
}