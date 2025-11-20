#!/bin/bash

# TeamFlow Local Development Startup Script
# This script starts both the backend and frontend servers for local development
# Backend runs on http://localhost:5000 (uses MySQL from Docker container)
# Frontend runs on http://localhost:5173
#
# Prerequisites:
# - Docker installed and running
# - Database will be created automatically if it doesn't exist
# - User: root, Password: secret

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=========================================="
echo "TeamFlow Local Development Environment"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "${YELLOW}⚠ Docker is not running. Please start Docker and try again.${NC}"
    exit 1
fi

echo "${BLUE}Checking Docker MySQL container...${NC}"

# Check if MySQL container exists
if ! docker ps -a --format '{{.Names}}' | grep -q "^mysql-db$"; then
    echo "${BLUE}Creating MySQL container from docker-compose...${NC}"
    cd Docker
    docker-compose -f mysql-adm-docker-compose.yml up -d
    cd ..
    sleep 3
elif ! docker ps --format '{{.Names}}' | grep -q "^mysql-db$"; then
    echo "${BLUE}Starting MySQL container...${NC}"
    docker start mysql-db
    sleep 3
else
    echo "${GREEN}✓ MySQL container is running${NC}"
fi

# Create database if it doesn't exist
echo "${BLUE}Ensuring TeamFlowDb_Dev database exists...${NC}"
docker exec -it mysql-db mysql -uroot -psecret -e "CREATE DATABASE IF NOT EXISTS TeamFlowDb_Dev;" 2>/dev/null || true

echo ""
echo "Starting services..."
echo ""

# Check if node_modules exists in Frontend
if [ ! -d "Frontend/node_modules" ]; then
    echo "${BLUE}Installing frontend dependencies...${NC}"
    cd Frontend
    npm install
    cd ..
fi

echo ""
echo "${GREEN}✓ Starting Backend (.NET)${NC}"
echo "  URL: http://localhost:5000"
echo "  API: http://localhost:5000/api"
echo ""

echo "${GREEN}✓ Starting Frontend (Vite React)${NC}"
echo "  URL: http://localhost:5173"
echo ""

echo "=========================================="
echo "Press Ctrl+C to stop all services"
echo "=========================================="
echo ""

# Start backend in background
(
    cd Backend/TeamFlow.Api
    export ASPNETCORE_ENVIRONMENT=Development
    export ASPNETCORE_URLS="http://0.0.0.0:5000"
    dotnet run
) &

BACKEND_PID=$!

# Give backend a moment to start
sleep 3

# Start frontend in background
(
    cd Frontend
    npm run dev
) &

FRONTEND_PID=$!

# Trap SIGINT to kill both processes
trap "kill $BACKEND_PID $FRONTEND_PID 2>/dev/null; echo ''; echo 'Services stopped'; exit 0" SIGINT

# Wait for both processes
wait $BACKEND_PID $FRONTEND_PID

