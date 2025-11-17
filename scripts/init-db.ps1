# Initializes EF Core migrations and applies them to create/update the SQLite database.
# Usage: run from repository root (PowerShell):
#   .\scripts\init-db.ps1

# Ensure dotnet-ef is available; you may need to install the global tool:
#   dotnet tool install --global dotnet-ef

# Create an initial migration (only if no migrations exist). You can edit the migration name.
if (-not (Test-Path -Path "Data/Migrations")) {
    dotnet ef migrations add InitialCreate -o Data/Migrations
}

# Apply pending migrations (creates the DB file specified in DbInitializer)
dotnet ef database update

Write-Host "Migrations applied (or ensured)." -ForegroundColor Green
