using System;
using System.Text;

namespace WorkTimeTracker.Interfaces
{
    /// <summary>
    /// Interfejs definiujący możliwość generowania raportów.
    /// </summary>
    public interface IRaportowalny
    {
        /// <summary>
        /// Generuje raport w formie tekstowej.
        /// </summary>
        /// <returns>Tekstowa reprezentacja raportu</returns>
        string GenerujRaport();

        /// <summary>
        /// Zapisuje raport do pliku.
        /// </summary>
        /// <param name="sciezka">Ścieżka do pliku wyjściowego</param>
        void ZapiszRaport(string sciezka);
    }
}