-- Migration: Create tbl_inventories table
-- Description: Creates the inventories table with all required columns, constraints
-- Date: Generated migration for inventories table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_inventories table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_inventories (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Inventory Fields
        current_stock INT NOT NULL DEFAULT 0,
        reorder_level INT NOT NULL DEFAULT 0,
        
        -- Audit Fields
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        
        -- Foreign Key
        variant_id INT NOT NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_inventories PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_variant_id ON dbo.tbl_inventories(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_archives ON dbo.tbl_inventories(archives) WHERE archives IS NULL;
    
    -- Add Foreign Key Constraint (if tbl_variants table exists)
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_inventories
        ADD CONSTRAINT FK_tbl_inventories_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    -- Create unique constraint on variant_id to ensure one inventory record per variant
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND name = 'UQ_tbl_inventories_variant_id')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_inventories_variant_id ON dbo.tbl_inventories(variant_id) WHERE archives IS NULL;
    END;
    
    PRINT 'Table dbo.tbl_inventories created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_inventories already exists.';
END
GO

