# WorkTimeTracker

Prosty szkielet aplikacji desktopowej WPF (.NET) - WorkTimeTracker.

Struktura katalogów utworzona:

- Models
- Services
- Interfaces
- Utils
- Views

Założenia:

- Projekt WPF targeting `net7.0-windows` (`UseWPF=true`).
- Minimalne klasy przykładowe (TimeEntry, ITimeEntryService, TimeEntryService, TimeFormatter).

Jak otworzyć w Visual Studio:

1. Otwórz katalog `WorkTimeTracker` w Visual Studio (File -> Open -> Project/Solution -> wybierz plik `WorkTimeTracker.csproj`).

Jak zbudować z linii poleceń (PowerShell):

```powershell
cd "c:\Users\pawel\Dokumenty\Studia\05_studia\ProjketPwSW\WorkTime\WorkTimeTracker"
# Przywróć pakiety i zbuduj
dotnet restore; dotnet build
```


