$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

$required = @(
    'ConsoleApplication',
    'ASPNETApplication',
    'PWA',
    'Database/SQLScripts/001_CreateDatabase.sql',
    'Database/SQLScripts/002_CreateTables.sql',
    'Database/ERDiagram/water-supply-er-diagram.mmd',
    'Documentation/SRS/SoftwareRequirementsSpecification.md',
    'Documentation/PPT/PresentationOutline.md',
    'Documentation/FinalReport/FinalReportOutline.md',
    'README.md'
)

$missing = @($required | Where-Object { -not (Test-Path (Join-Path $repoRoot $_)) })
if ($missing.Count -gt 0) {
    Write-Error "FAIL: missing required paths: $($missing -join ', ')"
}

$webRoot = Join-Path $repoRoot 'ASPNETApplication/WaterSupply.Web/wwwroot'
@('manifest.webmanifest', 'service-worker.js', 'offline.html', 'js/pwa-notifications.js') | ForEach-Object {
    if (-not (Test-Path (Join-Path $webRoot $_))) { Write-Error "FAIL: missing PWA asset $_" }
}

$sql = Get-Content (Join-Path $repoRoot 'Database/SQLScripts/002_CreateTables.sql') -Raw
foreach ($table in @('Residents', 'WaterConnections', 'MeterReadings', 'Bills', 'ServiceRequests')) {
    if ($sql -notmatch "CREATE TABLE dbo\.$table") { Write-Error "FAIL: missing table $table" }
}
if (($sql -split 'FOREIGN KEY').Count -lt 5) { Write-Error 'FAIL: expected foreign-key relationships were not found' }

Write-Output 'PASS: repository structure, documentation, PWA assets, five tables, and relationships found.'
