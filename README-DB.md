# Database (SQLite + EF Core)

This project now supports SQLite persistence via Entity Framework Core.

Files / behavior

- The SQLite file is created at: `%LocalAppData%\WorkTimeTracker\worktimedata.db` by default.
- On application startup, the app will attempt to apply any pending EF Core migrations (via `DbInitializer.ApplyMigrations()`).

Creating and applying migrations (local development, PowerShell):

1. Install dotnet ef tool if needed (once):

```powershell
dotnet tool install --global dotnet-ef
```

2. Create a migration (from repository root):

```powershell
.\scripts\create-migration.ps1 -Name InitialCreate
```

3. Apply migrations / create or update DB:

```powershell
.\scripts\init-db.ps1
# or
dotnet ef database update
```

Notes

- `Microsoft.EntityFrameworkCore.Tools` and `Microsoft.EntityFrameworkCore.Design` packages are included and allow `dotnet ef` to work.
- If you prefer not to auto-apply migrations at startup, remove the call to `DbInitializer.ApplyMigrations()` in `App.xaml.cs`.
- The `Data/Migrations` folder is created when you run `dotnet ef migrations add ...` locally; it is not committed here automatically.
