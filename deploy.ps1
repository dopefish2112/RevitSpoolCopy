# Build + install RevitSpoolCopy for one or more Revit versions (2025-2027).
# Usage:  .\deploy.ps1               -> builds vs Revit 2025, installs for all installed versions
#         .\deploy.ps1 -Versions 2026
param(
    [string[]]$Versions = @("2025","2026","2027")
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# Revit API assemblies come from NuGet (reference-only) - no local Revit
# install needed to compile.
dotnet build "$root\RevitSpoolCopy.csproj" -c Release
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

$dll = "$root\bin\Release\RevitSpoolCopy.dll"

foreach ($v in $Versions) {
    if (-not (Test-Path "C:\Program Files\Autodesk\Revit $v")) {
        Write-Host "Revit $v not installed - skipping." ; continue
    }
    $addinDir = "$env:APPDATA\Autodesk\Revit\Addins\$v"
    New-Item -ItemType Directory -Force -Path $addinDir | Out-Null

    # Write a manifest pointing at the actual built DLL.
    $manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>RevitSpoolCopy</Name>
    <Assembly>$dll</Assembly>
    <FullClassName>RevitSpoolCopy.App</FullClassName>
    <ClientId>8f3a2b14-9c7d-4e61-b2a0-1d5e6f7a8c90</ClientId>
    <VendorId>MCHN</VendorId>
    <VendorDescription>Mike Chandler</VendorDescription>
  </AddIn>
</RevitAddIns>
"@
    Set-Content -Path "$addinDir\RevitSpoolCopy.addin" -Value $manifest -Encoding UTF8
    Write-Host "Installed for Revit $v -> $addinDir"
}
Write-Host "Done. Restart Revit."
