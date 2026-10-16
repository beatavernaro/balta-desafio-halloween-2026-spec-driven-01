# Proves that each quality gate rejects a violation. Every violation is injected into a disposable copy of HEAD,
# so the repository itself is never modified. Exits 1 when any gate fails to reject its violation.
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$workDirectory = Join-Path ([IO.Path]::GetTempPath()) ("gate-checks-" + [guid]::NewGuid().ToString('N'))
$archive = "$workDirectory.zip"
$utf8 = New-Object Text.UTF8Encoding $false
$results = New-Object System.Collections.Generic.List[object]

function Invoke-Dotnet([string[]] $Arguments) {
    # Gates report failures on stderr; under 'Stop', Windows PowerShell would turn that output into terminating errors.
    $ErrorActionPreference = 'Continue'
    $output = & dotnet @Arguments 2>&1 | ForEach-Object { "$_" } | Out-String
    return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output }
}

function Add-Result([string] $Gate, [bool] $AsExpected, [string] $Evidence) {
    $results.Add([pscustomobject]@{ Gate = $Gate; AsExpected = $AsExpected; Evidence = $Evidence })
}

# Injects a probe file, runs the gate and checks it fails because of the probe (identified by ExpectedText).
function Test-GateRejects([string] $Gate, [string] $ProbePath, [string] $ProbeContent, [string[]] $Arguments, [string] $ExpectedText) {
    [IO.File]::WriteAllText($ProbePath, $ProbeContent, $utf8)
    try {
        $run = Invoke-Dotnet $Arguments
        $rejected = $run.ExitCode -ne 0 -and $run.Output.Contains($ExpectedText)
        Add-Result $Gate $rejected "exit $($run.ExitCode), expected '$ExpectedText' found: $($run.Output.Contains($ExpectedText))"
    }
    finally {
        Remove-Item $ProbePath -Force
    }
}

try {
    git -C $root archive --format=zip -o $archive HEAD
    if ($LASTEXITCODE -ne 0) {
        throw 'Could not export HEAD.'
    }
    Expand-Archive -Path $archive -DestinationPath $workDirectory

    $solution = Join-Path $workDirectory 'PasswordGenerator.slnx'
    $apiDirectory = Join-Path $workDirectory 'src/PasswordGenerator.Api'
    $unitTestsProject = Join-Path $workDirectory 'tests/PasswordGenerator.UnitTests'
    $checkCoverage = Join-Path $workDirectory 'scripts/check-coverage.ps1'

    # Baseline: the untouched copy must pass, otherwise the rejections below would prove nothing.
    $baseline = Invoke-Dotnet @('build', $solution)
    if ($baseline.ExitCode -ne 0) {
        Write-Output $baseline.Output
        throw 'Baseline build failed; gate checks are inconclusive.'
    }

    Test-GateRejects -Gate 'Build without warnings' `
        -ProbePath (Join-Path $apiDirectory 'GateProbe.cs') `
        -ProbeContent "namespace PasswordGenerator.Api;`n`ninternal static class GateProbe`n{`n    public static void Run()`n    {`n        var unused = 1;`n    }`n}`n" `
        -Arguments @('build', $solution, '--no-restore') `
        -ExpectedText 'CS0219'

    Test-GateRejects -Gate 'Formatting' `
        -ProbePath (Join-Path $apiDirectory 'GateProbe.cs') `
        -ProbeContent "namespace PasswordGenerator.Api;`n`ninternal static class GateProbe`n{`n    public static int Run()   =>   1;`n}`n" `
        -Arguments @('format', $solution, '--verify-no-changes', '--no-restore') `
        -ExpectedText 'WHITESPACE'

    Test-GateRejects -Gate 'Tests' `
        -ProbePath (Join-Path $unitTestsProject 'GateProbeTests.cs') `
        -ProbeContent "namespace PasswordGenerator.UnitTests;`n`npublic class GateProbeTests`n{`n    [Fact]`n    public void Probe_AlwaysFails() => Assert.Fail(`"gate probe failure`");`n}`n" `
        -Arguments @('test', $unitTestsProject, '--no-restore') `
        -ExpectedText 'gate probe failure'

    $coverageReport = Join-Path $workDirectory 'synthetic-cobertura.xml'
    foreach ($case in @(@{ Covered = 7999; Expected = 1 }, @{ Covered = 8000; Expected = 0 })) {
        [IO.File]::WriteAllText($coverageReport, "<coverage lines-covered=`"$($case.Covered)`" lines-valid=`"10000`" />", $utf8)
        $output = & $checkCoverage -CoberturaPath $coverageReport -MinimumLineCoverage 80 | Out-String
        $exitCode = $LASTEXITCODE
        $gate = if ($case.Expected -eq 1) { 'Coverage below 80% (79.99%)' } else { 'Coverage at 80% (control, must pass)' }
        Add-Result $gate ($exitCode -eq $case.Expected) "exit $exitCode, $($output.Trim())"
    }
}
finally {
    if (Test-Path $archive) {
        Remove-Item $archive -Force
    }
    if (Test-Path $workDirectory) {
        Remove-Item $workDirectory -Recurse -Force
    }
}

$results | Format-Table -AutoSize -Wrap | Out-String -Width 200 | Write-Output

if ($results | Where-Object { -not $_.AsExpected }) {
    Write-Output 'FAILED: at least one gate did not behave as expected.'
    exit 1
}

Write-Output 'PASSED: every gate rejected its violation.'
exit 0
