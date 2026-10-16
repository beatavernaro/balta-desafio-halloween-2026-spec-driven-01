# Runs every test project with Coverlet and consolidates their results into a single coverage report.
param(
    [switch] $NoBuild
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
