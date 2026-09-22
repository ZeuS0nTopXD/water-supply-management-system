# Software Requirements Specification

## 1. Purpose

The Water Supply Management System gives utility staff a simple application for maintaining residents and water connections, recording meter readings, creating bills, handling service requests, and viewing reports.

## 2. Project phases

| Phase | Purpose | Deliverable |
| --- | --- | --- |
| 1 | Learn OOP and implement reusable business logic | Menu-driven console application |
| 2 | Build the database-driven system | ASP.NET Core MVC application with Identity and SQL Server |
| 3 | Extend the system for mobile users | Installable PWA with offline shell and notifications |

## 3. Actors

| Actor | Capabilities |
| --- | --- |
| Administrator | Sign in, manage records, view dashboard, update service requests, and print reports |
| Staff user | Sign in, search records, record readings, create bills, and view requests |

## 4. Functional requirements

- Console: menu-driven interface, OOP classes/objects, inheritance, encapsulation, exception handling, collections, search, and reports.
- Authentication: login, logout, and change password.
- Master management: add, update, delete, and search resident records; add and search water connections.
- Operations: record meter readings, calculate consumption, create bills, and add/update service requests.
- Dashboard: resident count, active connection count, monthly consumption, unpaid bill amount, and open request count.
- Reports: filter bills by status or connection and print the report from the browser.
- PWA: responsive interface, install manifest, service worker, offline fallback, and browser notification permission flow.

## 5. Out of scope

Payments, payment gateways, a separate resident portal, a notifications business table, SMS/email delivery, and advanced approval workflows are intentionally excluded from this academic version.

## 6. Non-functional requirements

- Use normalized SQL Server tables with primary and foreign keys.
- Keep domain rules reusable between console and web layers.
- Validate input and handle expected exceptions without crashing the application.
- Provide a mobile-friendly layout and an offline application shell.

## 7. Data model

The five business tables are `Residents`, `WaterConnections`, `MeterReadings`, `Bills`, and `ServiceRequests`. ASP.NET Identity supplies its own authentication tables. See `Database/ERDiagram/water-supply-er-diagram.mmd`.
