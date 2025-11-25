SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Migration: Add supplier_id column to tbl_addresses table
-- Description: Associates addresses with suppliers
-- Date: Add supplier_id foreign key column

PRINT '';
PRINT '========================================';
PRINT 'Adding supplier_id to tbl_addresses...';
PRINT '========================================';
GO

-- ========================================
-- Step 1: Add supplier_id to tbl_addresses
-- ========================================
PRINT '';
PRINT 'Step 1: Adding supplier_id to tbl_addresses...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_addresses') AND name = 'supplier_id')
BEGIN
    ALTER TABLE dbo.tbl_addresses
    ADD supplier_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_addresses_supplier_id ON dbo.tbl_addresses(supplier_id);
    
    -- Add foreign key constraint
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_addresses
        ADD CONSTRAINT FK_tbl_addresses_supplier FOREIGN KEY (supplier_id) REFERENCES dbo.tbl_suppliers(id);
    END
    
    PRINT 'supplier_id column added to tbl_addresses.';
END
ELSE
BEGIN
    PRINT 'supplier_id column already exists in tbl_addresses.';
END
GO

PRINT '';
PRINT 'Migration completed: supplier_id column added to tbl_addresses table.';
GO

