@echo off
REM ================================
REM PostgreSQL Database Reset Script
REM ================================

set PGPASSWORD=postgres

echo Forcing all connections to close...
psql -U postgres -h localhost -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='wg';"

echo Dropping existing database...
psql -U postgres -h localhost -c "DROP DATABASE IF EXISTS wg;"

if errorlevel 1 (
    echo Failed to drop database. Exiting.
    exit /b 1
)

echo Creating new database...
psql -U postgres -h localhost -c "CREATE DATABASE wg;"

if errorlevel 1 (
    echo Failed to create database. Exiting.
    exit /b 1
)

echo Seeding database...
psql -U postgres -d wg -f ".\Wg-backend-api\Migrations\wg-init-db-seeder.sql"

if errorlevel 1 (
    echo Failed to run seeder.
    exit /b 1
)

echo Done!
