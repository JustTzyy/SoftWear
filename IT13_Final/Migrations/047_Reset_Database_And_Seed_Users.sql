-- Migration: Reset Database and Seed Users
-- Description: Deletes all data except roles, then seeds dummy users
-- Date: 2025-01-23
-- Works for both Local and Azure SQL databases
--
-- This script:
--   1. Deletes all data from all tables except tbl_roles
--   2. Seeds the following users:
--      - JohnPaul@SoftWear.com (admin)
--      - JackieClaire@SoftWear.com (Seller)
--      - RechelleAnn@SoftWear.com (Accounting) -> connected to seller
--      - JustinDigal@SoftWear.com (Stock clerk) -> connected to seller
--      - MichaelKevinHernandez@SoftWear.com (cashier) -> connected to seller
--
-- All users have password: password123

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Database Reset and User Seed Script';
PRINT '========================================';
PRINT 'This script will:';
PRINT '  1. Delete all data except roles';
PRINT '  2. Seed dummy users';
PRINT '========================================';
PRINT '';

-- ========================================
-- PART 1: Delete All Data Except Roles
-- ========================================
PRINT '';
PRINT '========================================';
PRINT 'PART 1: Deleting All Data Except Roles';
PRINT '========================================';
PRINT '';

SET NOCOUNT ON;

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

-- Delete from tbl_users (but keep roles)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
BEGIN
    UPDATE dbo.tbl_users SET user_id = NULL;
    DELETE FROM dbo.tbl_users;
    PRINT '  - Deleted from tbl_users';
END
GO

-- Reset identity columns
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
BEGIN
    DBCC CHECKIDENT ('dbo.tbl_users', RESEED, 0);
    PRINT '  - Reset identity for tbl_users';
END
GO

PRINT '';
PRINT 'All data deleted successfully (except roles)!';
PRINT '';

-- ========================================
-- PART 2: Seed Dummy Users
-- ========================================
PRINT '';
PRINT '========================================';
PRINT 'PART 2: Seeding Dummy Users';
PRINT '========================================';
PRINT '';

-- Verify roles exist
DECLARE @AdminRoleId INT;
DECLARE @SellerRoleId INT;
DECLARE @AccountingRoleId INT;
DECLARE @StockClerkRoleId INT;
DECLARE @CashierRoleId INT;

SELECT @AdminRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'admin';
SELECT @SellerRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'seller';
SELECT @AccountingRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting';
SELECT @StockClerkRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk';
SELECT @CashierRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier';

IF @AdminRoleId IS NULL OR @SellerRoleId IS NULL OR @AccountingRoleId IS NULL OR @StockClerkRoleId IS NULL OR @CashierRoleId IS NULL
BEGIN
    PRINT 'ERROR: One or more roles not found!';
    PRINT 'Please ensure all roles exist before running this script.';
    RETURN;
END

PRINT 'All roles verified.';
PRINT '';

-- Insert Admin User
PRINT 'Inserting admin user...';
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JohnPaul@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @AdminPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
    
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, lname, role_id, is_active, must_change_pw, created_at)
    VALUES ('JohnPaul@SoftWear.com', @AdminPasswordHash, 'John Paul', 'John', 'Paul', @AdminRoleId, 1, 0, SYSUTCDATETIME());
    PRINT '  - Admin user inserted: JohnPaul@SoftWear.com (password: password123)';
END
ELSE
BEGIN
    PRINT '  - Admin user already exists';
END
GO

-- Insert Seller User
PRINT '';
PRINT 'Inserting seller user...';
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JackieClaire@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerRoleId2 INT;
    SELECT @SellerRoleId2 = id FROM dbo.tbl_roles WHERE LOWER(name) = 'seller';
    
    DECLARE @SellerPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
    
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, lname, role_id, is_active, must_change_pw, created_at)
    VALUES ('JackieClaire@SoftWear.com', @SellerPasswordHash, 'Jackie Claire', 'Jackie', 'Claire', @SellerRoleId2, 1, 0, SYSUTCDATETIME());
    PRINT '  - Seller user inserted: JackieClaire@SoftWear.com (password: password123)';
END
ELSE
BEGIN
    PRINT '  - Seller user already exists';
END
GO

