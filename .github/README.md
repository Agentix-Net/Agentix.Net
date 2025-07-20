# GitHub Actions & NuGet Publishing Setup

This document explains the CI/CD pipeline and NuGet package publishing setup for Agentix.Net.

## Overview

The Agentix.Net framework consists of multiple NuGet packages that are built and published automatically:

- **Agentix.Core** - Core framework
- **Agentix.Providers.Claude** - Claude AI provider
- **Agentix.Channels.Console** - Console channel adapter
- **Agentix.Channels.Slack** - Slack channel adapter

## Workflow Files

### 1. CI Workflow (`.github/workflows/ci.yml`)

**Triggers:**
- Push to `main`, `develop`, or `feature/*` branches
- Pull requests to `main` or `develop`
- Manual dispatch

**Jobs:**
- **Build & Test**: Builds solution in Debug and Release configurations, runs tests
- **Code Quality**: Runs code analysis and quality checks

**Features:**
- Matrix builds (Debug/Release)
- NuGet package caching
- Test result uploading
- Build artifact uploading (Release builds only)

### 2. Release Workflow (`.github/workflows/release.yml`)

**Triggers:**
- Git tags matching `v*.*.*` (e.g., `v1.0.0`)
- Manual dispatch with version input

**Jobs:**
- **Build & Publish**: Creates and publishes NuGet packages to NuGet.org
- **Publish to GitHub Packages**: Also publishes to GitHub Packages

**Features:**
- Automatic version detection from git tags
- Manual version override
- Prerelease detection
- GitHub Release creation
- Symbol package publishing

### 3. Preview Workflow (`.github/workflows/preview.yml`)

**Triggers:**
- Push to `develop` branch
- Manual dispatch

**Features:**
- Creates preview packages with timestamp versions
- Publishes only to GitHub Packages
- Useful for testing pre-release changes

## Required Secrets

Configure these secrets in your GitHub repository settings:

### NuGet.org Publishing
- `NUGET_API_KEY`: Your NuGet.org API key
  - Get from: https://www.nuget.org/account/apikeys
  - Permissions: Push new packages and package versions

### GitHub Packages
- `GITHUB_TOKEN`: Automatically provided by GitHub Actions
  - No configuration needed

## Version Management

### Centralized Versioning
- `Directory.Build.props` manages common properties and versioning
- Individual `.csproj` files inherit from this
- Version can be overridden during build

### Version Strategy
- **Development**: `0.0.1-preview.{timestamp}+{commit}`
- **Release**: `{major}.{minor}.{patch}` (from git tags)
- **Prerelease**: `{major}.{minor}.{patch}-{suffix}` (alpha, beta, rc)

## Package Configuration

Each project includes:
```xml
<PropertyGroup>
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  <PackageId>Agentix.PackageName</PackageId>
  <Description>Package description</Description>
  <PackageTags>ai;agents;agentix</PackageTags>
  <RepositoryUrl>https://github.com/agentix/agentix.net</RepositoryUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageReadmeFile>README.md</PackageReadmeFile>
</PropertyGroup>
```

## Local Development

### Build Scripts
- **Windows**: `build.ps1`
- **Unix/Linux/macOS**: `build.sh`

### Basic Usage
```bash
# Build everything
./build.ps1                    # Windows
./build.sh                     # Unix

# Build with custom version
./build.ps1 -Version "1.0.0"   # Windows
./build.sh -v "1.0.0"          # Unix

# Debug build, skip tests
./build.ps1 -Configuration Debug -SkipTests   # Windows
./build.sh -c Debug -t                        # Unix
```

### Manual Package Creation
```bash
# Clean and build
dotnet clean
dotnet restore
dotnet build --configuration Release

# Create packages
dotnet pack --configuration Release --output ./packages

# Publish to NuGet.org (requires API key)
dotnet nuget push "./packages/*.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Release Process

### 1. Automatic Releases (Recommended)
1. Ensure all changes are merged to `main`
2. Create and push a git tag:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
3. GitHub Actions will automatically:
   - Build and test the solution
   - Create NuGet packages
   - Publish to NuGet.org and GitHub Packages
   - Create a GitHub Release

### 2. Manual Releases
1. Go to Actions → Release workflow
2. Click "Run workflow"
3. Enter the version number
4. Specify if it's a prerelease
5. Click "Run workflow"

### 3. Preview Releases
Preview packages are automatically created when pushing to `develop` branch and published to GitHub Packages only.

## Consuming Packages

### From NuGet.org (Stable releases)
```xml
<PackageReference Include="Agentix.Core" Version="1.0.0" />
<PackageReference Include="Agentix.Providers.Claude" Version="1.0.0" />
<PackageReference Include="Agentix.Channels.Console" Version="1.0.0" />
```

### From GitHub Packages (Previews)
1. Add GitHub Packages as a source:
   ```xml
   <!-- In nuget.config -->
   <add key="github" value="https://nuget.pkg.github.com/agentix/index.json" />
   ```

2. Reference preview packages:
   ```xml
   <PackageReference Include="Agentix.Core" Version="0.0.1-preview.*" />
   ```

## Troubleshooting

### Common Issues

**Build fails with package restore errors:**
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Delete `bin/` and `obj/` folders
- Run `dotnet restore` again

**NuGet publish fails:**
- Verify API key is correct and has push permissions
- Check if package version already exists
- Ensure package ID matches exactly

**GitHub Actions fails:**
- Check workflow logs for specific errors
- Verify all required secrets are configured
- Ensure branch protection rules don't block the workflow

**Preview packages not appearing:**
- GitHub Packages may take a few minutes to appear
- Verify you have access to the repository packages
- Check the packages section of the GitHub repository

### Getting Help

- Check GitHub Actions logs for detailed error information
- Review the build scripts for local development issues
- Create an issue if you encounter problems with the CI/CD setup 