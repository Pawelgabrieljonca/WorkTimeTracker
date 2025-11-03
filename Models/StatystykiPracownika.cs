using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkTimeTracker.Models
{
    public class StatystykiPracownika
    {
        public int PracownikId { get; }
        public string ImieNazwisko { get; }
        public decimal CalkowiteGodziny { get; private set; }
        public decimal GodzinyNadliczbowe { get; private set; }
        public int DniUrlopowe { get; private set; }
        public decimal SredniaGodzinDziennie { get; private set; }
        public decimal WynagrodzenieCalkowite { get; private set; }
        public Dictionary<int, decimal> GodzinyMiesieczne { get; }

        public StatystykiPracownika(Pracownik pracownik)
        {
            PracownikId = pracownik.Id;
            ImieNazwisko = $"{pracownik.Imie} {pracownik.Nazwisko}";
            GodzinyMiesieczne = new Dictionary<int, decimal>();
        }

        public void AktualizujStatystyki(IEnumerable<WpisCzasu> wpisy)
        {
            if (!wpisy.Any())
                return;

            CalkowiteGodziny = 0;
            GodzinyNadliczbowe = 0;
            DniUrlopowe = 0;
            GodzinyMiesieczne.Clear();

            var dniRobocze = wpisy.Count();

            foreach (var wpis in wpisy)
            {
                switch (wpis)
                {
                    case Urlop:
                        DniUrlopowe++;
                        break;
                    case Nadgodziny nadgodziny:
                        CalkowiteGodziny += nadgodziny.LiczbaGodzin;
                        GodzinyNadliczbowe += Math.Max(0, nadgodziny.LiczbaGodzin - 8);
                        break;
                    case ZwyklyDzien zwykly:
                        CalkowiteGodziny += zwykly.LiczbaGodzin;
                        break;
                }

                // Agreguj godziny miesięczne
                var miesiac = wpis.Data.Month;
                if (!GodzinyMiesieczne.ContainsKey(miesiac))
                    GodzinyMiesieczne[miesiac] = 0;

                if (wpis is not Urlop)
                    GodzinyMiesieczne[miesiac] += (wpis as dynamic).LiczbaGodzin;
            }

            // Oblicz średnią dzienną (bez urlopów)
            var dniPracujace = dniRobocze - DniUrlopowe;
            SredniaGodzinDziennie = dniPracujace > 0 ? Math.Round(CalkowiteGodziny / dniPracujace, 2) : 0;
        }

        public override string ToString()
        {
            return $"Statystyki dla: {ImieNazwisko}\n" +
                   $"Całkowite godziny: {CalkowiteGodziny:F1}\n" +
                   $"Godziny nadliczbowe: {GodzinyNadliczbowe:F1}\n" +
                   $"Dni urlopowe: {DniUrlopowe}\n" +
                   $"Średnia godzin dziennie: {SredniaGodzinDziennie:F1}";
        }
    }
}