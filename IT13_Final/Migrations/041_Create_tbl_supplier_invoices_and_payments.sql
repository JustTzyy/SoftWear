-- Migration: Create tbl_supplier_invoices and tbl_supplier_payments tables
-- Description: Creates tables for managing supplier invoices and payments
-- Date: 2025-01-23

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_supplier_invoices table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_invoices') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_supplier_invoices (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Invoice Fields
        invoice_number NVARCHAR(100) NOT NULL,
        supplier_id INT NOT NULL,
        invoice_date DATE NOT NULL,
        total_amount DECIMAL(18,2) NOT NULL,
        description NVARCHAR(1000) NULL,
        
        -- Source tracking
        source_type NVARCHAR(50) NOT NULL DEFAULT 'Manual', -- 'StockIn' or 'Manual'
        stock_in_id INT NULL, -- If source is StockIn, reference to stock_in record
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        
        -- Foreign Keys
        created_by INT NOT NULL, -- The accounting user (user.id) who created this invoice
        seller_user_id INT NOT NULL, -- The seller (user.user_id) for filtering
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_supplier_invoices PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_invoice_number ON dbo.tbl_supplier_invoices(invoice_number);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_supplier_id ON dbo.tbl_supplier_invoices(supplier_id);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_invoice_date ON dbo.tbl_supplier_invoices(invoice_date);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_seller_user_id ON dbo.tbl_supplier_invoices(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_archived_at ON dbo.tbl_supplier_invoices(archived_at) WHERE archived_at IS NULL;
    
    -- Create unique constraint on invoice_number per seller
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_supplier_invoices_invoice_number_seller 
    ON dbo.tbl_supplier_invoices(invoice_number, seller_user_id) 
    WHERE archived_at IS NULL;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_invoices
        ADD CONSTRAINT FK_tbl_supplier_invoices_supplier 
        FOREIGN KEY (supplier_id) 
        REFERENCES dbo.tbl_suppliers(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_invoices
        ADD CONSTRAINT FK_tbl_supplier_invoices_created_by 
        FOREIGN KEY (created_by) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_in') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_invoices
        ADD CONSTRAINT FK_tbl_supplier_invoices_stock_in 
        FOREIGN KEY (stock_in_id) 
        REFERENCES dbo.tbl_stock_in(id);
    END;
    
    PRINT 'Table tbl_supplier_invoices created successfully.';
END
ELSE
BEGIN
    PRINT 'Table tbl_supplier_invoices already exists.';
END
GO

-- Create tbl_supplier_payments table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_supplier_payments (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Payment Fields
        invoice_id INT NULL, -- Optional: for manually created invoices
        po_id INT NULL, -- Optional: for Purchase Order payments
        amount_paid DECIMAL(18,2) NOT NULL,
        payment_method NVARCHAR(50) NOT NULL, -- Cash, GCash, Bank
        payment_date DATE NOT NULL,
        reference_number NVARCHAR(100) NULL,
        notes NVARCHAR(1000) NULL,
        receipt_image_base64 NVARCHAR(MAX) NULL, -- Base64 encoded image
        receipt_image_content_type NVARCHAR(50) NULL, -- image/jpeg, image/png, etc.
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        
        -- Foreign Keys
        created_by INT NOT NULL, -- The accounting user (user.id) who created this payment
        seller_user_id INT NOT NULL, -- The seller (user.user_id) for filtering
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_supplier_payments PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_invoice_id ON dbo.tbl_supplier_payments(invoice_id) WHERE invoice_id IS NOT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_po_id ON dbo.tbl_supplier_payments(po_id) WHERE po_id IS NOT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_payment_date ON dbo.tbl_supplier_payments(payment_date);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_seller_user_id ON dbo.tbl_supplier_payments(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_archived_at ON dbo.tbl_supplier_payments(archived_at) WHERE archived_at IS NULL;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_invoices') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_payments
        ADD CONSTRAINT FK_tbl_supplier_payments_invoice 
        FOREIGN KEY (invoice_id) 
        REFERENCES dbo.tbl_supplier_invoices(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_payments
        ADD CONSTRAINT FK_tbl_supplier_payments_po 
        FOREIGN KEY (po_id) 
        REFERENCES dbo.tbl_purchase_orders(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_payments
        ADD CONSTRAINT FK_tbl_supplier_payments_created_by 
        FOREIGN KEY (created_by) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table tbl_supplier_payments created successfully.';
END
ELSE
BEGIN
    -- Add po_id column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND name = 'po_id')
    BEGIN
        ALTER TABLE dbo.tbl_supplier_payments
        ADD po_id INT NULL;
        
        CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_po_id ON dbo.tbl_supplier_payments(po_id) WHERE po_id IS NOT NULL;
        
        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
        BEGIN
            ALTER TABLE dbo.tbl_supplier_payments
            ADD CONSTRAINT FK_tbl_supplier_payments_po 
            FOREIGN KEY (po_id) 
            REFERENCES dbo.tbl_purchase_orders(id);
        END;
        
        PRINT 'Added po_id column to tbl_supplier_payments.';
    END
    
    PRINT 'Table tbl_supplier_payments already exists.';
END
GO

