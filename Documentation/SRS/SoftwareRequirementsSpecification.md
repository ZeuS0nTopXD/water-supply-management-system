# Software Requirements Specification

## 1. Purpose

The Water Supply Management System gives water utility staff a single place to maintain residents and connections, record meter readings, generate bills, receive payments, track service requests, and produce management reports. Residents can sign in to view their own portal data and raise requests.

## 2. Actors

| Actor | Capabilities |
| --- | --- |
| Administrator | Maintain residents/connections, record readings, generate bills, record payments, update requests, view reports and dashboard metrics |
| Resident | Sign in, view own bills, create and track own service requests, change password |

## 3. Functional requirements

- Authentication: login, logout, role-aware authorization, change password.
- Resident and water-connection master management: add, update, delete, search.
- Meter readings: record readings and derive consumption with validation against decreasing readings and duplicate dates.
- Billing: calculate total from units, rate, fixed charge, and tax; track paid and outstanding balances.
- Payments: validate amount against outstanding balance and update bill status.
- Service requests: create, filter, and update status with staff notes.
- Dashboard: resident count, active connection count, current-month consumption, unpaid amount, collections, and open request count.
- Reports: outstanding bills, consumption, and payment reports with filters and browser print support.
- PWA: install manifest, service worker, offline shell, responsive layout, and notification permission flow.

## 4. Non-functional requirements

- Use normalized relational data with primary and foreign keys.
- Enforce domain validation and server-side authorization.
- Keep business rules reusable between the console and ASP.NET layers.
- Provide mobile-friendly layouts and an offline fallback for the application shell.
- Use SQL Server for deployment and tests for critical business logic.

## 5. Data model

The database contains seven business tables: `Residents`, `WaterConnections`, `MeterReadings`, `Bills`, `Payments`, `ServiceRequests`, and `Notifications`. ASP.NET Identity tables provide authentication storage. See `Database/ERDiagram/water-supply-er-diagram.mmd`.
