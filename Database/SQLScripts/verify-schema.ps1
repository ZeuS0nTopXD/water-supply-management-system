$ErrorActionPreference = 'Stop'
$schemaPath = Join-Path $PSScriptRoot '002_CreateTables.sql'

if (-not (Test-Path -LiteralPath $schemaPath)) {
    Write-Error "Schema file not found: $schemaPath"
    exit 1
}

$sql = Get-Content -Raw -LiteralPath $schemaPath
$tables = @('Residents', 'WaterConnections', 'MeterReadings', 'Bills', 'Payments', 'ServiceRequests', 'Notifications')
$missing = [System.Collections.Generic.List[string]]::new()

foreach ($table in $tables) {
    if ($sql -notmatch "(?is)CREATE\s+TABLE\s+(?:\[?dbo\]?\.)?\[?$table\]?") { $missing.Add("table $table") }
}

foreach ($table in $tables) {
    if ($sql -notmatch "(?is)CREATE\s+TABLE.*?\bPRIMARY\s+KEY\b") { $missing.Add("primary key coverage") ; break }
}

$relationships = @(
    'WaterConnections.*REFERENCES.*Residents',
    'MeterReadings.*REFERENCES.*WaterConnections',
    'Bills.*REFERENCES.*WaterConnections',
    'Payments.*REFERENCES.*Bills',
    'ServiceRequests.*REFERENCES.*Residents',
    'Notifications.*REFERENCES.*Residents'
)

foreach ($relationship in $relationships) {
    if ($sql -notmatch "(?is)$relationship") { $missing.Add("foreign key $relationship") }
}

if ($missing.Count -gt 0) {
    Write-Error ("Schema verification failed: " + ($missing -join ', '))
    exit 1
}

Write-Output "PASS: seven tables, primary keys, and required foreign-key relationships found."
