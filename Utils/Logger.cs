using System;
using System.IO;
using System.Text;
using WorkTimeTracker.Models;

namespace WorkTimeTracker.Utils
{
    /// <summary>
    /// Prosty logger zapisujący komunikaty do pliku tekstowego. Subskrybuje zdarzenie Raport.RaportWygenerowano.
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkTimeTracker", "Logs");
        private static string _logFile = Path.Combine(_logFolder, "raporty.log");
        private static bool _initialized = false;

        /// <summary>
        /// Inicjalizuje loggera i subskrybuje zdarzenie raportowe.
        /// </summary>
        public static void Initialize(string? folder = null)
        {
            if (_initialized) return;
            if (!string.IsNullOrEmpty(folder))
            {
                _logFolder = folder;
                _logFile = Path.Combine(_logFolder, "raporty.log");
            }

            Directory.CreateDirectory(_logFolder);

            // Subskrybuj zdarzenie
            Raport.RaportWygenerowano += HandleRaportWygenerowano;
            _initialized = true;
        }

        private static void HandleRaportWygenerowano(object? sender, RaportEventArgs e)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Raport wygenerowany: {e.Tytul}");
                sb.AppendLine($"    Data generowania: {e.DataGenerowania:yyyy-MM-dd HH:mm:ss}");
                if (!string.IsNullOrEmpty(e.SciezkaPliku))
                    sb.AppendLine($"    Zapisano do: {e.SciezkaPliku}");

                var tekst = sb.ToString();

                lock (_lock)
                {
                    File.AppendAllText(_logFile, tekst + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Nie przerywamy działania aplikacji z powodu błędów logowania
            }
        }
    }
}
