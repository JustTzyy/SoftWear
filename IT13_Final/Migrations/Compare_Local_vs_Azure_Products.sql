-- ============================================
-- Compare Local vs Azure Products
-- Run this on LOCAL database first, then on AZURE
-- ============================================

USE db_SoftWear;
GO

PRINT '========================================';
PRINT 'Product Comparison: Local vs Azure';
PRINT '========================================';
PRINT '';
PRINT 'Run this script on BOTH databases and compare results';
PRINT '';

-- ============================================
-- LOCAL DATABASE - Run this first
-- ============================================
PRINT '=== LOCAL DATABASE ===';
PRINT '';

SELECT 
    id,
    name,
    category_id,
    status,
    user_id,
    CASE 
        WHEN image IS NULL THEN 'No Image'
        ELSE CONCAT('Has Image (', CAST(DATALENGTH(image) AS VARCHAR), ' bytes)')
    END AS ImageInfo,
    CASE 
        WHEN archived_at IS NULL THEN 'Active'
        ELSE 'Archived'
    END AS Status,
    created_at
FROM dbo.tbl_products
ORDER BY id;
GO

-- ============================================
-- Count Summary
-- ============================================
PRINT '';
PRINT '=== COUNT SUMMARY ===';
PRINT '';

SELECT 
    'LOCAL' AS Database,
    COUNT(*) AS TotalProducts,
    COUNT(CASE WHEN archived_at IS NULL THEN 1 END) AS ActiveProducts,
    COUNT(CASE WHEN archived_at IS NOT NULL THEN 1 END) AS ArchivedProducts
FROM dbo.tbl_products;
GO

-- ============================================
-- Products by ID (for easy comparison)
-- ============================================
PRINT '';
PRINT '=== PRODUCT IDs (for comparison) ===';
PRINT '';

SELECT 
    id,
    name,
    CASE WHEN archived_at IS NULL THEN 'Active' ELSE 'Archived' END AS Status
FROM dbo.tbl_products
ORDER BY id;
GO

PRINT '';
PRINT '========================================';
PRINT 'INSTRUCTIONS:';
PRINT '1. Copy the results above';
PRINT '2. Run this same script on AZURE database';
PRINT '3. Compare the IDs - which product ID is missing in Azure?';
PRINT '========================================';
PRINT '';












