-- Migration: Create tbl_histories table
-- Description: Creates the audit/history log table with all required columns, constraints, and indexes
-- Date: Generated migration for existing table schema
-- Note: This table references tbl_users via foreign key

-- Drop table if exists (optional - uncomment if you want to recreate)
-- DROP TABLE IF EXISTS dbo.tbl_histories;

-- Create tbl_histories table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_histories') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_histories (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Foreign Key
        user_id INT NOT NULL,
        
        -- Audit Fields
        status NVARCHAR(32) NOT NULL,
        module NVARCHAR(64) NOT NULL,
        description NVARCHAR(256) NULL,
        ts DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_histories PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_histories_user_id ON dbo.tbl_histories(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_histories_ts ON dbo.tbl_histories(ts DESC);
    CREATE NONCLUSTERED INDEX IX_tbl_histories_status ON dbo.tbl_histories(status);
    CREATE NONCLUSTERED INDEX IX_tbl_histories_module ON dbo.tbl_histories(module);
    
    -- Add Foreign Key Constraint (if tbl_users table exists)
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_histories
        ADD CONSTRAINT FK_tbl_histories_user_id 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_histories created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_histories already exists.';
END
GO
















