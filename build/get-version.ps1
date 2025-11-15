#!/usr/bin/env pwsh
# Script to calculate the current version using Nerdbank.GitVersioning

param(
    [switch]$ShowAll,
    [string]$Variable = "SemVer2"
)

Write-Host "🔍 Calculating version with Nerdbank.GitVersioning..." -ForegroundColor Cyan
Write-Host ""

function Ensure-NbgvInstalled {
    $nbgvCmd = Get-Command dotnet-nbgv -ErrorAction SilentlyContinue
    if ($nbgvCmd) {
        return
    }

    Write-Host "⚠️  Nerdbank.GitVersioning CLI not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global nbgv --version 3.*
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install Nerdbank.GitVersioning CLI!" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    $toolPath = Join-Path $env:USERPROFILE ".dotnet\tools"
    if ($env:PATH -notlike "*$toolPath*") {
        $env:PATH = "$env:PATH;$toolPath"
    }

    Write-Host "✅ Nerdbank.GitVersioning CLI installed successfully!" -ForegroundColor Green
    Write-Host ""
}

function Get-VersionInfo {
    Ensure-NbgvInstalled
    $json = dotnet nbgv get-version -f json
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ dotnet nbgv get-version failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    return $json | ConvertFrom-Json
}

$versionInfo = Get-VersionInfo

if ($ShowAll) {
    Write-Host "All version variables:" -ForegroundColor Yellow
    $versionInfo | ConvertTo-Json -Depth 4
    return
}

$aliases = @{
    "SemVer" = "SemVer2"
    "FullSemVer" = "AssemblyInformationalVersion"
    "MajorMinorPatch" = "SimpleVersion"
    "NuGetVersion" = "NuGetPackageVersion"
}

$propertyName = if ($aliases.ContainsKey($Variable)) { $aliases[$Variable] } else { $Variable }

if (-not $versionInfo.PSObject.Properties[$propertyName]) {
    Write-Host "⚠️  Unknown variable '$Variable'. Available keys:" -ForegroundColor Yellow
    $versionInfo.PSObject.Properties.Name | Sort-Object | ForEach-Object { Write-Host "  $_" }
    exit 1
}

$value = $versionInfo.$propertyName

Write-Host "✅ Version ($Variable): " -ForegroundColor Green -NoNewline
Write-Host $value -ForegroundColor White

if ($propertyName -eq "SemVer2") {
    Write-Host ""
    Write-Host "Useful variables:" -ForegroundColor Yellow
    Write-Host "  SemVer2:                $($versionInfo.SemVer2)" -ForegroundColor Gray
    Write-Host "  AssemblyInformational:  $($versionInfo.AssemblyInformationalVersion)" -ForegroundColor Gray
    Write-Host "  SimpleVersion:          $($versionInfo.SimpleVersion)" -ForegroundColor Gray
    Write-Host "  NuGetPackageVersion:    $($versionInfo.NuGetPackageVersion)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Run with -ShowAll to see the full JSON payload." -ForegroundColor DarkGray
}
