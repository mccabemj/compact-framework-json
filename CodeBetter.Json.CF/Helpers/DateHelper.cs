
namespace CodeBetter.Json.Helpers
{
    using System;

    public static class DateHelper
    {
        private static readonly DateTime _epoc = new DateTime(1970, 1, 1);

        public static int ToUnixTime(DateTime time)
        {
            return (int)time.Subtract(_epoc).TotalSeconds;
        }

        public static DateTime FromUnixTime(int seconds)
        {
            return _epoc.AddSeconds(seconds);
        }
    }
}
