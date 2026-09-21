# Basic Water Supply Management System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (\`- [ ]\`) syntax for tracking.

**Goal:** Replace the oversized water-supply project with a small, readable three-phase .NET project that still satisfies every mandatory course guideline.

**Architecture:** Keep the existing solution and repository folders. Use a small shared domain model for the console and web layers, an in-memory store for the console, EF Core plus Identity for the web application, and a minimal PWA shell in \`wwwroot\`. Use five business tables only: Residents, WaterConnections, MeterReadings, Bills, and ServiceRequests.

**Tech Stack:** .NET 10, C#, ASP.NET Core MVC, Razor views, ASP.NET Core Identity, EF Core SQL Server, Bootstrap, JavaScript service worker, xUnit, FluentAssertions, and PowerShell verification scripts.

**Spec:** \`docs/superpowers/specs/2026-09-21-basic-water-supply-management-design.md\`

## Global Constraints

- Target framework remains .NET 10 (\`net10.0\`).
- The system manages only residents, water connections, meter readings, simple bills, and service requests.
- The SQL Server business schema contains exactly five meaningful tables; Identity tables are separate.
- Authentication includes login, logout, and change password.
- The console must demonstrate menu navigation, OOP, inheritance, encapsulation, exceptions, collections, search, and reports.
- The PWA must be responsive, installable, service-worker-backed, offline-capable for read-only pages, and notification-capable.
- Do not add payment processing, stored notifications, external services, resident portal flows, GIS, or background jobs.
- Production behavior must be implemented after a focused failing test except for static configuration and documentation changes.

## Review Focus

- A meter reading with \`CurrentReading < PreviousReading\` must be rejected; pin this in \`Shared/WaterSupply.Application.Tests/BasicDomainTests.cs\`.
- A duplicate connection number or duplicate reading date must not create a second record; pin this in \`Shared/WaterSupply.Application.Tests/BasicDomainTests.cs\` and \`ASPNETApplication/WaterSupply.Web.Tests/DatabaseMappingTests.cs\`.
- An unauthenticated user must be redirected to login and a resident must not access another resident's request; pin this in \`ASPNETApplication/WaterSupply.Web.Tests/BasicWebFlowTests.cs\`.
- An offline browser must receive cached read-only content while write actions display an offline message; pin this in \`ASPNETApplication/WaterSupply.Web.Tests/PwaAssetTests.cs\`.
- The SQL model must expose five business tables and all required foreign keys; pin this in \`ASPNETApplication/WaterSupply.Web.Tests/DatabaseMappingTests.cs\` and \`Database/SQLScripts/verify-schema.ps1\`.

---

### Task 1: Replace the shared model with five simple domain entities

**Files:**
- Modify: \`Shared/WaterSupply.Domain/Entities/Person.cs\`
- Modify: \`Shared/WaterSupply.Domain/Entities/Resident.cs\`
- Modify: \`Shared/WaterSupply.Domain/Entities/WaterConnection.cs\`
- Modify: \`Shared/WaterSupply.Domain/Entities/MeterReading.cs\`
- Modify: \`Shared/WaterSupply.Domain/Entities/Bill.cs\`
- Modify: \`Shared/WaterSupply.Domain/Entities/ServiceRequest.cs\`
- Modify: \`Shared/WaterSupply.Domain/Enums/DomainEnums.cs\`
- Modify: \`Shared/WaterSupply.Domain/Exceptions/DomainValidationException.cs\`
- Modify: \`Shared/WaterSupply.Application/Abstractions/ServiceInterfaces.cs\`
- Modify: \`Shared/WaterSupply.Application/InMemory/InMemoryRepository.cs\`
- Modify: \`Shared/WaterSupply.Application/Services/ResidentService.cs\`
- Modify: \`Shared/WaterSupply.Application/Services/WaterConnectionService.cs\`
- Modify: \`Shared/WaterSupply.Application/Services/MeterReadingService.cs\`
- Modify: \`Shared/WaterSupply.Application/Services/BillingService.cs\`
- Modify: \`Shared/WaterSupply.Application/Services/ServiceRequestService.cs\`
- Modify: \`Shared/WaterSupply.Application/Services/ReportService.cs\`
- Create: \`Shared/WaterSupply.Application.Tests/BasicDomainTests.cs\`
- Delete: \`Shared/WaterSupply.Domain/Entities/Entity.cs\`
- Delete: \`Shared/WaterSupply.Domain/Entities/Payment.cs\`
- Delete: \`Shared/WaterSupply.Domain/Entities/Notification.cs\`
- Delete: \`Shared/WaterSupply.Domain/Entities/StaffUser.cs\`
- Delete: \`Shared/WaterSupply.Application.Tests/BillingServiceTests.cs\`
- Delete: \`Shared/WaterSupply.Application.Tests/DomainValidationTests.cs\`
- Delete: \`Shared/WaterSupply.Application.Tests/SearchAndReportTests.cs\`

**Interfaces:**
- \`Resident : Person\` exposes \`ResidentId\`, \`Address\`, \`RegistrationDate\`, and \`IsActive\`.
- \`WaterConnection\` exposes \`WaterConnectionId\`, \`ResidentId\`, \`ConnectionNumber\`, \`MeterNumber\`, \`ConnectionType\`, \`Status\`, and \`ConnectionDate\`.
- \`MeterReading\` exposes \`MeterReadingId\`, \`WaterConnectionId\`, \`ReadingDate\`, \`PreviousReading\`, \`CurrentReading\`, \`Consumption\`, and \`RecordReading(previous, current)\`.
- \`Bill\` exposes \`BillId\`, \`WaterConnectionId\`, \`MeterReadingId\`, \`BillDate\`, \`UnitsConsumed\`, \`RatePerUnit\`, \`TotalAmount\`, \`DueDate\`, \`Status\`, and \`CalculateTotal()\`.
- \`ServiceRequest\` exposes \`ServiceRequestId\`, \`ResidentId\`, nullable \`WaterConnectionId\`, \`RequestType\`, \`Description\`, \`CreatedAt\`, \`Status\`, \`StaffNotes\`, and \`ChangeStatus(status, notes)\`.
- \`InMemoryWaterSupplyStore\` owns \`List<Resident> Residents\`, \`List<WaterConnection> Connections\`, \`List<MeterReading> Readings\`, \`List<Bill> Bills\`, and \`List<ServiceRequest> ServiceRequests\`.
- \`ResidentService.Search(string query)\` performs case-insensitive name, email, or phone matching.
- \`ReportService.GetSummary()\` returns a \`SummaryReport\` record with resident count, active connection count, total consumption, total bill amount, and open request count.

- [ ] **Step 1: Write the failing domain tests.**

Replace the three old application test files with tests in \`BasicDomainTests.cs\` covering these exact behaviors:

~~~
[Fact]
public void Reading_rejects_current_value_below_previous_value()
{
    var reading = new MeterReading(1, DateOnly.FromDateTime(DateTime.Today));
    Assert.Throws<DomainValidationException>(() => reading.RecordReading(100, 90));
}

[Fact]
public void Bill_calculates_total_from_units_and_rate()
{
    var bill = new Bill(1, 1, DateOnly.FromDateTime(DateTime.Today), 12, 25m);
    bill.CalculateTotal();
    bill.TotalAmount.Should().Be(300m);
}

[Fact]
public void Resident_search_matches_name_without_case_sensitivity()
{
    var service = new ResidentService(new InMemoryWaterSupplyStore
    {
        Residents = { new Resident("Asha Kumar", "asha@example.com", "9999999999", "Main Road") }
    });
    service.Search("asha").Should().ContainSingle();
}

[Fact]
public void Summary_counts_only_open_service_requests()
{
    var store = new InMemoryWaterSupplyStore();
    store.ServiceRequests.Add(new ServiceRequest(1, null, "Leak", "Tap leak"));
    store.ServiceRequests.Add(new ServiceRequest(1, null, "Repair", "Closed repair") { Status = RequestStatus.Closed });
    new ReportService(store).GetSummary().OpenRequests.Should().Be(1);
}
~~~

- [ ] **Step 2: Run the focused tests and confirm the expected RED state.**

Run:

~~~
dotnet test Shared/WaterSupply.Application.Tests/WaterSupply.Application.Tests.csproj --filter FullyQualifiedName~BasicDomainTests
~~~

Expected: the new tests fail to compile or fail because the simplified constructors, methods, and summary type do not yet exist. Do not keep any unrelated old test failures hidden.

- [ ] **Step 3: Implement the minimal model and services.**

Use private setters, public constructors for valid objects, and \`DomainValidationException\` for invalid values. Define only these enum values: \`ConnectionType.Residential\`, \`ConnectionType.Commercial\`, \`ConnectionStatus.Active\`, \`ConnectionStatus.Inactive\`, \`BillStatus.Unpaid\`, \`BillStatus.Paid\`, \`RequestStatus.Open\`, \`RequestStatus.InProgress\`, \`RequestStatus.Closed\`, \`RequestType.Leak\`, \`RequestType.NoSupply\`, and \`RequestType.Other\`. Use \`List<T>\` for records and \`Dictionary<int, T>\` only where keyed lookup is needed.

- [ ] **Step 4: Run the focused tests and then the shared test project.**

~~~
dotnet test Shared/WaterSupply.Application.Tests/WaterSupply.Application.Tests.csproj --filter FullyQualifiedName~BasicDomainTests
dotnet test Shared/WaterSupply.Application.Tests/WaterSupply.Application.Tests.csproj
~~~

Expected: all focused tests and all remaining shared tests pass with zero failures.

- [ ] **Step 5: Commit the shared simplification.**

~~~
git add Shared
git commit -m "refactor: simplify shared water supply domain"
~~~

### Task 2: Reduce the console application to eight readable menu options

**Files:**
- Modify: \`ConsoleApplication/WaterSupply.Console/Program.cs\`
- Modify: \`ConsoleApplication/WaterSupply.Console/Menu/ConsoleMenu.cs\`
- Modify: \`ConsoleApplication/WaterSupply.Console/Menu/InputReader.cs\`
- Modify: \`ConsoleApplication/WaterSupply.Console/Menu/InputValidationException.cs\`
- Modify: \`ConsoleApplication/WaterSupply.Console/Seed/ConsoleSeedData.cs\`
- Modify: \`ConsoleApplication/WaterSupply.Console.Tests/MenuSmokeTests.cs\`
- Modify: \`ConsoleApplication/WaterSupply.Console.Tests/InputReaderTests.cs\`

**Interfaces:**
- \`ConsoleMenu.Run()\` prints the exact options \`1\` through \`8\` and \`0\` from the approved specification.
- Option \`3\` searches residents by name, email, or phone.
- Option \`5\` records a reading and automatically creates one simple bill.
- Option \`8\` prints \`Summary Report\` and the five summary values returned by \`ReportService\`.
- Invalid numeric input is caught by \`InputReader\` and returns the user to the menu without terminating the process.

- [ ] **Step 1: Replace the smoke tests with the basic menu contract.**

Add tests that feed \`0\` and assert the menu includes \`Add resident\`, \`Search resident\`, \`Record meter reading\`, \`View bills\`, \`Add service request\`, \`View summary report\`, and \`Exit\`. Keep the input-reader test that proves \`abc\` is rejected without throwing out of the application.

- [ ] **Step 2: Run the console tests and confirm RED.**

~~~
dotnet test ConsoleApplication/WaterSupply.Console.Tests/WaterSupply.Console.Tests.csproj
~~~

Expected: the menu assertion fails because the old menu still exposes advanced operations or uses different labels.

- [ ] **Step 3: Implement the small menu and demo seed.**

Use one \`InMemoryWaterSupplyStore\`, one \`ResidentService\`, one \`WaterConnectionService\`, one \`MeterReadingService\`, one \`ServiceRequestService\`, and one \`ReportService\`. Seed two residents, two connections, two readings, two bills, and one open request. Each handler should validate input, call one service method, print a short result, and catch \`DomainValidationException\`.

- [ ] **Step 4: Run the console tests and a scripted smoke test.**

~~~
dotnet test ConsoleApplication/WaterSupply.Console.Tests/WaterSupply.Console.Tests.csproj
dotnet run --project ConsoleApplication/WaterSupply.Console/WaterSupply.Console.csproj --no-restore
~~~

Enter \`0\` at the prompt. Expected: the menu prints once, exits cleanly, and tests pass.

- [ ] **Step 5: Commit the console simplification.**

~~~
git add ConsoleApplication
git commit -m "refactor: simplify console menus and reports"
~~~

### Task 3: Reduce the SQL Server and EF Core model to five business tables

**Files:**
- Modify: \`ASPNETApplication/WaterSupply.Web/Data/ApplicationDbContext.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Data/DbInitializer.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Program.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/appsettings.json\`
- Modify: \`ASPNETApplication/WaterSupply.Web.Tests/DatabaseMappingTests.cs\`
- Modify: \`Database/SQLScripts/001_CreateDatabase.sql\`
- Modify: \`Database/SQLScripts/002_CreateTables.sql\`
- Modify: \`Database/SQLScripts/003_SeedReferenceData.sql\`
- Modify: \`Database/SQLScripts/004_SeedDemoData.sql\`
- Modify: \`Database/SQLScripts/verify-schema.ps1\`
- Modify: \`Database/ERDiagram/water-supply-er-diagram.mmd\`
- Modify: \`Database/README.md\`
- Delete: \`ASPNETApplication/WaterSupply.Web/Migrations/20260920123614_InitialWaterSupplySchema.cs\`
- Delete: \`ASPNETApplication/WaterSupply.Web/Migrations/20260920123614_InitialWaterSupplySchema.Designer.cs\`
- Delete: \`ASPNETApplication/WaterSupply.Web/Migrations/ApplicationDbContextModelSnapshot.cs\`

**Interfaces:**
- \`ApplicationDbContext\` exposes exactly \`DbSet<Resident> Residents\`, \`DbSet<WaterConnection> WaterConnections\`, \`DbSet<MeterReading> MeterReadings\`, \`DbSet<Bill> Bills\`, and \`DbSet<ServiceRequest> ServiceRequests\` in addition to Identity tables.
- \`ApplicationDbContext.OnModelCreating\` defines required foreign keys and unique indexes on \`ConnectionNumber\` and \`(WaterConnectionId, ReadingDate)\`.
- \`001_CreateDatabase.sql\` creates \`WaterSupplyManagement\` only.
- \`002_CreateTables.sql\` creates exactly the five business tables with primary keys, foreign keys, and check constraints for non-negative readings and amounts.
- \`004_SeedDemoData.sql\` inserts two residents, two connections, two readings, two bills, and one service request idempotently.

- [ ] **Step 1: Write the failing mapping test.**

Replace \`DatabaseMappingTests.cs\` with a test that creates the EF model and asserts the five business entity types are present, \`WaterConnection.ResidentId\` is a foreign key, \`MeterReading.WaterConnectionId\` is a foreign key, \`Bill.MeterReadingId\` is a foreign key, and \`ServiceRequest.ResidentId\` is a foreign key. Assert that \`Payment\`, \`Notification\`, and any old business entity are absent.

- [ ] **Step 2: Run the mapping test and confirm RED.**

~~~
dotnet test ASPNETApplication/WaterSupply.Web.Tests/WaterSupply.Web.Tests.csproj --filter FullyQualifiedName~DatabaseMappingTests
~~~

Expected: the test fails because the current EF model still exposes payment/notification relationships and the old seven-table mapping.

- [ ] **Step 3: Implement the five-table EF model and scripts.**

Map enum values as strings for readability. Keep Identity configuration and the existing SQL Server provider. Recreate the initial migration from the final model with:

~~~
dotnet ef migrations add BasicSchema --project ASPNETApplication/WaterSupply.Web/WaterSupply.Web.csproj --startup-project ASPNETApplication/WaterSupply.Web/WaterSupply.Web.csproj --output-dir Migrations
~~~

The initializer should seed one administrator, one resident login, and the matching resident record without adding any payment or notification records.

- [ ] **Step 4: Run EF/model tests and static schema verification.**

~~~
dotnet test ASPNETApplication/WaterSupply.Web.Tests/WaterSupply.Web.Tests.csproj --filter FullyQualifiedName~DatabaseMappingTests
pwsh -NoProfile -File Database/SQLScripts/verify-schema.ps1 -SqlFile Database/SQLScripts/002_CreateTables.sql
~~~

Expected: the EF test passes and the verifier reports five business tables with the required keys/relationships.

- [ ] **Step 5: Commit the database simplification.**

~~~
git add ASPNETApplication/WaterSupply.Web/Data ASPNETApplication/WaterSupply.Web/Program.cs ASPNETApplication/WaterSupply.Web/appsettings.json ASPNETApplication/WaterSupply.Web/Migrations Database
git commit -m "refactor: reduce database to five business tables"
~~~

### Task 4: Replace the web workflows with basic MVC pages

**Files:**
- Modify: \`ASPNETApplication/WaterSupply.Web/Controllers/AccountController.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Controllers/DashboardController.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Controllers/ResidentsController.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Controllers/WaterConnectionsController.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Controllers/MeterReadingsController.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Controllers/BillsController.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Controllers/ServiceRequestsController.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Controllers/ReportsController.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Services/DashboardQueryService.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Models/DashboardSummaryViewModel.cs\`
- Modify: models under \`ASPNETApplication/WaterSupply.Web/Models\` for residents, connections, readings, bills, service requests, and reports
- Modify: \`ASPNETApplication/WaterSupply.Web/Views/Shared/_Layout.cshtml\`
- Modify: views under \`ASPNETApplication/WaterSupply.Web/Views/Residents\`, \`WaterConnections\`, \`MeterReadings\`, \`Bills\`, and \`ServiceRequests\`
- Create: \`ASPNETApplication/WaterSupply.Web.Tests/BasicWebFlowTests.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web.Tests/AuthorizationTests.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web.Tests/ResidentCrudTests.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web.Tests/ResidentOwnershipTests.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web.Tests/DashboardQueryTests.cs\`
- Modify: \`ASPNETApplication/WaterSupply.Web.Tests/ReportFilterTests.cs\`
- Delete: \`ASPNETApplication/WaterSupply.Web/Controllers/PaymentsController.cs\`
- Delete: \`ASPNETApplication/WaterSupply.Web/Controllers/ResidentPortalController.cs\`
- Delete: all files under \`ASPNETApplication/WaterSupply.Web/Views/Payments\`
- Delete: all files under \`ASPNETApplication/WaterSupply.Web/Views/ResidentPortal\`
- Delete: \`ASPNETApplication/WaterSupply.Web/Models/ResidentPortalViewModel.cs\`

**Interfaces:**
- \`ResidentsController\` supports \`Index(search)\`, \`Create\`, \`Edit\`, \`Delete\`, and \`Details\` for administrators.
- \`WaterConnectionsController\` supports \`Index(search)\`, \`Create\`, \`Edit\`, \`Delete\`, and \`Details\` for administrators.
- \`MeterReadingsController.Create\` accepts a connection id and current reading, validates the previous reading, and creates the matching simple bill.
- \`BillsController.Index\` lists bills and supports a status filter; it does not accept payments.
- \`ServiceRequestsController\` allows a resident to create a request and an administrator to change its status.
- \`ReportsController.Index\` accepts optional \`status\`, \`from\`, and \`to\` query parameters and returns a single printable report.
- \`DashboardSummaryViewModel\` contains only \`ResidentCount\`, \`ActiveConnectionCount\`, \`TotalConsumption\`, \`TotalBillAmount\`, and \`OpenRequestCount\`.

- [ ] **Step 1: Write failing web-flow tests.**

Create \`BasicWebFlowTests.cs\` with tests for anonymous dashboard redirection, resident search, and report filtering. Keep the ownership tests and rewrite them to use only \`ServiceRequest\` and \`Resident\` records.

~~~
[Fact]
public async Task Anonymous_dashboard_request_redirects_to_login()
{
    using var factory = new TestWebApplicationFactory();
    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    var response = await client.GetAsync("/Dashboard");
    response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    response.Headers.Location!.ToString().Should().Contain("/Account/Login");
}
~~~

- [ ] **Step 2: Run the web-flow tests and confirm RED.**

~~~
dotnet test ASPNETApplication/WaterSupply.Web.Tests/WaterSupply.Web.Tests.csproj --filter FullyQualifiedName~BasicWebFlowTests
~~~

Expected: the tests fail because the old controllers, routes, and view models do not match the basic routes.

- [ ] **Step 3: Implement only the approved controllers and views.**

Use \`[Authorize]\` on all management actions, \`[Authorize(Roles = "Administrator")]\` on administrator-only mutations, anti-forgery validation on POST actions, and a resident-id ownership predicate on resident request queries. Keep the account controller’s login/logout/change-password actions. Use simple table views with one search/filter form and Bootstrap buttons.

- [ ] **Step 4: Run all web tests and verify the route surface.**

~~~
dotnet test ASPNETApplication/WaterSupply.Web.Tests/WaterSupply.Web.Tests.csproj
rg 'Payments|ResidentPortal|Payment|Notification' ASPNETApplication/WaterSupply.Web --glob '!obj/**'
~~~

Expected: all tests pass and the final \`rg\` command returns no obsolete workflow references.

- [ ] **Step 5: Commit the web simplification.**

~~~
git add ASPNETApplication/WaterSupply.Web ASPNETApplication/WaterSupply.Web.Tests
git commit -m "refactor: simplify ASP.NET workflows"
~~~

### Task 5: Keep only a basic responsive PWA shell

**Files:**
- Modify: \`ASPNETApplication/WaterSupply.Web/wwwroot/manifest.webmanifest\`
- Modify: \`ASPNETApplication/WaterSupply.Web/wwwroot/service-worker.js\`
- Modify: \`ASPNETApplication/WaterSupply.Web/wwwroot/offline.html\`
- Modify: \`ASPNETApplication/WaterSupply.Web/wwwroot/js/pwa-notifications.js\`
- Modify: \`ASPNETApplication/WaterSupply.Web/wwwroot/js/site.js\`
- Modify: \`ASPNETApplication/WaterSupply.Web/wwwroot/css/site.css\`
- Modify: \`ASPNETApplication/WaterSupply.Web/Views/Shared/_Layout.cshtml\`
- Modify: \`ASPNETApplication/WaterSupply.Web.Tests/PwaAssetTests.cs\`
- Modify: \`PWA/README.md\`

**Interfaces:**
- Manifest declares \`name\`, \`short_name\`, \`start_url\`, \`display: standalone\`, theme/background colors, and 192px/512px icons.
- Service worker caches \`/\`, \`/Home/Index\`, \`/Residents\`, \`/offline.html\`, CSS, JavaScript, and icons during install.
- GET requests use network-first with a cached fallback; POST/PUT/DELETE requests are not queued and show the offline page/message.
- Notification JavaScript requests permission only after clicking \`Enable notifications\` and shows one local “Water supply reminder” notification when supported.

- [ ] **Step 1: Rewrite the PWA tests first.**

Assert the manifest has the required install fields and the service worker contains \`install\`, \`activate\`, \`fetch\`, cache fallback, and a non-GET guard. Run:

~~~
dotnet test ASPNETApplication/WaterSupply.Web.Tests/WaterSupply.Web.Tests.csproj --filter FullyQualifiedName~PwaAssetTests
~~~

Expected: tests fail if any old asset behavior is missing.

- [ ] **Step 2: Implement the minimal manifest, service worker, offline page, and notification button.**

Keep the existing SVG icons. Add one offline status banner to \`_Layout.cshtml\` and link only the required scripts/styles.

- [ ] **Step 3: Run PWA tests and commit.**

~~~
dotnet test ASPNETApplication/WaterSupply.Web.Tests/WaterSupply.Web.Tests.csproj --filter FullyQualifiedName~PwaAssetTests
git add ASPNETApplication/WaterSupply.Web/wwwroot ASPNETApplication/WaterSupply.Web/Views/Shared/_Layout.cshtml ASPNETApplication/WaterSupply.Web.Tests/PwaAssetTests.cs PWA/README.md
git commit -m "refactor: keep a basic offline PWA shell"
~~~

### Task 6: Rewrite documentation and clean repository noise

**Files:**
- Modify: \`README.md\`
- Modify: \`Documentation/SRS/SoftwareRequirementsSpecification.md\`
- Modify: \`Documentation/PhaseChecklist.md\`
- Modify: \`Documentation/PPT/PresentationOutline.md\`
- Modify: \`Documentation/FinalReport/FinalReportOutline.md\`
- Modify: \`Documentation/VerificationResults.md\`
- Modify: \`Database/README.md\`
- Modify: \`PWA/README.md\`
- Modify: \`.gitignore\`

**Interfaces:**
- Every documentation file names only the five business tables and the eight console menu options.
- README commands match the final solution and do not require a running SQL Server for build-only tests.
- \`.gitignore\` includes \`SQLEXPR_x64_ENU/\`, \`work/\`, \`bin/\`, and \`obj/\` so installer extraction cannot pollute the repository.

- [ ] **Step 1: Search documentation for obsolete scope.**

~~~
rg 'Payments|Payment|Notifications|Notification|ResidentPortal|seven tables|seven normalized|multi-tenant' README.md Documentation Database PWA docs --glob '*.md' --glob '*.mmd'
~~~

Expected before the edits: matches exist in the old documentation, proving the documentation task is needed.

- [ ] **Step 2: Update the documentation to the approved basic scope.**

Use one short feature table in the README, list the five database tables, show the console menu, show the demo credentials, and document that SQL Server is required only for live web execution.

- [ ] **Step 3: Verify documentation consistency and commit.**

~~~
rg 'Payments|Payment|Notifications|Notification|ResidentPortal|seven tables|seven normalized|multi-tenant' README.md Documentation Database PWA docs --glob '*.md' --glob '*.mmd'
git diff --check
git add README.md Documentation Database/README.md PWA/README.md .gitignore
git commit -m "docs: document basic project scope"
~~~

Expected: the obsolete-scope search returns no matches and \`git diff --check\` is clean.

### Task 7: Run complete verification and package the simplified project

**Files:**
- Modify: \`Documentation/VerificationResults.md\` with fresh command output and counts.
- Modify: \`Documentation/verify-rubric.ps1\` only if its checks reference deleted workflows.
- Modify: \`Database/SQLScripts/verify-schema.ps1\` only if its five-table assertions need alignment.

**Interfaces:**
- The full solution must restore, test, and build without a live SQL Server.
- The console must exit cleanly when given \`0\`.
- The static rubric verifier must confirm console, ASP.NET, PWA, database, and repository requirements.

- [ ] **Step 1: Run the full test suite.**

~~~
dotnet restore WaterSupplyManagement.sln
dotnet test WaterSupplyManagement.sln --no-restore
~~~

Expected: all test projects pass with zero failures and zero compile errors.

- [ ] **Step 2: Run the Release build.**

~~~
dotnet build WaterSupplyManagement.sln --configuration Release --no-restore
~~~

Expected: exit code \`0\` with zero errors.

- [ ] **Step 3: Run console, schema, rubric, and stale-reference checks.**

~~~
"0" | dotnet run --project ConsoleApplication/WaterSupply.Console/WaterSupply.Console.csproj --no-build
pwsh -NoProfile -File Database/SQLScripts/verify-schema.ps1 -SqlFile Database/SQLScripts/002_CreateTables.sql
pwsh -NoProfile -File Documentation/verify-rubric.ps1
rg 'Payments|Payment|Notifications|Notification|ResidentPortal|StaffUser' . --glob '!**/bin/**' --glob '!**/obj/**' --glob '!SQLEXPR_x64_ENU/**'
~~~

Expected: the console exits, static verifiers pass, and obsolete production references are absent.

- [ ] **Step 4: Update verification results with actual output.**

Record the exact test count, build result, console smoke result, schema table count, rubric result, and any limitation that requires live SQL Server. Do not claim database runtime success without a successful SQL Server connection.

- [ ] **Step 5: Review the final diff and commit the verification record.**

~~~
git diff --check
git status --short
git add Documentation/VerificationResults.md Documentation/verify-rubric.ps1 Database/SQLScripts/verify-schema.ps1
git commit -m "chore: verify simplified project"
~~~

## Plan self-review

- Spec coverage: Tasks 1–2 cover the console OOP requirements; Task 3 covers the five-table SQL Server schema; Task 4 covers authentication, CRUD, dashboard, reports, and print output; Task 5 covers responsive/installable/offline/notification PWA behavior; Tasks 6–7 cover repository structure, documentation, and verification.
- Placeholder scan: no step depends on a future decision; every task names files, interfaces, commands, and expected outcomes.
- Type consistency: \`SummaryReport\`, \`RequestStatus\`, \`InMemoryWaterSupplyStore\`, \`ResidentService.Search\`, and \`ReportService.GetSummary\` are introduced in Task 1 and consumed only by later tasks.
- Review focus coverage: each of the five review risks has a named test or verifier in the task that owns the behavior.
- Deliberate limitation: live SQL Server execution is verified only when an accessible SQL Server instance exists; compile-time, EF-model, and static-schema verification remain mandatory.

