-- Migration: Add return_id column to subscription transactions
-- Description: Allows tracking of fee reversals when items are returned
-- Date: 2024-12-14

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Add return_id column to track fee reversals for returns
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_subscription_transactions') AND name = 'return_id')
BEGIN
    ALTER TABLE dbo.tbl_subscription_transactions
    ADD return_id INT NULL;
    
    PRINT 'Added return_id column to tbl_subscription_transactions';
END
ELSE
BEGIN
    PRINT 'return_id column already exists in tbl_subscription_transactions';
END
GO

-- Make sale_id nullable since returns won't have a sale_id
-- (Returns will have return_id instead)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_subscription_transactions') AND name = 'sale_id' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.tbl_subscription_transactions
    ALTER COLUMN sale_id INT NULL;
    
    PRINT 'Made sale_id column nullable in tbl_subscription_transactions';
END
GO

PRINT '';
PRINT 'Migration completed: Subscription transactions can now track return fee reversals.';
GO
