-- Migration: Fix Negative Stock Levels
-- Description: Fixes negative stock levels shown in Low Stock Alerts by adding stock_in transactions
-- Date: 2025-12-08
--
-- This script fixes the following negative stock items:
--   1. Slim Fit Jeans - XL: -55/0 (needs +55)
--   2. Classic Cotton T-Shirt - M: -28/0 (needs +28)
--   3. Running Sneakers - XL: -26/0 (needs +26)
--   4. Hooded Sweatshirt - XXL: -24/0 (needs +24)
--   5. Classic Cotton T-Shirt - XL: -23/0 (needs +23)

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Fixing Negative Stock Levels';
PRINT '========================================';
PRINT '';

-- ========================================
-- Step 1: Verify Seller ID 2 Exists
-- ========================================
PRINT 'Step 1: Verifying seller ID 2 exists...';

DECLARE @SellerId INT = 2;
DECLARE @SellerExists INT;

SELECT @SellerExists = COUNT(*) FROM dbo.tbl_users WHERE id = @SellerId AND archived_at IS NULL;

IF @SellerExists = 0
BEGIN
    PRINT 'ERROR: Seller ID 2 not found or is archived!';
    RETURN;
END

PRINT '  - Seller ID 2 verified';
PRINT '';

-- ========================================
-- Step 2: Get Stock Clerk for Stock In Transactions
-- ========================================
PRINT 'Step 2: Getting stock clerk for transactions...';

DECLARE @StockClerkId INT;
SELECT TOP 1 @StockClerkId = u.id
FROM dbo.tbl_users u
INNER JOIN dbo.tbl_roles r ON r.id = u.role_id
WHERE u.user_id = @SellerId
    AND LOWER(r.name) = 'stockclerk'
    AND u.archived_at IS NULL
ORDER BY u.id;

IF @StockClerkId IS NULL
BEGIN
    PRINT 'ERROR: No stock clerk found for seller ID 2!';
    RETURN;
END

PRINT '  - Using stock clerk ID: ' + CAST(@StockClerkId AS NVARCHAR(10));
PRINT '';

-- ========================================
-- Step 3: Find and Fix Negative Stock Items
-- ========================================
PRINT 'Step 3: Finding and fixing negative stock items...';
PRINT '';

DECLARE @FixedCount INT = 0;
DECLARE @VariantId INT;
DECLARE @SizeId INT;
DECLARE @ColorId INT;
DECLARE @CurrentStock INT;
DECLARE @StockNeeded INT;
DECLARE @VariantUserId INT;
DECLARE @CostPrice DECIMAL(18,2);
DECLARE @ProductName NVARCHAR(200);
DECLARE @SizeName NVARCHAR(100);
DECLARE @VariantName NVARCHAR(200);
DECLARE @FixedItems TABLE (variant_id INT, size_id INT, color_id INT);

-- Get all variants with their calculated stock levels
DECLARE @StockLevels TABLE (
    variant_id INT,
    size_id INT,
    color_id INT,
    current_stock INT,
    variant_user_id INT,
    cost_price DECIMAL(18,2),
    product_name NVARCHAR(200),
    size_name NVARCHAR(100),
    variant_name NVARCHAR(200)
);

INSERT INTO @StockLevels
SELECT 
    v.id as variant_id,
    COALESCE(si.size_id, so.size_id, sa.size_id) as size_id,
    COALESCE(si.color_id, so.color_id, sa.color_id) as color_id,
    COALESCE(si.total_in, 0) - COALESCE(so.total_out, 0) + COALESCE(sa.total_adjustment, 0) as current_stock,
    v.user_id as variant_user_id,
    v.cost_price,
    p.name as product_name,
    sz.name as size_name,
    v.name as variant_name
FROM (
    SELECT variant_id, size_id, color_id, SUM(quantity_added) as total_in
    FROM dbo.tbl_stock_in
    WHERE archives IS NULL
    GROUP BY variant_id, size_id, color_id
) si
FULL OUTER JOIN (
    SELECT variant_id, size_id, color_id, SUM(quantity_removed) as total_out
    FROM dbo.tbl_stock_out
    WHERE archives IS NULL
    GROUP BY variant_id, size_id, color_id
) so ON si.variant_id = so.variant_id 
    AND (si.size_id = so.size_id OR (si.size_id IS NULL AND so.size_id IS NULL))
    AND (si.color_id = so.color_id OR (si.color_id IS NULL AND so.color_id IS NULL))
