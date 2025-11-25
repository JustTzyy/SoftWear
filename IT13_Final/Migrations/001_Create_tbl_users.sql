    -- Migration: Create tbl_users table
-- Description: Creates the users table with all required columns, constraints, and foreign keys
-- Date: Generated migration for existing table schema

-- Drop table if exists (optional - uncomment if you want to recreate)
-- DROP TABLE IF EXISTS dbo.tbl_users;

-- Create tbl_users table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_users (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Authentication Fields
        email NVARCHAR(256) NOT NULL,
        pwd_hash NVARCHAR(256) NOT NULL,
        
        -- Name Fields
        name NVARCHAR(150) NULL,
        fname NVARCHAR(100) NULL,
        mname NVARCHAR(100) NULL,
        lname NVARCHAR(100) NULL,
        
        -- Contact Information
        contact_no NVARCHAR(30) NULL,
        
        -- Personal Information
        bday DATE NULL,
        age INT NULL,
        sex TINYINT NULL,
        
        -- Status Fields
        is_active BIT NOT NULL DEFAULT 1,
        must_change_pw BIT NOT NULL DEFAULT 0,
        
        -- Audit Fields
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        
        -- Foreign Key
        role_id INT NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_users PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_users_email ON dbo.tbl_users(email);
    CREATE NONCLUSTERED INDEX IX_tbl_users_role_id ON dbo.tbl_users(role_id);
    CREATE NONCLUSTERED INDEX IX_tbl_users_archived_at ON dbo.tbl_users(archived_at) WHERE archived_at IS NULL;
    
    -- Add Foreign Key Constraint (if tbl_roles table exists)
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_roles') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_users
        ADD CONSTRAINT FK_tbl_users_role_id 
        FOREIGN KEY (role_id) 
        REFERENCES dbo.tbl_roles(id);
    END;
    
    PRINT 'Table dbo.tbl_users created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_users already exists.';
END
GO




























