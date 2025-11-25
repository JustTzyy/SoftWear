-- Migration: Add updated_by to tbl_purchase_orders
-- Description: Adds updated_by column to track who last modified the PO status
-- Date: Add updated_by tracking to purchase orders

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Add updated_by column to tbl_purchase_orders if it doesn't exist
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND name = 'updated_by')
    BEGIN
        ALTER TABLE dbo.tbl_purchase_orders
        ADD updated_by INT NULL;
        
        PRINT 'Column updated_by added to dbo.tbl_purchase_orders.';
        
        -- Create index for better performance
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND name = 'IX_tbl_purchase_orders_updated_by')
        BEGIN
            CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_updated_by ON dbo.tbl_purchase_orders(updated_by);
        END;
        
        -- Add Foreign Key Constraint
        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
        BEGIN
            ALTER TABLE dbo.tbl_purchase_orders
            ADD CONSTRAINT FK_tbl_purchase_orders_updated_by 
            FOREIGN KEY (updated_by) 
            REFERENCES dbo.tbl_users(id);
        END;
        
        PRINT 'Foreign key constraint FK_tbl_purchase_orders_updated_by added successfully.';
    END
    ELSE
    BEGIN
        PRINT 'Column updated_by already exists in dbo.tbl_purchase_orders.';
    END
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_purchase_orders does not exist.';
END
GO

