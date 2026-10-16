# Runs every test project with Coverlet, consolidates their results into a single coverage report
# and fails when the aggregate line coverage is below the minimum.
param(
    [switch] $NoBuild,

    [double] $MinimumLineCoverage = 80
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root 'PasswordGenerator.slnx'
$runSettings = Join-Path $root 'coverlet.runsettings'
$resultsDirectory = Join-Path $root 'TestResults'
$reportDirectory = Join-Path $resultsDirectory 'coverage-report'

# Stale results from previous runs would be merged into the report, so start clean.
if (Test-Path $resultsDirectory) {
    Remove-Item $resultsDirectory -Recurse -Force
}

$testArguments = @('test', $solution, '--settings', $runSettings, '--collect', 'XPlat Code Coverage', '--results-directory', $resultsDirectory)
if ($NoBuild) {
    $testArguments += '--no-build'
}

& dotnet @testArguments
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& dotnet tool run reportgenerator `
    "-reports:$resultsDirectory/**/coverage.cobertura.xml" `
    "-targetdir:$reportDirectory" `
    '-reporttypes:Cobertura;TextSummary'
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Get-Content (Join-Path $reportDirectory 'Summary.txt')

& (Join-Path $PSScriptRoot 'check-coverage.ps1') `
    -CoberturaPath (Join-Path $reportDirectory 'Cobertura.xml') `
    -MinimumLineCoverage $MinimumLineCoverage
exit $LASTEXITCODE
