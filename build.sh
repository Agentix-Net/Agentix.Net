#!/bin/bash

# Build script for Agentix.Net on Unix systems

set -e

# Default values
CONFIGURATION="Release"
SKIP_TESTS=false
SKIP_PACK=false
VERSION=""
CLEAN=false

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
function write_step() {
    echo -e "${BLUE}🔨 $1${NC}"
}

function write_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

function write_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

function write_error() {
    echo -e "${RED}❌ $1${NC}"
}

function show_help() {
    cat << EOF
Usage: $0 [options]

Build script for Agentix.Net

Options:
    -c, --configuration CONFIGURATION  Build configuration (Debug or Release). Default: Release
    -t, --skip-tests                   Skip running tests
    -p, --skip-pack                    Skip creating NuGet packages
    -v, --version VERSION              Override version for packages
    --clean                            Clean before building
    -h, --help                         Show this help message

Examples:
    $0
    $0 -c Debug -t
    $0 -v "1.0.0" --clean
EOF
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -t|--skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        -p|--skip-pack)
            SKIP_PACK=true
            shift
            ;;
        -v|--version)
            VERSION="$2"
            shift 2
            ;;
        --clean)
            CLEAN=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Change to script directory
cd "$(dirname "$0")"

echo -e "${BLUE}🚀 Building Agentix.Net${NC}"
echo -e "${YELLOW}Configuration: $CONFIGURATION${NC}"

# Check for .NET SDK
write_step "Checking .NET SDK"
if ! command -v dotnet &> /dev/null; then
    write_error ".NET SDK not found. Please install .NET 8.0 or later."
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
write_success ".NET SDK version: $DOTNET_VERSION"

# Clean if requested
if [ "$CLEAN" = true ]; then
    write_step "Cleaning solution"
    dotnet clean --configuration "$CONFIGURATION" --verbosity minimal
    write_success "Clean completed"
fi

# Restore packages
write_step "Restoring NuGet packages"
dotnet restore --verbosity minimal
write_success "Packages restored"

# Build solution
write_step "Building solution"
BUILD_ARGS=(
    "build"
    "--configuration" "$CONFIGURATION"
    "--no-restore"
    "--verbosity" "minimal"
)

if [ -n "$VERSION" ]; then
    BUILD_ARGS+=("-p:VersionPrefix=$VERSION")
    BUILD_ARGS+=("-p:VersionSuffix=")
    echo -e "${YELLOW}Using version: $VERSION${NC}"
fi

dotnet "${BUILD_ARGS[@]}"
write_success "Build completed"

# Run tests
if [ "$SKIP_TESTS" = false ]; then
    write_step "Running tests"
    dotnet test --configuration "$CONFIGURATION" --no-build --verbosity minimal --logger "console;verbosity=normal"
    write_success "All tests passed"
else
    write_warning "Skipping tests"
fi

# Create packages
if [ "$SKIP_PACK" = false ] && [ "$CONFIGURATION" = "Release" ]; then
    write_step "Creating NuGet packages"
    PACK_DIR="./packages"
    rm -rf "$PACK_DIR"
    mkdir -p "$PACK_DIR"
    
    PACK_ARGS=(
        "pack"
        "--configuration" "$CONFIGURATION"
        "--no-build"
        "--output" "$PACK_DIR"
        "--verbosity" "minimal"
    )
    
    if [ -n "$VERSION" ]; then
        PACK_ARGS+=("-p:VersionPrefix=$VERSION")
        PACK_ARGS+=("-p:VersionSuffix=")
    fi
    
    dotnet "${PACK_ARGS[@]}"
    
    # List created packages
    PACKAGE_COUNT=$(find "$PACK_DIR" -name "*.nupkg" | wc -l)
    write_success "Created $PACKAGE_COUNT packages:"
    find "$PACK_DIR" -name "*.nupkg" -exec basename {} \; | sed 's/^/  📦 /'
elif [ "$CONFIGURATION" != "Release" ]; then
    write_warning "Skipping packaging (not Release configuration)"
else
    write_warning "Skipping packaging"
fi

write_success "Build completed successfully! 🎉" 