using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WorkTimeTracker.Models;

namespace WorkTimeTracker.Services
{
    public class RaportService
    {
        private readonly WorkTimeTracker.Interfaces.IDataService _dataService;

        public RaportService(WorkTimeTracker.Interfaces.IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<string> GenerujRaportMiesieczny(int pracownikId, int rok, int miesiac)
        {
            var pracownicy = await _dataService.WczytajPracownikow();
            var pracownik = pracownicy.Find(p => p.Id == pracownikId);
            if (pracownik == null)
                throw new KeyNotFoundException($"Nie znaleziono pracownika o ID {pracownikId}");

            var rejestry = await _dataService.WczytajRejestry();
            var entries = rejestry.TryGetValue(pracownikId, out var list) ? list : new List<RejestrCzasu>();

            // Filtruj wpisy dla wybranego miesiąca
            var monthEntries = entries.FindAll(r => r.Data.Year == rok && r.Data.Month == miesiac);

            // Konwertuj na odpowiednie typy WpisCzasu
            var wpisy = new List<WpisCzasu>();
            foreach (var r in monthEntries)
            {
                if (r.CzyUrlop)
                    wpisy.Add(new Urlop(r.Data));
                else if (r.CzyNadgodziny || r.LiczbaGodzin > 8)
                    wpisy.Add(new Nadgodziny(r.Data, r.LiczbaGodzin));
                else
                    wpisy.Add(new ZwyklyDzien(r.Data, r.LiczbaGodzin));
            }

            var raport = new RaportMiesieczny(pracownik, wpisy, new DateTime(rok, miesiac, 1));
            return raport.GenerujRaport();
        }

        public async Task<string> GenerujRaportPracownika(int pracownikId)
        {
            var pracownicy = await _dataService.WczytajPracownikow();
            var pracownik = pracownicy.Find(p => p.Id == pracownikId);
            if (pracownik == null)
                throw new KeyNotFoundException($"Nie znaleziono pracownika o ID {pracownikId}");

            var rejestry = await _dataService.WczytajRejestry();
            var entries = rejestry.TryGetValue(pracownikId, out var list) ? list : new List<RejestrCzasu>();

            // Konwertuj wszystkie wpisy
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

            // Determine date range from entries
            var dataOd = wpisy.Any() ? wpisy.Min(w => w.Data) : DateTime.Today;
            var dataDo = wpisy.Any() ? wpisy.Max(w => w.Data) : DateTime.Today;

            var raport = new RaportPracownika(pracownik, wpisy, dataOd, dataDo);
            return raport.GenerujRaport();
        }

        public async Task ZapiszRaport(string raportTekst, int pracownikId)
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WorkTimeTracker",
                "Raporty"
            );

            Directory.CreateDirectory(folder);
            var sciezka = Path.Combine(folder, $"raport_{pracownikId}_{DateTime.Now:yyyyMMddHHmmss}.txt");
            await File.WriteAllTextAsync(sciezka, raportTekst);
        }
    }
}