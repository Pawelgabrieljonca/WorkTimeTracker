
using System;
using System.Collections.Generic;
using System.Linq;

using WorkTimeTracker.Utils;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Reprezentuje pracownika i pozwala obliczać miesięczne wynagrodzenie na podstawie przepracowanych godzin.
    /// Zastosowano hermetyzację poprzez prywatne pola i walidujące settery.
    /// </summary>
    [OpisKlasy("Klasa reprezentująca pracownika z funkcjami obliczania wynagrodzenia. " +
               "Zawiera podstawowe dane osobowe oraz stawkę godzinową.",
               "System", "2025-10-28")]
    public class Pracownik
    {
        private int _id;
        private string _imie = string.Empty;
        private string _nazwisko = string.Empty;
        private string _stanowisko = string.Empty;
        private decimal _stawkaGodzinowa;

        public int Id
        {
            get => _id;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Id), "Id nie może być ujemne.");
                _id = value;
            }
        }

        // Uwaga: używam 'Imie' zamiast 'Imię' (ASCII) dla lepszej kompatybilności z narzędziami/edytorami.
        public string Imie
        {
            get => _imie;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Imię nie może być puste.", nameof(Imie));
                _imie = value.Trim();
            }
        }

        public string Nazwisko
        {
            get => _nazwisko;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Nazwisko nie może być puste.", nameof(Nazwisko));
                _nazwisko = value.Trim();
            }
        }

        public string Stanowisko
        {
            get => _stanowisko;
            set => _stanowisko = value?.Trim() ?? string.Empty;
        }

        public decimal StawkaGodzinowa
        {
            get => _stawkaGodzinowa;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(StawkaGodzinowa), "Stawka godzinowa musi być nieujemna.");
                _stawkaGodzinowa = value;
            }
        }

        public Pracownik(int id, string imie, string nazwisko, string stanowisko, decimal stawkaGodzinowa)
        {
            Id = id;
            Imie = imie;
            Nazwisko = nazwisko;
            Stanowisko = stanowisko;
            StawkaGodzinowa = stawkaGodzinowa;
        }

        public Pracownik() { }

        /// <summary>
        /// Oblicza miesięczne wynagrodzenie na podstawie wpisów czasu pracy.
        /// </summary>
        public decimal ObliczWynagrodzenieMiesieczne(IEnumerable<WpisCzasu> wpisyCzasu)
        {
            if (wpisyCzasu == null) throw new ArgumentNullException(nameof(wpisyCzasu));
            return Math.Round(wpisyCzasu.Sum(wpis => wpis.ObliczWynagrodzenie(StawkaGodzinowa)), 2);
        }
    }
}
