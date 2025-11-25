-- Migration: Create trigger for stock adjustments to update inventory
-- Description: Automatically updates tbl_inventories when stock adjustments are made
-- Date: Create trigger for stock adjustments

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Drop trigger if it exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TR_tbl_stock_adjustments_UpdateInventory') AND type = 'TR')
BEGIN
    DROP TRIGGER dbo.TR_tbl_stock_adjustments_UpdateInventory;
END
GO

-- Create trigger to update inventory on Stock Adjustment
CREATE TRIGGER TR_tbl_stock_adjustments_UpdateInventory
ON dbo.tbl_stock_adjustments
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    SET ANSI_NULLS ON;
    SET QUOTED_IDENTIFIER ON;
    SET ARITHABORT ON;
    
    BEGIN TRY
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
            FROM inserted sa
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
    END TRY
    BEGIN CATCH
        -- Log error but don't fail the insert
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        PRINT 'Error in TR_tbl_stock_adjustments_UpdateInventory: ' + @ErrorMessage;
        
        -- Re-throw if it's a critical error
        IF @ErrorSeverity > 16
        BEGIN
            RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        END
    END CATCH
END
GO

PRINT 'Trigger TR_tbl_stock_adjustments_UpdateInventory created successfully.';
GO

