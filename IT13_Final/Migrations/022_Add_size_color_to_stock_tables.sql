-- Migration: Add size_id and color_id to stock_in and stock_out tables
-- Description: Adds size_id and color_id columns to track specific variant-size-color combinations
-- Date: Generated migration for size and color tracking

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ========================================
-- Add size_id and color_id to tbl_stock_in
-- ========================================
PRINT '';
PRINT 'Adding size_id and color_id to tbl_stock_in...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_in') AND name = 'size_id')
BEGIN
    ALTER TABLE dbo.tbl_stock_in
    ADD size_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_size_id ON dbo.tbl_stock_in(size_id);
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_in
        ADD CONSTRAINT FK_tbl_stock_in_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    PRINT 'size_id column added to tbl_stock_in.';
END
ELSE
BEGIN
    PRINT 'size_id column already exists in tbl_stock_in.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_in') AND name = 'color_id')
BEGIN
    ALTER TABLE dbo.tbl_stock_in
    ADD color_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_color_id ON dbo.tbl_stock_in(color_id);
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_in
        ADD CONSTRAINT FK_tbl_stock_in_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'color_id column added to tbl_stock_in.';
END
ELSE
BEGIN
    PRINT 'color_id column already exists in tbl_stock_in.';
END
GO

-- ========================================
-- Add size_id and color_id to tbl_stock_out
-- ========================================
PRINT '';
PRINT 'Adding size_id and color_id to tbl_stock_out...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_out') AND name = 'size_id')
BEGIN
    ALTER TABLE dbo.tbl_stock_out
    ADD size_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_size_id ON dbo.tbl_stock_out(size_id);
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_out
        ADD CONSTRAINT FK_tbl_stock_out_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    PRINT 'size_id column added to tbl_stock_out.';
END
ELSE
BEGIN
    PRINT 'size_id column already exists in tbl_stock_out.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_out') AND name = 'color_id')
BEGIN
    ALTER TABLE dbo.tbl_stock_out
    ADD color_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_color_id ON dbo.tbl_stock_out(color_id);
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_out
        ADD CONSTRAINT FK_tbl_stock_out_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'color_id column added to tbl_stock_out.';
END
ELSE
BEGIN
    PRINT 'color_id column already exists in tbl_stock_out.';
END
GO

PRINT '';
PRINT 'Migration completed successfully!';
GO

