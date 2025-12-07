-- ============================================
-- Find Missing Products in Sync
-- This script helps identify which product is not being synced
-- ============================================

USE db_SoftWear;
GO

PRINT '========================================';
PRINT 'Product Sync Analysis';
PRINT '========================================';
PRINT '';

-- ============================================
-- 1. All Products (Including Archived)
-- ============================================
PRINT '1. ALL Products (including archived):';
PRINT '';

SELECT 
    id,
    name,
    description,
    category_id,
    status,
    user_id,
    CASE 
        WHEN archived_at IS NULL THEN 'Active'
        ELSE 'Archived'
    END AS Status,
    archived_at,
    created_at
FROM dbo.tbl_products
ORDER BY id;
GO

-- ============================================
-- 2. Active Products Only (What Should Sync)
-- ============================================
PRINT '';
PRINT '2. Active Products (should be synced):';
PRINT '';

SELECT 
    id,
    name,
    description,
    category_id,
    status,
    user_id,
    created_at,
    CASE 
        WHEN image IS NULL THEN 'No Image'
        ELSE CONCAT('Has Image (', CAST(DATALENGTH(image) AS VARCHAR), ' bytes)')
    END AS ImageInfo
FROM dbo.tbl_products
WHERE archived_at IS NULL
ORDER BY id;
GO

-- ============================================
-- 3. Archived Products (Won't Be Synced)
-- ============================================
PRINT '';
PRINT '3. Archived Products (WON''T be synced):';
PRINT '';

SELECT 
    id,
    name,
    description,
    category_id,
    status,
    user_id,
    archived_at,
    created_at
FROM dbo.tbl_products
WHERE archived_at IS NOT NULL
ORDER BY id;
GO

-- ============================================
-- 4. Products with Potential Issues
-- ============================================
PRINT '';
PRINT '4. Products with Potential Sync Issues:';
PRINT '';

SELECT 
    id,
    name,
    category_id,
    status,
    user_id,
    CASE 
        WHEN name IS NULL OR name = '' THEN 'Missing Name'
        WHEN category_id IS NULL THEN 'Missing Category'
        WHEN user_id IS NULL THEN 'Missing User ID'
        ELSE 'OK'
    END AS Issue,
    created_at,
    archived_at
FROM dbo.tbl_products
WHERE archived_at IS NULL
    AND (
        name IS NULL OR name = ''
        OR category_id IS NULL
        OR status IS NULL
    )
ORDER BY id;
GO

-- ============================================
-- 5. Product Count Summary
-- ============================================
PRINT '';
PRINT '5. Product Count Summary:';
PRINT '';

SELECT 
    COUNT(*) AS TotalProducts,
    COUNT(CASE WHEN archived_at IS NULL THEN 1 END) AS ActiveProducts,
    COUNT(CASE WHEN archived_at IS NOT NULL THEN 1 END) AS ArchivedProducts,
    COUNT(CASE WHEN image IS NOT NULL AND archived_at IS NULL THEN 1 END) AS ActiveProductsWithImages
FROM dbo.tbl_products;
GO

-- ============================================
-- 6. Check for NULL or Empty Values
-- ============================================
PRINT '';
PRINT '6. Products with NULL/Empty Critical Fields:';
PRINT '';

SELECT 
    id,
    name,
    category_id,
    status,
    user_id,
    CASE 
        WHEN name IS NULL OR name = '' THEN 'YES' ELSE 'NO'
    END AS HasNameIssue,
    CASE 
        WHEN category_id IS NULL THEN 'YES' ELSE 'NO'
    END AS HasCategoryIssue,
    CASE 
        WHEN status IS NULL THEN 'YES' ELSE 'NO'
    END AS HasStatusIssue,
    archived_at
FROM dbo.tbl_products
WHERE archived_at IS NULL
ORDER BY id;
GO

PRINT '';
PRINT '========================================';
PRINT 'Analysis Complete';
PRINT '========================================';
PRINT '';
PRINT 'Check the results above to find:';
PRINT '1. If one product is archived (archived_at IS NOT NULL)';
PRINT '2. If one product has missing required fields';
PRINT '3. Compare with Azure to see which product is missing';
PRINT '';












