using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Microsoft.EntityFrameworkCore;
using WorkTimeTracker.Models;
using WorkTimeTracker.Services;
using WorkTimeTracker.Data;
using WorkTimeTracker.Utils;

namespace WorkTimeTracker.Views
{
    public partial class MainWindow : Window
    {
    private readonly WorkTimeTracker.Interfaces.IDataService _dataService;
    private readonly RaportService _raportService;
    private readonly StatystykiService _statystykiService;
        private List<Pracownik> _employees = new();
        private Dictionary<int, List<RejestrCzasu>> _rejestry = new();
        private Pracownik? _editingEmployee;

        public MainWindow()
        {
            // Choose data service implementation based on settings
            if (AppSettings.UseEfDataService)
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkTimeTracker");
                Directory.CreateDirectory(folder);
                var dbPath = Path.Combine(folder, "worktimedata.db");
                var conn = $"Data Source={dbPath}";
                var options = new DbContextOptionsBuilder<WorkTimeContext>()
                    .UseSqlite(conn)
                    .Options;
                var ctx = new WorkTimeContext(options);
                _dataService = new EfDataService(ctx);
            }
            else
            {
                _dataService = new DataService();
            }

            _raportService = new RaportService(_dataService);
            _statystykiService = new StatystykiService(_dataService);

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

            // Pobierz wybrany miesiąc i rok
            var selectedMonth = ReportMonthPicker.SelectedIndex + 1;
            var selectedYear = (int)ReportYearPicker.SelectedItem;

            try
            {
                var raportTekst = await _raportService.GenerujRaportMiesieczny(pracownik.Id, selectedYear, selectedMonth);
                ReportBox.Text = raportTekst;
                await _raportService.ZapiszRaport(raportTekst, pracownik.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show("Raport został wygenerowany i zapisany.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void GenerateEmployeeReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesList.SelectedItem is not EmployeeDisplay ed)
            {
                MessageBox.Show("Wybierz pracownika z listy (kliknij wpis).", "Brak wyboru", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var pracownik = ed.Employee;

            try
            {
                var raportTekst = await _raportService.GenerujRaportPracownika(pracownik.Id);
                ReportBox.Text = raportTekst;
                await _raportService.ZapiszRaport(raportTekst, pracownik.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show("Raport został wygenerowany i zapisany.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowDetailedStatsButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesList.SelectedItem is not EmployeeDisplay ed)
            {
                MessageBox.Show("Wybierz pracownika z listy.", "Brak wyboru", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var statystykiWindow = new StatystykiWindow(_statystykiService, ed.Employee.Id)
            {
                Owner = this
            };
            statystykiWindow.ShowDialog();
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