#!/usr/bin/env pwsh
# Script to calculate the current version using GitVersion

param(
    [switch]$ShowAll,
    [string]$Variable = "SemVer"
)

Write-Host "?? Calculating version with GitVersion..." -ForegroundColor Cyan
Write-Host ""

# Check if GitVersion is installed
$gitVersionCmd = Get-Command dotnet-gitversion -ErrorAction SilentlyContinue
if (-not $gitVersionCmd) {
    $gitVersionCmd = Get-Command gitversion -ErrorAction SilentlyContinue
}

if (-not $gitVersionCmd) {
    Write-Host "??  GitVersion not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global GitVersion.Tool
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Failed to install GitVersion!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
 
    Write-Host "? GitVersion installed successfully!" -ForegroundColor Green
    Write-Host ""
}

# Calculate version
if ($ShowAll) {
    Write-Host "All version variables:" -ForegroundColor Yellow
    & dotnet gitversion
} else {
    $version = & dotnet gitversion /showvariable $Variable
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Version: " -ForegroundColor Green -NoNewline
        Write-Host $version -ForegroundColor White
     
        if ($Variable -eq "SemVer") {
            Write-Host ""
            Write-Host "Useful variables:" -ForegroundColor Yellow
            Write-Host "  SemVer:          $(& dotnet gitversion /showvariable SemVer)" -ForegroundColor Gray
            Write-Host "  FullSemVer:      $(& dotnet gitversion /showvariable FullSemVer)" -ForegroundColor Gray
            Write-Host "  MajorMinorPatch: $(& dotnet gitversion /showvariable MajorMinorPatch)" -ForegroundColor Gray
            Write-Host "  NuGetVersion:    $(& dotnet gitversion /showvariable NuGetVersionV2)" -ForegroundColor Gray
            Write-Host ""
            Write-Host "Run with -ShowAll to see all variables" -ForegroundColor DarkGray
        }
    } else {
        Write-Host "? Failed to calculate version!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}
