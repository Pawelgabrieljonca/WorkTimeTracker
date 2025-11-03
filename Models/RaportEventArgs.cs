using System;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Argumenty zdarzenia informujące o wygenerowaniu raportu.
    /// </summary>
    public class RaportEventArgs : EventArgs
    {
        public string Tytul { get; }
        public DateTime DataGenerowania { get; }
        public string? SciezkaPliku { get; }

        public RaportEventArgs(string tytul, DateTime dataGenerowania, string? sciezkaPliku = null)
        {
            Tytul = tytul;
            DataGenerowania = dataGenerowania;
            SciezkaPliku = sciezkaPliku;
        }
    }
}
