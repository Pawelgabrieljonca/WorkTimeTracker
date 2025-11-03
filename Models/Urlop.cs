using System;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Reprezentuje dzień urlopowy, za który przysługuje 80% wynagrodzenia.
    /// </summary>
    public class Urlop : WpisCzasu
    {
        public const decimal WSPOLCZYNNIK_URLOPU = 0.8m;

        public Urlop(DateTime data) : base(data, 8m) { } // Urlop zawsze liczymy jako 8h

        public override decimal ObliczWynagrodzenie(decimal stawkaGodzinowa)
        {
            return Math.Round(stawkaGodzinowa * LiczbaGodzin * WSPOLCZYNNIK_URLOPU, 2);
        }

        public override string ToString() => $"{Data:d} - URLOP";
    }
}