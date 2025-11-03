using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkTimeTracker.Models;

namespace WorkTimeTracker.Services
{
    public class StatystykiService
    {
        private readonly DataService _dataService;
        private Dictionary<int, StatystykiPracownika> _statystykiCache;

        public StatystykiService(DataService dataService)
        {
            _dataService = dataService;
            _statystykiCache = new Dictionary<int, StatystykiPracownika>();
        }

        public async Task<StatystykiPracownika> PobierzStatystykiPracownika(int pracownikId)
        {
            var pracownicy = await _dataService.WczytajPracownikow();
            var pracownik = pracownicy.Find(p => p.Id == pracownikId);
            if (pracownik == null)
                throw new KeyNotFoundException($"Nie znaleziono pracownika o ID {pracownikId}");

            var rejestry = await _dataService.WczytajRejestry();
            var wpisy = await KonwertujRejestryCzasu(pracownikId);

            var statystyki = new StatystykiPracownika(pracownik);
            statystyki.AktualizujStatystyki(wpisy);
            _statystykiCache[pracownikId] = statystyki;

            return statystyki;
        }

        public async Task<List<StatystykiPracownika>> PobierzStatystykiWszystkichPracownikow()
        {
            var pracownicy = await _dataService.WczytajPracownikow();
            var statystyki = new List<StatystykiPracownika>();

            foreach (var pracownik in pracownicy)
            {
                var stat = await PobierzStatystykiPracownika(pracownik.Id);
                statystyki.Add(stat);
            }

            return statystyki;
        }

        public async Task<Dictionary<int, decimal>> PobierzStatystykiMiesieczne(int pracownikId, int rok)
        {
            var rejestry = await _dataService.WczytajRejestry();
            var wpisy = await KonwertujRejestryCzasu(pracownikId);

            // Filtruj wpisy dla danego roku
            wpisy = wpisy.Where(w => w.Data.Year == rok).ToList();

            var statystykiMiesieczne = new Dictionary<int, decimal>();
            for (int miesiac = 1; miesiac <= 12; miesiac++)
            {
                var wpisyMiesiaca = wpisy.Where(w => w.Data.Month == miesiac).ToList();
                decimal godziny = 0;
                foreach (var wpis in wpisyMiesiaca)
                {
                    if (wpis is not Urlop)
                        godziny += (wpis as dynamic).LiczbaGodzin;
                }
                statystykiMiesieczne[miesiac] = godziny;
            }

            return statystykiMiesieczne;
        }

        private async Task<List<WpisCzasu>> KonwertujRejestryCzasu(int pracownikId)
        {
            var rejestry = await _dataService.WczytajRejestry();
            var entries = rejestry.TryGetValue(pracownikId, out var list) ? list : new List<RejestrCzasu>();

            var wpisy = new List<WpisCzasu>();
            foreach (var r in entries)
            {
                if (r.CzyUrlop)
                    wpisy.Add(new Urlop(r.Data));
                else if (r.CzyNadgodziny || r.LiczbaGodzin > 8)
                    wpisy.Add(new Nadgodziny(r.Data, r.LiczbaGodzin));
                else
                    wpisy.Add(new ZwyklyDzien(r.Data, r.LiczbaGodzin));
            }

            return wpisy;
        }
    }
}