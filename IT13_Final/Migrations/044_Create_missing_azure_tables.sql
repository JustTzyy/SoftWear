-- Migration: Create missing tables in Azure
-- Tables: tbl_daily_sales_verifications, tbl_expenses, tbl_supplier_invoices, tbl_supplier_payments
-- Date: 2025-01-23

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT 'Creating missing tables for Azure...';
PRINT '';

-- ============================================
-- Create tbl_daily_sales_verifications table
-- ============================================
PRINT 'Creating tbl_daily_sales_verifications...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_daily_sales_verifications') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_daily_sales_verifications (
        id INT IDENTITY(1,1) NOT NULL,
        cashier_user_id INT NOT NULL,
        sale_date DATE NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        notes NVARCHAR(1000) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        verified_by INT NULL,
        seller_user_id INT NOT NULL,
        CONSTRAINT PK_tbl_daily_sales_verifications PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_cashier_date ON dbo.tbl_daily_sales_verifications(cashier_user_id, sale_date);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_seller_date ON dbo.tbl_daily_sales_verifications(seller_user_id, sale_date);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_status ON dbo.tbl_daily_sales_verifications(status);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_verified_by ON dbo.tbl_daily_sales_verifications(verified_by);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_archived_at ON dbo.tbl_daily_sales_verifications(archived_at) WHERE archived_at IS NULL;
    
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_daily_sales_verifications_cashier_date_seller 
    ON dbo.tbl_daily_sales_verifications(cashier_user_id, sale_date, seller_user_id) 
    WHERE archived_at IS NULL;
    
    PRINT 'Table tbl_daily_sales_verifications created successfully.';
END
ELSE
BEGIN
    PRINT 'Table tbl_daily_sales_verifications already exists.';
END
GO

-- ============================================
-- Create tbl_expenses table
-- ============================================
PRINT '';
PRINT 'Creating tbl_expenses...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_expenses') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_expenses (
        id INT IDENTITY(1,1) NOT NULL,
        expense_type NVARCHAR(100) NOT NULL,
        amount DECIMAL(18,2) NOT NULL,
        description NVARCHAR(1000) NULL,
        expense_date DATE NOT NULL,
        receipt_image NVARCHAR(MAX) NULL,
        receipt_content_type NVARCHAR(100) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        created_by INT NOT NULL,
        seller_user_id INT NOT NULL,
        CONSTRAINT PK_tbl_expenses PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_expense_type ON dbo.tbl_expenses(expense_type);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_expense_date ON dbo.tbl_expenses(expense_date);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_seller_user_id ON dbo.tbl_expenses(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_created_by ON dbo.tbl_expenses(created_by);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_archived_at ON dbo.tbl_expenses(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table tbl_expenses created successfully.';
END
ELSE
BEGIN
    PRINT 'Table tbl_expenses already exists.';
END
GO

-- ============================================
-- Create tbl_supplier_invoices table
-- ============================================
PRINT '';
PRINT 'Creating tbl_supplier_invoices...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_invoices') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_supplier_invoices (
        id INT IDENTITY(1,1) NOT NULL,
        invoice_number NVARCHAR(100) NOT NULL,
        supplier_id INT NOT NULL,
        invoice_date DATE NOT NULL,
        total_amount DECIMAL(18,2) NOT NULL,
        description NVARCHAR(1000) NULL,
        source_type NVARCHAR(50) NOT NULL DEFAULT 'Manual',
        stock_in_id INT NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        created_by INT NOT NULL,
        seller_user_id INT NOT NULL,
        CONSTRAINT PK_tbl_supplier_invoices PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_invoice_number ON dbo.tbl_supplier_invoices(invoice_number);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_supplier_id ON dbo.tbl_supplier_invoices(supplier_id);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_invoice_date ON dbo.tbl_supplier_invoices(invoice_date);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_seller_user_id ON dbo.tbl_supplier_invoices(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_archived_at ON dbo.tbl_supplier_invoices(archived_at) WHERE archived_at IS NULL;
    
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_supplier_invoices_invoice_number_seller 
    ON dbo.tbl_supplier_invoices(invoice_number, seller_user_id) 
    WHERE archived_at IS NULL;
    
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

-- ============================================
-- Create tbl_supplier_payments table
-- ============================================
PRINT '';
PRINT 'Creating tbl_supplier_payments...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_supplier_payments (
        id INT IDENTITY(1,1) NOT NULL,
        invoice_id INT NULL,
        po_id INT NULL,
        amount_paid DECIMAL(18,2) NOT NULL,
        payment_method NVARCHAR(50) NOT NULL,
        payment_date DATE NOT NULL,
        reference_number NVARCHAR(100) NULL,
        notes NVARCHAR(1000) NULL,
        receipt_image_base64 NVARCHAR(MAX) NULL,
        receipt_image_content_type NVARCHAR(50) NULL,
        stock_in_group_key NVARCHAR(100) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        created_by INT NOT NULL,
        seller_user_id INT NOT NULL,
        CONSTRAINT PK_tbl_supplier_payments PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_invoice_id ON dbo.tbl_supplier_payments(invoice_id) WHERE invoice_id IS NOT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_po_id ON dbo.tbl_supplier_payments(po_id) WHERE po_id IS NOT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_payment_date ON dbo.tbl_supplier_payments(payment_date);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_seller_user_id ON dbo.tbl_supplier_payments(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_archived_at ON dbo.tbl_supplier_payments(archived_at) WHERE archived_at IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_stock_in_group_key ON dbo.tbl_supplier_payments(stock_in_group_key) WHERE stock_in_group_key IS NOT NULL;
    
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
    PRINT 'Table tbl_supplier_payments already exists.';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'All 4 missing tables created successfully!';
PRINT '========================================';
PRINT '';
GO


