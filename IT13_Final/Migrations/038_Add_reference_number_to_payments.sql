-- Migration: Add reference_number to tbl_payments
-- Description: Adds reference_number field for GCash and other digital payment transaction IDs
-- Date: 2025-01-23

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Add reference_number column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_payments') AND name = 'reference_number')
BEGIN
    ALTER TABLE dbo.tbl_payments
    ADD reference_number NVARCHAR(100) NULL;
    
    PRINT 'Column reference_number added to tbl_payments successfully.';
END
ELSE
BEGIN
    PRINT 'Column reference_number already exists in tbl_payments.';
END
GO

-- Create index for reference_number if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_payments') AND name = 'IX_tbl_payments_reference_number')
BEGIN
    CREATE NONCLUSTERED INDEX IX_tbl_payments_reference_number ON dbo.tbl_payments(reference_number) WHERE reference_number IS NOT NULL;
    PRINT 'Index IX_tbl_payments_reference_number created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_tbl_payments_reference_number already exists.';
END
GO

