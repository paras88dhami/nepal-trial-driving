[CmdletBinding()]
param([string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe")

$ErrorActionPreference = "Stop"
$projectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if (-not (Test-Path -LiteralPath $UnityExe -PathType Leaf)) {
    throw "Unity Editor not found: $UnityExe. Pass -UnityExe or install the matching editor and Android Build Support."
}
$folder = Join-Path $projectPath "Build\QA"
New-Item -ItemType Directory -Force -Path $folder | Out-Null
$log = Join-Path $folder "Unity_AndroidBuild.log"
$apk = Join-Path $projectPath "Build\Android\NepalDrivingTrial-dev.apk"
if (Test-Path $apk) { Remove-Item -LiteralPath $apk -Force }
$cliArgs = @("-batchmode", "-nographics", "-quit", "-buildTarget", "Android",
    "-projectPath", $projectPath,
    "-executeMethod", "NepalTrial.EditorTools.Phase5BuildPipeline.BuildDevelopmentAndroid",
    "-logFile", $log)
& $UnityExe @cliArgs
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $apk)) {
    throw "Development Android build failed or APK missing. Inspect $log. Make sure Phase 4 scenes are generated and registered."
}
Write-Host "Development APK created: $apk"
Write-Host "Install on a real Android device and run the full driving/touch/physics QA checklist."
