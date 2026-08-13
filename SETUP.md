# NexusIT Quick Setup

## Visual Studio

1. Install Visual Studio 2022.
2. Select **ASP.NET and web development**.
3. Make sure SQL Server LocalDB is installed.
4. Open `NexusIT.sln`.
5. Select the `https` launch profile.
6. Press **F5**.

The first run applies migrations and seeds demo users/data automatically.

## Command line

```powershell
dotnet restore
dotnet build
dotnet run --project .\NexusIT\NexusIT.csproj
```

## If the database needs a clean reset

For a local demo database, remove the NexusIT LocalDB database and run the application again. The startup migration and demo seeder will rebuild it.

## Default demo credentials

- Administrator: `admin@nexusit.local` / `NexusIT123!`
- IT Manager: `manager@nexusit.local` / `NexusIT123!`
- IT Technician: `technician@nexusit.local` / `NexusIT123!`
- Employee: `employee@nexusit.local` / `NexusIT123!`

## Production warning

These are demo credentials. Never publish them or use them for a production deployment.
