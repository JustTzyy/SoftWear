-- Migration: Create triggers to automatically update inventory on stock in/out
-- Description: Creates triggers that automatically update tbl_inventories when stock_in or stock_out records are inserted
-- Date: Generated migration for inventory update triggers

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ========================================
-- Trigger: Update inventory on Stock In
-- ========================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TR_tbl_stock_in_UpdateInventory') AND type = 'TR')
BEGIN
    DROP TRIGGER dbo.TR_tbl_stock_in_UpdateInventory;
END
GO

CREATE TRIGGER TR_tbl_stock_in_UpdateInventory
ON dbo.tbl_stock_in
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update or insert inventory record for each inserted stock_in record
    MERGE dbo.tbl_inventories AS target
    USING (
        SELECT variant_id, SUM(quantity_added) as total_quantity
        FROM inserted
        WHERE archives IS NULL
        GROUP BY variant_id
    ) AS source
    ON target.variant_id = source.variant_id AND target.archives IS NULL
    WHEN MATCHED THEN
        UPDATE SET 
            current_stock = target.current_stock + source.total_quantity,
            timestamps = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (variant_id, current_stock, reorder_level, timestamps)
        VALUES (source.variant_id, source.total_quantity, 0, SYSUTCDATETIME());
END
GO

-- ========================================
-- Trigger: Update inventory on Stock Out
-- ========================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TR_tbl_stock_out_UpdateInventory') AND type = 'TR')
BEGIN
    DROP TRIGGER dbo.TR_tbl_stock_out_UpdateInventory;
END
GO

CREATE TRIGGER TR_tbl_stock_out_UpdateInventory
ON dbo.tbl_stock_out
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update inventory record for each inserted stock_out record
    UPDATE i
    SET 
        current_stock = i.current_stock - so.total_quantity,
        timestamps = SYSUTCDATETIME()
    FROM dbo.tbl_inventories i
    INNER JOIN (
        SELECT variant_id, SUM(quantity_removed) as total_quantity
        FROM inserted
        WHERE archives IS NULL
        GROUP BY variant_id
    ) so ON i.variant_id = so.variant_id
    WHERE i.archives IS NULL;
END
GO

PRINT 'Triggers for automatic inventory updates created successfully.';
GO

