#!/bin/bash

# TeamFlow Local Development Startup Script
# This script starts both the backend and frontend servers for local development
# Backend runs on http://localhost:5000 (uses MySQL from Docker container)
# Frontend runs on http://localhost:5173
#
# Prerequisites:
# - MySQL container running on port 3306 (docker-compose up in Docker/ directory)
# - Database: TeamFlowDb_Dev
# - User: root, Password: secret

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=========================================="
echo "TeamFlow Local Development Environment"
echo "=========================================="
echo ""
echo "Starting services..."
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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
