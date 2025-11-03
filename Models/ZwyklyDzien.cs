using System;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Reprezentuje zwykły dzień pracy (do 8 godzin).
    /// </summary>
    public class ZwyklyDzien : WpisCzasu
    {
        public const decimal STANDARDOWE_GODZINY = 8m;

        public ZwyklyDzien(DateTime data, decimal liczbaGodzin) : base(data, liczbaGodzin)
        {
            if (liczbaGodzin > STANDARDOWE_GODZINY)
                throw new ArgumentException($"Zwykły dzień nie może mieć więcej niż {STANDARDOWE_GODZINY} godzin.");
        }

        public override decimal ObliczWynagrodzenie(decimal stawkaGodzinowa)
        {
            return Math.Round(stawkaGodzinowa * LiczbaGodzin, 2);
        }

        public override string ToString() => $"{Data:d} - PRACA ({LiczbaGodzin}h)";
    }
}