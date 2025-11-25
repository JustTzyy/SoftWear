-- Migration: Create tbl_po_items table
-- Description: Creates the purchase order items table for managing PO line items
-- Date: Generated migration for PO items table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_po_items table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_po_items') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_po_items (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- PO Item Fields
        po_id INT NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        quantity INT NOT NULL DEFAULT 1,
        unit_price DECIMAL(18,2) NOT NULL,
        total_price DECIMAL(18,2) NOT NULL,
        received_quantity INT NOT NULL DEFAULT 0, -- Track how much has been received
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_po_items PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_po_items_po_id ON dbo.tbl_po_items(po_id);
    CREATE NONCLUSTERED INDEX IX_tbl_po_items_variant_id ON dbo.tbl_po_items(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_po_items_size_id ON dbo.tbl_po_items(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_po_items_color_id ON dbo.tbl_po_items(color_id);
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_po_items
        ADD CONSTRAINT FK_tbl_po_items_purchase_order 
        FOREIGN KEY (po_id) 
        REFERENCES dbo.tbl_purchase_orders(id) ON DELETE CASCADE;
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_po_items
        ADD CONSTRAINT FK_tbl_po_items_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_po_items
        ADD CONSTRAINT FK_tbl_po_items_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_po_items
        ADD CONSTRAINT FK_tbl_po_items_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'Table dbo.tbl_po_items created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_po_items already exists.';
END
GO

