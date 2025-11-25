-- Migration: Fix double-counted adjustment
-- Description: Removes the duplicate adjustment that was already applied by the trigger
-- Date: Fix double counting

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT 'Fixing double-counted adjustment...';
GO

-- Remove the adjustment that was already applied (ID 6, which added +10)
-- This adjustment was created after the trigger was fixed, so it was already applied
-- The backfill script then applied it again, causing double-counting
UPDATE dbo.tbl_inventories 
SET current_stock = current_stock - 10,
    timestamps = SYSUTCDATETIME()
WHERE variant_id = 4 
    AND size_id = 4 
    AND color_id = 4 
    AND archives IS NULL;
GO

PRINT 'Double-counted adjustment fixed.';
PRINT 'Note: Going forward, all new stock adjustments will automatically update inventory.';
GO

