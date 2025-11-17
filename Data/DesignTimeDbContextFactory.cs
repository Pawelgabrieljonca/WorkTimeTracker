using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorkTimeTracker.Data
{
    /// <summary>
    /// Design-time factory for WorkTimeContext so dotnet-ef tools can create the context when adding migrations.
    /// This uses the same %LocalAppData%\WorkTimeTracker\worktimedata.db path as the application.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WorkTimeContext>
    {
        public WorkTimeContext CreateDbContext(string[] args)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkTimeTracker");
            Directory.CreateDirectory(folder);
            var dbPath = Path.Combine(folder, "worktimedata.db");
            var conn = $"Data Source={dbPath}";

            var options = new DbContextOptionsBuilder<WorkTimeContext>()
                .UseSqlite(conn)
                .Options;

            return new WorkTimeContext(options);
        }
    }
}
