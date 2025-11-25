-- Migration: Create tbl_return_items table
-- Description: Creates the return items table for managing individual items in a return
-- Date: Generated migration for return items table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_return_items table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_return_items') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_return_items (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Return Item Fields
        return_id INT NOT NULL,
        sale_item_id INT NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        quantity INT NOT NULL DEFAULT 1,
        condition NVARCHAR(50) NULL, -- New, Used, Damaged, etc.
        
        -- Audit Fields
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_return_items PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_return_id ON dbo.tbl_return_items(return_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_sale_item_id ON dbo.tbl_return_items(sale_item_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_variant_id ON dbo.tbl_return_items(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_size_id ON dbo.tbl_return_items(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_color_id ON dbo.tbl_return_items(color_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_archives ON dbo.tbl_return_items(archives) WHERE archives IS NULL;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_returns') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_return 
        FOREIGN KEY (return_id) 
        REFERENCES dbo.tbl_returns(id) ON DELETE CASCADE;
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales_items') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_sale_item 
        FOREIGN KEY (sale_item_id) 
        REFERENCES dbo.tbl_sales_items(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'Table dbo.tbl_return_items created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_return_items already exists.';
END
GO

