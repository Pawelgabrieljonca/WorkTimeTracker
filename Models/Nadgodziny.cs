using System;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Reprezentuje dzień z nadgodzinami (>8h), gdzie nadgodziny są płatne 150%.
    /// </summary>
    public class Nadgodziny : WpisCzasu
    {
        public const decimal STANDARDOWE_GODZINY = 8m;
        public const decimal MNOZNIK_NADGODZIN = 1.5m;

        public decimal GodzinyStandardowe => Math.Min(STANDARDOWE_GODZINY, LiczbaGodzin);
        public decimal GodzinyDodatkowe => Math.Max(0, LiczbaGodzin - STANDARDOWE_GODZINY);

        public Nadgodziny(DateTime data, decimal liczbaGodzin) : base(data, liczbaGodzin)
        {
            if (liczbaGodzin <= STANDARDOWE_GODZINY)
                throw new ArgumentException($"Nadgodziny wymagają więcej niż {STANDARDOWE_GODZINY} godzin pracy.");
        }

        public override decimal ObliczWynagrodzenie(decimal stawkaGodzinowa)
        {
            var podstawowe = stawkaGodzinowa * GodzinyStandardowe;
            var extra = stawkaGodzinowa * GodzinyDodatkowe * MNOZNIK_NADGODZIN;
            return Math.Round(podstawowe + extra, 2);
        }

        public override string ToString() =>
            $"{Data:d} - NADGODZINY (Standard: {GodzinyStandardowe}h, Extra: {GodzinyDodatkowe}h)";
    }
}