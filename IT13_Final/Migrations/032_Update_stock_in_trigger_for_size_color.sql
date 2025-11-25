-- Migration: Update stock_in trigger to handle size_id and color_id
-- Description: Updates the trigger to properly handle inventory updates with size and color
-- Date: Update trigger for size/color support

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Drop existing trigger
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TR_tbl_stock_in_UpdateInventory') AND type = 'TR')
BEGIN
    DROP TRIGGER dbo.TR_tbl_stock_in_UpdateInventory;
END
GO

-- Create updated trigger that handles size_id and color_id
CREATE TRIGGER TR_tbl_stock_in_UpdateInventory
ON dbo.tbl_stock_in
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
                si.variant_id,
                si.size_id,
                si.color_id,
                SUM(si.quantity_added) as total_quantity
            FROM inserted si
            WHERE si.archives IS NULL
            GROUP BY si.variant_id, si.size_id, si.color_id
        ) AS source
        ON target.variant_id = source.variant_id 
            AND (target.size_id = source.size_id OR (target.size_id IS NULL AND source.size_id IS NULL))
            AND (target.color_id = source.color_id OR (target.color_id IS NULL AND source.color_id IS NULL))
            AND target.archives IS NULL
        WHEN MATCHED THEN
            UPDATE SET 
                current_stock = target.current_stock + source.total_quantity,
                timestamps = SYSUTCDATETIME()
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (variant_id, size_id, color_id, current_stock, reorder_level, timestamps)
            VALUES (
                source.variant_id, 
                source.size_id, 
                source.color_id, 
                source.total_quantity, 
                0, 
                SYSUTCDATETIME()
            );
    END TRY
    BEGIN CATCH
        -- Log error but don't fail the insert
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        PRINT 'Error in TR_tbl_stock_in_UpdateInventory: ' + @ErrorMessage;
        
        -- Re-throw if it's a critical error
        IF @ErrorSeverity > 16
        BEGIN
            RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        END
    END CATCH
END
GO

PRINT 'Trigger TR_tbl_stock_in_UpdateInventory updated successfully.';
GO

