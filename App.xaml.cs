using System.Windows;
using WorkTimeTracker.Utils;

namespace WorkTimeTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Apply EF Core migrations (creates SQLite DB if needed)
            WorkTimeTracker.Data.DbInitializer.ApplyMigrations();

            // Inicjalizuj logger, aby subskrybował zdarzenia raportów
            Logger.Initialize();
            // Show login window first
            var login = new Views.LoginWindow();
            login.Show();
        }
    }
}