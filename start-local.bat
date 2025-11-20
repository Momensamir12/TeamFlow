@echo off
REM TeamFlow Local Development Startup Script (Windows)
REM This script starts both the backend and frontend servers for local development
REM Backend runs on http://localhost:5000
REM Frontend runs on http://localhost:5173

setlocal enabledelayedexpansion

cd /d "%~dp0"

echo ==========================================
echo TeamFlow Local Development Environment
echo ==========================================
echo.
echo Starting services...
echo.

REM Check if node_modules exists in Frontend
if not exist "Frontend\node_modules" (
    echo Installing frontend dependencies...
    cd Frontend
    call npm install
    cd ..
)

echo.
echo [32m✓ Starting Backend (.NET)[0m
echo   URL: http://localhost:5000
echo   API: http://localhost:5000/api
echo.

echo [32m✓ Starting Frontend (Vite React)[0m
echo   URL: http://localhost:5173
echo.

echo ==========================================
echo Press Ctrl+C to stop all services
echo ==========================================
echo.

REM Start backend in new window
cd Backend\TeamFlow.Api
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://0.0.0.0:5000
start "TeamFlow Backend" cmd /k "dotnet run"
cd ..\..

REM Give backend a moment to start
timeout /t 3 /nobreak

REM Start frontend in new window
cd Frontend
start "TeamFlow Frontend" cmd /k "npm run dev"
cd ..

echo.
echo Services started in new windows. Close the windows to stop them.
pause
