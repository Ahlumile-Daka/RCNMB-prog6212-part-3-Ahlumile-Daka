@echo off
REM Starts the API (http://localhost:5100) and the MVC site (http://localhost:5280) together.
REM Run from this folder (the one containing RaceDay.sln).
set ASPNETCORE_ENVIRONMENT=Development
start "RaceDay API" cmd /k dotnet run --project RaceDay.Api --urls http://localhost:5100
echo Waiting for the API to start...
timeout /t 10 /nobreak >nul
start "RaceDay MVC" cmd /k dotnet run --project RaceDay.Mvc --urls http://localhost:5280
timeout /t 6 /nobreak >nul
start http://localhost:5280
