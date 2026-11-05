param(
    [Parameter(Mandatory)][string]$Name,
    [string[]]$Modes = @('safe', 'forge', 'forge-gc'),
    [string]$OutputDirectory
)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Common.ps1"
$results = New-ResultsDirectory -Kind $Name -Path $OutputDirectory
$executable = Join-Path $WorkspaceRoot "builds/$Name/Research.exe"
if (!(Test-Path -LiteralPath $executable)) { throw "Player missing: $executable" }
foreach ($mode in $Modes) {
    $path = Join-Path $results "$Name-$mode.txt"
    $log = Join-Path $results "$Name-$mode-player.log"
    $arguments = @('-batchmode', '-nographics', '-silent-crashes', '-logFile', $log, '--research-case', $mode, '--research-output', $path)
    $run = Invoke-ExperimentProcess -FilePath $executable -Arguments $arguments
    "exitCode=$($run.ExitCode)" | Add-Content -LiteralPath $path
    if ($run.TimedOut) { 'timeoutSeconds=60' | Add-Content -LiteralPath $path }
    $expected = @(0)
    if ($mode -eq 'metadata-constraint' -and $Name.EndsWith('-il2cpp')) { $expected += -1073741819 }
    if ($run.TimedOut -or $run.ExitCode -notin $expected) { throw "Experiment failed: $path; player log: $log" }
    Write-Host "${Name}/${mode}: exit $($run.ExitCode)"
}
Write-Host "Results: $results"