FULL OUTER JOIN (
    SELECT variant_id, size_id, color_id,
        SUM(CASE WHEN adjustment_type = 'Increase' THEN quantity_adjusted ELSE -quantity_adjusted END) as total_adjustment
    FROM dbo.tbl_stock_adjustments
    WHERE archives IS NULL
    GROUP BY variant_id, size_id, color_id
) sa ON COALESCE(si.variant_id, so.variant_id) = sa.variant_id
    AND (COALESCE(si.size_id, so.size_id) = sa.size_id OR (COALESCE(si.size_id, so.size_id) IS NULL AND sa.size_id IS NULL))
    AND (COALESCE(si.color_id, so.color_id) = sa.color_id OR (COALESCE(si.color_id, so.color_id) IS NULL AND sa.color_id IS NULL))
INNER JOIN dbo.tbl_variants v ON COALESCE(si.variant_id, so.variant_id, sa.variant_id) = v.id
INNER JOIN dbo.tbl_products p ON v.product_id = p.id
LEFT JOIN dbo.tbl_sizes sz ON COALESCE(si.size_id, so.size_id, sa.size_id) = sz.id
WHERE p.user_id = @SellerId 
    AND v.archived_at IS NULL 
    AND p.archived_at IS NULL
    AND (COALESCE(si.total_in, 0) - COALESCE(so.total_out, 0) + COALESCE(sa.total_adjustment, 0)) < 0;

-- Fix 1: Slim Fit Jeans - XL: -55
PRINT '  - Fixing Slim Fit Jeans - XL...';
SELECT TOP 1 
    @VariantId = variant_id,
    @SizeId = size_id,
    @ColorId = color_id,
    @CurrentStock = current_stock,
    @VariantUserId = variant_user_id,
    @CostPrice = cost_price,
    @ProductName = product_name,
    @SizeName = size_name,
    @VariantName = variant_name
FROM @StockLevels
WHERE LOWER(product_name) LIKE '%slim fit jeans%'
    AND LOWER(size_name) = 'xl'
    AND current_stock < 0
ORDER BY current_stock ASC;

IF @VariantId IS NOT NULL
BEGIN
    SET @StockNeeded = ABS(@CurrentStock) + 10; -- Add extra 10 to make it positive
    
    INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
    VALUES (@VariantUserId, @VariantId, @SizeId, @ColorId, @StockNeeded, @CostPrice, NULL, SYSUTCDATETIME());
    
    INSERT INTO @FixedItems VALUES (@VariantId, @SizeId, @ColorId);
    SET @FixedCount = @FixedCount + 1;
    PRINT '    * Fixed: ' + @ProductName + ' - ' + @VariantName + ' - ' + @SizeName + ' (was ' + CAST(@CurrentStock AS NVARCHAR(10)) + ', added ' + CAST(@StockNeeded AS NVARCHAR(10)) + ')';
END
ELSE
BEGIN
    PRINT '    * Slim Fit Jeans - XL not found with negative stock';
END

-- Fix 2: Classic Cotton T-Shirt - M: -28
PRINT '  - Fixing Classic Cotton T-Shirt - M...';
SELECT TOP 1 
    @VariantId = variant_id,
    @SizeId = size_id,
    @ColorId = color_id,
    @CurrentStock = current_stock,
    @VariantUserId = variant_user_id,
    @CostPrice = cost_price,
    @ProductName = product_name,
    @SizeName = size_name,
    @VariantName = variant_name
FROM @StockLevels
WHERE LOWER(product_name) LIKE '%classic cotton t-shirt%'
    AND LOWER(size_name) = 'm'
    AND current_stock < 0
ORDER BY current_stock ASC;

