# NexusIT — IT Asset & Infrastructure Management Platform

NexusIT is a polished ASP.NET Core 8 MVC portfolio application for demonstrating practical IT service management, asset management, support operations and auditability.

## Included in this final build

- Premium dark NexusIT dashboard
- Responsive desktop/tablet/mobile UI
- ASP.NET Core Identity authentication
- Role-based access control
  - Administrator
  - IT Manager
  - IT Technician
  - Employee
- Support ticket workflow
  - Open → In Progress → Waiting → Resolved → Closed
  - Reopen support workflow
  - Priority and category filtering
  - Assignment
  - Ticket comments and internal notes
  - Activity timeline
- SLA monitoring
  - Response SLA
  - Resolution SLA
  - At-risk and breached states
  - SLA health dashboard metrics
- IT asset lifecycle
  - Available
  - Assigned
  - Maintenance
  - Retired
  - Assignment/return/maintenance/restore/retire actions
  - Asset history
  - Warranty and purchase information
- Maintenance management
  - Scheduled
  - In Progress
  - Completed
  - Cost tracking
- Employee directory
- Reports and operational analytics
- Activity/audit log
- Administrator settings
- Validation, anti-forgery protection and authorization checks
- SQL Server / LocalDB Entity Framework Core database
- Automatic migration on application startup
- Repeatable professional demo data for a fresh database
- `/health` endpoint for a quick application/database health check

## Requirements

- Windows 10/11
- Visual Studio 2022 with ASP.NET and web development workload, or the .NET 8 SDK
- SQL Server LocalDB (included with Visual Studio in common installations)

## Run the application

1. Extract the project folder.
2. Open `NexusIT.sln` in Visual Studio.
3. Restore NuGet packages.
4. Build the solution.
5. Run the `NexusIT` project.

The application automatically applies EF Core migrations and creates the demo dataset on a fresh database.

### CLI

From the folder containing `NexusIT.sln`:

```powershell
dotnet restore
dotnet build
dotnet run --project .\NexusIT\NexusIT.csproj
```

The HTTPS development profile is configured for `https://localhost:7227` and HTTP for `http://localhost:5028`.

## Demo accounts

These accounts are intended for local portfolio/demo use only. Change or remove them before deploying publicly.

| Role | Email | Password |
|---|---|---|
| Administrator | `admin@nexusit.local` | `NexusIT123!` |
| IT Manager | `manager@nexusit.local` | `NexusIT123!` |
| IT Technician | `technician@nexusit.local` | `NexusIT123!` |
| Employee | `employee@nexusit.local` | `NexusIT123!` |

The employee account is linked to the seeded Joshua Barnes employee profile so the employee-scoped ticket experience can be demonstrated immediately.

## Database

The default development connection uses SQL Server LocalDB and a database named `aspnet-NexusIT-d99f36c5-e603-49e5-856b-9bc68fcbe14b`.

If you want a different SQL Server instance, edit `NexusIT/appsettings.json` and update `ConnectionStrings:DefaultConnection`.

## Important deployment note

The demo seeder intentionally creates known demo credentials. Do **not** expose the seeded credentials or use them in production. For deployment, remove/disable `DemoDataSeeder`, replace the connection string with a secure secret, and use production Identity/password policies.

## Project structure

```text
NexusIT/
├── Areas/Identity/          Authentication pages
├── Controllers/             MVC controllers
├── Data/                     DbContext, roles, migrations and seeders
├── Models/                   Domain models
├── Models/ViewModels/        Dashboard/ticket view models
├── Services/                 Activity logging service
├── Views/                    MVC Razor views
├── wwwroot/css/              NexusIT UI styling
├── wwwroot/js/               Client-side interactions
├── appsettings.json          Database and application configuration
└── NexusIT.csproj            .NET 8 project file
```

## Portfolio demo flow

For a strong demonstration, log in as Administrator and walk through:

1. Dashboard KPIs and SLA health
2. Ticket queue and ticket details
3. Ticket comments/internal notes and timeline
4. Asset inventory and lifecycle history
5. Maintenance records
6. Employee directory
7. Reports/analytics
8. Activity/audit log
9. User management and role changes
10. System settings

## Health check

After starting the app, open `/health`. A healthy application returns a JSON response showing the NexusIT service as healthy and confirms that the configured database can be reached.
