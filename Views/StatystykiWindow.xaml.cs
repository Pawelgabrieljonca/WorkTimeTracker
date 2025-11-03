using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WorkTimeTracker.Models;
using WorkTimeTracker.Services;

namespace WorkTimeTracker.Views
{
    public partial class StatystykiWindow : Window
    {
        private readonly StatystykiService _statystykiService;
        private readonly int _pracownikId;
        private StatystykiPracownika? _statystyki;

        public StatystykiWindow(StatystykiService statystykiService, int pracownikId)
        {
            InitializeComponent();
            _statystykiService = statystykiService;
            _pracownikId = pracownikId;
            InitializeYearPicker();
            LoadStatystykiAsync();
        }

        private void InitializeYearPicker()
        {
            var currentYear = DateTime.Today.Year;
            var years = Enumerable.Range(currentYear - 2, 3).ToList(); // Ostatnie 2 lata + bieżący
            YearPicker.ItemsSource = years;
            YearPicker.SelectedItem = currentYear;
        }

        private async void LoadStatystykiAsync()
        {
            try
            {
                _statystyki = await _statystykiService.PobierzStatystykiPracownika(_pracownikId);
                UpdateUI();
                await LoadMonthlyStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas ładowania statystyk: {ex.Message}",
                              "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUI()
        {
            if (_statystyki == null) return;

            EmployeeNameBlock.Text = _statystyki.ImieNazwisko;
            TotalHoursBlock.Text = $"{_statystyki.CalkowiteGodziny:F1}h";
            OvertimeHoursBlock.Text = $"{_statystyki.GodzinyNadliczbowe:F1}h";
            VacationDaysBlock.Text = _statystyki.DniUrlopowe.ToString();
            AverageHoursBlock.Text = $"{_statystyki.SredniaGodzinDziennie:F1}h";
        }

        private async void YearPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (YearPicker.SelectedItem != null)
            {
                await LoadMonthlyStats();
            }
        }

        private async System.Threading.Tasks.Task LoadMonthlyStats()
        {
            try
            {
                var selectedYear = (int)YearPicker.SelectedItem;
                var monthlyStats = await _statystykiService.PobierzStatystykiMiesieczne(_pracownikId, selectedYear);

                var stats = new List<MonthlyStatDisplay>();
                var months = new[]
                {
                    "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
                    "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
                };

                for (int i = 0; i < 12; i++)
                {
                    var month = i + 1;
                    var hours = monthlyStats.TryGetValue(month, out var h) ? h : 0;
                    stats.Add(new MonthlyStatDisplay
                    {
                        Miesiac = months[i],
                        Godziny = hours,
                        Nadgodziny = Math.Max(0, hours - (20 * 8)), // Przybliżona liczba dni roboczych * 8h
                        Urlopy = _statystyki?.GodzinyMiesieczne.TryGetValue(month, out _) == true ?
                                _statystyki.DniUrlopowe : 0
                    });
                }

                MonthlyStatsGrid.ItemsSource = stats;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas ładowania statystyk miesięcznych: {ex.Message}",
                              "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class MonthlyStatDisplay
        {
            public string Miesiac { get; set; } = string.Empty;
            public decimal Godziny { get; set; }
            public decimal Nadgodziny { get; set; }
            public int Urlopy { get; set; }
        }
    }
}