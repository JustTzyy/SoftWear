-- ============================================
-- Compare Local vs Azure Data
-- Run this script to see what data exists in both databases
-- ============================================
-- Instructions:
-- 1. Connect to LOCAL database and run sections marked "LOCAL"
-- 2. Connect to AZURE database and run sections marked "AZURE"
-- 3. Compare the results
-- ============================================

-- ============================================
-- LOCAL DATABASE QUERIES
-- Run these on: localhost\SQLEXPRESS
-- ============================================
USE db_SoftWear;
GO

PRINT '========================================';
PRINT 'LOCAL DATABASE - Row Counts';
PRINT '========================================';
PRINT '';

-- Quick summary of all tables
SELECT 
    'LOCAL' AS DatabaseType,
    'tbl_roles' AS TableName,
    COUNT(*) AS RowCount
FROM dbo.tbl_roles
UNION ALL
SELECT 'LOCAL', 'tbl_users', COUNT(*) FROM dbo.tbl_users WHERE archived_at IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_histories', COUNT(*) FROM dbo.tbl_histories
UNION ALL
SELECT 'LOCAL', 'tbl_addresses', COUNT(*) FROM dbo.tbl_addresses WHERE archived_at IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_colors', COUNT(*) FROM dbo.tbl_colors WHERE archived_at IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_sizes', COUNT(*) FROM dbo.tbl_sizes WHERE archived_at IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_categories', COUNT(*) FROM dbo.tbl_categories WHERE archived_at IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_products', COUNT(*) FROM dbo.tbl_products WHERE archived_at IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_variants', COUNT(*) FROM dbo.tbl_variants WHERE archived_at IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_variant_sizes', COUNT(*) FROM dbo.tbl_variant_sizes
UNION ALL
SELECT 'LOCAL', 'tbl_variant_colors', COUNT(*) FROM dbo.tbl_variant_colors
UNION ALL
SELECT 'LOCAL', 'tbl_suppliers', COUNT(*) FROM dbo.tbl_suppliers WHERE archived_at IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_inventories', COUNT(*) FROM dbo.tbl_inventories WHERE archives IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_stock_in', COUNT(*) FROM dbo.tbl_stock_in WHERE archives IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_stock_out', COUNT(*) FROM dbo.tbl_stock_out WHERE archives IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_stock_adjustments', COUNT(*) FROM dbo.tbl_stock_adjustments WHERE archives IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_purchase_orders', COUNT(*) FROM dbo.tbl_purchase_orders WHERE archived_at IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_po_items', COUNT(*) FROM dbo.tbl_po_items
UNION ALL
SELECT 'LOCAL', 'tbl_sales', COUNT(*) FROM dbo.tbl_sales WHERE archives IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_sales_items', COUNT(*) FROM dbo.tbl_sales_items WHERE archives IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_payments', COUNT(*) FROM dbo.tbl_payments WHERE archives IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_returns', COUNT(*) FROM dbo.tbl_returns WHERE archives IS NULL
UNION ALL
SELECT 'LOCAL', 'tbl_return_items', COUNT(*) FROM dbo.tbl_return_items WHERE archives IS NULL
ORDER BY TableName;
GO

-- ============================================
-- DETAILED DATA VIEWS - LOCAL
-- ============================================

PRINT '';
PRINT 'LOCAL - All Users:';
SELECT id, email, name, role_id, is_active, created_at 
FROM dbo.tbl_users 
WHERE archived_at IS NULL 
ORDER BY id;
GO

PRINT '';
PRINT 'LOCAL - All Products:';
SELECT id, name, category_id, status, user_id, created_at 
FROM dbo.tbl_products 
WHERE archived_at IS NULL 
ORDER BY id;
GO

PRINT '';
PRINT 'LOCAL - All Variants:';
SELECT id, name, price, product_id, user_id, created_at 
FROM dbo.tbl_variants 
WHERE archived_at IS NULL 
ORDER BY id;
GO

PRINT '';
PRINT 'LOCAL - All Sales:';
SELECT id, sale_number, amount, payment_type, status, user_id, timestamps 
FROM dbo.tbl_sales 
WHERE archives IS NULL 
ORDER BY timestamps DESC;
GO

-- ============================================
-- AZURE DATABASE QUERIES
-- Run these on: jusstzy.database.windows.net
-- ============================================
-- Copy the queries below and run them on Azure database
-- ============================================

