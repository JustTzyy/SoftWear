-- Migration: Add po_id column to tbl_supplier_payments
-- Description: Adds po_id column to link payments directly to Purchase Orders
-- Date: 2025-01-23

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Add po_id column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND name = 'po_id')
BEGIN
    ALTER TABLE dbo.tbl_supplier_payments
    ADD po_id INT NULL;
    
    PRINT 'Added po_id column to tbl_supplier_payments.';
END
ELSE
BEGIN
    PRINT 'Column po_id already exists in tbl_supplier_payments.';
END
GO

-- Create index on po_id if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND name = 'IX_tbl_supplier_payments_po_id')
BEGIN
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_po_id ON dbo.tbl_supplier_payments(po_id) WHERE po_id IS NOT NULL;
    PRINT 'Created index IX_tbl_supplier_payments_po_id.';
END
ELSE
BEGIN
    PRINT 'Index IX_tbl_supplier_payments_po_id already exists.';
END
GO

-- Add foreign key constraint if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_tbl_supplier_payments_po')
BEGIN
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_payments
        ADD CONSTRAINT FK_tbl_supplier_payments_po 
        FOREIGN KEY (po_id) 
        REFERENCES dbo.tbl_purchase_orders(id);
        
        PRINT 'Added foreign key constraint FK_tbl_supplier_payments_po.';
    END
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_tbl_supplier_payments_po already exists.';
END
GO

-- Make invoice_id nullable if it's not already
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND name = 'invoice_id' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.tbl_supplier_payments
    ALTER COLUMN invoice_id INT NULL;
    
    PRINT 'Made invoice_id nullable in tbl_supplier_payments.';
END
ELSE
BEGIN
    PRINT 'Column invoice_id is already nullable or does not exist.';
END
GO

