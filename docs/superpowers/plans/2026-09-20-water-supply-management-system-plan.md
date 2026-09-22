# Water Supply Management System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a complete three-phase Water Supply Management System that satisfies the console, ASP.NET Core MVC, SQL Server, PWA, database, and repository requirements in the approved design specification.

**Architecture:** Use one .NET solution with a domain library, application services, a console client, and an ASP.NET Core MVC client. The console uses in-memory repositories; the web app uses EF Core SQL Server and Identity; the PWA is implemented as the web app's installable responsive/offline client shell.

**Tech Stack:** .NET 10, C#, ASP.NET Core MVC, Razor views, Entity Framework Core SQL Server, ASP.NET Core Identity, xUnit, Bootstrap, browser Service Worker APIs, SQL Server scripts, Mermaid ER diagram.

**Spec:** `docs/superpowers/specs/2026-09-20-water-supply-management-system-design.md`

## Global Constraints

- Target framework is .NET 10 (`net10.0`).
- The business schema contains seven normalized tables: Residents, WaterConnections, MeterReadings, Bills, Payments, ServiceRequests, Notifications.
- The console must include a menu, OOP classes/objects, inheritance, encapsulation, exception handling, collections, search, and reports.
- The ASP.NET application must include login, logout, change password, CRUD, search, dashboard metrics, filtered reports, printable output, SQL Server, primary keys, foreign keys, and relationships.
- The PWA must include responsive design, mobile-friendly UI, installability, a service worker, an offline read-only feature, and notifications.
- Resident data must be ownership-filtered; changing an identifier in a URL must not expose another resident's records.
- State-changing MVC forms require anti-forgery validation; passwords are managed by ASP.NET Core Identity.
- Mutating PWA actions remain online-only and must display a clear offline message.

## Review Focus

- Duplicate readings for the same connection/date must be rejected instead of silently overwriting data; pinned by Task 2's repository/service test.
- A resident requesting another resident's bill by URL must receive a denial/empty result; pinned by Task 6's authorization test.
- A payment larger than the bill balance must be rejected and must not change the bill status; pinned by Task 2's billing test.
- An offline mutation must not appear to succeed or lose data; pinned by Task 8's service-worker behavior test/documented browser smoke test.
- Invalid or empty console input must return to the menu without crashing the process; pinned by Task 3's input/menu test.

---

### Task 1: Create the solution and project skeleton

**Files:**
- Create: `WaterSupplyManagement.sln`
- Create: `ConsoleApplication/WaterSupply.Console/WaterSupply.Console.csproj`
- Create: `ConsoleApplication/WaterSupply.Console.Tests/WaterSupply.Console.Tests.csproj`
- Create: `Shared/WaterSupply.Domain/WaterSupply.Domain.csproj`
- Create: `Shared/WaterSupply.Application/WaterSupply.Application.csproj`
- Create: `Shared/WaterSupply.Application.Tests/WaterSupply.Application.Tests.csproj`
- Create: `ASPNETApplication/WaterSupply.Web/WaterSupply.Web.csproj`
- Create: `ASPNETApplication/WaterSupply.Web.Tests/WaterSupply.Web.Tests.csproj`
- Create: `.gitignore`

**Interfaces:**
- Produces project references from `WaterSupply.Console` and `WaterSupply.Web` to `WaterSupply.Application`; `WaterSupply.Application` references `WaterSupply.Domain`.
- Produces test projects that reference the project they test and use xUnit.

- [ ] **Step 1: Create the solution and projects.**

Run each command from the repository root:

```powershell
dotnet new sln -n WaterSupplyManagement
dotnet new classlib -n WaterSupply.Domain -o Shared/WaterSupply.Domain --framework net10.0
dotnet new classlib -n WaterSupply.Application -o Shared/WaterSupply.Application --framework net10.0
dotnet new console -n WaterSupply.Console -o ConsoleApplication/WaterSupply.Console --framework net10.0
dotnet new mvc -n WaterSupply.Web -o ASPNETApplication/WaterSupply.Web --framework net10.0 --auth Individual
dotnet new xunit -n WaterSupply.Application.Tests -o Shared/WaterSupply.Application.Tests --framework net10.0
dotnet new xunit -n WaterSupply.Console.Tests -o ConsoleApplication/WaterSupply.Console.Tests --framework net10.0
dotnet new xunit -n WaterSupply.Web.Tests -o ASPNETApplication/WaterSupply.Web.Tests --framework net10.0
```

- [ ] **Step 2: Add projects to the solution and references.**

