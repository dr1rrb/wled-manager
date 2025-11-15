#!/usr/bin/env pwsh
# Script to build and push the Docker image in a single command

param(
  [Parameter(Mandatory=$true)]
    [string]$Version,
  [string]$Registry = "ghcr.io",
    [string]$Owner = $env:GITHUB_REPOSITORY_OWNER,
    [switch]$NoCache,
    [switch]$AlsoTagLatest
)

$ErrorActionPreference = "Stop"

# Determine the repository owner
if (-not $Owner) {
    $gitRemote = git remote get-url origin 2>$null
    if ($gitRemote -match 'github\.com[:/]([^/]+)/') {
        $Owner = $Matches[1]
    } else {
        $Owner = "dr1rrb"  # Fallback
    }
}

Write-Host "?? Build and Push Pipeline" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build
Write-Host "?? Step 1/2: Building image..." -ForegroundColor Magenta
Write-Host ""

$buildParams = @{
    Tag = $Version
    Registry = $Registry
    Owner = $Owner
}

if ($NoCache) {
    $buildParams.NoCache = $true
}

& "$PSScriptRoot/build-local.ps1" @buildParams

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "? Pipeline failed at build step!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "?? Step 2/2: Pushing image..." -ForegroundColor Magenta
Write-Host ""

# Step 2: Push
$pushParams = @{
    Version = $Version
    Registry = $Registry
    Owner = $Owner
}

if ($AlsoTagLatest) {
    $pushParams.AlsoTagLatest = $true
}

& "$PSScriptRoot/push-ghcr.ps1" @pushParams

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "? Pipeline failed at push step!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "?? Pipeline completed successfully!" -ForegroundColor Green
