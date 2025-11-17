using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace WorkTimeTracker.Data
{
    public static class DbInitializer
    {
        public static void ApplyMigrations()
        {
            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkTimeTracker");
                Directory.CreateDirectory(folder);
                var dbPath = Path.Combine(folder, "worktimedata.db");
                var connectionString = $"Data Source={dbPath}";

                var options = new DbContextOptionsBuilder<WorkTimeContext>()
                    .UseSqlite(connectionString)
                    .Options;

                using var ctx = new WorkTimeContext(options);
                ctx.Database.Migrate();
            }
            catch (Exception ex)
            {
                // In a real app log the error; avoid crashing the UI - migrations are best-effort here
                Console.Error.WriteLine($"Błąd podczas stosowania migracji: {ex.Message}");
            }
        }
    }
}
