#!/bin/bash

# McpPaywall Deployment Script
# Automates build and deployment to remote server

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 McpPaywall Deployment Script${NC}"
echo "=================================="

# Get deployment configuration
read -p "Enter remote username (default: root): " REMOTE_USER
REMOTE_USER=${REMOTE_USER:-root}

read -p "Enter remote host (default: sup3r.cool): " REMOTE_HOST
REMOTE_HOST=${REMOTE_HOST:-sup3r.cool}

read -p "Enter service name (default: paywall): " SERVICE_NAME
SERVICE_NAME=${SERVICE_NAME:-paywall}

read -p "Enter remote path (default: /var/www/paywall): " REMOTE_PATH
REMOTE_PATH=${REMOTE_PATH:-/var/www/paywall}

echo ""
echo -e "${YELLOW}📋 Deployment Configuration:${NC}"
echo "  User: $REMOTE_USER"
echo "  Host: $REMOTE_HOST"
echo "  Service: $SERVICE_NAME"
echo "  Path: $REMOTE_PATH"
echo ""

read -p "Continue with deployment? (y/N): " CONFIRM
if [[ ! $CONFIRM =~ ^[Yy]$ ]]; then
    echo -e "${RED}❌ Deployment cancelled${NC}"
    exit 1
fi

echo ""
echo -e "${BLUE}🧹 Cleaning previous build...${NC}"
rm -rf ./publish/*

echo -e "${BLUE}🔨 Building application...${NC}"
dotnet publish -c Release -r linux-x64 --self-contained false -o ./publish

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Build failed!${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Build completed successfully${NC}"

echo ""
echo -e "${BLUE}🔄 Deploying to remote server...${NC}"

# Function to run SSH commands with error handling
run_ssh() {
    local cmd="$1"
    local desc="$2"
    
    echo -e "${YELLOW}  → $desc${NC}"
    if ssh -o ConnectTimeout=10 "$REMOTE_USER@$REMOTE_HOST" "$cmd"; then
        echo -e "${GREEN}    ✅ Success${NC}"
    else
        echo -e "${RED}    ❌ Failed: $desc${NC}"
        exit 1
    fi
}

# Stop service
run_ssh "sudo systemctl stop $SERVICE_NAME" "Stopping service"

# Clear remote directory
run_ssh "rm -rf $REMOTE_PATH/*" "Clearing remote directory"

# Upload files
echo -e "${YELLOW}  → Uploading files${NC}"
if scp -r -o ConnectTimeout=10 publish/* "$REMOTE_USER@$REMOTE_HOST:$REMOTE_PATH/"; then
    echo -e "${GREEN}    ✅ Upload completed${NC}"
else
    echo -e "${RED}    ❌ Upload failed${NC}"
    exit 1
fi

# Set permissions
run_ssh "chmod +x $REMOTE_PATH" "Setting executable permissions"

# Start service
run_ssh "sudo systemctl start $SERVICE_NAME" "Starting service"

# Check service status
run_ssh "sudo systemctl is-active $SERVICE_NAME --quiet" "Verifying service is running"

echo ""
echo -e "${GREEN}🎉 Deployment completed successfully!${NC}"
echo ""
echo -e "${BLUE}📊 Service Status:${NC}"
ssh "$REMOTE_USER@$REMOTE_HOST" "sudo systemctl status $SERVICE_NAME --no-pager -l"

echo ""
echo -e "${GREEN}✅ McpPaywall is now deployed and running${NC}"
echo -e "${BLUE}🌐 Check: https://$REMOTE_HOST/paywall${NC}"