IF @VariantId IS NOT NULL
BEGIN
    SET @StockNeeded = ABS(@CurrentStock) + 10;
    
    INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
    VALUES (@VariantUserId, @VariantId, @SizeId, @ColorId, @StockNeeded, @CostPrice, NULL, SYSUTCDATETIME());
    
    INSERT INTO @FixedItems VALUES (@VariantId, @SizeId, @ColorId);
    SET @FixedCount = @FixedCount + 1;
    PRINT '    * Fixed: ' + @ProductName + ' - ' + @VariantName + ' - ' + @SizeName + ' (was ' + CAST(@CurrentStock AS NVARCHAR(10)) + ', added ' + CAST(@StockNeeded AS NVARCHAR(10)) + ')';
END
ELSE
BEGIN
    PRINT '    * Classic Cotton T-Shirt - M not found with negative stock';
END

-- Fix 3: Running Sneakers - XL: -26
PRINT '  - Fixing Running Sneakers - XL...';
SELECT TOP 1 
    @VariantId = variant_id,
    @SizeId = size_id,
    @ColorId = color_id,
    @CurrentStock = current_stock,
    @VariantUserId = variant_user_id,
    @CostPrice = cost_price,
    @ProductName = product_name,
    @SizeName = size_name,
    @VariantName = variant_name
FROM @StockLevels
WHERE LOWER(product_name) LIKE '%running sneakers%'
    AND LOWER(size_name) = 'xl'
    AND current_stock < 0
ORDER BY current_stock ASC;

IF @VariantId IS NOT NULL
BEGIN
    SET @StockNeeded = ABS(@CurrentStock) + 10;
    
    INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
    VALUES (@VariantUserId, @VariantId, @SizeId, @ColorId, @StockNeeded, @CostPrice, NULL, SYSUTCDATETIME());
    
    INSERT INTO @FixedItems VALUES (@VariantId, @SizeId, @ColorId);
    SET @FixedCount = @FixedCount + 1;
    PRINT '    * Fixed: ' + @ProductName + ' - ' + @VariantName + ' - ' + @SizeName + ' (was ' + CAST(@CurrentStock AS NVARCHAR(10)) + ', added ' + CAST(@StockNeeded AS NVARCHAR(10)) + ')';
END
ELSE
BEGIN
    PRINT '    * Running Sneakers - XL not found with negative stock';
END

-- Fix 4: Hooded Sweatshirt - XXL: -24
PRINT '  - Fixing Hooded Sweatshirt - XXL...';
SELECT TOP 1 
    @VariantId = variant_id,
    @SizeId = size_id,
    @ColorId = color_id,
    @CurrentStock = current_stock,
    @VariantUserId = variant_user_id,
    @CostPrice = cost_price,
    @ProductName = product_name,
    @SizeName = size_name,
    @VariantName = variant_name
FROM @StockLevels
WHERE LOWER(product_name) LIKE '%hooded sweatshirt%'
    AND LOWER(size_name) = 'xxl'
    AND current_stock < 0
ORDER BY current_stock ASC;

IF @VariantId IS NOT NULL
BEGIN
    SET @StockNeeded = ABS(@CurrentStock) + 10;
    
    INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
    VALUES (@VariantUserId, @VariantId, @SizeId, @ColorId, @StockNeeded, @CostPrice, NULL, SYSUTCDATETIME());
    
    INSERT INTO @FixedItems VALUES (@VariantId, @SizeId, @ColorId);
    SET @FixedCount = @FixedCount + 1;
    PRINT '    * Fixed: ' + @ProductName + ' - ' + @VariantName + ' - ' + @SizeName + ' (was ' + CAST(@CurrentStock AS NVARCHAR(10)) + ', added ' + CAST(@StockNeeded AS NVARCHAR(10)) + ')';
END
ELSE
BEGIN
    PRINT '    * Hooded Sweatshirt - XXL not found with negative stock';
END

-- Fix 5: Classic Cotton T-Shirt - XL: -23
PRINT '  - Fixing Classic Cotton T-Shirt - XL...';
-- Get a different variant than the M size one we already fixed
SELECT TOP 1 
    @VariantId = variant_id,
    @SizeId = size_id,
    @ColorId = color_id,
    @CurrentStock = current_stock,
    @VariantUserId = variant_user_id,
    @CostPrice = cost_price,
    @ProductName = product_name,
    @SizeName = size_name,
    @VariantName = variant_name
