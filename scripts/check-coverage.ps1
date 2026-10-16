# Fails (exit code 1) when the aggregate line coverage of a Cobertura report is below the minimum.
param(
    [Parameter(Mandatory = $true)]
    [string] $CoberturaPath,

    [double] $MinimumLineCoverage = 80
)

$ErrorActionPreference = 'Stop'

[xml] $report = Get-Content -Path $CoberturaPath -Raw
$coveredLines = [int] $report.coverage.'lines-covered'
$validLines = [int] $report.coverage.'lines-valid'

if ($validLines -eq 0) {
    Write-Output 'No coverable lines found in the coverage report.'
    exit 1
}

$lineCoverage = [math]::Round(100 * $coveredLines / $validLines, 2)
$culture = [Globalization.CultureInfo]::InvariantCulture
$result = "Line coverage: {0}% ({1} of {2} lines). Minimum: {3}%." -f `
    $lineCoverage.ToString($culture), $coveredLines, $validLines, $MinimumLineCoverage.ToString($culture)

if ($lineCoverage -lt $MinimumLineCoverage) {
    Write-Output "FAILED. $result"
    exit 1
}

Write-Output "PASSED. $result"
exit 0
