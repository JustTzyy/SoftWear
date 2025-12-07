-- Migration: Add stock_in_group_key to tbl_supplier_payments
-- Description: Adds stock_in_group_key to link payments to stock-in groups (supplier + date)
-- Date: 2025-01-23

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Add stock_in_group_key column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND name = 'stock_in_group_key')
BEGIN
    ALTER TABLE dbo.tbl_supplier_payments
    ADD stock_in_group_key NVARCHAR(100) NULL;
    
    PRINT 'Added stock_in_group_key column to tbl_supplier_payments.';
END
ELSE
BEGIN
    PRINT 'Column stock_in_group_key already exists in tbl_supplier_payments.';
END
GO

-- Create index on stock_in_group_key if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND name = 'IX_tbl_supplier_payments_stock_in_group_key')
BEGIN
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_stock_in_group_key ON dbo.tbl_supplier_payments(stock_in_group_key) WHERE stock_in_group_key IS NOT NULL;
    PRINT 'Created index IX_tbl_supplier_payments_stock_in_group_key.';
END
ELSE
BEGIN
    PRINT 'Index IX_tbl_supplier_payments_stock_in_group_key already exists.';
END
GO

