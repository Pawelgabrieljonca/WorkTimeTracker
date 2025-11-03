using System;
using System.Collections.Generic;
using System.Linq;
using WorkTimeTracker.Utils;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Raport zawierający statystyki i podsumowanie pracy pracownika.
    /// </summary>
    public class RaportPracownika : Raport
    {
        private readonly Pracownik _pracownik;
        private readonly IEnumerable<WpisCzasu> _wpisy;
        private readonly DateTime _dataOd;
        private readonly DateTime _dataDo;

        public RaportPracownika(Pracownik pracownik, IEnumerable<WpisCzasu> wpisy,
            DateTime dataOd, DateTime dataDo)
            : base($"Raport pracownika - {dataOd:d} do {dataDo:d}")
        {
            _pracownik = pracownik ?? throw new ArgumentNullException(nameof(pracownik));
            _wpisy = wpisy ?? throw new ArgumentNullException(nameof(wpisy));
            _dataOd = dataOd.Date;
            _dataDo = dataDo.Date;

            if (_dataOd > _dataDo)
                throw new ArgumentException("Data początkowa nie może być późniejsza niż końcowa.");
        }

        public override string GenerujRaport()
        {
            WyczyscBuilder();
            DodajNaglowek();

            // Informacje o pracowniku
            _builder.AppendLine("DANE PRACOWNIKA");
            _builder.AppendLine($"ID: {_pracownik.Id}");
            _builder.AppendLine($"Imię i nazwisko: {_pracownik.Imie} {_pracownik.Nazwisko}");
            _builder.AppendLine($"Stanowisko: {_pracownik.Stanowisko}");
            _builder.AppendLine($"Stawka godzinowa: {_pracownik.StawkaGodzinowa:C2}");
            _builder.AppendLine();

            // Filtrowanie wpisów dla zakresu dat
            var wpisyOkresu = _wpisy.Where(w => w.Data >= _dataOd && w.Data <= _dataDo);

            // Statystyki szczegółowe
            var przepracowaneDni = wpisyOkresu.Select(w => w.Data).Distinct().Count();
            var sumaGodzin = wpisyOkresu.Sum(w => w.LiczbaGodzin);

            var nadgodziny = wpisyOkresu.OfType<Nadgodziny>();
            var sumaNadgodzin = nadgodziny.Sum(n => n.GodzinyDodatkowe);
            var dniZNadgodzinami = nadgodziny.Count();

            var urlopy = wpisyOkresu.OfType<Urlop>();
            var dniUrlopu = urlopy.Count();

            _builder.AppendLine("STATYSTYKI OGÓLNE");
            _builder.AppendLine($"Okres: {_dataOd:d} - {_dataDo:d} ({(_dataDo - _dataOd).Days + 1} dni)");
            _builder.AppendLine($"Przepracowane dni: {przepracowaneDni}");
            _builder.AppendLine($"Suma godzin: {sumaGodzin}h");
            _builder.AppendLine($"Średnia dzienna: {(przepracowaneDni > 0 ? sumaGodzin / przepracowaneDni : 0):F1}h");
            _builder.AppendLine();

            _builder.AppendLine("SZCZEGÓŁY");
            _builder.AppendLine($"Dni z nadgodzinami: {dniZNadgodzinami}");
            _builder.AppendLine($"Suma nadgodzin: {sumaNadgodzin}h");
            _builder.AppendLine($"Wykorzystane dni urlopu: {dniUrlopu}");
            _builder.AppendLine();

            // Wynagrodzenie za okres
            var wynagrodzenie = _pracownik.ObliczWynagrodzenieMiesieczne(wpisyOkresu);
            _builder.AppendLine($"WYNAGRODZENIE ZA OKRES: {wynagrodzenie:C2}");
            if (przepracowaneDni > 0)
            {
                _builder.AppendLine($"Średnie wynagrodzenie dzienne: {wynagrodzenie / przepracowaneDni:C2}");
            }

            DodajStopke();
            var raport = _builder.ToString();
            // Powiadom słuchaczy, że raport został wygenerowany
            OnRaportWygenerowano();
            return raport;
        }
    }
}