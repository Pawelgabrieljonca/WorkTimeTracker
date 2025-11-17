namespace WorkTimeTracker.Utils
{
    public static class AppSettings
    {
        // Set to true to use EF Core/SQLite data service; false to use file-based JSON DataService
        public static bool UseEfDataService { get; set; } = true;

        // Optional: override via environment variable WORKTIMETRACKER_USE_EF (true/false)
        static AppSettings()
        {
            var env = System.Environment.GetEnvironmentVariable("WORKTIMETRACKER_USE_EF");
            if (!string.IsNullOrEmpty(env) && bool.TryParse(env, out var val))
                UseEfDataService = val;
        }
    }
}