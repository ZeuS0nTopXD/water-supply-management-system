# Basic Water Supply Management System Design Specification

**Date:** 2026-09-21  
**Project:** Water Supply Management System  
**Target framework:** .NET 10 (`net10.0`)  
**Purpose:** Provide a small, understandable academic project that satisfies the three required development phases without unnecessary business complexity.

## 1. Design goal

The existing project will be reduced to a student-friendly version. The application will demonstrate the required concepts with a short set of workflows that can be explained in a viva or presentation.

The project will keep the existing repository structure and project name. Existing code that is outside the approved basic scope will be removed or replaced rather than exposed through additional menus and pages.

## 2. Scope

The system manages:

- Residents
- Water connections
- Meter readings
- Simple bills calculated from readings
- Service requests

The system will not include payment processing, notifications stored in the database, resident self-service portals, GIS, online payment gateways, email/SMS delivery, or background jobs.

## 3. Users and access

The web application has two roles:

- **Administrator:** manages all records, updates service-request status, views the dashboard, and prints reports.
- **Resident:** can log in, view their own basic information, and submit or view their own service request.

Authentication includes login, logout, and change password. Administrator-only actions require the Administrator role. Resident queries must be filtered by the authenticated resident identity.

## 4. Simplified architecture

The solution keeps four logical areas:

```text
WaterSupplyManagementSystem/
├── ConsoleApplication/
├── ASPNETApplication/
├── PWA/
├── Shared/
├── Database/
├── Documentation/
└── README.md
```

The shared layer contains simple domain classes and a few application services. The console app uses in-memory collections. The ASP.NET app uses EF Core and SQL Server. The PWA is the responsive ASP.NET interface plus a small manifest and service worker.

The implementation should prefer direct, readable classes over generic abstractions, deep inheritance hierarchies, or unnecessary service layers.

## 5. Basic domain model

The console phase will use these classes:

- `Person` — abstract base class with common name and contact properties.
- `Resident` — inherits from `Person` and stores address and active status.
- `WaterConnection` — stores connection number, resident, meter number, and status.
- `MeterReading` — stores previous reading, current reading, date, and calculated consumption.
- `Bill` — calculates total amount from consumption and a fixed rate.
- `ServiceRequest` — stores request type, description, date, and status.

Encapsulation will be demonstrated with private setters and methods such as `RecordReading`, `CalculateBill`, and `ChangeStatus`. Validation exceptions will reject empty names, invalid readings, negative consumption, and invalid service-request status values.

## 6. Database design

The SQL Server business schema will contain exactly five meaningful tables:

### Residents

- `ResidentId` primary key
- `FullName`, `Email`, `Phone`, `Address`
- `RegistrationDate`, `IsActive`

### WaterConnections

- `WaterConnectionId` primary key
- `ResidentId` foreign key to `Residents`
- `ConnectionNumber`, `MeterNumber`, `ConnectionType`, `Status`, `ConnectionDate`

### MeterReadings

- `MeterReadingId` primary key
- `WaterConnectionId` foreign key to `WaterConnections`
- `ReadingDate`, `PreviousReading`, `CurrentReading`, `Consumption`
- Unique reading date per connection

### Bills

- `BillId` primary key
- `WaterConnectionId` foreign key to `WaterConnections`
- `MeterReadingId` foreign key to `MeterReadings`
- `BillDate`, `UnitsConsumed`, `RatePerUnit`, `TotalAmount`, `DueDate`, `Status`

### ServiceRequests

- `ServiceRequestId` primary key
- `ResidentId` foreign key to `Residents`
- `WaterConnectionId` nullable foreign key to `WaterConnections`
- `RequestType`, `Description`, `CreatedAt`, `Status`, `StaffNotes`

Relationships:

```text
Resident 1 ──── * WaterConnection 1 ──── * MeterReading
WaterConnection 1 ──── * Bill
MeterReading 1 ──── 0..1 Bill
Resident 1 ──── * ServiceRequest
WaterConnection 0..1 ──── * ServiceRequest
```

ASP.NET Core Identity tables are created separately for authentication and are not counted as business tables.

## 7. Phase 1: basic console application

The main menu will be:

```text
1. Add resident
2. List residents
3. Search resident
4. Add water connection
5. Record meter reading
6. View bills
7. Add service request
8. View summary report
0. Exit
```

The console app will use `List<T>` and `Dictionary<int, T>`, seed a few demo records, validate input, catch expected exceptions, and continue showing the menu after an invalid operation.

The summary report will show total residents, active connections, total consumption, total bill amount, and open service requests.

## 8. Phase 2: basic ASP.NET application

The web app will use ASP.NET Core MVC, Razor views, EF Core SQL Server, and ASP.NET Core Identity.

Required pages:

- Login, logout, and change password
- Administrator dashboard with five count/amount cards
- Resident list with add, edit, delete, and search
- Water connection list with add, edit, delete, and search
- Meter reading entry and list
- Simple bill list generated from meter readings
- Service request list with create and status update
- One report page with status/date filtering and print CSS

The UI will use basic Bootstrap styling. Business logic will stay in small services or domain methods, not in Razor views.

## 9. Phase 3: basic PWA

The existing ASP.NET application will be made installable with:

- Responsive Bootstrap layout
- `manifest.webmanifest`
- Service worker registration
- Cached home page and resident list for offline read-only access
- Clear offline message for create/update/delete actions
- A simple browser notification button after permission is granted

No push-notification server or background notification infrastructure is required.

## 10. Testing and acceptance

The project is acceptable when:

1. The console includes a menu, classes/objects, inheritance, encapsulation, exception handling, collections, search, and a report.
2. The web app includes login, logout, change password, CRUD, search, dashboard statistics, filtered reports, and printable output.
3. The database contains five normalized business tables with primary keys, foreign keys, and relationships.
4. The PWA is responsive, installable, service-worker-backed, and provides one offline read-only feature plus browser notification capability.
5. The solution builds and automated tests pass.
6. The repository contains the required ConsoleApplication, ASPNETApplication, PWA, Database, Documentation, and README.md paths.

## 11. Deliberate simplifications

- No payment table or payment workflow.
- No notifications table or stored notification workflow.
- No resident portal controller separate from the main resident pages.
- No advanced billing rules; bills use `consumption * rate`.
- No generic repository framework unless a small concrete class is needed by tests.
- No external services or deployment setup.
