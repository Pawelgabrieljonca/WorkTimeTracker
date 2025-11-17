using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WorkTimeTracker.Models;
using WorkTimeTracker.Utils;

namespace WorkTimeTracker.Services
{
    public class DataService : WorkTimeTracker.Interfaces.IDataService
    {
        // In-memory collections recommended by the user
        // Lista pracowników
        private List<Pracownik> _pracownicyCache = new();
        // Słownik: klucz = Id pracownika, wartość = lista rejestrów czasu
        private Dictionary<int, List<RejestrCzasu>> _rejestryCache = new();

        private readonly string _folderSciezka;
        private readonly JsonSerializerSettings _jsonSettings;

        public DataService(string folderSciezka = null)
        {
            _folderSciezka = folderSciezka ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WorkTimeTracker"
            );

            // Utworzenie folderu jeśli nie istnieje
            Directory.CreateDirectory(_folderSciezka);

            // Konfiguracja serializacji JSON
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new List<JsonConverter> { new WpisCzasuJsonConverter() }
            };
        }

        private string GetSciezkaPliku(string nazwaPliku)
            => Path.Combine(_folderSciezka, nazwaPliku);

        // Zapis danych
        public async Task ZapiszPracownikow(IEnumerable<Pracownik> pracownicy, string nazwaPliku = "pracownicy.json")
        {
            var json = JsonConvert.SerializeObject(pracownicy, _jsonSettings);
            await File.WriteAllTextAsync(GetSciezkaPliku(nazwaPliku), json);
            // Zaktualizuj cache
            _pracownicyCache = pracownicy?.ToList() ?? new List<Pracownik>();
        }

        // Aktualizacja pracownika
        public async Task AktualizujPracownika(Pracownik pracownik)
        {
            var index = _pracownicyCache.FindIndex(p => p.Id == pracownik.Id);
            if (index == -1)
                throw new KeyNotFoundException($"Nie znaleziono pracownika o ID {pracownik.Id}");

            _pracownicyCache[index] = pracownik;
            await ZapiszPracownikow(_pracownicyCache);
        }

        // Usuwanie pracownika
        public async Task UsunPracownika(int pracownikId)
        {
            var pracownik = _pracownicyCache.FirstOrDefault(p => p.Id == pracownikId);
            if (pracownik == null)
                throw new KeyNotFoundException($"Nie znaleziono pracownika o ID {pracownikId}");

            _pracownicyCache.Remove(pracownik);
            if (_rejestryCache.ContainsKey(pracownikId))
                _rejestryCache.Remove(pracownikId);

            await ZapiszPracownikow(_pracownicyCache);
            await ZapiszRejestry(_rejestryCache);
        }

        public async Task ZapiszWpisyCzasu(IEnumerable<WpisCzasu> wpisy, string nazwaPliku = "wpisy.json")
        {
            var json = JsonConvert.SerializeObject(wpisy, _jsonSettings);
            await File.WriteAllTextAsync(GetSciezkaPliku(nazwaPliku), json);
        }

        // Nowe metody do pracy z RejestrCzasu jako Dictionary<int, List<RejestrCzasu>>
        public async Task ZapiszRejestry(Dictionary<int, List<RejestrCzasu>> rejestry, string nazwaPliku = "rejestry.json")
        {
            var json = JsonConvert.SerializeObject(rejestry, _jsonSettings);
            await File.WriteAllTextAsync(GetSciezkaPliku(nazwaPliku), json);
            _rejestryCache = rejestry ?? new Dictionary<int, List<RejestrCzasu>>();
        }

        public async Task<Dictionary<int, List<RejestrCzasu>>> WczytajRejestry(string nazwaPliku = "rejestry.json")
        {
            var sciezka = GetSciezkaPliku(nazwaPliku);
            if (!File.Exists(sciezka))
            {
                _rejestryCache = new Dictionary<int, List<RejestrCzasu>>();
                return _rejestryCache;
            }

            var json = await File.ReadAllTextAsync(sciezka);
            var dict = JsonConvert.DeserializeObject<Dictionary<int, List<RejestrCzasu>>>(json, _jsonSettings);
            _rejestryCache = dict ?? new Dictionary<int, List<RejestrCzasu>>();
            return _rejestryCache;
        }

        /// <summary>
        /// Dodaje rejestr czasu dla konkretnego pracownika (w pamięci i zapisuje plik).
        /// Waliduje liczbę godzin i używa własnego wyjątku przy nieprawidłowych danych.
        /// </summary>
        public async Task DodajRejestrDlaPracownika(int pracownikId, RejestrCzasu rejestr)
        {
            try
            {
                if (rejestr == null) throw new ArgumentNullException(nameof(rejestr));

                // Walidacja godziny: muszą być w przedziale [0,24]
                if (rejestr.LiczbaGodzin < 0 || rejestr.LiczbaGodzin > 24)
                    throw new NieprawidloweDaneException($"Nieprawidłowa liczba godzin: {rejestr.LiczbaGodzin}. Zakres dopuszczalny: 0-24.");

                if (!_rejestryCache.ContainsKey(pracownikId))
                    _rejestryCache[pracownikId] = new List<RejestrCzasu>();

                _rejestryCache[pracownikId].Add(rejestr);
                await ZapiszRejestry(_rejestryCache);
            }
            catch (NieprawidloweDaneException)
            {
                // Obsłużenie sytuacji nieprawidłowych danych - logujemy i przepuszczamy dalej
                // W realnej aplikacji powinniśmy użyć loggera; tutaj wypisujemy na stderr dla prostoty
                Console.Error.WriteLine($"Błąd dodawania rejestru dla pracownika {pracownikId}: nieprawidłowe dane.");
                throw;
            }
            catch (Exception ex)
            {
                // Inne nieoczekiwane błędy - zapisz i opakuj
                Console.Error.WriteLine($"Nieoczekiwany błąd podczas dodawania rejestru: {ex.Message}");
                throw;
            }
        }

        public List<RejestrCzasu> PobierzRejestryDlaPracownika(int pracownikId)
        {
            return _rejestryCache.TryGetValue(pracownikId, out var list) ? list : new List<RejestrCzasu>();
        }

        // Odczyt danych
        public async Task<List<Pracownik>> WczytajPracownikow(string nazwaPliku = "pracownicy.json")
        {
            var sciezka = GetSciezkaPliku(nazwaPliku);
            if (!File.Exists(sciezka))
                return new List<Pracownik>();

            var json = await File.ReadAllTextAsync(sciezka);
            return JsonConvert.DeserializeObject<List<Pracownik>>(json, _jsonSettings) ?? new List<Pracownik>();
        }

        public async Task<List<WpisCzasu>> WczytajWpisyCzasu(string nazwaPliku = "wpisy.json")
        {
            var sciezka = GetSciezkaPliku(nazwaPliku);
            if (!File.Exists(sciezka))
                return new List<WpisCzasu>();

            var json = await File.ReadAllTextAsync(sciezka);
            return JsonConvert.DeserializeObject<List<WpisCzasu>>(json, _jsonSettings) ?? new List<WpisCzasu>();
        }

        // Pomocnicze metody do zapisu/odczytu pojedynczych elementów
        public async Task ZapiszPracownika(Pracownik pracownik, int id)
        {
            var pracownicy = await WczytajPracownikow();
            var istniejacy = pracownicy.FirstOrDefault(p => p.Id == id);
            if (istniejacy != null)
                pracownicy.Remove(istniejacy);

            pracownicy.Add(pracownik);
            await ZapiszPracownikow(pracownicy);
        }

        public async Task DodajWpisCzasu(WpisCzasu wpis)
        {
            var wpisy = await WczytajWpisyCzasu();
            wpisy.Add(wpis);
            await ZapiszWpisyCzasu(wpisy);
        }

        // Metody do pobierania wpisów dla konkretnego miesiąca
        public async Task<List<WpisCzasu>> WczytajWpisyMiesiaca(int rok, int miesiac)
        {
            var wpisy = await WczytajWpisyCzasu();
            return wpisy.Where(w => w.Data.Year == rok && w.Data.Month == miesiac).ToList();
        }

        // Metoda do czyszczenia danych (przydatna przy testach)
        public void WyczyscDane()
        {
            var pliki = new[] { "pracownicy.json", "wpisy.json", "rejestry.json" };
            foreach (var plik in pliki)
            {
                var sciezka = GetSciezkaPliku(plik);
                if (File.Exists(sciezka))
                    File.Delete(sciezka);
            }
            // Wyczyść także cache w pamięci
            _pracownicyCache.Clear();
            _rejestryCache.Clear();
        }
    }
} 