-- Migration: Backfill inventory from existing stock adjustments
-- Description: Updates inventory to reflect adjustments that were created before the trigger was fixed
-- Date: Backfill inventory adjustments

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT 'Backfilling inventory from existing stock adjustments...';
GO

-- Update inventory for all existing adjustments
MERGE dbo.tbl_inventories AS target
USING (
    SELECT 
        sa.variant_id,
        sa.size_id,
        sa.color_id,
        SUM(CASE 
            WHEN sa.adjustment_type = 'Increase' THEN sa.quantity_adjusted
            WHEN sa.adjustment_type = 'Decrease' THEN -sa.quantity_adjusted
            ELSE 0
        END) as net_adjustment
    FROM dbo.tbl_stock_adjustments sa
    WHERE sa.archives IS NULL
    GROUP BY sa.variant_id, sa.size_id, sa.color_id
) AS source
ON target.variant_id = source.variant_id 
    AND (target.size_id = source.size_id OR (target.size_id IS NULL AND source.size_id IS NULL))
    AND (target.color_id = source.color_id OR (target.color_id IS NULL AND source.color_id IS NULL))
    AND target.archives IS NULL
WHEN MATCHED THEN
    UPDATE SET 
        current_stock = CASE 
            WHEN (target.current_stock + source.net_adjustment) < 0 THEN 0
            ELSE target.current_stock + source.net_adjustment
        END,
        timestamps = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (variant_id, size_id, color_id, current_stock, reorder_level, timestamps)
    VALUES (
        source.variant_id, 
        source.size_id, 
        source.color_id, 
        CASE 
            WHEN source.net_adjustment < 0 THEN 0
            ELSE source.net_adjustment
        END, 
        0, 
        SYSUTCDATETIME()
    );
GO

PRINT 'Inventory backfilled successfully.';
PRINT 'Note: This script applies ALL adjustments. If adjustments were already applied, inventory may be double-counted.';
PRINT 'Please verify inventory counts after running this script.';
GO

