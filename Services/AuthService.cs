using System;
using System.Threading.Tasks;
using WorkTimeTracker.Models;

namespace WorkTimeTracker.Services
{
    public enum Role { Admin, Employee }

    public class AuthResult
    {
        public bool Success { get; set; }
        public Role Role { get; set; }
        public int? PracownikId { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Simple authentication service for demo purposes.
    /// - Admin login: username="admin", password="admin"
    /// - Employee login: username starting with 'p' followed by id, password 'p{Id}' (e.g. p1/p1)
    /// This avoids requiring a users table while still providing role separation.
    /// </summary>
    public class AuthService
    {
        public Task<AuthResult> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Task.FromResult(new AuthResult { Success = false, Message = "Proszę podać nazwę użytkownika." });

            if (username == "admin" && password == "admin")
            {
                return Task.FromResult(new AuthResult { Success = true, Role = Role.Admin });
            }

            // employee pattern: p{Id} / p{Id}
            if (username.StartsWith("p", StringComparison.OrdinalIgnoreCase))
            {
                var rest = username.Substring(1);
                if (int.TryParse(rest, out var id))
                {
                    if (password == $"p{id}")
                    {
                        return Task.FromResult(new AuthResult { Success = true, Role = Role.Employee, PracownikId = id });
                    }
                    return Task.FromResult(new AuthResult { Success = false, Message = "Nieprawidłowe hasło." });
                }
            }

            return Task.FromResult(new AuthResult { Success = false, Message = "Nieznany użytkownik." });
        }
    }
}
