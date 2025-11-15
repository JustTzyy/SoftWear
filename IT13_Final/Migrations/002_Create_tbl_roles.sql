-- Migration: Create tbl_roles table
-- Description: Creates the roles table with all required columns and constraints
-- Date: Generated migration for existing table schema
-- Note: This table is referenced by tbl_users via foreign key

-- Drop table if exists (optional - uncomment if you want to recreate)
-- DROP TABLE IF EXISTS dbo.tbl_roles;

-- Create tbl_roles table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_roles') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_roles (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Role Information
        name NVARCHAR(100) NOT NULL,
        [desc] NVARCHAR(500) NULL,
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_roles PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Index for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_roles_name ON dbo.tbl_roles(name);
    
    -- Create Unique Constraint on name to prevent duplicate role names
    ALTER TABLE dbo.tbl_roles
    ADD CONSTRAINT UQ_tbl_roles_name UNIQUE (name);
    
    PRINT 'Table dbo.tbl_roles created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_roles already exists.';
END
GO
















