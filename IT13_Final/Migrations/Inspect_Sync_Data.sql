-- ============================================
-- SQL Script to Inspect Data for Sync
-- Run this in SQL Server Management Studio
-- ============================================
-- This script shows all data in local database
-- and what would be synced to Azure
-- ============================================

USE db_SoftWear;
GO

PRINT '========================================';
PRINT 'Data Inspection for Sync';
PRINT '========================================';
PRINT '';

-- ============================================
-- 1. Show Row Counts for All Tables
-- ============================================
PRINT '1. Row Counts Comparison:';
PRINT '';

SELECT 
    'tbl_roles' AS TableName,
    (SELECT COUNT(*) FROM dbo.tbl_roles) AS LocalRows,
    'Check Azure manually' AS AzureRows
UNION ALL
SELECT 
    'tbl_users',
    (SELECT COUNT(*) FROM dbo.tbl_users WHERE archived_at IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_histories',
    (SELECT COUNT(*) FROM dbo.tbl_histories),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_addresses',
    (SELECT COUNT(*) FROM dbo.tbl_addresses WHERE archived_at IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_colors',
    (SELECT COUNT(*) FROM dbo.tbl_colors WHERE archived_at IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_sizes',
    (SELECT COUNT(*) FROM dbo.tbl_sizes WHERE archived_at IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_categories',
    (SELECT COUNT(*) FROM dbo.tbl_categories WHERE archived_at IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_products',
    (SELECT COUNT(*) FROM dbo.tbl_products WHERE archived_at IS NULL),
    CONCAT('Images: ', (SELECT COUNT(*) FROM dbo.tbl_products WHERE archived_at IS NULL AND image IS NOT NULL))
UNION ALL
SELECT 
    'tbl_variants',
    (SELECT COUNT(*) FROM dbo.tbl_variants WHERE archived_at IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_variant_sizes',
    (SELECT COUNT(*) FROM dbo.tbl_variant_sizes),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_variant_colors',
    (SELECT COUNT(*) FROM dbo.tbl_variant_colors),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_suppliers',
    (SELECT COUNT(*) FROM dbo.tbl_suppliers WHERE archived_at IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_inventories',
    (SELECT COUNT(*) FROM dbo.tbl_inventories WHERE archives IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_stock_in',
    (SELECT COUNT(*) FROM dbo.tbl_stock_in WHERE archives IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_stock_out',
    (SELECT COUNT(*) FROM dbo.tbl_stock_out WHERE archives IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_stock_adjustments',
    (SELECT COUNT(*) FROM dbo.tbl_stock_adjustments WHERE archives IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_purchase_orders',
    (SELECT COUNT(*) FROM dbo.tbl_purchase_orders WHERE archived_at IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_po_items',
    (SELECT COUNT(*) FROM dbo.tbl_po_items),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_sales',
    (SELECT COUNT(*) FROM dbo.tbl_sales WHERE archives IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_sales_items',
    (SELECT COUNT(*) FROM dbo.tbl_sales_items WHERE archives IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_payments',
    (SELECT COUNT(*) FROM dbo.tbl_payments WHERE archives IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_returns',
    (SELECT COUNT(*) FROM dbo.tbl_returns WHERE archives IS NULL),
    'Check Azure manually'
UNION ALL
SELECT 
    'tbl_return_items',
    (SELECT COUNT(*) FROM dbo.tbl_return_items WHERE archives IS NULL),
    'Check Azure manually'
ORDER BY TableName;
GO

-- ============================================
-- 2. View All Users Data
-- ============================================
PRINT '';
PRINT '2. All Users Data (Local):';
PRINT '';

SELECT 
    id,
    email,
    name,
    fname,
    lname,
    role_id,
    is_active,
    created_at,
    archived_at
FROM dbo.tbl_users
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 3. View All Roles Data
-- ============================================
PRINT '';
PRINT '3. All Roles Data (Local):';
PRINT '';

SELECT 
    id,
    name,
    [desc],
    created_at
FROM dbo.tbl_roles
ORDER BY id;
GO

-- ============================================
-- 4. View All Products Data
-- ============================================
PRINT '';
PRINT '4. All Products Data (Local):';
PRINT '';

SELECT 
    id,
    name,
    description,
    category_id,
    status,
    user_id,
    image_content_type,
    CASE 
        WHEN image IS NULL THEN 'No Image'
        ELSE CONCAT('Image Size: ', CAST(DATALENGTH(image) AS VARCHAR), ' bytes')
    END AS ImageInfo,
    created_at,
    archived_at
FROM dbo.tbl_products
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 4a. View Products with Image Details
-- ============================================
PRINT '';
PRINT '4a. Products with Image Information (Local):';
PRINT '';

SELECT 
    id,
    name,
    image_content_type,
    DATALENGTH(image) AS ImageSizeBytes,
    CASE 
        WHEN image IS NULL THEN 'No Image'
        WHEN DATALENGTH(image) = 0 THEN 'Empty Image'
        ELSE 'Has Image'
    END AS ImageStatus
FROM dbo.tbl_products
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 5. View All Variants Data
-- ============================================
PRINT '';
PRINT '5. All Variants Data (Local):';
PRINT '';

SELECT 
    id,
    name,
    price,
    cost_price,
    product_id,
    user_id,
    created_at,
    archived_at
FROM dbo.tbl_variants
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 6. View All Categories Data
-- ============================================
PRINT '';
PRINT '6. All Categories Data (Local):';
PRINT '';

SELECT 
    id,
    name,
    description,
    user_id,
    created_at,
    archived_at
FROM dbo.tbl_categories
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 7. View All Colors Data
-- ============================================
PRINT '';
PRINT '7. All Colors Data (Local):';
PRINT '';

SELECT 
    id,
    name,
    hex_value,
    description,
    user_id,
    created_at,
    archived_at
FROM dbo.tbl_colors
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 8. View All Sizes Data
-- ============================================
PRINT '';
PRINT '8. All Sizes Data (Local):';
PRINT '';

SELECT 
    id,
    name,
    description,
    user_id,
    created_at,
    archived_at
FROM dbo.tbl_sizes
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 9. View All Suppliers Data
-- ============================================
PRINT '';
PRINT '9. All Suppliers Data (Local):';
PRINT '';

SELECT 
    id,
    company_name,
    contact_person,
    email,
    contact_number,
    status,
    user_id,
    created_at,
    archived_at
FROM dbo.tbl_suppliers
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 10. View All Inventories Data
-- ============================================
PRINT '';
PRINT '10. All Inventories Data (Local):';
PRINT '';

SELECT 
    id,
    variant_id,
    size_id,
    color_id,
    current_stock,
    reorder_level,
    user_id,
    timestamps,
    archives
FROM dbo.tbl_inventories
WHERE archives IS NULL
ORDER BY id;
GO

-- ============================================
-- 11. View All Stock In Data
-- ============================================
PRINT '';
PRINT '11. All Stock In Data (Local):';
PRINT '';

SELECT 
    id,
    variant_id,
    size_id,
    color_id,
    quantity_added,
    cost_price,
    user_id,
    supplier_id,
    timestamps,
    archives
FROM dbo.tbl_stock_in
WHERE archives IS NULL
ORDER BY timestamps DESC;
GO

-- ============================================
-- 12. View All Stock Out Data
-- ============================================
PRINT '';
PRINT '12. All Stock Out Data (Local):';
PRINT '';

SELECT 
    id,
    variant_id,
    size_id,
    color_id,
    quantity_removed,
    reason,
    user_id,
    timestamps,
    archives
FROM dbo.tbl_stock_out
WHERE archives IS NULL
ORDER BY timestamps DESC;
GO

-- ============================================
-- 13. View All Stock Adjustments Data
-- ============================================
PRINT '';
PRINT '13. All Stock Adjustments Data (Local):';
PRINT '';

SELECT 
    id,
    variant_id,
    size_id,
    color_id,
    adjustment_type,
    quantity_adjusted,
    reason,
    user_id,
    timestamps,
    archives
FROM dbo.tbl_stock_adjustments
WHERE archives IS NULL
ORDER BY timestamps DESC;
GO

-- ============================================
-- 14. View All Purchase Orders Data
-- ============================================
PRINT '';
PRINT '14. All Purchase Orders Data (Local):';
PRINT '';

SELECT 
    id,
    po_number,
    supplier_id,
    status,
    total_amount,
    notes,
    expected_delivery_date,
    created_by,
    updated_by,
    created_at,
    updated_at,
    archived_at
FROM dbo.tbl_purchase_orders
WHERE archived_at IS NULL
ORDER BY created_at DESC;
GO

-- ============================================
-- 15. View All PO Items Data
-- ============================================
PRINT '';
PRINT '15. All PO Items Data (Local):';
PRINT '';

SELECT 
    id,
    po_id,
    variant_id,
    size_id,
    color_id,
    quantity,
    unit_price,
    total_price,
    received_quantity,
    created_at,
    updated_at
FROM dbo.tbl_po_items
ORDER BY id;
GO

-- ============================================
-- 16. View All Sales Data
-- ============================================
PRINT '';
PRINT '16. All Sales Data (Local):';
PRINT '';

SELECT 
    id,
    sale_number,
    amount,
    payment_type,
    status,
    user_id,
    timestamps,
    archives
FROM dbo.tbl_sales
WHERE archives IS NULL
ORDER BY timestamps DESC;
GO

-- ============================================
-- 17. View All Sales Items Data
-- ============================================
PRINT '';
PRINT '17. All Sales Items Data (Local):';
PRINT '';

SELECT 
    id,
    sale_id,
    variant_id,
    size_id,
    color_id,
    quantity,
    price,
    subtotal,
    timestamps,
    archives
FROM dbo.tbl_sales_items
WHERE archives IS NULL
ORDER BY id;
GO

-- ============================================
-- 18. View All Payments Data
-- ============================================
PRINT '';
PRINT '18. All Payments Data (Local):';
PRINT '';

SELECT 
    id,
    sale_id,
    amount_paid,
    payment_method,
    change_given,
    reference_number,
    timestamps,
    archives
FROM dbo.tbl_payments
WHERE archives IS NULL
ORDER BY timestamps DESC;
GO

-- ============================================
-- 19. View All Returns Data
-- ============================================
PRINT '';
PRINT '19. All Returns Data (Local):';
PRINT '';

SELECT 
    id,
    return_number,
    sale_id,
    reason,
    status,
    user_id,
    approved_by,
    timestamps,
    archives
FROM dbo.tbl_returns
WHERE archives IS NULL
ORDER BY timestamps DESC;
GO

-- ============================================
-- 20. View All Return Items Data
-- ============================================
PRINT '';
PRINT '20. All Return Items Data (Local):';
PRINT '';

SELECT 
    id,
    return_id,
    sale_item_id,
    variant_id,
    size_id,
    color_id,
    quantity,
    [condition],
    timestamps,
    archives
FROM dbo.tbl_return_items
WHERE archives IS NULL
ORDER BY id;
GO

-- ============================================
-- 21. View All Histories Data
-- ============================================
PRINT '';
PRINT '21. All Histories Data (Local):';
PRINT '';

SELECT 
    id,
    user_id,
    status,
    module,
    description,
    ts
FROM dbo.tbl_histories
ORDER BY ts DESC;
GO

-- ============================================
-- 22. View All Addresses Data
-- ============================================
PRINT '';
PRINT '22. All Addresses Data (Local):';
PRINT '';

SELECT 
    id,
    user_id,
    supplier_id,
    street,
    city,
    province,
    zip,
    created_at,
    updated_at,
    archived_at
FROM dbo.tbl_addresses
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 23. View All Variant Sizes Data
-- ============================================
PRINT '';
PRINT '23. All Variant Sizes Data (Local):';
PRINT '';

SELECT 
    id,
    variant_id,
    size_id,
    created_at
FROM dbo.tbl_variant_sizes
ORDER BY id;
GO

-- ============================================
-- 24. View All Variant Colors Data
-- ============================================
PRINT '';
PRINT '24. All Variant Colors Data (Local):';
PRINT '';

SELECT 
    id,
    variant_id,
    color_id,
    created_at
FROM dbo.tbl_variant_colors
ORDER BY id;
GO

-- ============================================
-- 25. Export All Data to See What Will Be Synced
-- ============================================
PRINT '';
PRINT '========================================';
PRINT 'Summary: Total Rows to Sync';
PRINT '========================================';
PRINT '';

SELECT 
    'tbl_roles' AS TableName,
    COUNT(*) AS TotalRows
FROM dbo.tbl_roles
UNION ALL
SELECT 'tbl_users', COUNT(*) FROM dbo.tbl_users WHERE archived_at IS NULL
UNION ALL
SELECT 'tbl_histories', COUNT(*) FROM dbo.tbl_histories
UNION ALL
SELECT 'tbl_addresses', COUNT(*) FROM dbo.tbl_addresses WHERE archived_at IS NULL
UNION ALL
SELECT 'tbl_colors', COUNT(*) FROM dbo.tbl_colors WHERE archived_at IS NULL
UNION ALL
SELECT 'tbl_sizes', COUNT(*) FROM dbo.tbl_sizes WHERE archived_at IS NULL
UNION ALL
SELECT 'tbl_categories', COUNT(*) FROM dbo.tbl_categories WHERE archived_at IS NULL
UNION ALL
SELECT 'tbl_products', COUNT(*) FROM dbo.tbl_products WHERE archived_at IS NULL
UNION ALL
SELECT 'tbl_variants', COUNT(*) FROM dbo.tbl_variants WHERE archived_at IS NULL
UNION ALL
SELECT 'tbl_variant_sizes', COUNT(*) FROM dbo.tbl_variant_sizes
UNION ALL
SELECT 'tbl_variant_colors', COUNT(*) FROM dbo.tbl_variant_colors
UNION ALL
SELECT 'tbl_suppliers', COUNT(*) FROM dbo.tbl_suppliers WHERE archived_at IS NULL
UNION ALL
SELECT 'tbl_inventories', COUNT(*) FROM dbo.tbl_inventories WHERE archives IS NULL
UNION ALL
SELECT 'tbl_stock_in', COUNT(*) FROM dbo.tbl_stock_in WHERE archives IS NULL
UNION ALL
SELECT 'tbl_stock_out', COUNT(*) FROM dbo.tbl_stock_out WHERE archives IS NULL
UNION ALL
SELECT 'tbl_stock_adjustments', COUNT(*) FROM dbo.tbl_stock_adjustments WHERE archives IS NULL
UNION ALL
SELECT 'tbl_purchase_orders', COUNT(*) FROM dbo.tbl_purchase_orders WHERE archived_at IS NULL
UNION ALL
SELECT 'tbl_po_items', COUNT(*) FROM dbo.tbl_po_items
UNION ALL
SELECT 'tbl_sales', COUNT(*) FROM dbo.tbl_sales WHERE archives IS NULL
UNION ALL
SELECT 'tbl_sales_items', COUNT(*) FROM dbo.tbl_sales_items WHERE archives IS NULL
UNION ALL
SELECT 'tbl_payments', COUNT(*) FROM dbo.tbl_payments WHERE archives IS NULL
UNION ALL
SELECT 'tbl_returns', COUNT(*) FROM dbo.tbl_returns WHERE archives IS NULL
UNION ALL
SELECT 'tbl_return_items', COUNT(*) FROM dbo.tbl_return_items WHERE archives IS NULL
ORDER BY TableName;
GO

PRINT '';
PRINT '========================================';
PRINT 'Script completed!';
PRINT '========================================';
PRINT '';
PRINT 'To view data in Azure, connect to Azure SQL Database';
PRINT 'and run the same queries there.';
PRINT '';

