-- ============================================
-- Check for Duplicate Products or Missing Products
-- ============================================

USE db_SoftWear;
GO

PRINT '========================================';
PRINT 'Product Duplicate and Missing Analysis';
PRINT '========================================';
PRINT '';

-- ============================================
-- 1. Check for Duplicate IDs (Shouldn't happen with IDENTITY)
-- ============================================
PRINT '1. Checking for Duplicate Product IDs:';
PRINT '';

SELECT 
    id,
    COUNT(*) AS DuplicateCount
FROM dbo.tbl_products
GROUP BY id
HAVING COUNT(*) > 1;
GO

-- ============================================
-- 2. All Products with Full Details
-- ============================================
PRINT '';
PRINT '2. All Products with Full Details:';
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
        ELSE CONCAT('Has Image (', CAST(DATALENGTH(image) AS VARCHAR), ' bytes)')
    END AS ImageInfo,
    archived_at,
    created_at,
    updated_at
FROM dbo.tbl_products
ORDER BY id;
GO

-- ============================================
-- 3. Products that Would Be Synced (Active Only)
-- ============================================
PRINT '';
PRINT '3. Products that SHOULD be synced (Active):';
PRINT '';

SELECT 
    id,
    name,
    category_id,
    status,
    user_id,
    created_at
FROM dbo.tbl_products
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 4. Check for Products with Same Name (Potential Duplicates)
-- ============================================
PRINT '';
PRINT '4. Products with Same Name (Potential Duplicates):';
PRINT '';

SELECT 
    name,
    COUNT(*) AS Count,
    STRING_AGG(CAST(id AS VARCHAR), ', ') AS ProductIDs
FROM dbo.tbl_products
WHERE archived_at IS NULL
GROUP BY name
HAVING COUNT(*) > 1;
GO

-- ============================================
-- 5. Compare with Azure (Run this on Azure database)
-- ============================================
PRINT '';
PRINT '5. To check what exists in Azure, run this on Azure database:';
PRINT '';

PRINT 'SELECT id, name, category_id, status, user_id, created_at';
PRINT 'FROM dbo.tbl_products';
PRINT 'WHERE archived_at IS NULL';
PRINT 'ORDER BY id;';
PRINT '';

-- ============================================
-- 6. Find Products Missing in Azure
-- ============================================
PRINT '';
PRINT '6. To find which products are missing in Azure:';
PRINT '   Run the query below on LOCAL database, then compare with Azure';
PRINT '';

SELECT 
    id,
    name,
    category_id,
    status,
    user_id,
    created_at,
    'This product should be in Azure' AS Note
FROM dbo.tbl_products
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 7. Check for NULL or Invalid Category IDs
-- ============================================
PRINT '';
PRINT '7. Products with Invalid Category IDs:';
PRINT '';

SELECT 
    p.id,
    p.name,
    p.category_id,
    CASE 
        WHEN p.category_id IS NULL THEN 'NULL Category'
        WHEN NOT EXISTS (SELECT 1 FROM dbo.tbl_categories WHERE id = p.category_id) THEN 'Invalid Category'
        ELSE 'Valid Category'
    END AS CategoryStatus
FROM dbo.tbl_products p
WHERE p.archived_at IS NULL
ORDER BY p.id;
GO

PRINT '';
PRINT '========================================';
PRINT 'Analysis Complete';
PRINT '========================================';
PRINT '';
PRINT 'If you see 4 products above but only 3 sync:';
PRINT '1. Check if one has archived_at set';
PRINT '2. Check if one already exists in Azure (same ID)';
PRINT '3. Check if one has invalid category_id';
PRINT '4. Check error messages in the sync result';
PRINT '';












