-- Migration Runner Script for SQL Server Management Studio (SSMS)
-- 
-- NOTE: This file uses :r commands which are sqlcmd-specific.
-- If you see syntax errors in SSMS, this is expected - the :r command
-- only works when using sqlcmd from command line.
--
-- For SSMS: Open and run each migration file individually in order:
--   1. 002_Create_tbl_roles.sql
--   2. 001_Create_tbl_users.sql
--   3. 003_Create_tbl_histories.sql
--   4. 004_Create_tbl_addresses.sql
--
-- OR use the batch file: Run_Migrations.bat (double-click it)
--
-- ========================================

USE db_SoftWear;
GO

PRINT '========================================';
PRINT 'IMPORTANT: This script uses :r commands';
PRINT 'which only work with sqlcmd, not SSMS.';
PRINT '';
PRINT 'For SSMS: Run each migration file individually';
PRINT 'For Command Line: Use Run_Migrations.bat instead';
PRINT '========================================';
GO

-- The :r commands below will only work with sqlcmd
-- If you want to use this in SSMS, you'll need to run each file separately

/*
-- Step 1: Create roles table
:r "002_Create_tbl_roles.sql"
GO

-- Step 2: Create users table
:r "001_Create_tbl_users.sql"
GO

-- Step 3: Create histories table
:r "003_Create_tbl_histories.sql"
GO

-- Step 4: Create addresses table
:r "004_Create_tbl_addresses.sql"
GO
*/
