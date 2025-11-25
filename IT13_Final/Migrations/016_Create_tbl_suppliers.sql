-- Migration: Create tbl_suppliers table
-- Description: Creates the suppliers table with all required columns, constraints
-- Date: Generated migration for suppliers table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_suppliers table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_suppliers (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Supplier Fields
        company_name NVARCHAR(200) NOT NULL,
        contact_person NVARCHAR(150) NULL,
        email NVARCHAR(256) NULL,
        contact_number NVARCHAR(30) NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'Active',
        
        -- Audit Fields
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        
        -- Foreign Key
        user_id INT NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_suppliers PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_suppliers_company_name ON dbo.tbl_suppliers(company_name);
    CREATE NONCLUSTERED INDEX IX_tbl_suppliers_archived_at ON dbo.tbl_suppliers(archived_at) WHERE archived_at IS NULL;
    
    -- Add Foreign Key Constraint (if tbl_users table exists)
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_suppliers
        ADD CONSTRAINT FK_tbl_suppliers_user 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    -- Create index on user_id
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND name = 'IX_tbl_suppliers_user_id')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_tbl_suppliers_user_id ON dbo.tbl_suppliers(user_id);
    END;
    
    PRINT 'Table dbo.tbl_suppliers created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_suppliers already exists.';
END
GO

