#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Checks if the project version has changed compared to the latest GitHub release.

.DESCRIPTION
    This script extracts the version from AssemblyInfo.cs, compares it with the
    latest GitHub release version, and determines if a new release should be created.

.PARAMETER AssemblyInfoPath
    Path to the AssemblyInfo.cs file

.PARAMETER GitHubRepo
    GitHub repository in format "owner/repo"

.PARAMETER GitHubToken
    GitHub token for API authentication
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$AssemblyInfoPath,

    [Parameter(Mandatory=$true)]
    [string]$GitHubRepo,

    [Parameter(Mandatory=$false)]
    [string]$GitHubToken = $env:GITHUB_TOKEN
)

# Function to extract version from AssemblyInfo.cs
function Get-AssemblyVersion {
    param([string]$FilePath)

    if (-not (Test-Path $FilePath)) {
        Write-Error "AssemblyInfo.cs not found at: $FilePath"
        exit 1
    }

    $content = Get-Content $FilePath -Raw

    # Match [assembly: AssemblyVersion("x.x.x")]
    if ($content -match '\[assembly:\s*AssemblyVersion\s*\(\s*"([^"]+)"\s*\)\]') {
        return $matches[1]
    }

    Write-Error "Could not extract version from AssemblyInfo.cs"
    exit 1
}

# Function to get latest GitHub release version
function Get-LatestGitHubRelease {
    param(
        [string]$Repo,
        [string]$Token
    )

    $headers = @{
        "Accept" = "application/vnd.github.v3+json"
        "User-Agent" = "GitHub-Actions"
    }

    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }

    $uri = "https://api.github.com/repos/$Repo/releases/latest"

    try {
        $response = Invoke-RestMethod -Uri $uri -Headers $headers -Method Get -ErrorAction Stop

        # Extract version from tag_name (remove 'v' prefix if present)
        $tagName = $response.tag_name
        if ($tagName -match '^v?(.+)$') {
            return $matches[1]
        }

        return $tagName
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 404) {
            Write-Warning "No releases found for repository $Repo"
            return "0.0.0"
        }

        Write-Error "Failed to fetch latest release: $_"
        exit 1
    }
}

# Function to compare versions
function Compare-Versions {
    param(
        [string]$Version1,
        [string]$Version2
    )

    try {
        $v1 = [version]$Version1
        $v2 = [version]$Version2

        if ($v1 -gt $v2) {
            return 1  # Version1 is newer
        }
        elseif ($v1 -eq $v2) {
            return 0  # Versions are equal
        }
        else {
            return -1  # Version1 is older
        }
    }
    catch {
        Write-Error "Failed to compare versions: $_"
        exit 1
    }
}

# Main execution
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Version Check for $GitHubRepo" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get current version from AssemblyInfo.cs
Write-Host "Reading version from: $AssemblyInfoPath" -ForegroundColor Yellow
$currentVersion = Get-AssemblyVersion -FilePath $AssemblyInfoPath
Write-Host "  Current version: $currentVersion" -ForegroundColor Green

# Get latest release version from GitHub
Write-Host "Fetching latest release from GitHub..." -ForegroundColor Yellow
$latestVersion = Get-LatestGitHubRelease -Repo $GitHubRepo -Token $GitHubToken
Write-Host "  Latest release: $latestVersion" -ForegroundColor Green

# Compare versions
Write-Host ""
Write-Host "Comparing versions..." -ForegroundColor Yellow
$comparison = Compare-Versions -Version1 $currentVersion -Version2 $latestVersion

$shouldRelease = $false
$releaseVersion = "v$currentVersion"

switch ($comparison) {
    1 {
        Write-Host "  ✓ Version has INCREASED: $latestVersion -> $currentVersion" -ForegroundColor Green
        Write-Host "  ✓ A new release SHOULD be created" -ForegroundColor Green
        $shouldRelease = $true
    }
    0 {
        Write-Host "  ⚠ Version is UNCHANGED: $currentVersion" -ForegroundColor Yellow
        Write-Host "  ⚠ No new release needed" -ForegroundColor Yellow
        $shouldRelease = $false
    }
    -1 {
        Write-Host "  ✗ Version has DECREASED: $latestVersion -> $currentVersion" -ForegroundColor Red
        Write-Host "  ✗ This is likely an error!" -ForegroundColor Red
        Write-Error "Version downgrade detected! Current version ($currentVersion) is older than latest release ($latestVersion)"
        exit 1
    }
}

# Set GitHub Actions outputs
Write-Host ""
Write-Host "Setting GitHub Actions outputs..." -ForegroundColor Yellow

if ($env:GITHUB_OUTPUT) {
    Add-Content -Path $env:GITHUB_OUTPUT -Value "current_version=$currentVersion"
    Add-Content -Path $env:GITHUB_OUTPUT -Value "latest_version=$latestVersion"
    Add-Content -Path $env:GITHUB_OUTPUT -Value "should_release=$($shouldRelease.ToString().ToLower())"
    Add-Content -Path $env:GITHUB_OUTPUT -Value "release_version=$releaseVersion"
    Add-Content -Path $env:GITHUB_OUTPUT -Value "version_changed=$($shouldRelease.ToString().ToLower())"

    Write-Host "  ✓ Outputs written to GITHUB_OUTPUT" -ForegroundColor Green
}
else {
    Write-Host "  current_version=$currentVersion"
    Write-Host "  latest_version=$latestVersion"
    Write-Host "  should_release=$($shouldRelease.ToString().ToLower())"
    Write-Host "  release_version=$releaseVersion"
    Write-Host "  version_changed=$($shouldRelease.ToString().ToLower())"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Version check completed successfully" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

exit 0
