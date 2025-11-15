#!/usr/bin/env pwsh
# Script to build the Docker image locally

param(
    [string]$Tag = "latest",
    [string]$Registry = "ghcr.io",
    [string]$Owner = $env:GITHUB_REPOSITORY_OWNER,
    [switch]$NoCache
)

# Determine the repository owner
if (-not $Owner) {
    $gitRemote = git remote get-url origin 2>$null
    if ($gitRemote -match 'github\.com[:/]([^/]+)/') {
        $Owner = $Matches[1]
    } else {
        $Owner = "dr1rrb"  # Fallback
    }
}

$imageName = "$Registry/$Owner/wled-manager"
$dockerContext = "./src/WledManager"
$dockerfile = "$dockerContext/Dockerfile"

Write-Host "?? Building Docker image..." -ForegroundColor Cyan
Write-Host "   Image: $imageName:$Tag" -ForegroundColor Gray
Write-Host "   Context: $dockerContext" -ForegroundColor Gray
Write-Host "   Dockerfile: $dockerfile" -ForegroundColor Gray
Write-Host ""

$buildArgs = @(
    "build"
    "-t", "$imageName:$Tag"
    "-f", $dockerfile
)

if ($NoCache) {
  $buildArgs += "--no-cache"
}

$buildArgs += $dockerContext

& docker @buildArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To test the image:" -ForegroundColor Yellow
    Write-Host "  docker run -p 8080:8080 $imageName:$Tag" -ForegroundColor Gray
} else {
    Write-Host "? Build failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
