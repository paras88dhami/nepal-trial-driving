[CmdletBinding()]
param(
    [switch]$IncludeKenneyFallback,
    [ValidateSet('1k','2k','4k')][string]$Resolution='2k'
)

$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$roadFolder=Join-Path $project 'Assets\Game\Art\Environment\Roads\Asphalt01'
$carFolder=Join-Path $project 'Assets\Game\Art\Vehicles\Car\Peugeot205'
New-Item -Path $roadFolder,$carFolder -ItemType Directory -Force | Out-Null

Write-Host "Downloading Poly Haven Asphalt 01 ($Resolution), CC0 license..." -ForegroundColor Cyan
# Resolve URLs from the official API to avoid hard-coded, changing CDN paths.
$catalog=Invoke-RestMethod -Uri 'https://api.polyhaven.com/files/asphalt_01' -TimeoutSec 45
$maps=@('diff','nor_gl','rough')
foreach($map in $maps) {
    $section=$catalog.PSObject.Properties[$map]
    if ($null -eq $section) {
        # Certain Poly Haven API versions use the longer 'diffuse' label.
        if($map -eq 'diff') { $section=$catalog.PSObject.Properties['diffuse'] }
    }
    if($null -eq $section) { throw "Poly Haven did not list '$map' map. Open https://polyhaven.com/a/asphalt_01 to download manually." }
    $entry=$section.Value.PSObject.Properties[$Resolution]
    if($null -eq $entry) { throw "Poly Haven has no $Resolution map for $map." }
    $jpg=$entry.Value.PSObject.Properties['jpg']
    if($null -eq $jpg) { throw "Poly Haven has no JPG download for $map." }
    $uri=[string]$jpg.Value.url
    if([string]::IsNullOrWhiteSpace($uri) -or !$uri.StartsWith('https://')) {
        throw "Could not read a secure Poly Haven URL for $map."
    }
    $dest=Join-Path $roadFolder ("asphalt_01_"+$map+"_"+$Resolution+".jpg")
    Invoke-WebRequest -UseBasicParsing -Uri $uri -OutFile $dest -TimeoutSec 180
    Write-Host "  Saved: $dest"
}

if($IncludeKenneyFallback) {
    Write-Host 'Downloading Kenney Car Kit as a temporary CC0 fallback...' -ForegroundColor Cyan
    $zip=Join-Path $project 'Build\Downloads\kenney_car-kit_3.1.zip'
    New-Item -Path (Split-Path $zip) -ItemType Directory -Force | Out-Null
    Invoke-WebRequest -UseBasicParsing -Uri 'https://opengameart.org/sites/default/files/kenney_car-kit_3.1.zip' -OutFile $zip -TimeoutSec 180
    Write-Host "  Saved archive: $zip"
    Write-Host '  Extract the FBX files into Assets/Game/Art/Vehicles/Car/KenneyFallback if you choose this fallback.'
}

Write-Host ''
Write-Host 'Asphalt downloaded. Reopen Unity to import assets.' -ForegroundColor Green
Write-Host 'Use Nepal Trial > Graphics > Apply Selected Road Texture to Full Grounds (select the downloaded diffuse JPG first).'
Write-Host ''
Write-Warning 'Peugeot 205 requires your Sketchfab login. Its official Download API does not support anonymous downloads.'
Write-Host 'Visit: https://sketchfab.com/3d-models/peugeot-205-low-poly-game-ready-3d-model-8636fde6c69c4a82bcc1d9d3d6f8fbcd'
Write-Host 'Download it with your account; export as FBX in Blender if you receive only glTF; copy model+textures under:'
Write-Host "  $carFolder"
