#!/bin/bash

# McpPaywall Demo Runner
# This script sets up and runs the McpPaywall demonstration

echo "🔐 McpPaywall Demo Setup"
echo "======================="
echo

# Check for .NET 9.0
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK not found"
    echo "Please install .NET 9.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "✅ Found .NET SDK: $DOTNET_VERSION"

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Failed to restore packages"
    exit 1
fi

# Build the project
echo "🔨 Building project..."
dotnet build --no-restore

if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "✅ Build successful"
echo

# Display information
echo "🚀 Starting McpPaywall Demo"
echo "============================="
echo
echo "Demo will be available at:"
echo "  🌐 Web Interface: http://localhost:5000/demo"
echo "  💳 Paywall API:   http://localhost:5000/demo/paywall" 
echo "  🔧 MCP Endpoint:  http://localhost:5000/demo/mcp (requires payment)"
echo
echo "💰 Payment Info:"
echo "  Amount: 10 satoshis"
echo "  Method: Cashu eCash via Lightning"
echo "  Mint: https://mint.minibits.cash/Bitcoin"
echo
echo "Press Ctrl+C to stop the demo"
echo "=========================="
echo

# Run the application
dotnet run --no-build --no-restore