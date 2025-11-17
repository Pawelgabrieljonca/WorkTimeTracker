using System;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Reprezentuje dzień pracy pracownika, zawierając informacje o przepracowanych godzinach,
    /// urlopie i nadgodzinach.
    /// </summary>
    public class RejestrCzasu
    {
        // Primary key for persistence
        public int Id { get; set; }
        // Foreign key to Pracownik when stored in database
        public int PracownikId { get; set; }

        private DateTime _data;
        private decimal _liczbaGodzin;
        private bool _czyUrlop;
        private bool _czyNadgodziny;

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

        public bool CzyUrlop
        {
            get => _czyUrlop;
            set
            {
                if (value && _czyNadgodziny)
                    throw new InvalidOperationException("Dzień nie może być jednocześnie urlopem i zawierać nadgodzin.");
                _czyUrlop = value;
            }
        }

        public bool CzyNadgodziny
        {
            get => _czyNadgodziny;
            set
            {
                if (value && _czyUrlop)
                    throw new InvalidOperationException("Dzień nie może być jednocześnie urlopem i zawierać nadgodzin.");
                if (value && _liczbaGodzin <= 8)
                    throw new InvalidOperationException("Nadgodziny można zarejestrować tylko gdy przepracowano więcej niż 8 godzin.");
                _czyNadgodziny = value;
            }
        }

        /// <summary>
        /// Tworzy nowy wpis rejestru czasu pracy.
        /// </summary>
        /// <param name="data">Data dnia pracy</param>
        /// <param name="liczbaGodzin">Liczba przepracowanych godzin (0-24)</param>
        /// <param name="czyUrlop">Czy dzień jest urlopem</param>
        /// <param name="czyNadgodziny">Czy zarejestrowano nadgodziny (tylko gdy > 8h)</param>
        public RejestrCzasu(DateTime data, decimal liczbaGodzin, bool czyUrlop = false, bool czyNadgodziny = false)
        {
            // Ustawiamy właściwości w kolejności zapewniającej poprawną walidację
            Data = data;
            LiczbaGodzin = liczbaGodzin;

            // Najpierw urlop (bo nadgodziny sprawdzają urlop)
            CzyUrlop = czyUrlop;
            CzyNadgodziny = czyNadgodziny;
        }

        /// <summary>
        /// Tworzy nowy wpis rejestru czasu na bieżący dzień.
        /// </summary>
        public RejestrCzasu() : this(DateTime.Today, 0) { }

        public override string ToString()
        {
            var status = CzyUrlop ? "URLOP" :
                        CzyNadgodziny ? $"NADGODZINY ({LiczbaGodzin}h)" :
                        $"PRACA ({LiczbaGodzin}h)";

            return $"{Data:d} - {status}";
        }
    }
}