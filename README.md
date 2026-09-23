# Water Supply Management System

A three-phase .NET project for learning OOP, ASP.NET Core MVC, SQL Server, and Progressive Web Applications.

## Repository structure

```text
WaterSupplyManagementSystem/
├── ConsoleApplication/       # Phase 1: menu-driven OOP console app
├── ASPNETApplication/         # Phase 2: ASP.NET Core MVC + Identity + EF Core
├── PWA/                       # Phase 3 notes; assets are shared with the web app
├── Shared/                    # reusable domain and application services
├── Database/                  # SQL Server scripts and Mermaid ER diagram
├── Documentation/             # SRS, presentation, report, and checklists
└── README.md
```

## Project scope

The project manages five business tables: `Residents`, `WaterConnections`, `MeterReadings`, `Bills`, and `ServiceRequests`. It includes separate administrator and resident workflows, connection deactivation that preserves history, human-readable connection numbers, and a lightweight PWA shell. Payments are outside the current scope.

## Application workflow

- Registration automatically creates a `Resident` account and resident profile. It does not create a water connection.
- An administrator assigns a resident's water connection, records meter readings, and creates bills.
- Residents use the Resident Portal to view their own account data and submit service requests.
- Administrators review the service-request queue and update request statuses; they do not raise requests from the admin dashboard.
- Staff-created resident profiles are available for cases where an administrator needs to enter a resident manually.

## Technology

- .NET 10 and C#
- ASP.NET Core MVC and ASP.NET Core Identity
- Entity Framework Core with SQL Server
- Bootstrap responsive UI, web manifest, service worker, offline page, and browser notifications
- xUnit and FluentAssertions tests

## Run the console application

```powershell
dotnet run --project ConsoleApplication/WaterSupply.Console
```

The console menu supports adding/listing/searching residents, adding connections, recording readings, viewing bills, adding requests, and viewing a summary report. It starts with small demo data.

## Run the web application and PWA

1. Start SQL Server or LocalDB.
2. Run `Database/SQLScripts/001_CreateDatabase.sql`, then `002_CreateTables.sql`, `003_SeedReferenceData.sql`, and optionally `004_SeedDemoData.sql`.
3. Update `ASPNETApplication/WaterSupply.Web/appsettings.json` if your SQL Server instance differs.
4. Run:

```powershell
dotnet run --project ASPNETApplication/WaterSupply.Web
```

The development initializer creates `admin@watersupply.local` / `Admin@12345` and `resident@watersupply.local` / `Resident@12345`. Change these before real deployment. Open the site on a mobile browser and use the browser install action to install the PWA.

For a presentation preview without SQL Server, run the app in Testing mode with demo data enabled:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Testing"
$env:WATER_SUPPLY_DEMO_DATA = "true"
dotnet run --project ASPNETApplication/WaterSupply.Web --urls http://127.0.0.1:5077
```

This preview includes four residents, four connections, current-month meter readings, bills, and service requests. Use `admin@watersupply.local` / `Admin@12345` to open the administrator dashboard, or `resident@watersupply.local` / `Resident@12345` to open the resident portal.

### Deploy the presentation preview to Vercel

The repository includes `Dockerfile.vercel` and `vercel.json` for Vercel's container runtime. The Vercel deployment runs the seeded presentation preview with in-memory data; it is intended for demonstration, not durable production records. Connect this repository to Vercel or run `vercel deploy --prod` from the repository root after authenticating with Vercel.

### Deploy the presentation preview to Render

The repository also includes `render.yaml`. Create a new Render Blueprint from this repository and Render will build `Dockerfile.vercel`, use the `/Account/Login` health check, and start the seeded presentation preview. This mode uses in-memory demo data so it does not require SQL Server. For durable records, set `WATER_SUPPLY_DEMO_DATA=false`, provide the MonsterASP SQL Server value as `ConnectionStrings__DefaultConnection`, and run `Database/SQLScripts/005_AddConnectionRequests.sql` on that database before switching the service to production data.

## Test and build

```powershell
dotnet restore WaterSupplyManagement.sln
dotnet test WaterSupplyManagement.sln
dotnet build WaterSupplyManagement.sln --configuration Release
```

The repository includes `Documentation/verify-rubric.ps1` and `Database/SQLScripts/verify-schema.ps1` for checklist verification. SQL Server is required for live database execution; automated web tests use EF Core InMemory.