```powershell
dotnet sln WaterSupplyManagement.sln add Shared/WaterSupply.Domain/WaterSupply.Domain.csproj
dotnet sln WaterSupplyManagement.sln add Shared/WaterSupply.Application/WaterSupply.Application.csproj
dotnet sln WaterSupplyManagement.sln add ConsoleApplication/WaterSupply.Console/WaterSupply.Console.csproj
dotnet sln WaterSupplyManagement.sln add ASPNETApplication/WaterSupply.Web/WaterSupply.Web.csproj
dotnet sln WaterSupplyManagement.sln add Shared/WaterSupply.Application.Tests/WaterSupply.Application.Tests.csproj
dotnet sln WaterSupplyManagement.sln add ConsoleApplication/WaterSupply.Console.Tests/WaterSupply.Console.Tests.csproj
dotnet sln WaterSupplyManagement.sln add ASPNETApplication/WaterSupply.Web.Tests/WaterSupply.Web.Tests.csproj
dotnet add Shared/WaterSupply.Application/WaterSupply.Application.csproj reference Shared/WaterSupply.Domain/WaterSupply.Domain.csproj
dotnet add ConsoleApplication/WaterSupply.Console/WaterSupply.Console.csproj reference Shared/WaterSupply.Application/WaterSupply.Application.csproj
dotnet add ASPNETApplication/WaterSupply.Web/WaterSupply.Web.csproj reference Shared/WaterSupply.Application/WaterSupply.Application.csproj
dotnet add Shared/WaterSupply.Application.Tests/WaterSupply.Application.Tests.csproj reference Shared/WaterSupply.Application/WaterSupply.Application.csproj
dotnet add ConsoleApplication/WaterSupply.Console.Tests/WaterSupply.Console.Tests.csproj reference ConsoleApplication/WaterSupply.Console/WaterSupply.Console.csproj
dotnet add ASPNETApplication/WaterSupply.Web.Tests/WaterSupply.Web.Tests.csproj reference ASPNETApplication/WaterSupply.Web/WaterSupply.Web.csproj
```

- [ ] **Step 3: Add test and persistence packages.**

```powershell
dotnet add Shared/WaterSupply.Application.Tests package FluentAssertions
dotnet add ASPNETApplication/WaterSupply.Web package Microsoft.EntityFrameworkCore.SqlServer
dotnet add ASPNETApplication/WaterSupply.Web package Microsoft.EntityFrameworkCore.Tools
dotnet add ASPNETApplication/WaterSupply.Web package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add ASPNETApplication/WaterSupply.Web.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add ASPNETApplication/WaterSupply.Web.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add ASPNETApplication/WaterSupply.Web.Tests package FluentAssertions
```

- [ ] **Step 4: Add the required repository folders and a baseline ignore file.**

