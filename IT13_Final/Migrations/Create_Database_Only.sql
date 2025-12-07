-- Quick script to create the database only
-- Run this first, then run Azure_SQL_Setup.sql

-- Connect to master database first
USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'db_SoftWear')
BEGIN
    CREATE DATABASE db_SoftWear
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Database db_SoftWear created successfully.';
END
ELSE
BEGIN
    PRINT 'Database db_SoftWear already exists.';
END
GO

-- Verify creation
SELECT name, database_id, create_date 
FROM sys.databases 
WHERE name = 'db_SoftWear';
GO












