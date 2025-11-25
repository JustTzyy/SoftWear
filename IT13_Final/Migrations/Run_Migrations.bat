@echo off
REM Migration Runner Batch Script
REM This script runs all migrations using sqlcmd in the correct order
REM Make sure you're in the Migrations folder when running this

echo ========================================
echo Running Database Migrations
echo ========================================
echo.

echo Step 1: Creating tbl_roles table...
sqlcmd -S localhost\SQLEXPRESS -d db_SoftWear -i "002_Create_tbl_roles.sql"
if %errorlevel% neq 0 (
    echo Error running 002_Create_tbl_roles.sql
    pause
    exit /b %errorlevel%
)

echo.
echo Step 2: Creating tbl_users table...
sqlcmd -S localhost\SQLEXPRESS -d db_SoftWear -i "001_Create_tbl_users.sql"
if %errorlevel% neq 0 (
    echo Error running 001_Create_tbl_users.sql
    pause
    exit /b %errorlevel%
)

echo.
echo Step 3: Creating tbl_histories table...
sqlcmd -S localhost\SQLEXPRESS -d db_SoftWear -i "003_Create_tbl_histories.sql"
if %errorlevel% neq 0 (
    echo Error running 003_Create_tbl_histories.sql
    pause
    exit /b %errorlevel%
)

echo.
echo Step 4: Creating tbl_addresses table...
sqlcmd -S localhost\SQLEXPRESS -d db_SoftWear -i "004_Create_tbl_addresses.sql"
if %errorlevel% neq 0 (
    echo Error running 004_Create_tbl_addresses.sql
    pause
    exit /b %errorlevel%
)

echo.
echo ========================================
echo All migrations completed successfully!
echo ========================================
pause




























