# Water Supply Management System Design Specification

**Date:** 2026-09-20  
**Project:** Water Supply Management System  
**Target framework:** .NET 10 (`net10.0`)  
**Audience:** College project team building the three required phases

## 1. Purpose and scope

The Water Supply Management System will manage residents, water connections, meter readings, bills, payments, and service requests for a small municipal water-supply office. It will be delivered in three connected phases:

1. A console application that demonstrates object-oriented programming and reusable business logic.
2. An ASP.NET Core MVC web application backed by SQL Server with authentication, dashboards, CRUD management, and reports.
3. A Progressive Web Application experience layered onto the ASP.NET application for responsive mobile use, installation, offline access, and browser notifications.

The system is intentionally sized for an academic project. It provides real end-to-end flows without adding unrelated concerns such as online payment-gateway integration, GIS mapping, or multi-tenant billing.

## 2. Users and permissions

### Administrator

- Logs in, logs out, and changes password.
- Creates, searches, updates, and deletes resident records.
- Creates and maintains water connections.
- Records and reviews meter readings.
- Generates bills and records payments.
- Reviews and updates service-request status.
- Views dashboard metrics and printable reports.

### Resident

- Logs in, logs out, and changes password.
- Views their own resident profile, connection, readings, bills, payments, and service requests.
- Searches their own bills and requests.
- Creates service requests and views request status.
- Receives an in-app/browser reminder for unpaid bills when notification permission is granted.

Authorization will be enforced at the controller/action level and repeated in service methods for operations that accept an authenticated-user identifier. A resident must never be able to query or mutate another resident's records by changing an identifier in a URL.

## 3. Solution architecture

The repository will contain one .NET solution with a shared class library and two runnable application projects:

```text
WaterSupplyManagementSystem/
├── WaterSupplyManagement.sln
├── ConsoleApplication/
│   ├── WaterSupply.Console/
│   └── WaterSupply.Console.Tests/
├── Shared/
│   ├── WaterSupply.Domain/
│   ├── WaterSupply.Application/
│   └── WaterSupply.Application.Tests/
├── ASPNETApplication/
│   ├── WaterSupply.Web/
│   └── WaterSupply.Web.Tests/
├── PWA/
│   ├── manifest.webmanifest
│   ├── service-worker.js
│   └── pwa-notifications.js
├── Database/
│   ├── SQLScripts/
│   │   ├── 001_CreateDatabase.sql
│   │   ├── 002_CreateTables.sql
│   │   ├── 003_SeedReferenceData.sql
│   │   └── 004_SeedDemoData.sql
│   └── ERDiagram/
│       └── water-supply-er-diagram.mmd
├── Documentation/
│   ├── SRS/
│   ├── PPT/
│   └── FinalReport/
├── docs/superpowers/specs/
├── README.md
└── .gitignore
```

`WaterSupply.Domain` will contain entities, value objects, enumerations, and domain exceptions. `WaterSupply.Application` will contain service interfaces, validation, report models, and in-memory implementations used by the console phase and tests. `WaterSupply.Web` will contain EF Core persistence, Identity configuration, MVC controllers/views, dashboard queries, and PWA static assets. The web layer will depend on the application layer; views will not contain business rules.

## 4. Domain model and database design

The business schema uses seven tables. ASP.NET Core Identity tables are managed separately by Identity migrations and are not counted as business tables.

### Residents

- `ResidentId` — primary key.
- `IdentityUserId` — nullable unique link to the Identity user for residents who can log in.
- `FullName`, `Email`, `Phone`, `Address` — required contact details.
- `RegistrationDate`, `IsActive` — lifecycle fields.

### WaterConnections

- `WaterConnectionId` — primary key.
- `ResidentId` — foreign key to `Residents`.
- `ConnectionNumber` — required unique public identifier.
- `ConnectionType` — residential or commercial.
- `MeterNumber`, `ConnectionDate`, `Status` — connection details.

### MeterReadings

- `MeterReadingId` — primary key.
- `WaterConnectionId` — foreign key to `WaterConnections`.
- `ReadingDate`, `PreviousReading`, `CurrentReading`, `Consumption` — measurement fields.
- Unique constraint on `(WaterConnectionId, ReadingDate)`.

### Bills

- `BillId` — primary key.
- `WaterConnectionId` — foreign key to `WaterConnections`.
- `BillingPeriodStart`, `BillingPeriodEnd`, `UnitsConsumed`, `RatePerUnit`, `FixedCharge`, `TaxAmount`, `TotalAmount`, `DueDate`.
- `Status` — `Unpaid`, `PartiallyPaid`, `Paid`, or `Overdue`.
- Unique constraint on `(WaterConnectionId, BillingPeriodStart, BillingPeriodEnd)`.

### Payments

- `PaymentId` — primary key.
- `BillId` — foreign key to `Bills`.
- `PaymentDate`, `Amount`, `PaymentMethod`, `ReferenceNumber`.
- Unique payment reference number.

### ServiceRequests

- `ServiceRequestId` — primary key.
- `ResidentId` — foreign key to `Residents`.
- `WaterConnectionId` — nullable foreign key to `WaterConnections`.
- `RequestType`, `Description`, `CreatedAt`, `ResolvedAt`, `Status`, `StaffNotes`.

### Notifications

