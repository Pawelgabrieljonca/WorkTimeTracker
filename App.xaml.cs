using System.Windows;
using WorkTimeTracker.Utils;

namespace WorkTimeTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Inicjalizuj logger, aby subskrybował zdarzenia raportów
            Logger.Initialize();
        }
    }
}