/*
USE db_SoftWear;
GO

PRINT '========================================';
PRINT 'AZURE DATABASE - Row Counts';
PRINT '========================================';
PRINT '';

SELECT 
    'AZURE' AS DatabaseType,
    'tbl_roles' AS TableName,
    COUNT(*) AS RowCount
FROM dbo.tbl_roles
UNION ALL
SELECT 'AZURE', 'tbl_users', COUNT(*) FROM dbo.tbl_users WHERE archived_at IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_histories', COUNT(*) FROM dbo.tbl_histories
UNION ALL
SELECT 'AZURE', 'tbl_addresses', COUNT(*) FROM dbo.tbl_addresses WHERE archived_at IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_colors', COUNT(*) FROM dbo.tbl_colors WHERE archived_at IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_sizes', COUNT(*) FROM dbo.tbl_sizes WHERE archived_at IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_categories', COUNT(*) FROM dbo.tbl_categories WHERE archived_at IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_products', COUNT(*) FROM dbo.tbl_products WHERE archived_at IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_variants', COUNT(*) FROM dbo.tbl_variants WHERE archived_at IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_variant_sizes', COUNT(*) FROM dbo.tbl_variant_sizes
UNION ALL
SELECT 'AZURE', 'tbl_variant_colors', COUNT(*) FROM dbo.tbl_variant_colors
UNION ALL
SELECT 'AZURE', 'tbl_suppliers', COUNT(*) FROM dbo.tbl_suppliers WHERE archived_at IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_inventories', COUNT(*) FROM dbo.tbl_inventories WHERE archives IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_stock_in', COUNT(*) FROM dbo.tbl_stock_in WHERE archives IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_stock_out', COUNT(*) FROM dbo.tbl_stock_out WHERE archives IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_stock_adjustments', COUNT(*) FROM dbo.tbl_stock_adjustments WHERE archives IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_purchase_orders', COUNT(*) FROM dbo.tbl_purchase_orders WHERE archived_at IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_po_items', COUNT(*) FROM dbo.tbl_po_items
UNION ALL
SELECT 'AZURE', 'tbl_sales', COUNT(*) FROM dbo.tbl_sales WHERE archives IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_sales_items', COUNT(*) FROM dbo.tbl_sales_items WHERE archives IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_payments', COUNT(*) FROM dbo.tbl_payments WHERE archives IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_returns', COUNT(*) FROM dbo.tbl_returns WHERE archives IS NULL
UNION ALL
SELECT 'AZURE', 'tbl_return_items', COUNT(*) FROM dbo.tbl_return_items WHERE archives IS NULL
ORDER BY TableName;
GO

PRINT '';
PRINT 'AZURE - All Users:';
SELECT id, email, name, role_id, is_active, created_at 
FROM dbo.tbl_users 
WHERE archived_at IS NULL 
ORDER BY id;
GO

PRINT '';
PRINT 'AZURE - All Products:';
SELECT id, name, category_id, status, user_id, created_at 
FROM dbo.tbl_products 
WHERE archived_at IS NULL 
ORDER BY id;
GO

PRINT '';
PRINT 'AZURE - All Variants:';
SELECT id, name, price, product_id, user_id, created_at 
FROM dbo.tbl_variants 
WHERE archived_at IS NULL 
ORDER BY id;
GO

PRINT '';
PRINT 'AZURE - All Sales:';
SELECT id, sale_number, amount, payment_type, status, user_id, timestamps 
FROM dbo.tbl_sales 
WHERE archives IS NULL 
ORDER BY timestamps DESC;
GO
*/

-- ============================================
-- QUERY TO SEE WHAT WILL BE INSERTED
-- This shows rows that exist in LOCAL but might not be in AZURE
-- ============================================

PRINT '';
PRINT '========================================';
PRINT 'Sample: Users that would be synced';
PRINT '========================================';
PRINT 'This shows all users from local database';
PRINT 'These are the rows that would be inserted to Azure';
PRINT '';

SELECT 
    id,
    email,
    name,
    fname,
    lname,
    role_id,
    is_active,
    created_at
FROM dbo.tbl_users
WHERE archived_at IS NULL
ORDER BY id;
GO

PRINT '';
PRINT 'To see what exists in Azure, connect to Azure database';
PRINT 'and run: SELECT id, email, name FROM dbo.tbl_users WHERE archived_at IS NULL';
PRINT '';












