using System;
using System.Threading.Tasks;
using System.Windows;
using WorkTimeTracker.Services;
using WorkTimeTracker.Views;

namespace WorkTimeTracker.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _auth = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text?.Trim() ?? string.Empty;
            var password = PasswordBox.Password ?? string.Empty;

            var result = await _auth.AuthenticateAsync(username, password);
            if (!result.Success)
            {
                MessageBox.Show(result.Message ?? "Logowanie nie powiodło się.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Open appropriate window
            if (result.Role == Role.Admin)
            {
                var main = new MainWindow();
                main.Show();
            }
            else
            {
                // Ensure we have an id
                var id = result.PracownikId ?? 0;
                var empWin = new EmployeeWindow(id);
                empWin.Show();
            }

            this.Close();
        }
    }
}
