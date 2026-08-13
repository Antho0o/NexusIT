$ErrorActionPreference = 'Stop'

Write-Host "==============================================="
Write-Host " NexusIT - Build & Run"
Write-Host "==============================================="

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET SDK was not found. Install the .NET 8 SDK or open the solution in Visual Studio."
}

dotnet restore .\NexusIT.sln
if ($LASTEXITCODE -ne 0) { throw "NuGet restore failed." }

dotnet build .\NexusIT.sln --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

dotnet run --project .\NexusIT\NexusIT.csproj --no-build
