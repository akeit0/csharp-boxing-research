param(
    [switch]$IncludePreview,
    [string]$PreviewSdkVersion = '11.0.100-rc.3.26522.148',
    [string[]]$Modes = @('safe', 'stack-box', 'constraint-only', 'metadata-constraint', 'invalid-il', 'invalid-constraint'),
    [string]$OutputDirectory
)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Common.ps1"
$results = New-ResultsDirectory -Kind nativeaot -Path $OutputDirectory
$sdkVersion = (Get-Content -LiteralPath "$WorkspaceRoot/global.json" -Raw | ConvertFrom-Json).sdk.version
$targets = @(@{ Framework = 'net10.0'; Sdk = $sdkVersion })
if ($IncludePreview) { $targets += @{ Framework = 'net11.0'; Sdk = $PreviewSdkVersion } }

foreach ($target in $targets) {
    $framework = $target.Framework
    $targetResults = New-ResultsDirectory -Kind $framework -Path (Join-Path $results $framework)
    $publishDirectory = Join-Path $WorkspaceRoot "builds/nativeaot/$framework"
    # Select the SDK without changing the workspace's global.json.
    @{ sdk = @{ version = $target.Sdk; rollForward = 'disable' } } |
        ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $targetResults 'global.json') -Encoding utf8
    Push-Location $targetResults
    try {
        & dotnet publish "$WorkspaceRoot/dotnet/Experiments/Experiments.csproj" -c Release -r win-x64 `
            -p:NativeAot=true "-p:ExperimentFramework=$framework" -o $publishDirectory --nologo *> (Join-Path $targetResults 'publish.log')
        if ($LASTEXITCODE -ne 0) { throw "Native AOT publish failed: $targetResults/publish.log" }
        Write-RunInfo -Kind "nativeaot-$framework" -ResultDirectory $targetResults
        @{ framework = $framework; sdk = $target.Sdk; rid = 'win-x64'; publishAot = $true } |
            ConvertTo-Json | Set-Content -LiteralPath (Join-Path $targetResults 'compiler.json') -Encoding utf8
        foreach ($mode in $Modes) {
            $path = Join-Path $targetResults "nativeaot-$framework-$mode.txt"
            $run = Invoke-ExperimentProcess -FilePath "$publishDirectory/Experiments.exe" -Arguments @($mode) -OutputPath $path
            "exitCode=$($run.ExitCode)" | Add-Content -LiteralPath $path
            if ($run.TimedOut) { 'timeoutSeconds=60' | Add-Content -LiteralPath $path }
            # Invalid concrete interface calls terminated with STATUS_FAIL_FAST_EXCEPTION.
            $expected = @(0)
            if ($mode -in @('invalid-il', 'invalid-constraint')) { $expected += -1073740286 }
            if ($run.TimedOut -or $run.ExitCode -notin $expected) { throw "Experiment failed: $path" }
            Write-Host "$framework/${mode}: exit $($run.ExitCode)"
        }
    } finally { Pop-Location }
}
Write-Host "Results: $results"
