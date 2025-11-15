-- Migration: Add permission request columns to tbl_users table
-- Description: Adds columns to store pending permission requests for seller personal info and address changes
-- Date: Add permission request tracking columns

-- Step 1: Add permission_request_type column
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.tbl_users') 
    AND name = 'permission_request_type'
)
BEGIN
    ALTER TABLE dbo.tbl_users
    ADD permission_request_type NVARCHAR(50) NULL;
    
    PRINT 'Column permission_request_type added to tbl_users table.';
END
ELSE
BEGIN
    PRINT 'Column permission_request_type already exists in tbl_users table.';
END
GO

-- Step 2: Add permission_request_data column (JSON string)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.tbl_users') 
    AND name = 'permission_request_data'
)
BEGIN
    ALTER TABLE dbo.tbl_users
    ADD permission_request_data NVARCHAR(MAX) NULL;
    
    PRINT 'Column permission_request_data added to tbl_users table.';
END
ELSE
BEGIN
    PRINT 'Column permission_request_data already exists in tbl_users table.';
END
GO

-- Step 3: Add permission_request_status column
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.tbl_users') 
    AND name = 'permission_request_status'
)
BEGIN
    ALTER TABLE dbo.tbl_users
    ADD permission_request_status NVARCHAR(20) NULL;
    
    PRINT 'Column permission_request_status added to tbl_users table.';
END
ELSE
BEGIN
    PRINT 'Column permission_request_status already exists in tbl_users table.';
END
GO

-- Step 4: Add permission_request_date column
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.tbl_users') 
    AND name = 'permission_request_date'
)
BEGIN
    ALTER TABLE dbo.tbl_users
    ADD permission_request_date DATETIME2(0) NULL;
    
    PRINT 'Column permission_request_date added to tbl_users table.';
END
ELSE
BEGIN
    PRINT 'Column permission_request_date already exists in tbl_users table.';
END
GO

-- Step 5: Create index on permission_request_status for better performance
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE object_id = OBJECT_ID(N'dbo.tbl_users') 
    AND name = 'IX_tbl_users_permission_request_status'
)
BEGIN
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;
    SET ANSI_PADDING ON;
    SET ANSI_WARNINGS ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;
    
    CREATE NONCLUSTERED INDEX IX_tbl_users_permission_request_status 
    ON dbo.tbl_users(permission_request_status)
    WHERE permission_request_status = 'pending';
    
    PRINT 'Index IX_tbl_users_permission_request_status created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_tbl_users_permission_request_status already exists.';
END
GO

PRINT '';
PRINT 'Migration completed: Permission request columns added to tbl_users table.';

