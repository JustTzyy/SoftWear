-- ============================================
-- Create Database Script for Azure Portal
-- ============================================
-- Run this in Azure Portal Query Editor
-- Server: jussstzy.database.windows.net
-- ============================================

-- Connect to master database
USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'db_SoftWear')
BEGIN
    CREATE DATABASE db_SoftWear
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT '========================================';
    PRINT 'Database db_SoftWear created successfully!';
    PRINT '========================================';
END
ELSE
BEGIN
    PRINT 'Database db_SoftWear already exists.';
END
GO

-- Verify creation
SELECT 
    name AS DatabaseName,
    database_id,
    create_date,
    state_desc AS Status
FROM sys.databases 
WHERE name = 'db_SoftWear';
GO

PRINT '';
PRINT 'Next step: Go to the db_SoftWear database and run Azure_SQL_Setup.sql';
PRINT 'to create all tables and seed data.';
GO