- `NotificationId` — primary key.
- `ResidentId` — foreign key to `Residents`.
- `Title`, `Message`, `NotificationType`, `CreatedAt`, `ReadAt`.

Relationships:

```text
Resident 1 ──── * WaterConnection 1 ──── * MeterReading
WaterConnection 1 ──── * Bill 1 ──── * Payment
Resident 1 ──── * ServiceRequest
WaterConnection 0..1 ──── * ServiceRequest
Resident 1 ──── * Notification
```

Domain invariants include non-negative meter readings, current readings not lower than previous readings, positive payment amounts, payments not exceeding bill balance, due dates not preceding billing-period end, and only valid status transitions.

## 5. Phase 1 console behavior

The console app will start with a main menu:

```text
1. Manage residents
2. Manage water connections
3. Record meter reading
4. Generate and view bills
5. Record payment
6. Manage service requests
7. Search records
8. View reports
0. Exit
```

Each menu handler will read input through a small input helper, call an application service, and catch expected domain/input exceptions without terminating the process. The in-memory repository will use `List<T>` for ordered records and `Dictionary<int, T>` for keyed lookup.

Required OOP demonstrations:

- `Person` abstract base class with `Resident` and `StaffUser` derived classes.
- Private setters and methods such as `WaterConnection.ChangeStatus()` and `Bill.RecordPayment()` for encapsulation.
- Interfaces such as `IResidentService`, `IBillingService`, and `IReportService` for reuse across console and web layers.
- Polymorphic report rows for consumption and collections reports.
- Search by resident name, connection number, bill status, and service-request status.
- Reports for resident summary, monthly consumption, unpaid bills, payment collection, and open requests.

The console will include seeded demo records so all menus work on first run without manual setup.

## 6. Phase 2 ASP.NET Core behavior

The web project will use:

- ASP.NET Core MVC with Razor views.
- Entity Framework Core SQL Server provider.
- ASP.NET Core Identity with roles `Administrator` and `Resident`.
- Data annotations and service-level validation.
- Bootstrap-based responsive layout and print CSS.

Required routes/features:

- `/Account/Login`, `/Account/Logout`, `/Account/ChangePassword`.
- `/Dashboard/Index` with cards for total residents, active connections, current-month consumption, unpaid amount, collection amount, and open requests.
- CRUD controllers for residents and water connections.
- Meter-reading entry and history pages.
- Bill listing, generation, detail, and payment entry.
- Service-request creation for residents and status management for administrators.
- Search/filter query parameters for each list page.
- `/Reports/Consumption`, `/Reports/Payments`, `/Reports/OutstandingBills`, and `/Reports/ServiceRequests` with date/status filters and a print stylesheet.

The first run will seed roles and demo users through a repeatable startup seeder. The README will instruct the user to replace demo passwords before any real deployment.

## 7. Phase 3 PWA behavior

The web app will expose a valid web app manifest with the application name, theme color, icons, `standalone` display mode, and start URL. A service worker will:

- Cache the app shell and versioned static assets on install.
- Serve cached shell assets when offline.
- Use network-first behavior for dashboard/search data and show the last cached response when the network is unavailable.
- Remove old caches during activation.

The offline feature required by the rubric will be the resident's cached dashboard summary and recently viewed bills. Mutating operations remain online-only and will display a clear offline message rather than silently losing data.

The notification script will request permission only after a user action, register the service worker, and display a local reminder when the authenticated resident has cached unpaid bills. Notification support will be capability-detected so unsupported browsers continue to work normally.

## 8. Security, reliability, and error handling

- Passwords will be managed by ASP.NET Core Identity; plaintext passwords will never be stored.
- Anti-forgery validation will be enabled for all state-changing MVC forms.
- Authorization policies will protect administrator actions and ownership checks will protect resident data.
- EF Core migrations and SQL scripts will both be included so the project can be run through Visual Studio or SQL Server Management Studio.
- Validation failures will return field-level messages in MVC and friendly messages in the console.
- Database errors will be logged through `ILogger` and surfaced to users as a non-sensitive generic message.
- The application will not expose stack traces or connection strings in production responses.

## 9. Testing strategy

- Domain tests cover meter validation, billing totals, payment balance rules, status transitions, and ownership filtering.
- Application tests cover resident search, report filters, and service-request workflows using in-memory repositories.
- Web tests cover authorization boundaries, dashboard aggregation, and important controller actions.
- The console project will be smoke-tested with a documented scripted interaction.
- The final verification will run `dotnet test` for the solution and `dotnet build` for all projects.

## 10. Acceptance criteria and rubric mapping

The project is accepted when:

1. The console application has a menu, classes/objects, inheritance, encapsulation, exception handling, collections, search, and reports.
2. The ASP.NET application supports login, logout, change password, CRUD, search, dashboard statistics, filtered reports, and printable output.
3. SQL Server scripts define 5–10 meaningful normalized business tables with primary keys, foreign keys, and relationships.
4. The PWA is responsive, installable, service-worker-backed, offline-capable for the documented read-only feature, and notification-capable.
5. The repository contains the requested phase, database, and documentation folders.
6. Automated tests and a clean solution build pass on the target .NET SDK.

## 11. Deliberate non-goals

- Real payment gateway processing.
- SMS or email delivery infrastructure.
- Geographic map visualizations.
- Multiple municipalities or tenant isolation.
- Background job hosting beyond the browser notification demonstration.

