-- Migration: Create tbl_addresses table
-- Description: Creates the addresses table with all required columns, constraints, and indexes
-- Date: Generated migration for existing table schema
-- Note: This table references tbl_users via foreign key

-- Drop table if exists (optional - uncomment if you want to recreate)
-- DROP TABLE IF EXISTS dbo.tbl_addresses;

-- Create tbl_addresses table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_addresses') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_addresses (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Address Fields
        street NVARCHAR(200) NULL,
        city NVARCHAR(100) NULL,
        province NVARCHAR(100) NULL,
        zip NVARCHAR(20) NULL,
        
        -- Audit Fields
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        
        -- Foreign Key
        user_id INT NOT NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_addresses PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_addresses_user_id ON dbo.tbl_addresses(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_addresses_archived_at ON dbo.tbl_addresses(archived_at) WHERE archived_at IS NULL;
    
    -- Add Foreign Key Constraint (if tbl_users table exists)
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_addresses
        ADD CONSTRAINT FK_tbl_addresses_user_id 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_addresses created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_addresses already exists.';
END
GO




























