using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using WorkTimeTracker.Services;

namespace WorkTimeTracker.Views
{
    public partial class EmployeeWindow : Window
    {
        private readonly IDataService _dataService;
        private readonly int _pracownikId;
        private Pracownik? _pracownik;

        public EmployeeWindow(int pracownikId)
        {
            InitializeComponent();
            _pracownikId = pracownikId;

            // choose data service same as MainWindow
            if (Utils.AppSettings.UseEfDataService)
                _dataService = new EfDataService(new Data.WorkTimeContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.WorkTimeContext>().UseSqlite($"Data Source={System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkTimeTracker", "worktimedata.db")} ").Options));
            else
                _dataService = new DataService();

            this.Loaded += EmployeeWindow_Loaded;
        }

        private async void EmployeeWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var people = await _dataService.WczytajPracownikow();
            _pracownik = people.FirstOrDefault(p => p.Id == _pracownikId);
            HeaderText.Text = _pracownik != null ? $"Panel Pracownika - {_pracownik.Imie} {_pracownik.Nazwisko}" : $"Panel Pracownika - ID {_pracownikId}";

            var rejestry = await _dataService.WczytajRejestry();
            var list = rejestry.TryGetValue(_pracownikId, out var lst) ? lst : new List<RejestrCzasu>();
            EntriesList.ItemsSource = list.Select(r => $"{r.Data:yyyy-MM-dd} - {r.LiczbaGodzin}h {(r.CzyUrlop?"(Urlop)":"")}{(r.CzyNadgodziny?" (Nadgodz.)":"")}").ToList();
        }

        private async void AddEntryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(HoursBox.Text, out var hours))
            {
                MessageBox.Show("Nieprawidłowa liczba godzin.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var date = EntryDatePicker.SelectedDate ?? DateTime.Today;
            var isLeave = IsLeaveCheck.IsChecked == true;
            var isOver = IsOvertimeCheck.IsChecked == true;

            var rejestr = new RejestrCzasu(date, hours, isLeave, isOver);

            try
            {
                await _dataService.DodajRejestrDlaPracownika(_pracownikId, rejestr);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
