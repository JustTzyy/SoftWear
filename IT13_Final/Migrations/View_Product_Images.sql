-- ============================================
-- View Product Images Data
-- This script shows product image information
-- ============================================

USE db_SoftWear;
GO

PRINT '========================================';
PRINT 'Product Images Information';
PRINT '========================================';
PRINT '';

-- ============================================
-- 1. Products with Images Summary
-- ============================================
PRINT '1. Products Image Summary:';
PRINT '';

SELECT 
    COUNT(*) AS TotalProducts,
    COUNT(image) AS ProductsWithImages,
    COUNT(*) - COUNT(image) AS ProductsWithoutImages,
    SUM(CASE WHEN image IS NOT NULL THEN DATALENGTH(image) ELSE 0 END) AS TotalImageSizeBytes,
    AVG(CASE WHEN image IS NOT NULL THEN CAST(DATALENGTH(image) AS FLOAT) ELSE NULL END) AS AvgImageSizeBytes
FROM dbo.tbl_products
WHERE archived_at IS NULL;
GO

-- ============================================
-- 2. All Products with Image Details
-- ============================================
PRINT '';
PRINT '2. All Products with Image Details:';
PRINT '';

SELECT 
    id,
    name,
    category_id,
    image_content_type,
    CASE 
        WHEN image IS NULL THEN 'No Image'
        WHEN DATALENGTH(image) = 0 THEN 'Empty Image (0 bytes)'
        ELSE CONCAT('Image: ', CAST(DATALENGTH(image) AS VARCHAR), ' bytes')
    END AS ImageStatus,
    CASE 
        WHEN image IS NULL THEN NULL
        ELSE CAST(DATALENGTH(image) AS BIGINT)
    END AS ImageSizeBytes,
    CASE 
        WHEN image IS NULL THEN NULL
        WHEN DATALENGTH(image) < 1024 THEN CONCAT(DATALENGTH(image), ' B')
        WHEN DATALENGTH(image) < 1048576 THEN CONCAT(CAST(DATALENGTH(image) / 1024.0 AS DECIMAL(10,2)), ' KB')
        ELSE CONCAT(CAST(DATALENGTH(image) / 1048576.0 AS DECIMAL(10,2)), ' MB')
    END AS ImageSizeFormatted,
    created_at
FROM dbo.tbl_products
WHERE archived_at IS NULL
ORDER BY 
    CASE WHEN image IS NULL THEN 1 ELSE 0 END,
    id;
GO

-- ============================================
-- 3. Products WITHOUT Images
-- ============================================
PRINT '';
PRINT '3. Products WITHOUT Images:';
PRINT '';

SELECT 
    id,
    name,
    category_id,
    created_at
FROM dbo.tbl_products
WHERE archived_at IS NULL
    AND image IS NULL
ORDER BY id;
GO

-- ============================================
-- 4. Products WITH Images (Detailed)
-- ============================================
PRINT '';
PRINT '4. Products WITH Images (Detailed):';
PRINT '';

SELECT 
    id,
    name,
    category_id,
    image_content_type,
    DATALENGTH(image) AS ImageSizeBytes,
    CASE 
        WHEN DATALENGTH(image) < 1024 THEN CONCAT(DATALENGTH(image), ' B')
        WHEN DATALENGTH(image) < 1048576 THEN CONCAT(CAST(DATALENGTH(image) / 1024.0 AS DECIMAL(10,2)), ' KB')
        ELSE CONCAT(CAST(DATALENGTH(image) / 1048576.0 AS DECIMAL(10,2)), ' MB')
    END AS ImageSize,
    created_at
FROM dbo.tbl_products
WHERE archived_at IS NULL
    AND image IS NOT NULL
    AND DATALENGTH(image) > 0
ORDER BY DATALENGTH(image) DESC;
GO

-- ============================================
-- 5. Image Content Types Summary
-- ============================================
PRINT '';
PRINT '5. Image Content Types Summary:';
PRINT '';

SELECT 
    image_content_type,
    COUNT(*) AS ProductCount,
    SUM(DATALENGTH(image)) AS TotalSizeBytes,
    AVG(CAST(DATALENGTH(image) AS FLOAT)) AS AvgSizeBytes
FROM dbo.tbl_products
WHERE archived_at IS NULL
    AND image IS NOT NULL
GROUP BY image_content_type
ORDER BY ProductCount DESC;
GO

-- ============================================
-- 6. Largest Images
-- ============================================
PRINT '';
PRINT '6. Top 10 Largest Product Images:';
PRINT '';

SELECT TOP 10
    id,
    name,
    image_content_type,
    DATALENGTH(image) AS ImageSizeBytes,
    CASE 
        WHEN DATALENGTH(image) < 1024 THEN CONCAT(DATALENGTH(image), ' B')
        WHEN DATALENGTH(image) < 1048576 THEN CONCAT(CAST(DATALENGTH(image) / 1024.0 AS DECIMAL(10,2)), ' KB')
        ELSE CONCAT(CAST(DATALENGTH(image) / 1048576.0 AS DECIMAL(10,2)), ' MB')
    END AS ImageSize
FROM dbo.tbl_products
WHERE archived_at IS NULL
    AND image IS NOT NULL
    AND DATALENGTH(image) > 0
ORDER BY DATALENGTH(image) DESC;
GO

PRINT '';
PRINT '========================================';
PRINT 'Note: Image data (VARBINARY) is not displayed';
PRINT 'but the size and content type are shown.';
PRINT '========================================';
PRINT '';












