using System;
using System.Collections.Generic;
using System.Linq;
using WorkTimeTracker.Utils;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Raport podsumowujący przepracowany czas w danym miesiącu.
    /// </summary>
    public class RaportMiesieczny : Raport
    {
        private readonly IEnumerable<WpisCzasu> _wpisy;
        private readonly DateTime _miesiac;
        private readonly Pracownik _pracownik;

        public RaportMiesieczny(Pracownik pracownik, IEnumerable<WpisCzasu> wpisy, DateTime miesiac)
            : base($"Raport miesięczny - {miesiac:MMMM yyyy}")
        {
            _pracownik = pracownik ?? throw new ArgumentNullException(nameof(pracownik));
            _wpisy = wpisy ?? throw new ArgumentNullException(nameof(wpisy));
            _miesiac = new DateTime(miesiac.Year, miesiac.Month, 1);
        }

        public override string GenerujRaport()
        {
            WyczyscBuilder();
            DodajNaglowek();

            // Dane pracownika
            _builder.AppendLine($"Pracownik: {_pracownik.Imie} {_pracownik.Nazwisko}");
            _builder.AppendLine($"Stanowisko: {_pracownik.Stanowisko}");
            _builder.AppendLine();

            // Filtrowanie wpisów dla danego miesiąca
            var wpisyMiesiaca = _wpisy.Where(w => w.Data.Year == _miesiac.Year && w.Data.Month == _miesiac.Month);

            // Statystyki
            var dniPracy = wpisyMiesiaca.OfType<ZwyklyDzien>().Count();
            var dniUrlopu = wpisyMiesiaca.OfType<Urlop>().Count();
            var dniNadgodziny = wpisyMiesiaca.OfType<Nadgodziny>().Count();

            var sumaNormalne = wpisyMiesiaca.OfType<ZwyklyDzien>().Sum(w => w.LiczbaGodzin);
            var sumaNadgodziny = wpisyMiesiaca.OfType<Nadgodziny>().Sum(w => w.GodzinyDodatkowe);
            var sumaUrlop = wpisyMiesiaca.OfType<Urlop>().Sum(w => w.LiczbaGodzin);

            _builder.AppendLine("PODSUMOWANIE MIESIĄCA");
            _builder.AppendLine($"Dni przepracowane: {dniPracy}");
            _builder.AppendLine($"Dni urlopu: {dniUrlopu}");
            _builder.AppendLine($"Dni z nadgodzinami: {dniNadgodziny}");
            _builder.AppendLine();
            _builder.AppendLine("GODZINY");
            _builder.AppendLine($"Normalne: {sumaNormalne}h");
            _builder.AppendLine($"Nadgodziny: {sumaNadgodziny}h");
            _builder.AppendLine($"Urlop: {sumaUrlop}h");
            _builder.AppendLine();

            // Wynagrodzenie
            var wynagrodzenie = _pracownik.ObliczWynagrodzenieMiesieczne(wpisyMiesiaca);
            _builder.AppendLine($"WYNAGRODZENIE CAŁKOWITE: {wynagrodzenie:C2}");

            DodajStopke();
            var raport = _builder.ToString();
            // Powiadom słuchaczy, że raport został wygenerowany
            OnRaportWygenerowano();
            return raport;
        }
    }
}