`.gitignore` must exclude `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, local database files, and generated publish output while keeping SQL scripts, Mermaid diagrams, documentation, and source code tracked.

- [ ] **Step 5: Restore and build the empty skeleton.**

Run: `dotnet restore WaterSupplyManagement.sln`  
Expected: restore completes successfully.  
Run: `dotnet build WaterSupplyManagement.sln --no-restore`  
Expected: all projects compile with zero errors.

- [ ] **Step 6: Commit.**

```powershell
git add WaterSupplyManagement.sln .gitignore ConsoleApplication Shared ASPNETApplication
git commit -m "chore: scaffold water supply solution"
```

### Task 2: Implement the shared domain and application services with TDD

**Files:**
- Create: `Shared/WaterSupply.Domain/Entities/Person.cs`
- Create: `Shared/WaterSupply.Domain/Entities/Resident.cs`
- Create: `Shared/WaterSupply.Domain/Entities/StaffUser.cs`
- Create: `Shared/WaterSupply.Domain/Entities/WaterConnection.cs`
- Create: `Shared/WaterSupply.Domain/Entities/MeterReading.cs`
- Create: `Shared/WaterSupply.Domain/Entities/Bill.cs`
- Create: `Shared/WaterSupply.Domain/Entities/Payment.cs`
- Create: `Shared/WaterSupply.Domain/Entities/ServiceRequest.cs`
- Create: `Shared/WaterSupply.Domain/Entities/Notification.cs`
- Create: `Shared/WaterSupply.Domain/Enums/*.cs`
- Create: `Shared/WaterSupply.Domain/Exceptions/DomainValidationException.cs`
- Create: `Shared/WaterSupply.Application/Abstractions/*.cs`
- Create: `Shared/WaterSupply.Application/Services/*.cs`
- Create: `Shared/WaterSupply.Application/Reports/*.cs`
- Create: `Shared/WaterSupply.Application/InMemory/*.cs`
- Create: `Shared/WaterSupply.Application.Tests/DomainValidationTests.cs`
- Create: `Shared/WaterSupply.Application.Tests/BillingServiceTests.cs`
- Create: `Shared/WaterSupply.Application.Tests/SearchAndReportTests.cs`

**Interfaces:**
- `IRepository<T>` exposes `IReadOnlyList<T> GetAll()`, `T? GetById(int id)`, `void Add(T entity)`, `void Update(T entity)`, and `void Delete(int id)`.
- `IResidentService` exposes `Resident Create(...)`, `IReadOnlyList<Resident> Search(string query)`, and `void Delete(int residentId)`.
- `IBillingService` exposes `Bill GenerateBill(...)`, `Payment RecordPayment(int billId, decimal amount, PaymentMethod method, DateTime paymentDate)`, and `IReadOnlyList<Bill> SearchBills(string? status, DateOnly? from, DateOnly? to)`.
- `IReportService` exposes `ConsumptionReport GetConsumptionReport(DateOnly from, DateOnly to)`, `PaymentCollectionReport GetPaymentCollectionReport(DateOnly from, DateOnly to)`, and `OutstandingBillsReport GetOutstandingBillsReport()`.

- [ ] **Step 1: Write failing tests for domain invariants.**

```csharp
[Fact]
public void MeterReading_rejects_current_value_below_previous_value()
{
    Action act = () => MeterReading.Create(1, new DateOnly(2026, 9, 1), 120, 119);
    act.Should().Throw<DomainValidationException>();
}

[Fact]
public void Bill_rejects_payment_above_outstanding_balance()
{
    var bill = Bill.Create(1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 20, 10, 2);
    Action act = () => bill.RecordPayment(500, PaymentMethod.Cash, new DateTime(2026, 9, 15));
    act.Should().Throw<DomainValidationException>();
    bill.Status.Should().Be(BillStatus.Unpaid);
}

[Fact]
public void In_memory_repository_rejects_duplicate_reading_date_for_connection()
{
    var repository = new InMemoryMeterReadingRepository();
    repository.Add(MeterReading.Create(1, new DateOnly(2026, 9, 1), 100, 120));
    Action act = () => repository.Add(MeterReading.Create(1, new DateOnly(2026, 9, 1), 120, 140));
    act.Should().Throw<DomainValidationException>();
}
```

- [ ] **Step 2: Run the focused tests to verify the expected RED state.**

Run: `dotnet test Shared/WaterSupply.Application.Tests --filter "FullyQualifiedName~DomainValidationTests|FullyQualifiedName~BillingServiceTests"`  
Expected: tests fail because the domain entities and services do not exist yet; fix only compilation/test setup errors until the failure is about missing behavior.

- [ ] **Step 3: Implement the minimal entities and enums.**

Use private setters, factory methods, and domain methods. `MeterReading.Create` must calculate `Consumption = current - previous`; `Bill.Create` must calculate `TotalAmount = units * rate + fixedCharge + tax`; `Bill.RecordPayment` must reject non-positive or over-balance amounts and update `PaidAmount` and `Status`.

- [ ] **Step 4: Run the focused tests to verify GREEN.**

Run the same `dotnet test` command.  
Expected: all focused domain tests pass.

- [ ] **Step 5: Write failing tests for search, ownership-safe lookup, and reports.**

```csharp
[Fact]
public void Resident_search_is_case_insensitive_and_matches_name_or_phone()
{
    var service = TestData.CreateResidentServiceWith("Asha Patil", "9876543210");
    service.Search("asha").Should().ContainSingle(r => r.FullName == "Asha Patil");
    service.Search("9876543210").Should().ContainSingle(r => r.FullName == "Asha Patil");
}

[Fact]
public void Resident_bill_query_returns_only_the_authenticated_residents_bills()
{
    var service = TestData.CreateBillingServiceWithResidents(10, 20);
    service.GetBillsForResident(10).Should().OnlyContain(b => b.ResidentId == 10);
}
```

- [ ] **Step 6: Run the search/report tests to verify RED, then implement services.**

Run: `dotnet test Shared/WaterSupply.Application.Tests --filter "FullyQualifiedName~SearchAndReportTests"`  
Expected: RED for missing service behavior. Implement `InMemoryRepository<T>`, specialized duplicate-checking repositories, `ResidentService`, `BillingService`, `ServiceRequestService`, and report services. Re-run the command.  
Expected: all search/report tests pass.

- [ ] **Step 7: Run the complete application test project.**

Run: `dotnet test Shared/WaterSupply.Application.Tests`  
Expected: all application tests pass with zero warnings caused by test failures.

- [ ] **Step 8: Commit.**

```powershell
git add Shared
git commit -m "feat: add shared water supply domain and services"
```

### Task 3: Build the menu-driven console application

**Files:**
- Modify: `ConsoleApplication/WaterSupply.Console/Program.cs`
- Create: `ConsoleApplication/WaterSupply.Console/Menu/ConsoleMenu.cs`
- Create: `ConsoleApplication/WaterSupply.Console/Menu/InputReader.cs`
- Create: `ConsoleApplication/WaterSupply.Console/Menu/MenuAction.cs`
- Create: `ConsoleApplication/WaterSupply.Console/Seed/ConsoleSeedData.cs`
- Create: `ConsoleApplication/WaterSupply.Console/Reports/ConsoleReportRenderer.cs`
- Create: `ConsoleApplication/WaterSupply.Console.Tests/InputReaderTests.cs`
- Create: `ConsoleApplication/WaterSupply.Console.Tests/MenuSmokeTests.cs`
- Create: `ConsoleApplication/WaterSupply.Console.Tests/Fixtures/smoke-input.txt`

**Interfaces:**
- `InputReader.ReadRequiredString(string prompt)` returns a trimmed non-empty string or throws `InputValidationException`.
- `InputReader.ReadInt(string prompt, int min, int max)` returns a validated range value.
- `ConsoleMenu.Run()` displays the main menu, dispatches services, catches expected exceptions, and returns on option `0`.

- [ ] **Step 1: Write failing tests for invalid input and menu exit.**

```csharp
[Fact]
public void ReadInt_rejects_non_numeric_input_without_terminating_the_process()
{
    var reader = new InputReader(new StringReader("x\n3\n"), TextWriter.Null);
    reader.ReadInt("Choice", 0, 5).Should().Be(3);
}

[Fact]
public void Menu_option_zero_exits_after_printing_the_menu()
{
    var output = new StringWriter();
    var menu = TestMenu.Create(new StringReader("0\n"), output);
    menu.Run();
    output.ToString().Should().Contain("Water Supply Management System");
}
```

- [ ] **Step 2: Run the console tests to verify RED.**

Run: `dotnet test ConsoleApplication/WaterSupply.Console.Tests`  
Expected: RED because `InputReader` and `ConsoleMenu` are not implemented.

- [ ] **Step 3: Implement the menu, input helper, seed data, and report renderer.**

Use `List<T>` for ordered display and `Dictionary<int, T>` through the application repositories. Each action must return to the main menu after success or a handled validation/error message; no expected bad input may terminate the process.

- [ ] **Step 4: Run console tests and a scripted smoke test.**

Run: `dotnet test ConsoleApplication/WaterSupply.Console.Tests`  
Expected: all console tests pass.  
Run: `dotnet run --project ConsoleApplication/WaterSupply.Console --no-build < ConsoleApplication/WaterSupply.Console.Tests/Fixtures/smoke-input.txt`  
Expected: output contains the main menu, a resident search result, an unpaid-bills report, and the exit message.

- [ ] **Step 5: Commit.**

```powershell
git add ConsoleApplication
git commit -m "feat: add menu-driven console application"
```

### Task 4: Create SQL Server scripts and ER diagram

**Files:**
- Create: `Database/SQLScripts/001_CreateDatabase.sql`
- Create: `Database/SQLScripts/002_CreateTables.sql`
- Create: `Database/SQLScripts/003_SeedReferenceData.sql`
- Create: `Database/SQLScripts/004_SeedDemoData.sql`
- Create: `Database/SQLScripts/verify-schema.ps1`
- Create: `Database/ERDiagram/water-supply-er-diagram.mmd`
- Create: `Database/README.md`

**Interfaces:**
- SQL table names and columns must match the EF Core entities in Task 5.
- The Mermaid diagram must use the same table and key names as the SQL scripts.

- [ ] **Step 1: Write a schema verification test/script before the implementation.**

Create `Database/SQLScripts/verify-schema.ps1` that reads `002_CreateTables.sql` and asserts the presence of all seven `CREATE TABLE` statements, each `PRIMARY KEY`, and the required `FOREIGN KEY` references. The script must exit with code `1` and a useful message if a table or relationship is missing.

- [ ] **Step 2: Run the verification script to establish RED.**

Run: `powershell -ExecutionPolicy Bypass -File Database/SQLScripts/verify-schema.ps1`  
Expected: RED because `002_CreateTables.sql` does not exist.

- [ ] **Step 3: Implement the SQL scripts and Mermaid diagram.**

Use `INT IDENTITY(1,1)` primary keys, `NVARCHAR` text columns, `DECIMAL(18,2)` money values, `DATE`/`DATETIME2` date values, `CHECK` constraints for positive amounts/status values, and foreign keys with sensible delete behavior. Keep `Notifications` linked to residents and allow service requests to have an optional connection.

- [ ] **Step 4: Run verification and SQL parse checks.**

Run the verification script again.  
Expected: PASS and all seven tables/relationships are reported. If a local SQL Server instance is available, execute the scripts in order and run `SELECT` checks for row counts and foreign-key metadata; otherwise record the local SQL Server prerequisite in `Database/README.md`.

- [ ] **Step 5: Commit.**

```powershell
git add Database
git commit -m "feat: add normalized SQL Server schema and ER diagram"
```

### Task 5: Add EF Core persistence, Identity, and seed data

**Files:**
- Modify: `ASPNETApplication/WaterSupply.Web/Program.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Data/ApplicationDbContext.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Data/DbInitializer.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Data/EntityConfigurations/*.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Models/*.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Migrations/*`
- Create: `ASPNETApplication/WaterSupply.Web.Tests/DatabaseMappingTests.cs`

**Interfaces:**
- `ApplicationDbContext` exposes `DbSet<Resident> Residents`, `DbSet<WaterConnection> WaterConnections`, `DbSet<MeterReading> MeterReadings`, `DbSet<Bill> Bills`, `DbSet<Payment> Payments`, `DbSet<ServiceRequest> ServiceRequests`, and `DbSet<Notification> Notifications`.
- `DbInitializer.InitializeAsync(IServiceProvider)` creates roles/users and idempotent demo records.

- [ ] **Step 1: Write failing mapping tests.**

```csharp
[Fact]
public async Task Sql_model_contains_required_tables_and_relationships()
{
    await using var db = TestDb.CreateContext();
    db.Model.FindEntityType(typeof(Resident))!.FindPrimaryKey().Should().NotBeNull();
    db.Model.FindEntityType(typeof(WaterConnection))!.FindForeignKeys()
      .Should().Contain(f => f.PrincipalEntityType.ClrType == typeof(Resident));
    db.Model.FindEntityType(typeof(Bill))!.FindForeignKeys()
      .Should().Contain(f => f.PrincipalEntityType.ClrType == typeof(WaterConnection));
}
```

- [ ] **Step 2: Run the mapping tests to verify RED.**

Run: `dotnet test ASPNETApplication/WaterSupply.Web.Tests --filter FullyQualifiedName~DatabaseMappingTests`  
Expected: RED because the context and entities are not wired.

- [ ] **Step 3: Implement EF Core entities/configurations and Identity setup.**

Map the domain entities to the seven business tables, enforce required/max-length fields and unique indexes, and configure relationships explicitly in `OnModelCreating`. Store the SQL Server connection string in configuration with a safe development default and environment override.

- [ ] **Step 4: Add and apply the first migration, then implement seeding.**

```powershell
dotnet ef migrations add InitialWaterSupplySchema --project ASPNETApplication/WaterSupply.Web --startup-project ASPNETApplication/WaterSupply.Web
dotnet ef database update --project ASPNETApplication/WaterSupply.Web --startup-project ASPNETApplication/WaterSupply.Web
```

`DbInitializer` must seed `Administrator` and `Resident` roles, one admin, one resident, and linked demo business records without duplicating rows on repeated startup.

- [ ] **Step 5: Run mapping, application, and solution tests.**

Run: `dotnet test ASPNETApplication/WaterSupply.Web.Tests` and then `dotnet test WaterSupplyManagement.sln`  
Expected: all tests pass.

- [ ] **Step 6: Commit.**

```powershell
git add ASPNETApplication/WaterSupply.Web
git add ASPNETApplication/WaterSupply.Web.Tests
git commit -m "feat: add EF Core persistence and Identity seed data"
```

### Task 6: Implement authentication, resident/connection CRUD, and ownership rules

**Files:**
- Create/modify: `ASPNETApplication/WaterSupply.Web/Controllers/AccountController.cs`
- Create/modify: `ASPNETApplication/WaterSupply.Web/Controllers/ResidentsController.cs`
- Create/modify: `ASPNETApplication/WaterSupply.Web/Controllers/WaterConnectionsController.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Controllers/ResidentPortalController.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Models/AccountViewModels/*.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Models/ResidentViewModels/*.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Views/Account/*.cshtml`
- Create: `ASPNETApplication/WaterSupply.Web/Views/Residents/*.cshtml`
- Create: `ASPNETApplication/WaterSupply.Web/Views/WaterConnections/*.cshtml`
- Create: `ASPNETApplication/WaterSupply.Web/Views/Shared/_ValidationScriptsPartial.cshtml`
- Create: `ASPNETApplication/WaterSupply.Web.Tests/AuthorizationTests.cs`
- Create: `ASPNETApplication/WaterSupply.Web.Tests/ResidentCrudTests.cs`

**Interfaces:**
- Account actions: `Login`, `Logout`, `ChangePassword`.
- Administrator CRUD actions: `Index`, `Details`, `Create`, `Edit`, `Delete` for residents and connections.
- Resident portal actions accept no resident identifier for profile/bills; they derive ownership from `UserManager.GetUserId(User)`.

- [ ] **Step 1: Write failing authorization and CRUD tests.**

```csharp
[Fact]
public async Task Resident_cannot_read_another_residents_bill()
{
    var client = await TestServer.CreateAuthenticatedClientAsync("resident-one");
    var response = await client.GetAsync("/ResidentPortal/Bill/999");
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}

[Fact]
public async Task Anonymous_user_is_redirected_to_login()
{
    var client = TestServer.CreateClient();
    var response = await client.GetAsync("/Dashboard");
    response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    response.Headers.Location!.AbsolutePath.Should().Contain("/Account/Login");
}
```

- [ ] **Step 2: Run tests to verify RED.**

Run: `dotnet test ASPNETApplication/WaterSupply.Web.Tests --filter "FullyQualifiedName~AuthorizationTests|FullyQualifiedName~ResidentCrudTests"`  
Expected: RED because the controllers, policies, and test host are not implemented.

- [ ] **Step 3: Implement Identity account flow and authorization policies.**

Use `SignInManager` and `UserManager` for login/logout/password changes. Add `[Authorize(Roles = "Administrator")]` to management controllers and a resident policy that requires an authenticated resident. Add anti-forgery tokens to every POST/PUT/DELETE form.

- [ ] **Step 4: Implement administrator CRUD and resident portal ownership checks.**

Use model binding/view models rather than binding entity keys from untrusted forms. Check that the authenticated resident owns the selected bill/connection before returning data; return `Forbid()` for an authenticated user who requests another resident's record.

- [ ] **Step 5: Run tests to verify GREEN and manually inspect form flows.**

Run the same filtered test command.  
Expected: all authorization/CRUD tests pass. Run `dotnet build WaterSupplyManagement.sln --no-restore` and inspect that login, logout, change-password, create, edit, delete, and search links render in the MVC layout.

- [ ] **Step 6: Commit.**

```powershell
git add ASPNETApplication/WaterSupply.Web ASPNETApplication/WaterSupply.Web.Tests
git commit -m "feat: add authentication and management CRUD"
```

### Task 7: Implement readings, billing, dashboard, reports, and print views

**Files:**
- Create: `ASPNETApplication/WaterSupply.Web/Controllers/MeterReadingsController.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Controllers/BillsController.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Controllers/PaymentsController.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Controllers/ServiceRequestsController.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Controllers/DashboardController.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Controllers/ReportsController.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Services/DashboardQueryService.cs`
- Create: `ASPNETApplication/WaterSupply.Web/Views/Dashboard/Index.cshtml`
- Create: `ASPNETApplication/WaterSupply.Web/Views/Reports/*.cshtml`
- Create: `ASPNETApplication/WaterSupply.Web/Views/Shared/_PrintLayout.cshtml`
- Create: `ASPNETApplication/WaterSupply.Web.Tests/DashboardQueryTests.cs`
- Create: `ASPNETApplication/WaterSupply.Web.Tests/ReportFilterTests.cs`

**Interfaces:**
- `DashboardQueryService.GetSummaryAsync(DateOnly month)` returns residents, active connections, consumption, outstanding amount, collection amount, and open-request counts.
- Report actions accept `from`, `to`, and status filters and return a typed report view model with a `Print` mode.

- [ ] **Step 1: Write failing dashboard and report-filter tests.**

```csharp
[Fact]
public async Task Dashboard_summary_aggregates_current_month_business_metrics()
{
    var summary = await _queries.GetSummaryAsync(new DateOnly(2026, 9, 1));
    summary.ActiveConnectionCount.Should().Be(2);
    summary.UnpaidBillAmount.Should().Be(180.00m);
    summary.OpenServiceRequestCount.Should().Be(1);
}

[Fact]
public async Task Outstanding_bill_report_applies_status_and_date_filters()
{
    var result = await _client.GetAsync("/Reports/OutstandingBills?status=Overdue&from=2026-09-01&to=2026-09-30");
    result.StatusCode.Should().Be(HttpStatusCode.OK);
    (await result.Content.ReadAsStringAsync()).Should().Contain("Overdue");
}
```

- [ ] **Step 2: Run the focused tests to verify RED.**

Run: `dotnet test ASPNETApplication/WaterSupply.Web.Tests --filter "FullyQualifiedName~DashboardQueryTests|FullyQualifiedName~ReportFilterTests"`  
Expected: RED because the dashboard query service and reports do not exist.

- [ ] **Step 3: Implement readings, bill generation, payments, and request status flows.**

Call shared application services from controllers. Validate readings and payments before saving. Refresh bill status after payments and write a notification for newly overdue bills.

- [ ] **Step 4: Implement dashboard aggregation and filtered report queries.**

Use server-side LINQ projection for counts/sums. Never load all records just to filter in memory. Report views must show filter values, totals, and an explicit `Print` button that opens a print-friendly route or uses `window.print()`.

- [ ] **Step 5: Add print CSS and run tests.**

Add `wwwroot/css/print.css` with `@media print` rules that hide navigation, buttons, and filter controls while preserving report headers, table rows, and totals. Run the focused test command and then `dotnet test WaterSupplyManagement.sln`.  
Expected: all tests pass.

- [ ] **Step 6: Commit.**

```powershell
git add ASPNETApplication/WaterSupply.Web ASPNETApplication/WaterSupply.Web.Tests
git commit -m "feat: add billing dashboard and printable reports"
```

### Task 8: Add responsive PWA, offline read-only access, and notifications

**Files:**
- Create: `ASPNETApplication/WaterSupply.Web/wwwroot/manifest.webmanifest`
- Create: `ASPNETApplication/WaterSupply.Web/wwwroot/service-worker.js`
- Create: `ASPNETApplication/WaterSupply.Web/wwwroot/js/pwa-notifications.js`
- Create: `ASPNETApplication/WaterSupply.Web/wwwroot/icons/icon-192.png`
- Create: `ASPNETApplication/WaterSupply.Web/wwwroot/icons/icon-512.png`
- Modify: `ASPNETApplication/WaterSupply.Web/Views/Shared/_Layout.cshtml`
- Modify: `ASPNETApplication/WaterSupply.Web/Views/Dashboard/Index.cshtml`
- Create: `PWA/README.md`
- Create: `ASPNETApplication/WaterSupply.Web.Tests/PwaAssetTests.cs`

**Interfaces:**
- `/manifest.webmanifest` returns valid JSON with `name`, `start_url`, `display`, `icons`, and `theme_color`.
- `/service-worker.js` defines install, activate, and fetch handlers.
- `pwa-notifications.js` exposes `registerPwa()` and `requestBillReminderPermission()`.

- [ ] **Step 1: Write failing asset tests.**

```csharp
[Fact]
public async Task Manifest_declares_installable_application_metadata()
{
    var response = await _client.GetAsync("/manifest.webmanifest");
    response.IsSuccessStatusCode.Should().BeTrue();
    var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    json.RootElement.GetProperty("display").GetString().Should().Be("standalone");
    json.RootElement.GetProperty("icons").GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
}

[Fact]
public async Task Service_worker_contains_offline_shell_and_cache_versioning()
{
    var script = await _client.GetStringAsync("/service-worker.js");
    script.Should().Contain("addEventListener('install'");
    script.Should().Contain("addEventListener('fetch'");
    script.Should().Contain("caches.open");
}
```

- [ ] **Step 2: Run asset tests to verify RED.**

Run: `dotnet test ASPNETApplication/WaterSupply.Web.Tests --filter FullyQualifiedName~PwaAssetTests`  
Expected: RED because the manifest and service worker do not exist.

- [ ] **Step 3: Implement PWA assets and responsive UI.**

Use a cache name such as `water-supply-v1`. Cache CSS/JS/layout assets during install, remove old cache names during activate, and use network-first for dashboard/report responses with a cached fallback. Add a visible offline banner and make tables scrollable on narrow screens. Use the generated icon assets consistently in the manifest.

- [ ] **Step 4: Implement notification permission and reminder behavior.**

Register the service worker on page load. Request permission only from a user-clicked “Enable bill reminders” button. If permission is granted and the dashboard reports unpaid bills, display a local notification; if unsupported or denied, show a non-blocking status message.

- [ ] **Step 5: Run PWA tests and inspect the rendered mobile layout.**

Run the focused PWA test command and then the complete web test project. Open the app at a narrow viewport, verify the install metadata, simulate offline mode, and verify cached read-only dashboard content remains visible while a create/payment action shows the offline message.

- [ ] **Step 6: Commit.**

```powershell
git add ASPNETApplication/WaterSupply.Web PWA
git commit -m "feat: add installable offline-capable PWA experience"
```

### Task 9: Complete documentation and repository structure

**Files:**
- Create: `README.md`
- Create: `Documentation/SRS/WaterSupplyManagement-SRS.md`
- Create: `Documentation/PPT/WaterSupplyManagement-Presentation-Outline.md`
- Create: `Documentation/FinalReport/WaterSupplyManagement-FinalReport-Outline.md`
- Create: `Documentation/PhaseChecklist.md`
- Create: `Documentation/verify-rubric.ps1`
- Create: `PWA/README.md`
- Modify: `Database/README.md`

- [ ] **Step 1: Write the documentation checklist before filling content.**

The checklist must name every rubric item and its file/evidence location: console menu/OOP/inheritance/encapsulation/exceptions/collections/search/reports; authentication; CRUD; dashboard; reports/print; SQL Server; primary/foreign keys; relationships; responsive/installable/service worker/offline/notifications; seven-table schema; and repository folders.

- [ ] **Step 2: Run a checklist verification to establish RED.**

Run: `powershell -ExecutionPolicy Bypass -File Documentation/verify-rubric.ps1`  
Expected: RED because the documentation files and evidence links are not complete.

- [ ] **Step 3: Write setup, usage, SRS, presentation, and final-report documentation.**

`README.md` must include prerequisites, SQL Server setup, migration commands, demo credentials marked for replacement, console run command, web run command, PWA installation instructions, test commands, and repository map. The SRS must include actors, functional requirements, non-functional requirements, use cases, data dictionary, and acceptance criteria. The presentation outline must have phase-by-phase demo slides. The final-report outline must include implementation, screenshots to capture, testing evidence, limitations, and future scope.

- [ ] **Step 4: Run the rubric verifier.**

Run: `powershell -ExecutionPolicy Bypass -File Documentation/verify-rubric.ps1`  
Expected: PASS with every required item linked to a concrete source file or documented manual verification.

- [ ] **Step 5: Commit.**

```powershell
git add README.md Documentation PWA/README.md Database/README.md
git commit -m "docs: add project documentation and rubric checklist"
```

### Task 10: Run final verification and package the project

**Files:**
- Create: `Documentation/VerificationResults.md`

- [ ] **Step 1: Run formatting and static checks.**

Run: `dotnet format WaterSupplyManagement.sln --verify-no-changes`  
Expected: no formatting changes required. If the command reports formatting differences, apply `dotnet format WaterSupplyManagement.sln` and inspect the diff before continuing.

- [ ] **Step 2: Run the complete test suite.**

Run: `dotnet test WaterSupplyManagement.sln --configuration Release`  
Expected: every test passes with zero failures.

- [ ] **Step 3: Run the complete build.**

Run: `dotnet build WaterSupplyManagement.sln --configuration Release --no-restore`  
Expected: every project builds with zero errors.

- [ ] **Step 4: Run the console smoke test, schema verifier, and rubric verifier.**

Run each command separately:

```powershell
dotnet run --project ConsoleApplication/WaterSupply.Console --configuration Release --no-build < ConsoleApplication/WaterSupply.Console.Tests/Fixtures/smoke-input.txt
powershell -ExecutionPolicy Bypass -File Database/SQLScripts/verify-schema.ps1
powershell -ExecutionPolicy Bypass -File Documentation/verify-rubric.ps1
```

Expected: the console reaches exit normally and each verifier reports PASS.

- [ ] **Step 5: Record evidence.**

Write `Documentation/VerificationResults.md` with the exact date, SDK version, test/build commands, pass counts, manual browser checks, and any local SQL Server prerequisites. Do not claim SQL execution passed unless a SQL Server instance actually executed the scripts.

- [ ] **Step 6: Commit the verified deliverable.**

```powershell
git add Documentation/VerificationResults.md
git commit -m "chore: record final project verification"
```

- [ ] **Step 7: Inspect repository status and create the handoff summary.**

Run: `git status --short --branch` and `git log --oneline --decorate -8`.  
Expected: the intended source/documentation files are committed, generated build output is ignored, and the final response links to `outputs/WaterSupplyManagementSystem/README.md` plus the specification and verification results.
