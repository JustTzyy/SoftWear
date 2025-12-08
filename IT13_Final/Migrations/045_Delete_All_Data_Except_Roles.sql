-- Migration: Delete All Data Except Roles
-- Description: Deletes all data from all tables except tbl_roles
-- Date: 2025-01-23
-- Note: This script respects foreign key constraints by deleting in the correct order

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Deleting All Data Except Roles...';
PRINT '========================================';
PRINT '';

-- Disable foreign key constraints temporarily for faster deletion
-- We'll delete in order to respect foreign keys, but disabling can help with performance
SET NOCOUNT ON;

-- ========================================
-- Step 1: Delete from child tables first (respecting foreign key order)
-- ========================================

PRINT 'Step 1: Deleting from child tables...';

-- Delete from tables with most dependencies first
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_supplier_payments;
    PRINT '  - Deleted from tbl_supplier_payments';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_invoices') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_supplier_invoices;
    PRINT '  - Deleted from tbl_supplier_invoices';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_expenses') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_expenses;
    PRINT '  - Deleted from tbl_expenses';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_daily_sales_verifications') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_daily_sales_verifications;
    PRINT '  - Deleted from tbl_daily_sales_verifications';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_return_items') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_return_items;
    PRINT '  - Deleted from tbl_return_items';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_returns') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_returns;
    PRINT '  - Deleted from tbl_returns';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_payments') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_payments;
    PRINT '  - Deleted from tbl_payments';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales_items') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_sales_items;
    PRINT '  - Deleted from tbl_sales_items';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_sales;
    PRINT '  - Deleted from tbl_sales';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_po_items') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_po_items;
    PRINT '  - Deleted from tbl_po_items';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_purchase_orders;
    PRINT '  - Deleted from tbl_purchase_orders';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_adjustments') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_stock_adjustments;
    PRINT '  - Deleted from tbl_stock_adjustments';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_out') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_stock_out;
    PRINT '  - Deleted from tbl_stock_out';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_in') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_stock_in;
    PRINT '  - Deleted from tbl_stock_in';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_inventories;
    PRINT '  - Deleted from tbl_inventories';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_suppliers;
    PRINT '  - Deleted from tbl_suppliers';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variant_colors') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_variant_colors;
    PRINT '  - Deleted from tbl_variant_colors';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variant_sizes') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_variant_sizes;
    PRINT '  - Deleted from tbl_variant_sizes';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_variants;
    PRINT '  - Deleted from tbl_variants';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_products') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_products;
    PRINT '  - Deleted from tbl_products';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_categories') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_categories;
    PRINT '  - Deleted from tbl_categories';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_sizes;
    PRINT '  - Deleted from tbl_sizes';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_colors;
    PRINT '  - Deleted from tbl_colors';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_addresses') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_addresses;
    PRINT '  - Deleted from tbl_addresses';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_histories') AND type in (N'U'))
BEGIN
    DELETE FROM dbo.tbl_histories;
    PRINT '  - Deleted from tbl_histories';
END
GO

-- ========================================
-- Step 2: Delete from tbl_users (but keep roles)
-- ========================================
PRINT '';
PRINT 'Step 2: Deleting from tbl_users...';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
BEGIN
    -- First, set user_id to NULL for all users to break self-referential constraints
    UPDATE dbo.tbl_users SET user_id = NULL;
    PRINT '  - Cleared user_id references';
    
    -- Then delete all users
    DELETE FROM dbo.tbl_users;
    PRINT '  - Deleted from tbl_users';
END
GO

-- ========================================
-- Step 3: Reset identity columns (optional, but helpful for clean IDs)
-- ========================================
PRINT '';
PRINT 'Step 3: Resetting identity columns...';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
BEGIN
    DBCC CHECKIDENT ('dbo.tbl_users', RESEED, 0);
    PRINT '  - Reset identity for tbl_users';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_products') AND type in (N'U'))
BEGIN
    DBCC CHECKIDENT ('dbo.tbl_products', RESEED, 0);
    PRINT '  - Reset identity for tbl_products';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
BEGIN
    DBCC CHECKIDENT ('dbo.tbl_suppliers', RESEED, 0);
    PRINT '  - Reset identity for tbl_suppliers';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
BEGIN
    DBCC CHECKIDENT ('dbo.tbl_purchase_orders', RESEED, 0);
    PRINT '  - Reset identity for tbl_purchase_orders';
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales') AND type in (N'U'))
BEGIN
    DBCC CHECKIDENT ('dbo.tbl_sales', RESEED, 0);
    PRINT '  - Reset identity for tbl_sales';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'All data deleted successfully (except roles)!';
PRINT '========================================';
PRINT '';
GO










