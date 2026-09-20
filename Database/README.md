# Database

The SQL Server database is named `WaterSupplyManagement` and contains seven normalized business tables. Run the scripts in this order using SQL Server Management Studio or `sqlcmd`:

1. `001_CreateDatabase.sql`
2. `002_CreateTables.sql`
3. `003_SeedReferenceData.sql`
4. `004_SeedDemoData.sql`

The schema uses primary keys on every table, foreign keys for all relationships, unique connection/reference values, and check constraints for statuses and non-negative amounts. The source ER diagram is `ERDiagram/water-supply-er-diagram.mmd`.

Run `powershell -ExecutionPolicy Bypass -File SQLScripts/verify-schema.ps1` to check the script text for all seven tables and required relationships. A live SQL Server instance is required to execute the scripts; the verifier does not replace SQL Server execution.
