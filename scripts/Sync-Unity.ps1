$ErrorActionPreference = 'Stop'
$workspace = Split-Path -Parent $PSScriptRoot
foreach ($name in @('Unity63', 'Unity7Alpha')) {
    $project = Join-Path $workspace "unity/$name"
    if (!(Test-Path "$project/ProjectSettings/ProjectVersion.txt")) { continue }
    New-Item -ItemType Directory -Force "$project/Assets/Research/Editor", "$project/Assets/Plugins" | Out-Null
    Copy-Item -LiteralPath "$workspace/unity/ResearchPlayer.cs" -Destination "$project/Assets/Research"
    Copy-Item -LiteralPath "$workspace/unity/ResearchEditor.cs" -Destination "$project/Assets/Research/Editor"
    Copy-Item -LiteralPath "$workspace/shared/Research.IL/bin/Release/netstandard2.1/Research.IL.dll" -Destination "$project/Assets/Plugins"
    Copy-Item -LiteralPath "$workspace/shared/Research.Core/bin/Release/netstandard2.1/Research.Core.dll" -Destination "$project/Assets/Plugins"
    if ($name -eq 'Unity63') {
        Copy-Item -LiteralPath "$workspace/shared/Research.Core/bin/Release/netstandard2.1/System.Runtime.CompilerServices.Unsafe.dll" -Destination "$project/Assets/Plugins"
    }
}
