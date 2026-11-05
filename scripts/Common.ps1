$WorkspaceRoot = Split-Path -Parent $PSScriptRoot

function New-ResultsDirectory {
    param([string]$Kind, [string]$Path)
    if (!$Path) {
        $stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
        $Path = Join-Path $WorkspaceRoot "artifacts/runs/$stamp-$Kind"
    } elseif (![IO.Path]::IsPathRooted($Path)) {
        $Path = Join-Path $WorkspaceRoot $Path
    }
    $Path = [IO.Path]::GetFullPath($Path)
    [IO.Directory]::CreateDirectory($Path) | Out-Null
    return $Path
}

function Build-Experiments {
    param([string]$ResultDirectory)
    $buildLog = Join-Path $ResultDirectory 'dotnet-build.log'
    & dotnet build "$WorkspaceRoot/dotnet/Experiments" -c Release --nologo *> $buildLog
    if ($LASTEXITCODE -ne 0) {
        Get-Content -LiteralPath $buildLog | Write-Host
        throw "Build failed: $buildLog"
    }
}

function Write-RunInfo {
    param([string]$Kind, [string]$ResultDirectory)
    $assemblies = foreach ($name in @('Research.Core', 'Research.IL')) {
        $path = Join-Path $WorkspaceRoot "shared/$name/bin/Release/netstandard2.1/$name.dll"
        @{ file = "$name.dll"; sha256 = (Get-FileHash -LiteralPath $path).Hash }
    }
    @{
        kind = $Kind
        createdUtc = [DateTime]::UtcNow.ToString('o')
        assemblies = @($assemblies)
    } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $ResultDirectory 'run.json') -Encoding utf8
}

function Get-DefaultModes {
    param([switch]$CoreClr, [switch]$DotNet)
    $modes = @('safe', 'forge', 'forge-gc', 'stack-box', 'constraint-only', 'metadata-constraint')
    if ($CoreClr) { $modes += @('forge-typehandle') }
    if ($DotNet) { $modes += @('invalid-il', 'invalid-constraint', 'dump-il') }
    return $modes
}

function Invoke-ExperimentProcess {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$OutputPath,
        [int]$TimeoutSeconds = 60,
        [hashtable]$Environment = @{}
    )
    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $FilePath
    $start.WorkingDirectory = $WorkspaceRoot
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
    foreach ($key in $Environment.Keys) { $start.Environment[$key] = $Environment[$key] }
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $start
    try {
        $process.Start() | Out-Null
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $timedOut = !$process.WaitForExit($TimeoutSeconds * 1000)
        if ($timedOut) { $process.Kill($true); $process.WaitForExit() }
        $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
        if ($OutputPath) { [IO.File]::WriteAllText($OutputPath, $output) }
        return [PSCustomObject]@{ ExitCode = $process.ExitCode; TimedOut = $timedOut }
    } finally { $process.Dispose() }
}
