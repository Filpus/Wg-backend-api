@echo off
setlocal

REM ========================================
REM dotnet format helper script
REM Usage:
REM   format              -> runs both (default)
REM   format both         -> runs both
REM   format style        -> runs style only
REM   format whitespace   -> runs whitespace only
REM ========================================

set PROJECT=./Wg-backend-api/Wg-backend-api.csproj

set MODE=%1
if "%MODE%"=="" set MODE=both

echo ----------------------------------------
echo Running dotnet format for project: %PROJECT%
echo Mode: %MODE%
echo ----------------------------------------

if /I "%MODE%"=="both" (
    echo Formatting style...
    dotnet format style "%PROJECT%" --exclude ./Wg-backend-api/Program.cs
    echo.
    echo Formatting whitespace...
    dotnet format whitespace "%PROJECT%" --exclude ./Wg-backend-api/Program.cs
) else if /I "%MODE%"=="style" (
    echo Formatting style only...
    dotnet format style "%PROJECT%" --exclude ./Wg-backend-api/Program.cs
) else if /I "%MODE%"=="whitespace" (
    echo Formatting whitespace only...
    dotnet format whitespace "%PROJECT%" --exclude ./Wg-backend-api/Program.cs
) else (
    echo Invalid option: %MODE%
    echo.
    echo Usage:
    echo   format.bat [both^|style^|whitespace]
    exit /b 1
)

echo.
echo Done.
endlocal