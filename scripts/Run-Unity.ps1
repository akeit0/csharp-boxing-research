param(
    [string[]]$Names = @('unity63-mono', 'unity63-il2cpp', 'unity7-coreclr', 'unity7-il2cpp'),
    [string[]]$Modes,
    [string]$OutputDirectory
)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Common.ps1"
$results = New-ResultsDirectory -Kind unity -Path $OutputDirectory
Push-Location $WorkspaceRoot
try {
    Build-Experiments -ResultDirectory $results
    & "$PSScriptRoot/Sync-Unity.ps1"
    Write-RunInfo -Kind unity -ResultDirectory $results
    foreach ($name in $Names) {
        switch ($name) {
            'unity63-mono' { $project = 'Unity63'; $backend = 'Mono2x' }
            'unity63-il2cpp' { $project = 'Unity63'; $backend = 'IL2CPP' }
            'unity7-coreclr' { $project = 'Unity7Alpha'; $backend = 'CoreCLR' }
            'unity7-il2cpp' { $project = 'Unity7Alpha'; $backend = 'IL2CPP' }
            default { throw "Unknown environment: $name" }
        }
        & unity run "unity/$project" --timeout 900 --log-file "$results/$name-build.log" --no-tail -- -nographics -executeMethod ResearchEditor.Build --research-backend $backend --research-build-output "$WorkspaceRoot/builds/$name/Research.exe"
        if ($LASTEXITCODE -ne 0) { throw "Unity build failed: $name; see $results/$name-build.log" }
        $selectedModes = $Modes
        if (!$selectedModes) { $selectedModes = @(Get-DefaultModes -CoreClr:($backend -eq 'CoreCLR')) }
        & "$PSScriptRoot/Run-Player.ps1" -Name $name -Modes $selectedModes -OutputDirectory $results
    }
    Write-Host "Results: $results"
} finally { Pop-Location }