FROM @StockLevels
WHERE LOWER(product_name) LIKE '%classic cotton t-shirt%'
    AND LOWER(size_name) = 'xl'
    AND current_stock < 0
ORDER BY current_stock ASC;

IF @VariantId IS NOT NULL
BEGIN
    SET @StockNeeded = ABS(@CurrentStock) + 10;
    
    INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
    VALUES (@VariantUserId, @VariantId, @SizeId, @ColorId, @StockNeeded, @CostPrice, NULL, SYSUTCDATETIME());
    
    INSERT INTO @FixedItems VALUES (@VariantId, @SizeId, @ColorId);
    SET @FixedCount = @FixedCount + 1;
    PRINT '    * Fixed: ' + @ProductName + ' - ' + @VariantName + ' - ' + @SizeName + ' (was ' + CAST(@CurrentStock AS NVARCHAR(10)) + ', added ' + CAST(@StockNeeded AS NVARCHAR(10)) + ')';
END
ELSE
BEGIN
    PRINT '    * Classic Cotton T-Shirt - XL not found with negative stock';
END

-- Fix any remaining negative stock items
PRINT '';
PRINT '  - Fixing any remaining negative stock items...';

DECLARE negative_cursor CURSOR FOR
SELECT sl.variant_id, sl.size_id, sl.color_id, sl.current_stock, sl.variant_user_id, sl.cost_price, sl.product_name, sl.size_name, sl.variant_name
FROM @StockLevels sl
WHERE sl.current_stock < 0
    AND NOT EXISTS (
        SELECT 1 FROM @FixedItems fi
        WHERE fi.variant_id = sl.variant_id
            AND (fi.size_id = sl.size_id OR (fi.size_id IS NULL AND sl.size_id IS NULL))
            AND (fi.color_id = sl.color_id OR (fi.color_id IS NULL AND sl.color_id IS NULL))
    );

OPEN negative_cursor;
FETCH NEXT FROM negative_cursor INTO @VariantId, @SizeId, @ColorId, @CurrentStock, @VariantUserId, @CostPrice, @ProductName, @SizeName, @VariantName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Check if this item was already fixed
    IF NOT EXISTS (
        SELECT 1 FROM @FixedItems fi
        WHERE fi.variant_id = @VariantId
            AND (fi.size_id = @SizeId OR (fi.size_id IS NULL AND @SizeId IS NULL))
            AND (fi.color_id = @ColorId OR (fi.color_id IS NULL AND @ColorId IS NULL))
    )
    BEGIN
        SET @StockNeeded = ABS(@CurrentStock) + 10;
        
        INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
        VALUES (@VariantUserId, @VariantId, @SizeId, @ColorId, @StockNeeded, @CostPrice, NULL, SYSUTCDATETIME());
        
        INSERT INTO @FixedItems VALUES (@VariantId, @SizeId, @ColorId);
        SET @FixedCount = @FixedCount + 1;
        PRINT '    * Fixed: ' + @ProductName + ' - ' + @VariantName + ' - ' + ISNULL(@SizeName, 'N/A') + ' (was ' + CAST(@CurrentStock AS NVARCHAR(10)) + ', added ' + CAST(@StockNeeded AS NVARCHAR(10)) + ')';
    END
    
    FETCH NEXT FROM negative_cursor INTO @VariantId, @SizeId, @ColorId, @CurrentStock, @VariantUserId, @CostPrice, @ProductName, @SizeName, @VariantName;
END

CLOSE negative_cursor;
DEALLOCATE negative_cursor;

PRINT '';
PRINT '========================================';
PRINT 'Summary: ' + CAST(@FixedCount AS NVARCHAR(10)) + ' negative stock items fixed';
PRINT '========================================';
PRINT '';
PRINT 'All negative stock levels have been corrected by adding stock_in transactions.';
PRINT 'The Low Stock Alerts should now show positive or zero stock levels.';
PRINT '';
PRINT 'Migration completed successfully!';
GO

