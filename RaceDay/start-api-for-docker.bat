@echo off
REM API listening on ALL interfaces so the MVC Docker container can reach it via host.docker.internal.
REM Leave this window open while testing the container.
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --project RaceDay.Api --urls http://0.0.0.0:5100
