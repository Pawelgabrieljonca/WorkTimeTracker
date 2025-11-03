using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WorkTimeTracker.Models;
using WorkTimeTracker.Services;
using WorkTimeTracker.Utils;

namespace WorkTimeTracker.Views
{
    public partial class MainWindow : Window
    {
        private readonly DataService _dataService = new();
        private List<Pracownik> _employees = new();
        private Dictionary<int, List<RejestrCzasu>> _rejestry = new();
        private Pracownik? _editingEmployee;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            InitializeReportPickers();
        }

        private void InitializeReportPickers()
        {
            // Dodaj nazwy miesięcy
            var months = new[]
            {
                "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
                "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
            };
            ReportMonthPicker.ItemsSource = months;
            ReportMonthPicker.SelectedIndex = DateTime.Today.Month - 1;

            // Dodaj lata (od roku poprzedniego do następnego)
            var currentYear = DateTime.Today.Year;
            var years = Enumerable.Range(currentYear - 1, 3).ToList();
            ReportYearPicker.ItemsSource = years;
            ReportYearPicker.SelectedItem = currentYear;
        }

        private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            EntryDatePicker.SelectedDate = DateTime.Today;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            _employees = await _dataService.WczytajPracownikow();
            _rejestry = await _dataService.WczytajRejestry();
            RefreshEmployeeList();
            UpdateStats();
        }

        private void RefreshEmployeeList()
        {
            var display = _employees.Select(p => new EmployeeDisplay(p, GetTotalHoursFor(p.Id))).ToList();
            EmployeesList.ItemsSource = display;
        }

        private decimal GetTotalHoursFor(int pracownikId)
        {
            if (_rejestry != null && _rejestry.TryGetValue(pracownikId, out var list))
                return list.Sum(r => r.LiczbaGodzin);
            return 0m;
        }

        private void UpdateStats()
        {
            var totalEmployees = _employees.Count;
            var totalEntries = _rejestry.Values.Sum(l => l.Count);
            var totalHours = _rejestry.Values.SelectMany(l => l).Sum(r => r.LiczbaGodzin);
            StatsBox.Text = $"Pracownicy: {totalEmployees}\nWpisy: {totalEntries}\nSuma godzin: {totalHours}";
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text) || string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                MessageBox.Show("Podaj imię i nazwisko pracownika.", "Brak danych", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(RateBox.Text, out var rate))
            {
                MessageBox.Show("Nieprawidłowa stawka godzinowa.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var id = _employees.Any() ? _employees.Max(p => p.Id) + 1 : 1;
            var pracownik = new Pracownik(id, FirstNameBox.Text.Trim(), LastNameBox.Text.Trim(), PositionBox.Text?.Trim() ?? string.Empty, rate);
            try
            {
                await _dataService.ZapiszPracownika(pracownik, id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie udało się zapisać pracownika: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddEntryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(EmployeeIdForEntryBox.Text, out var id))
            {
                MessageBox.Show("Podaj poprawne ID pracownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(HoursBox.Text, out var hours))
            {
                MessageBox.Show("Podaj poprawną liczbę godzin.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var date = EntryDatePicker.SelectedDate ?? DateTime.Today;
            var isLeave = IsLeaveCheck.IsChecked == true;
            var isOvertime = IsOvertimeCheck.IsChecked == true;

            var rejestr = new RejestrCzasu(date, hours, isLeave, isOvertime);

            try
            {
                await _dataService.DodajRejestrDlaPracownika(id, rejestr);
                // reload
                _rejestry = await _dataService.WczytajRejestry();
                RefreshEmployeeList();
                UpdateStats();
            }
            catch (NieprawidloweDaneException ndx)
            {
                MessageBox.Show(ndx.Message, "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesList.SelectedItem is not EmployeeDisplay ed)
            {
                MessageBox.Show("Wybierz pracownika z listy (kliknij wpis).", "Brak wyboru", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var pracownik = ed.Employee;
            // reload rejestry to ensure latest
            _rejestry = await _dataService.WczytajRejestry();
            var entries = _rejestry.TryGetValue(pracownik.Id, out var list) ? list : new List<RejestrCzasu>();

            // Pobierz wybrany miesiąc i rok
            var selectedMonth = ReportMonthPicker.SelectedIndex + 1; // Indeks + 1 daje nam numer miesiąca
            var selectedYear = (int)ReportYearPicker.SelectedItem;
            var reportDate = new DateTime(selectedYear, selectedMonth, 1);

            // Filtruj wpisy tylko dla wybranego miesiąca
            var monthEntries = entries.Where(r => r.Data.Year == selectedYear && r.Data.Month == selectedMonth).ToList();

            // Convert RejestrCzasu -> WpisCzasu (ZwyklyDzien/Urlop/Nadgodziny)
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

            var raport = new RaportMiesieczny(pracownik, wpisy, reportDate);
            var tekst = raport.GenerujRaport();
            ReportBox.Text = tekst;

            // Optionally save to file under LocalAppData
            try
            {
                var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkTimeTracker");
                System.IO.Directory.CreateDirectory(folder);
                var sciezka = System.IO.Path.Combine(folder, $"raport_{pracownik.Id}_{DateTime.Now:yyyyMMddHHmmss}.txt");
                raport.ZapiszRaport(sciezka);
            }
            catch { /* ignore save errors */ }
        }

        private class EmployeeDisplay
        {
            public string Display { get; }
            public Pracownik Employee { get; }

            public EmployeeDisplay(Pracownik p, decimal totalHours)
            {
                Employee = p;
                Display = $"{p.Id} - {p.Imie} {p.Nazwisko} ({totalHours}h)";
            }
        }

        private void ClearEmployeeForm()
        {
            FirstNameBox.Text = string.Empty;
            LastNameBox.Text = string.Empty;
            PositionBox.Text = string.Empty;
            RateBox.Text = string.Empty;
            _editingEmployee = null;
            EmployeeFormTitle.Text = "Dodaj pracownika";
            AddEmployeeButton.Visibility = Visibility.Visible;
            SaveEmployeeButton.Visibility = Visibility.Collapsed;
            CancelEditButton.Visibility = Visibility.Collapsed;
        }

        private void ShowEditForm(Pracownik pracownik)
        {
            _editingEmployee = pracownik;
            FirstNameBox.Text = pracownik.Imie;
            LastNameBox.Text = pracownik.Nazwisko;
            PositionBox.Text = pracownik.Stanowisko;
            RateBox.Text = pracownik.StawkaGodzinowa.ToString();
            EmployeeFormTitle.Text = $"Edytuj pracownika (ID: {pracownik.Id})";
            AddEmployeeButton.Visibility = Visibility.Collapsed;
            SaveEmployeeButton.Visibility = Visibility.Visible;
            CancelEditButton.Visibility = Visibility.Visible;
        }

        private void EmployeesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            EditEmployeeButton.IsEnabled = EmployeesList.SelectedItem != null;
            DeleteEmployeeButton.IsEnabled = EmployeesList.SelectedItem != null;
        }

        private void EditEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesList.SelectedItem is EmployeeDisplay ed)
            {
                ShowEditForm(ed.Employee);
            }
        }

        private async void SaveEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_editingEmployee == null) return;

            if (string.IsNullOrWhiteSpace(FirstNameBox.Text) || string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                MessageBox.Show("Podaj imię i nazwisko pracownika.", "Brak danych", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(RateBox.Text, out var rate))
            {
                MessageBox.Show("Nieprawidłowa stawka godzinowa.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var pracownik = new Pracownik(
                    _editingEmployee.Id,
                    FirstNameBox.Text.Trim(),
                    LastNameBox.Text.Trim(),
                    PositionBox.Text?.Trim() ?? string.Empty,
                    rate
                );

                await _dataService.AktualizujPracownika(pracownik);
                await LoadDataAsync();
                ClearEmployeeForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie udało się zaktualizować pracownika: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            ClearEmployeeForm();
        }

        private async void DeleteEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesList.SelectedItem is not EmployeeDisplay ed)
                return;

            var result = MessageBox.Show(
                $"Czy na pewno chcesz usunąć pracownika {ed.Employee.Imie} {ed.Employee.Nazwisko}?\n\nUsunięte zostaną również wszystkie jego wpisy czasu pracy!",
                "Potwierdź usunięcie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _dataService.UsunPracownika(ed.Employee.Id);
                    await LoadDataAsync();
                    if (_editingEmployee?.Id == ed.Employee.Id)
                        ClearEmployeeForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nie udało się usunąć pracownika: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}