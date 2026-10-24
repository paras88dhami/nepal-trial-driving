[CmdletBinding()]
param(
    [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe",
    [switch]$SkipPlayMode
)

$ErrorActionPreference = "Stop"
$projectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if (-not (Test-Path -LiteralPath $UnityExe -PathType Leaf)) {
    throw "Unity Editor not found at '$UnityExe'. Install 6000.6.7f1 or pass -UnityExe with the full path."
}

$out = Join-Path $projectPath "Build\QA"
New-Item -ItemType Directory -Path $out -Force | Out-Null

$platforms = @("EditMode", "PlayMode")
if ($SkipPlayMode) { $platforms = @("EditMode") }

foreach ($platform in $platforms) {
    $report = Join-Path $out ("TestResults_" + $platform + ".xml")
    $log = Join-Path $out ("Unity_" + $platform + ".log")
    Remove-Item $report -ErrorAction SilentlyContinue
    $cliArgs = @("-batchmode", "-nographics", "-quit",
        "-projectPath", $projectPath, "-runTests", "-testPlatform", $platform,
        "-testResults", $report, "-logFile", $log)
    & $UnityExe @cliArgs
    $exitCode = $LASTEXITCODE
    if (-not (Test-Path $report)) {
        throw "$platform test report missing (exit $exitCode). Open $log for compilation/license errors."
    }
    [xml]$xml = Get-Content -LiteralPath $report -Raw
    $root = $xml.DocumentElement
    if ($root -eq $null -or $root.Name -ne "test-run") {
        throw "$platform report not an NUnit test-run document. Check $report"
    }
    $total = [int]$root.GetAttribute("total")
    $failed = [int]$root.GetAttribute("failed")
    $passed = [int]$root.GetAttribute("passed")
    $inconclusive = [int]$root.GetAttribute("inconclusive")
    Write-Host "$platform -> total=$total, passed=$passed, failed=$failed, inconclusive=$inconclusive, editorExit=$exitCode"
    if ($exitCode -ne 0 -or $total -lt 1 -or $failed -gt 0 -or $inconclusive -gt 0) {
        throw "$platform failed or incomplete. Check $report and $log."
    }
}
Write-Host "Unity test reports were generated and checked. This does NOT certify Android device gameplay."
