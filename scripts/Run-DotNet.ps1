param(
    [switch]$IncludePreview,
    [string]$PreviewVersion = '11.0.0-rc.3.26522.148',
    [string[]]$Modes,
    [string]$OutputDirectory
)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Common.ps1"
$results = New-ResultsDirectory -Kind dotnet -Path $OutputDirectory
Push-Location $WorkspaceRoot
try {
    Build-Experiments -ResultDirectory $results
    Write-RunInfo -Kind dotnet -ResultDirectory $results
    if (!$Modes) { $Modes = @(Get-DefaultModes -CoreClr -DotNet) }
    $targets = @(@{ Name = 'net10.0'; Prefix = @() })
    if ($IncludePreview) {
        $targets += @{ Name = 'net11-preview'; Prefix = @('--fx-version', $PreviewVersion) }
    }
    foreach ($target in $targets) {
        $assembly = "$WorkspaceRoot/dotnet/Experiments/bin/Release/net10.0/Experiments.dll"
        foreach ($mode in $Modes) {
            $path = Join-Path $results "dotnet-$($target.Name)-$mode.txt"
            $arguments = @($target.Prefix) + @($assembly, $mode)
            $run = Invoke-ExperimentProcess -FilePath dotnet -Arguments $arguments -OutputPath $path -Environment @{ DOTNET_TieredCompilation = '0' }
            "exitCode=$($run.ExitCode)" | Add-Content -LiteralPath $path
            if ($run.TimedOut) { 'timeoutSeconds=60' | Add-Content -LiteralPath $path }
            if ($run.TimedOut -or $run.ExitCode -ne 0) { throw "Experiment failed: $path" }
            Write-Host "$($target.Name)/${mode}: exit $($run.ExitCode)"
        }
    }
    Write-Host "Results: $results"
} finally { Pop-Location }
