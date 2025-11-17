using System.Collections.Generic;
using System.Threading.Tasks;
using WorkTimeTracker.Models;

namespace WorkTimeTracker.Interfaces
{
    public interface IDataService
    {
    Task<System.Collections.Generic.List<Pracownik>> WczytajPracownikow(string nazwaPliku = "pracownicy.json");
    Task<System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<RejestrCzasu>>> WczytajRejestry(string nazwaPliku = "rejestry.json");
        Task ZapiszPracownika(Pracownik pracownik, int id);
        Task DodajRejestrDlaPracownika(int pracownikId, RejestrCzasu rejestr);
        // Update an existing employee
        Task AktualizujPracownika(Pracownik pracownik);
        // Remove employee (and related registers)
        Task UsunPracownika(int pracownikId);
    }
}