-- Insert Accounting User (connected to seller)
PRINT '';
PRINT 'Inserting accounting user (connected to seller)...';
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'RechelleAnn@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerIdForAccounting INT;
    SELECT @SellerIdForAccounting = id FROM dbo.tbl_users WHERE email = 'JackieClaire@SoftWear.com' AND archived_at IS NULL;
    
    IF @SellerIdForAccounting IS NULL
    BEGIN
        PRINT '  - ERROR: Seller user not found. Cannot connect accounting user.';
    END
    ELSE
    BEGIN
        DECLARE @AccountingRoleId2 INT;
        SELECT @AccountingRoleId2 = id FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting';
        
        DECLARE @AccountingPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
        
        INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, lname, role_id, user_id, is_active, must_change_pw, created_at)
        VALUES ('RechelleAnn@SoftWear.com', @AccountingPasswordHash, 'Rechelle Ann', 'Rechelle', 'Ann', @AccountingRoleId2, @SellerIdForAccounting, 1, 0, SYSUTCDATETIME());
        PRINT '  - Accounting user inserted: RechelleAnn@SoftWear.com (password: password123, connected to seller)';
    END
END
ELSE
BEGIN
    PRINT '  - Accounting user already exists';
END
GO

-- Insert Stock Clerk User (connected to seller)
PRINT '';
PRINT 'Inserting stock clerk user (connected to seller)...';
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JustinDigal@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerIdForStockClerk INT;
    SELECT @SellerIdForStockClerk = id FROM dbo.tbl_users WHERE email = 'JackieClaire@SoftWear.com' AND archived_at IS NULL;
    
    IF @SellerIdForStockClerk IS NULL
    BEGIN
        PRINT '  - ERROR: Seller user not found. Cannot connect stock clerk user.';
    END
    ELSE
    BEGIN
        DECLARE @StockClerkRoleId2 INT;
        SELECT @StockClerkRoleId2 = id FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk';
        
        DECLARE @StockClerkPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
        
        INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, lname, role_id, user_id, is_active, must_change_pw, created_at)
        VALUES ('JustinDigal@SoftWear.com', @StockClerkPasswordHash, 'Justin Digal', 'Justin', 'Digal', @StockClerkRoleId2, @SellerIdForStockClerk, 1, 0, SYSUTCDATETIME());
        PRINT '  - Stock Clerk user inserted: JustinDigal@SoftWear.com (password: password123, connected to seller)';
    END
END
ELSE
BEGIN
    PRINT '  - Stock Clerk user already exists';
END
GO

-- Insert Cashier User (connected to seller)
PRINT '';
PRINT 'Inserting cashier user (connected to seller)...';
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'MichaelKevinHernandez@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerIdForCashier INT;
    SELECT @SellerIdForCashier = id FROM dbo.tbl_users WHERE email = 'JackieClaire@SoftWear.com' AND archived_at IS NULL;
    
    IF @SellerIdForCashier IS NULL
    BEGIN
        PRINT '  - ERROR: Seller user not found. Cannot connect cashier user.';
    END
    ELSE
    BEGIN
        DECLARE @CashierRoleId2 INT;
        SELECT @CashierRoleId2 = id FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier';
        
        DECLARE @CashierPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
        
        INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, lname, role_id, user_id, is_active, must_change_pw, created_at)
        VALUES ('MichaelKevinHernandez@SoftWear.com', @CashierPasswordHash, 'Michael Kevin Hernandez', 'Michael Kevin', 'Hernandez', @CashierRoleId2, @SellerIdForCashier, 1, 0, SYSUTCDATETIME());
        PRINT '  - Cashier user inserted: MichaelKevinHernandez@SoftWear.com (password: password123, connected to seller)';
    END
END
ELSE
BEGIN
    PRINT '  - Cashier user already exists';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Database Reset and User Seed Completed!';
PRINT '========================================';
PRINT '';
PRINT 'Users Created:';
PRINT '  - Admin: JohnPaul@SoftWear.com / password123';
PRINT '  - Seller: JackieClaire@SoftWear.com / password123';
PRINT '  - Accounting: RechelleAnn@SoftWear.com / password123 (connected to seller)';
PRINT '  - Stock Clerk: JustinDigal@SoftWear.com / password123 (connected to seller)';
PRINT '  - Cashier: MichaelKevinHernandez@SoftWear.com / password123 (connected to seller)';
PRINT '';
PRINT 'Note: Run this script on both Local and Azure databases.';
PRINT '';




