Param(
    [string]$Name = "InitialCreate"
)

# Create migration with given name
if (-not (Get-Command dotnet-ef -ErrorAction SilentlyContinue)) {
    Write-Error "dotnet-ef not found. Install it as a global tool: dotnet tool install --global dotnet-ef"
    exit 1
}

dotnet ef migrations add $Name -o Data/Migrations
Write-Host "Migration $Name created in Data/Migrations" -ForegroundColor Green
