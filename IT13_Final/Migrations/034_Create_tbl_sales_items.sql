-- Migration: Create tbl_sales_items table
-- Description: Creates the sales items table for managing individual items in a sale
-- Date: Generated migration for sales items table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_sales_items table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales_items') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_sales_items (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Sales Item Fields
        sale_id INT NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        quantity INT NOT NULL DEFAULT 1,
        price DECIMAL(18,2) NOT NULL, -- unit price at time of sale
        subtotal DECIMAL(18,2) NOT NULL, -- quantity * price
        
        -- Audit Fields
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_sales_items PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_sale_id ON dbo.tbl_sales_items(sale_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_variant_id ON dbo.tbl_sales_items(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_size_id ON dbo.tbl_sales_items(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_color_id ON dbo.tbl_sales_items(color_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_archives ON dbo.tbl_sales_items(archives) WHERE archives IS NULL;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sales_items
        ADD CONSTRAINT FK_tbl_sales_items_sale 
        FOREIGN KEY (sale_id) 
        REFERENCES dbo.tbl_sales(id) ON DELETE CASCADE;
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sales_items
        ADD CONSTRAINT FK_tbl_sales_items_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sales_items
        ADD CONSTRAINT FK_tbl_sales_items_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sales_items
        ADD CONSTRAINT FK_tbl_sales_items_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'Table dbo.tbl_sales_items created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_sales_items already exists.';
END
GO

