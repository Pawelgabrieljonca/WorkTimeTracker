using System;

using WorkTimeTracker.Utils;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Abstrakcyjna klasa bazowa reprezentująca wpis czasu pracy.
    /// </summary>
    [OpisKlasy("Bazowa klasa abstrakcyjna dla wszystkich typów wpisów czasu pracy. " +
               "Definiuje wspólne właściwości jak data i liczba godzin oraz abstrakcyjną metodę obliczania wynagrodzenia.",
               "System", "2025-10-28")]
    public abstract class WpisCzasu
    {
        private DateTime _data;
        private decimal _liczbaGodzin;

        public DateTime Data
        {
            get => _data;
            set
            {
                if (value.Year < 2000)
                    throw new ArgumentException("Data nie może być wcześniejsza niż rok 2000.", nameof(Data));
                _data = value.Date; // Zerujemy część czasową
            }
        }

        public decimal LiczbaGodzin
        {
            get => _liczbaGodzin;
            set
            {
                if (value < 0 || value > 24)
                    throw new ArgumentOutOfRangeException(nameof(LiczbaGodzin),
                        "Liczba godzin musi być między 0 a 24.");
                _liczbaGodzin = value;
            }
        }

        protected WpisCzasu(DateTime data, decimal liczbaGodzin)
        {
            Data = data;
            LiczbaGodzin = liczbaGodzin;
        }

        /// <summary>
        /// Oblicza wynagrodzenie za dany wpis czasu.
        /// </summary>
        /// <param name="stawkaGodzinowa">Stawka godzinowa pracownika</param>
        /// <returns>Kwota wynagrodzenia za ten wpis</returns>
        public abstract decimal ObliczWynagrodzenie(decimal stawkaGodzinowa);

        public override string ToString() => $"{Data:d} ({LiczbaGodzin}h)";
    }
}