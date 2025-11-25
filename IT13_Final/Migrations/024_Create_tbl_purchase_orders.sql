-- Migration: Create tbl_purchase_orders table
-- Description: Creates the purchase orders table for managing supplier orders
-- Date: Generated migration for purchase orders table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_purchase_orders table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_purchase_orders (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Purchase Order Fields
        po_number NVARCHAR(50) NOT NULL,
        supplier_id INT NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Cancelled, Completed
        total_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        notes NVARCHAR(1000) NULL,
        expected_delivery_date DATETIME2(0) NULL,
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        
        -- Foreign Keys
        created_by INT NOT NULL, -- user_id who created the PO
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_purchase_orders PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_po_number ON dbo.tbl_purchase_orders(po_number);
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_supplier_id ON dbo.tbl_purchase_orders(supplier_id);
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_status ON dbo.tbl_purchase_orders(status);
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_created_by ON dbo.tbl_purchase_orders(created_by);
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_archived_at ON dbo.tbl_purchase_orders(archived_at) WHERE archived_at IS NULL;
    
    -- Create unique constraint on po_number
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND name = 'UQ_tbl_purchase_orders_po_number')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_purchase_orders_po_number ON dbo.tbl_purchase_orders(po_number) WHERE archived_at IS NULL;
    END;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_purchase_orders
        ADD CONSTRAINT FK_tbl_purchase_orders_supplier 
        FOREIGN KEY (supplier_id) 
        REFERENCES dbo.tbl_suppliers(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_purchase_orders
        ADD CONSTRAINT FK_tbl_purchase_orders_created_by 
        FOREIGN KEY (created_by) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_purchase_orders created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_purchase_orders already exists.';
END
GO

