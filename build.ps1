#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Build script for Agentix.Net
.DESCRIPTION
    This script builds, tests, and packages the Agentix.Net solution.
.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release
.PARAMETER SkipTests
    Skip running tests
.PARAMETER SkipPack
    Skip creating NuGet packages
.PARAMETER Version
    Override version for packages
.PARAMETER Clean
    Clean before building
.EXAMPLE
    ./build.ps1
    ./build.ps1 -Configuration Debug -SkipTests
    ./build.ps1 -Version "1.0.0" -Clean
#>

param(
    [string]$Configuration = "Release",
    [switch]$SkipTests,
    [switch]$SkipPack,
    [string]$Version,
    [switch]$Clean,
    [switch]$Help
)

if ($Help) {
    Get-Help $PSCommandPath -Full
    exit 0
}

$ErrorActionPreference = "Stop"

# Colors for output
$Red = [ConsoleColor]::Red
$Green = [ConsoleColor]::Green
$Yellow = [ConsoleColor]::Yellow
$Blue = [ConsoleColor]::Blue

function Write-Step {
    param([string]$Message)
    Write-Host "🔨 $Message" -ForegroundColor $Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Red
}

# Ensure we're in the right directory
$scriptDir = Split-Path -Parent $PSCommandPath
Push-Location $scriptDir

try {
    Write-Host "🚀 Building Agentix.Net" -ForegroundColor $Blue
    Write-Host "Configuration: $Configuration" -ForegroundColor $Yellow
    
    # Check for .NET SDK
    Write-Step "Checking .NET SDK"
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Error ".NET SDK not found. Please install .NET 8.0 or later."
        exit 1
    }
    Write-Success ".NET SDK version: $dotnetVersion"
    
    # Clean if requested
    if ($Clean) {
        Write-Step "Cleaning solution"
        dotnet clean --configuration $Configuration --verbosity minimal
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Clean failed"
            exit 1
        }
        Write-Success "Clean completed"
    }
    
    # Restore packages
    Write-Step "Restoring NuGet packages"
    dotnet restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Restore failed"
        exit 1
    }
    Write-Success "Packages restored"
    
    # Build solution
    Write-Step "Building solution"
    $buildArgs = @(
        "build"
        "--configuration", $Configuration
        "--no-restore"
        "--verbosity", "minimal"
    )
    
    if ($Version) {
        $buildArgs += "-p:VersionPrefix=$Version"
        $buildArgs += "-p:VersionSuffix="
        Write-Host "Using version: $Version" -ForegroundColor $Yellow
    }
    
    & dotnet @buildArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed"
        exit 1
    }
    Write-Success "Build completed"
    
    # Run tests
    if (-not $SkipTests) {
        Write-Step "Running tests"
        dotnet test --configuration $Configuration --no-build --verbosity minimal --logger "console;verbosity=normal"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Tests failed"
            exit 1
        }
        Write-Success "All tests passed"
    } else {
        Write-Warning "Skipping tests"
    }
    
    # Create packages
    if (-not $SkipPack -and $Configuration -eq "Release") {
        Write-Step "Creating NuGet packages"
        $packDir = "./packages"
        if (Test-Path $packDir) {
            Remove-Item $packDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $packDir -Force | Out-Null
        
        $packArgs = @(
            "pack"
            "--configuration", $Configuration
            "--no-build"
            "--output", $packDir
            "--verbosity", "minimal"
        )
        
        if ($Version) {
            $packArgs += "-p:VersionPrefix=$Version"
            $packArgs += "-p:VersionSuffix="
        }
        
        & dotnet @packArgs
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Packaging failed"
            exit 1
        }
        
        # List created packages
        $packages = Get-ChildItem -Path $packDir -Filter "*.nupkg"
        Write-Success "Created $($packages.Count) packages:"
        foreach ($package in $packages) {
            Write-Host "  📦 $($package.Name)" -ForegroundColor $Green
        }
    } elseif ($Configuration -ne "Release") {
        Write-Warning "Skipping packaging (not Release configuration)"
    } else {
        Write-Warning "Skipping packaging"
    }
    
    Write-Success "Build completed successfully! 🎉"
    
} catch {
    Write-Error "Build failed with error: $_"
    exit 1
} finally {
    Pop-Location
} 