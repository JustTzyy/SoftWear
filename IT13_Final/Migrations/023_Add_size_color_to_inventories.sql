-- Migration: Add size_id and color_id to tbl_inventories
-- Description: Adds size_id and color_id columns to support reorder levels per variant-size-color combination
-- Date: Generated migration

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Add size_id column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND name = 'size_id')
BEGIN
    ALTER TABLE dbo.tbl_inventories ADD size_id INT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_size_id ON dbo.tbl_inventories(size_id);
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_inventories ADD CONSTRAINT FK_tbl_inventories_size FOREIGN KEY (size_id) REFERENCES dbo.tbl_sizes(id);
    END
    PRINT 'size_id column added to tbl_inventories.';
END
ELSE
BEGIN
    PRINT 'size_id column already exists in tbl_inventories.';
END
GO

-- Add color_id column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND name = 'color_id')
BEGIN
    ALTER TABLE dbo.tbl_inventories ADD color_id INT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_color_id ON dbo.tbl_inventories(color_id);
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_inventories ADD CONSTRAINT FK_tbl_inventories_color FOREIGN KEY (color_id) REFERENCES dbo.tbl_colors(id);
    END
    PRINT 'color_id column added to tbl_inventories.';
END
ELSE
BEGIN
    PRINT 'color_id column already exists in tbl_inventories.';
END
GO

-- Drop the old unique constraint on variant_id only
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND name = 'UQ_tbl_inventories_variant_id')
BEGIN
    DROP INDEX UQ_tbl_inventories_variant_id ON dbo.tbl_inventories;
    PRINT 'Old unique constraint UQ_tbl_inventories_variant_id dropped.';
END
GO

-- Create new unique constraint on variant_id + size_id + color_id
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND name = 'UQ_tbl_inventories_variant_size_color')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_inventories_variant_size_color 
    ON dbo.tbl_inventories(variant_id, size_id, color_id) 
    WHERE archives IS NULL;
    PRINT 'New unique constraint UQ_tbl_inventories_variant_size_color created.';
END
ELSE
BEGIN
    PRINT 'Unique constraint UQ_tbl_inventories_variant_size_color already exists.';
END
GO

