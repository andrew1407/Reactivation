<#
.SYNOPSIS
Builds the game with the project's Unity version in batch mode and zips each build into Builds/.

.EXAMPLE
./build.ps1                                        # Windows + WebGL
./build.ps1 -Platforms Windows,Linux,macOS,WebGL   # every platform whose build support module is installed

Close the project in the editor first: Unity can only open it once.
#>
param(
    [ValidateSet('Windows', 'Linux', 'macOS', 'WebGL')]
    [string[]] $Platforms = @('Windows', 'WebGL'),
    # Editor executable; found from ProjectSettings/ProjectVersion.txt in the Hub's install folder when left out
    [string] $UnityPath
)

$ErrorActionPreference = 'Stop'
$project = $PSScriptRoot
$builds = Join-Path $project 'Builds'

# Unity build target and the PlaybackEngines module that has to be installed for it
$targets = @{
    Windows = @{ Target = 'Win64'; Module = 'windowsstandalonesupport' }
    Linux   = @{ Target = 'Linux64'; Module = 'LinuxStandaloneSupport' }
    macOS   = @{ Target = 'OSXUniversal'; Module = 'MacStandaloneSupport' }
    WebGL   = @{ Target = 'WebGL'; Module = 'WebGLSupport' }
}

function Find-Editors {
    $version = (Select-String -Path (Join-Path $project 'ProjectSettings/ProjectVersion.txt') -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value.Trim()
    # The Hub may install a version under its plain name or with an architecture suffix
    $hub = Join-Path $env:ProgramFiles 'Unity/Hub/Editor'
    $found = @(Get-ChildItem $hub -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -eq $version -or $_.Name -like "$version-*" } |
        ForEach-Object { Join-Path $_.FullName 'Editor/Unity.exe' } |
        Where-Object { Test-Path $_ })
    if (-not $found) { throw "Unity $version isn't installed in $hub (or pass -UnityPath)" }
    return $found
}

function Test-Module([string] $editor, [string] $module) {
    $engines = Join-Path (Split-Path $editor) 'Data/PlaybackEngines'
    return [bool](Get-ChildItem $engines -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -eq $module })
}

$editors = if ($UnityPath) { @($UnityPath) } else { Find-Editors }
New-Item -ItemType Directory -Force $builds | Out-Null
$failed = @()

foreach ($platform in $Platforms) {
    $target = $targets[$platform]
    $editor = $editors | Where-Object { Test-Module $_ $target.Module } | Select-Object -First 1
    if (-not $editor) {
        Write-Warning "Skipping ${platform}: no installed editor has its build support module ($($target.Module))"
        continue
    }

    $output = Join-Path $builds $platform
    $log = Join-Path $builds "$platform.log"
    Write-Host "Building $platform with $editor ..."
    if (Test-Path $output) { Remove-Item -Recurse -Force $output }
    $process = Start-Process -FilePath $editor -Wait -PassThru -NoNewWindow -ArgumentList @(
        '-batchmode', '-quit', '-nographics', '-projectPath', "`"$project`"", '-buildTarget', $target.Target,
        '-executeMethod', 'BuildScript.Build', '-buildPath', "`"$output`"", '-logFile', "`"$log`"")
    if ($process.ExitCode -ne 0) {
        Write-Warning "$platform build failed (exit $($process.ExitCode)), see $log"
        $failed += $platform
        continue
    }

    # The player's debug-only leftovers aren't shipped
    Get-ChildItem $output -Directory | Where-Object { $_.Name -match '_(BurstDebugInformation_DoNotShip|BackUpThisFolder_ButDontShipItWithYourGame)$' } |
        Remove-Item -Recurse -Force
    $archive = Join-Path $builds "Reactivation-$platform.zip"
    Compress-Archive -Path (Join-Path $output '*') -DestinationPath $archive -Force
    Write-Host "  -> $archive"
}

if ($failed) { throw "Failed: $($failed -join ', ')" }
