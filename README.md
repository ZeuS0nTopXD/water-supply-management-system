# Water Supply Management System

This repository is a three-phase .NET project for managing municipal water connections, meter readings, billing, payments, service requests, residents, and operational reports.

## Repository structure

```text
WaterSupplyManagementSystem/
├── ConsoleApplication/       # Phase 1: menu-driven OOP console application
├── ASPNETApplication/         # Phase 2: ASP.NET Core MVC + Identity + EF Core
├── PWA/                       # Phase 3 notes and mobile/offline behavior
├── Shared/                    # reusable domain and application services
├── Database/                  # SQL Server scripts and Mermaid ER diagram
├── Documentation/             # SRS, presentation outline, report outline
└── README.md
```

## Technology stack

- .NET 10, C# and nullable reference types
- ASP.NET Core MVC and ASP.NET Core Identity
- Entity Framework Core with SQL Server provider
- SQL Server schema with seven normalized business tables
- Bootstrap responsive UI, web manifest, service worker, offline shell
- xUnit and FluentAssertions tests

## Run Phase 1

```powershell
dotnet run --project ConsoleApplication/WaterSupply.Console
```

The console app is seeded with demo residents, connections, readings, bills, payments, and service requests. Its menu includes CRUD-style operations, search, and reports.

## Run Phase 2 and Phase 3

1. Start SQL Server or LocalDB.
2. Update `ASPNETApplication/WaterSupply.Web/appsettings.json` if your server instance differs.
3. Apply the schema with `Database/SQLScripts/001_CreateDatabase.sql` followed by `002_CreateTables.sql`. Optional demo data is in `003_SeedReferenceData.sql` and `004_SeedDemoData.sql`.
4. Run:

```powershell
dotnet run --project ASPNETApplication/WaterSupply.Web
```

The development seed creates `admin@watersupply.local` / `Admin@12345` and `resident@watersupply.local` / `Resident@12345`. Change these credentials before any real deployment.

The PWA is the same web application opened on a mobile browser. It exposes an install manifest, service worker, cached offline shell, responsive forms, and browser notification permission flow.

## Test and build

```powershell
dotnet restore WaterSupplyManagement.sln
dotnet test WaterSupplyManagement.sln
dotnet build WaterSupplyManagement.sln --configuration Release
```

The repository includes a PowerShell rubric verifier at `Documentation/verify-rubric.ps1` and a database schema verifier at `Database/SQLScripts/verify-schema.ps1`.

## Scope note

Database connectivity is intentionally configured for SQL Server. If SQL Server is unavailable on the development machine, application compilation and tests still run with the EF Core InMemory provider in the test project, but live database initialization must be completed on a machine with SQL Server access.
