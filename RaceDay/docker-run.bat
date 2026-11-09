@echo off
REM Builds and runs the MVC container. Start start-api-for-docker.bat FIRST (separate window).
docker info >nul 2>&1
if errorlevel 1 (
  echo Docker Desktop is not running. Start it and wait for "Engine running", then try again.
  exit /b 1
)
docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc:part3 .
if errorlevel 1 exit /b 1
docker rm -f raceday-mvc >nul 2>&1
docker run -d --name raceday-mvc -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5100/ raceday-mvc:part3
docker ps
echo Open http://localhost:8080
