-- Migration: Add user_id to tbl_inventories
-- Description: Adds user_id column to track who last updated the inventory/reorder level
-- Date: Add user_id tracking to inventories

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Add user_id column to tbl_inventories if it doesn't exist
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND name = 'user_id')
    BEGIN
        ALTER TABLE dbo.tbl_inventories
        ADD user_id INT NULL;
        
        PRINT 'Column user_id added to dbo.tbl_inventories.';
        
        -- Create index for better performance
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND name = 'IX_tbl_inventories_user_id')
        BEGIN
            CREATE NONCLUSTERED INDEX IX_tbl_inventories_user_id ON dbo.tbl_inventories(user_id);
        END;
        
        -- Add Foreign Key Constraint
        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
        BEGIN
            ALTER TABLE dbo.tbl_inventories
            ADD CONSTRAINT FK_tbl_inventories_user 
            FOREIGN KEY (user_id) 
            REFERENCES dbo.tbl_users(id);
        END;
        
        PRINT 'Foreign key constraint FK_tbl_inventories_user added successfully.';
    END
    ELSE
    BEGIN
        PRINT 'Column user_id already exists in dbo.tbl_inventories.';
    END
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_inventories does not exist.';
END
GO

