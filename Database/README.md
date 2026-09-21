# Database

The SQL Server database is named `WaterSupplyManagement` and contains five normalized business tables:

1. `Residents`
2. `WaterConnections`
3. `MeterReadings`
4. `Bills`
5. `ServiceRequests`

Run the scripts in this order using SQL Server Management Studio or `sqlcmd`:

1. `SQLScripts/001_CreateDatabase.sql`
2. `SQLScripts/002_CreateTables.sql`
3. `SQLScripts/003_SeedReferenceData.sql`
4. `SQLScripts/004_SeedDemoData.sql`

Every business table has a primary key. Foreign keys connect residents to connections and requests, connections to readings/bills/requests, and readings to bills. The source ER diagram is `ERDiagram/water-supply-er-diagram.mmd`.

Run `powershell -ExecutionPolicy Bypass -File SQLScripts/verify-schema.ps1` to check the script text. A live SQL Server instance is required to execute the scripts.
