# Water Supply Management System

A deliberately basic three-phase .NET project for learning OOP, ASP.NET Core MVC, SQL Server, and Progressive Web Applications.

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

## Basic scope

The project manages five business tables: `Residents`, `WaterConnections`, `MeterReadings`, `Bills`, and `ServiceRequests`. It intentionally excludes payments, notifications as a business table, resident portals, and advanced workflows so the project remains easy to understand.

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

## Test and build

```powershell
dotnet restore WaterSupplyManagement.sln
dotnet test WaterSupplyManagement.sln
dotnet build WaterSupplyManagement.sln --configuration Release
```

The repository includes `Documentation/verify-rubric.ps1` and `Database/SQLScripts/verify-schema.ps1` for checklist verification. SQL Server is required for live database execution; automated web tests use EF Core InMemory.
