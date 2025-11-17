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
        $version = $matches[1]

        # Check for wildcard version (e.g., "1.0.*")
        if ($version -match '\*') {
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Red
            Write-Host "ERROR: Wildcard Version Detected" -ForegroundColor Red
            Write-Host "========================================" -ForegroundColor Red
            Write-Host ""
            Write-Host "AssemblyVersion contains wildcard: $version" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "The automated release workflow requires a fixed version number." -ForegroundColor Yellow
            Write-Host "Wildcard versions (e.g., '1.0.*') cannot be used for version comparison." -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Please update the AssemblyInfo.cs file with a fixed version:" -ForegroundColor Cyan
            Write-Host "  File: $FilePath" -ForegroundColor White
            Write-Host "  Current: [assembly: AssemblyVersion(`"$version`")]" -ForegroundColor Red
            Write-Host "  Example: [assembly: AssemblyVersion(`"2.0.3`")]" -ForegroundColor Green
            Write-Host ""
            Write-Host "Steps to fix:" -ForegroundColor Cyan
            Write-Host "  1. Edit $FilePath" -ForegroundColor White
            Write-Host "  2. Replace wildcard version with fixed version (e.g., 2.0.3)" -ForegroundColor White
            Write-Host "  3. Also update AssemblyFileVersion to match" -ForegroundColor White
            Write-Host "  4. Commit and push the changes" -ForegroundColor White
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Red
            Write-Error "Wildcard version detected: $version. Please use a fixed version number."
            exit 1
        }

        return $version
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
