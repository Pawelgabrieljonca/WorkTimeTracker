using System;

namespace WorkTimeTracker.Utils
{
    public static class TimeFormatter
    {
        public static string FormatDuration(TimeSpan ts) => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }
}