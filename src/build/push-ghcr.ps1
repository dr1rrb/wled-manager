#!/usr/bin/env pwsh
# Script to push the Docker image to GitHub Container Registry

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    [string]$Registry = "ghcr.io",
    [string]$Owner = $env:GITHUB_REPOSITORY_OWNER,
    [switch]$AlsoTagLatest
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

Write-Host "?? Pushing Docker image to GHCR..." -ForegroundColor Cyan
Write-Host "   Image: $imageName:$Version" -ForegroundColor Gray
Write-Host ""

# Check if the image exists locally
$localImage = docker images -q "$imageName:$Version" 2>$null
if (-not $localImage) {
    Write-Host "? Image $imageName:$Version not found locally!" -ForegroundColor Red
    Write-Host "   Run './build/build-local.ps1 -Tag $Version' first" -ForegroundColor Yellow
    exit 1
}

# Login to GHCR (uses token from stdin or environment variable)
if ($env:GITHUB_TOKEN) {
    Write-Host "?? Logging in to GHCR..." -ForegroundColor Cyan
    $env:GITHUB_TOKEN | docker login $Registry -u $Owner --password-stdin
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Login failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} else {
    Write-Host "??  GITHUB_TOKEN not set, assuming already logged in..." -ForegroundColor Yellow
}

# Push the version
Write-Host "?? Pushing $imageName:$Version..." -ForegroundColor Cyan
docker push "$imageName:$Version"

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Push failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Tag and push latest if requested
if ($AlsoTagLatest) {
    Write-Host ""
    Write-Host "???  Tagging as latest..." -ForegroundColor Cyan
    docker tag "$imageName:$Version" "$imageName:latest"
    
    Write-Host "?? Pushing $imageName:latest..." -ForegroundColor Cyan
    docker push "$imageName:latest"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Push latest failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Write-Host ""
Write-Host "? Push successful!" -ForegroundColor Green
Write-Host ""
Write-Host "Image available at:" -ForegroundColor Yellow
Write-Host "  $imageName:$Version" -ForegroundColor Gray
if ($AlsoTagLatest) {
    Write-Host "  $imageName:latest" -ForegroundColor Gray
}
