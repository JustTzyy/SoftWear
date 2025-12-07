-- Migration: Seed Data for Seller User ID 2
-- Description: Creates Categories, Products, Sizes, Colors, Variants, Suppliers, and Supplier Addresses for seller user_id = 2
-- Requirements:
--   - 2 Categories (minimum)
--   - 5 Products per category (10 total products minimum)
--   - 7 Sizes (XS, S, M, L, XL, XXL, XXXL)
--   - 35 Colors (minimum)
--   - 35 Variants (minimum)
--   - 25 Suppliers (minimum)
--   - 25 Supplier Addresses (minimum)
-- Date: 2025-01-23

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Seeding Data for Seller User ID 2';
PRINT '========================================';
PRINT '';

-- Verify that user_id = 2 exists
DECLARE @SellerUserId INT = 2;
DECLARE @SellerExists INT;

SELECT @SellerExists = COUNT(*) 
FROM dbo.tbl_users 
WHERE id = @SellerUserId AND archived_at IS NULL;

IF @SellerExists = 0
BEGIN
    PRINT 'ERROR: User ID 2 does not exist or is archived!';
    PRINT 'Please ensure user_id = 2 exists before running this script.';
    RETURN;
END

PRINT 'Seller user ID 2 verified.';
PRINT '';
GO

-- ========================================
-- Step 1: Insert Categories (2 minimum)
-- ========================================
PRINT 'Step 1: Inserting Categories...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @Category1Id INT;
DECLARE @Category2Id INT;

-- Category 1: Clothing
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_categories WHERE user_id = @SellerUserId AND LOWER(name) = 'clothing' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_categories (user_id, name, description, created_at)
    VALUES (@SellerUserId, 'Clothing', 'Apparel and garments including shirts, pants, dresses, and accessories', SYSUTCDATETIME());
    SET @Category1Id = SCOPE_IDENTITY();
    PRINT '  - Category "Clothing" created (ID: ' + CAST(@Category1Id AS NVARCHAR(10)) + ')';
END
ELSE
BEGIN
    SELECT @Category1Id = id FROM dbo.tbl_categories WHERE user_id = @SellerUserId AND LOWER(name) = 'clothing' AND archived_at IS NULL;
    PRINT '  - Category "Clothing" already exists (ID: ' + CAST(@Category1Id AS NVARCHAR(10)) + ')';
END

-- Category 2: Footwear
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_categories WHERE user_id = @SellerUserId AND LOWER(name) = 'footwear' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_categories (user_id, name, description, created_at)
    VALUES (@SellerUserId, 'Footwear', 'Shoes, sneakers, boots, sandals, and other foot accessories', SYSUTCDATETIME());
    SET @Category2Id = SCOPE_IDENTITY();
    PRINT '  - Category "Footwear" created (ID: ' + CAST(@Category2Id AS NVARCHAR(10)) + ')';
END
ELSE
BEGIN
    SELECT @Category2Id = id FROM dbo.tbl_categories WHERE user_id = @SellerUserId AND LOWER(name) = 'footwear' AND archived_at IS NULL;
    PRINT '  - Category "Footwear" already exists (ID: ' + CAST(@Category2Id AS NVARCHAR(10)) + ')';
END

PRINT '';
GO

-- ========================================
-- Step 2: Insert Sizes (7 standard sizes)
-- ========================================
PRINT 'Step 2: Inserting Sizes (7 standard sizes)...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @SizeCounter INT = 0;

-- Insert standard sizes only
DECLARE @SizesToInsert TABLE (name NVARCHAR(100), description NVARCHAR(500));
INSERT INTO @SizesToInsert VALUES 
    ('XS', 'Extra Small size'), 
    ('S', 'Small size'), 
    ('M', 'Medium size'), 
    ('L', 'Large size'), 
    ('XL', 'Extra Large size'), 
    ('XXL', 'Double Extra Large size'), 
    ('XXXL', 'Triple Extra Large size');

DECLARE @SizeName NVARCHAR(100);
DECLARE @SizeDesc NVARCHAR(500);
DECLARE size_cursor CURSOR FOR SELECT name, description FROM @SizesToInsert;
OPEN size_cursor;
FETCH NEXT FROM size_cursor INTO @SizeName, @SizeDesc;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.tbl_sizes WHERE user_id = @SellerUserId AND LOWER(name) = LOWER(@SizeName) AND archived_at IS NULL)
    BEGIN
        INSERT INTO dbo.tbl_sizes (user_id, name, description, created_at)
        VALUES (@SellerUserId, @SizeName, @SizeDesc, SYSUTCDATETIME());
        SET @SizeCounter = @SizeCounter + 1;
    END
    FETCH NEXT FROM size_cursor INTO @SizeName, @SizeDesc;
END
CLOSE size_cursor;
DEALLOCATE size_cursor;

SELECT @SizeCounter = COUNT(*) FROM dbo.tbl_sizes WHERE user_id = @SellerUserId AND archived_at IS NULL;
PRINT '  - ' + CAST(@SizeCounter AS NVARCHAR(10)) + ' sizes created/verified';
PRINT '';
GO

-- ========================================
-- Step 3: Insert Colors (35 minimum)
-- ========================================
PRINT 'Step 3: Inserting Colors (35 minimum)...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @ColorCounter INT = 0;

-- Insert standard colors
DECLARE @ColorsToInsert TABLE (name NVARCHAR(100), hex_value NVARCHAR(7), description NVARCHAR(500));
INSERT INTO @ColorsToInsert VALUES 
    ('Black', '#000000', 'Black color'), ('White', '#FFFFFF', 'White color'), ('Gray', '#808080', 'Gray color'), 
    ('Navy', '#000080', 'Navy blue color'), ('Red', '#FF0000', 'Red color'), ('Blue', '#0000FF', 'Blue color'), 
    ('Green', '#008000', 'Green color'), ('Yellow', '#FFFF00', 'Yellow color'), ('Orange', '#FFA500', 'Orange color'), 
    ('Purple', '#800080', 'Purple color'), ('Pink', '#FFC0CB', 'Pink color'), ('Brown', '#A52A2A', 'Brown color'),
    ('Beige', '#F5F5DC', 'Beige color'), ('Khaki', '#C3B091', 'Khaki color'), ('Olive', '#808000', 'Olive color'), 
    ('Maroon', '#800000', 'Maroon color'), ('Teal', '#008080', 'Teal color'), ('Cyan', '#00FFFF', 'Cyan color'), 
    ('Magenta', '#FF00FF', 'Magenta color'), ('Lime', '#00FF00', 'Lime color'), ('Gold', '#FFD700', 'Gold color'), 
    ('Silver', '#C0C0C0', 'Silver color'), ('Coral', '#FF7F50', 'Coral color'), ('Salmon', '#FA8072', 'Salmon color'),
    ('Turquoise', '#40E0D0', 'Turquoise color'), ('Lavender', '#E6E6FA', 'Lavender color'), ('Mint', '#98FF98', 'Mint color'), 
    ('Peach', '#FFDAB9', 'Peach color'), ('Ivory', '#FFFFF0', 'Ivory color'), ('Charcoal', '#36454F', 'Charcoal color'), 
    ('Burgundy', '#800020', 'Burgundy color'), ('Indigo', '#4B0082', 'Indigo color'), ('Cream', '#FFFDD0', 'Cream color'), 
    ('Tan', '#D2B48C', 'Tan color'), ('Crimson', '#DC143C', 'Crimson color');

DECLARE @ColorName NVARCHAR(100);
DECLARE @ColorHex NVARCHAR(7);
DECLARE @ColorDesc NVARCHAR(500);
DECLARE color_cursor CURSOR FOR SELECT name, hex_value, description FROM @ColorsToInsert;
OPEN color_cursor;
FETCH NEXT FROM color_cursor INTO @ColorName, @ColorHex, @ColorDesc;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.tbl_colors WHERE user_id = @SellerUserId AND LOWER(name) = LOWER(@ColorName) AND archived_at IS NULL)
    BEGIN
        INSERT INTO dbo.tbl_colors (user_id, name, hex_value, description, created_at)
        VALUES (@SellerUserId, @ColorName, @ColorHex, @ColorDesc, SYSUTCDATETIME());
        SET @ColorCounter = @ColorCounter + 1;
    END
    FETCH NEXT FROM color_cursor INTO @ColorName, @ColorHex, @ColorDesc;
END
CLOSE color_cursor;
DEALLOCATE color_cursor;

-- Add more colors to reach 35 minimum
WHILE @ColorCounter < 35
BEGIN
    DECLARE @NewColorName NVARCHAR(100) = 'Color-' + CAST(@ColorCounter + 1 AS NVARCHAR(10));
    DECLARE @NewColorHex NVARCHAR(7) = '#' + RIGHT('000000' + CAST(CAST(RAND() * 16777215 AS INT) AS VARCHAR(6)), 6);
    IF NOT EXISTS (SELECT 1 FROM dbo.tbl_colors WHERE user_id = @SellerUserId AND LOWER(name) = LOWER(@NewColorName) AND archived_at IS NULL)
    BEGIN
        INSERT INTO dbo.tbl_colors (user_id, name, hex_value, description, created_at)
        VALUES (@SellerUserId, @NewColorName, @NewColorHex, 'Custom ' + @NewColorName, SYSUTCDATETIME());
        SET @ColorCounter = @ColorCounter + 1;
    END
    ELSE
    BEGIN
        SET @ColorCounter = @ColorCounter + 1;
    END
END

SELECT @ColorCounter = COUNT(*) FROM dbo.tbl_colors WHERE user_id = @SellerUserId AND archived_at IS NULL;
PRINT '  - ' + CAST(@ColorCounter AS NVARCHAR(10)) + ' colors created/verified';
PRINT '';
GO

-- ========================================
-- Step 4: Insert Products (5 per category = 10 minimum)
-- ========================================
PRINT 'Step 4: Inserting Products (5 per category)...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @Category1Id INT;
DECLARE @Category2Id INT;
DECLARE @ProductCounter INT = 0;

SELECT @Category1Id = id FROM dbo.tbl_categories WHERE user_id = @SellerUserId AND LOWER(name) = 'clothing' AND archived_at IS NULL;
SELECT @Category2Id = id FROM dbo.tbl_categories WHERE user_id = @SellerUserId AND LOWER(name) = 'footwear' AND archived_at IS NULL;

-- Clothing Products (Category 1)
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category1Id AND LOWER(name) = 'classic cotton t-shirt' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category1Id, 'Classic Cotton T-Shirt', 'Comfortable 100% cotton t-shirt, perfect for everyday wear', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Classic Cotton T-Shirt" created';
END

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category1Id AND LOWER(name) = 'slim fit jeans' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category1Id, 'Slim Fit Jeans', 'Modern slim-fit denim jeans with stretch for comfort', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Slim Fit Jeans" created';
END

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category1Id AND LOWER(name) = 'polo shirt' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category1Id, 'Polo Shirt', 'Classic polo shirt made from premium cotton blend', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Polo Shirt" created';
END

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category1Id AND LOWER(name) = 'hooded sweatshirt' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category1Id, 'Hooded Sweatshirt', 'Warm and cozy hooded sweatshirt, ideal for casual wear', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Hooded Sweatshirt" created';
END

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category1Id AND LOWER(name) = 'chino pants' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category1Id, 'Chino Pants', 'Versatile chino pants suitable for both casual and semi-formal occasions', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Chino Pants" created';
END

-- Footwear Products (Category 2)
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category2Id AND LOWER(name) = 'running sneakers' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category2Id, 'Running Sneakers', 'Lightweight running shoes with cushioned sole for maximum comfort', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Running Sneakers" created';
END

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category2Id AND LOWER(name) = 'leather dress shoes' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category2Id, 'Leather Dress Shoes', 'Elegant leather dress shoes perfect for formal occasions', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Leather Dress Shoes" created';
END

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category2Id AND LOWER(name) = 'canvas casual shoes' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category2Id, 'Canvas Casual Shoes', 'Comfortable canvas shoes for everyday casual wear', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Canvas Casual Shoes" created';
END

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category2Id AND LOWER(name) = 'athletic training shoes' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category2Id, 'Athletic Training Shoes', 'High-performance training shoes with excellent support', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Athletic Training Shoes" created';
END

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_products WHERE user_id = @SellerUserId AND category_id = @Category2Id AND LOWER(name) = 'comfort walking shoes' AND archived_at IS NULL)
BEGIN
    INSERT INTO dbo.tbl_products (user_id, category_id, name, description, status, created_at)
    VALUES (@SellerUserId, @Category2Id, 'Comfort Walking Shoes', 'Ergonomic walking shoes designed for all-day comfort', 'Active', SYSUTCDATETIME());
    SET @ProductCounter = @ProductCounter + 1;
    PRINT '  - Product "Comfort Walking Shoes" created';
END

SELECT @ProductCounter = COUNT(*) FROM dbo.tbl_products WHERE user_id = @SellerUserId AND archived_at IS NULL;
PRINT '  - Total: ' + CAST(@ProductCounter AS NVARCHAR(10)) + ' products created/verified';
PRINT '';
GO

-- ========================================
-- Step 5: Insert Variants (35 minimum)
-- ========================================
PRINT 'Step 5: Inserting Variants (35 minimum)...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @VariantCounter INT = 0;
DECLARE @ProductId INT;
DECLARE @SizeId INT;
DECLARE @ColorId INT;
DECLARE @VariantId INT;
DECLARE @VariantName NVARCHAR(200);
DECLARE @VariantPrice DECIMAL(18,2);
DECLARE @VariantCostPrice DECIMAL(18,2);
DECLARE @SizeName NVARCHAR(100);
DECLARE @ColorName NVARCHAR(100);
DECLARE @ProductName NVARCHAR(200);

-- Get all products for this seller
DECLARE @ProductIds TABLE (id INT, name NVARCHAR(200));
INSERT INTO @ProductIds
SELECT id, name FROM dbo.tbl_products WHERE user_id = @SellerUserId AND archived_at IS NULL;

-- Get all sizes for this seller
DECLARE @SizeIds TABLE (id INT, name NVARCHAR(100));
INSERT INTO @SizeIds
SELECT id, name FROM dbo.tbl_sizes WHERE user_id = @SellerUserId AND archived_at IS NULL;

-- Get all colors for this seller
DECLARE @ColorIds TABLE (id INT, name NVARCHAR(100));
INSERT INTO @ColorIds
SELECT id, name FROM dbo.tbl_colors WHERE user_id = @SellerUserId AND archived_at IS NULL;

-- Create variants by combining products with sizes and colors
DECLARE product_cursor CURSOR FOR SELECT id, name FROM @ProductIds;
OPEN product_cursor;
FETCH NEXT FROM product_cursor INTO @ProductId, @ProductName;

WHILE @@FETCH_STATUS = 0 AND @VariantCounter < 35
BEGIN
    -- For each product, create variants with different sizes and colors
    DECLARE size_cursor CURSOR FOR SELECT TOP 3 id, name FROM @SizeIds ORDER BY NEWID();
    OPEN size_cursor;
    FETCH NEXT FROM size_cursor INTO @SizeId, @SizeName;
    
    WHILE @@FETCH_STATUS = 0 AND @VariantCounter < 35
    BEGIN
        DECLARE color_cursor CURSOR FOR SELECT TOP 2 id, name FROM @ColorIds ORDER BY NEWID();
        OPEN color_cursor;
        FETCH NEXT FROM color_cursor INTO @ColorId, @ColorName;
        
        WHILE @@FETCH_STATUS = 0 AND @VariantCounter < 35
        BEGIN
            -- Create variant name
            SET @VariantName = @ProductName + ' - ' + @SizeName + ' - ' + @ColorName;
            
            -- Check if variant already exists
            IF NOT EXISTS (
                SELECT 1 FROM dbo.tbl_variants v
                WHERE v.user_id = @SellerUserId 
                AND v.product_id = @ProductId 
                AND v.name = @VariantName 
                AND v.archived_at IS NULL
            )
            BEGIN
                -- Generate random price between 500 and 5000
                SET @VariantPrice = 500 + (RAND() * 4500);
                SET @VariantCostPrice = @VariantPrice * (0.4 + RAND() * 0.3); -- Cost is 40-70% of price
                
                -- Insert variant
                INSERT INTO dbo.tbl_variants (user_id, product_id, name, price, cost_price, created_at)
                VALUES (@SellerUserId, @ProductId, @VariantName, @VariantPrice, @VariantCostPrice, SYSUTCDATETIME());
                SET @VariantId = SCOPE_IDENTITY();
                
                -- Link variant to size
                IF NOT EXISTS (SELECT 1 FROM dbo.tbl_variant_sizes WHERE variant_id = @VariantId AND size_id = @SizeId)
                BEGIN
                    INSERT INTO dbo.tbl_variant_sizes (variant_id, size_id, created_at)
                    VALUES (@VariantId, @SizeId, SYSUTCDATETIME());
                END
                
                -- Link variant to color
                IF NOT EXISTS (SELECT 1 FROM dbo.tbl_variant_colors WHERE variant_id = @VariantId AND color_id = @ColorId)
                BEGIN
                    INSERT INTO dbo.tbl_variant_colors (variant_id, color_id, created_at)
                    VALUES (@VariantId, @ColorId, SYSUTCDATETIME());
                END
                
                SET @VariantCounter = @VariantCounter + 1;
            END
            
            FETCH NEXT FROM color_cursor INTO @ColorId, @ColorName;
        END
        CLOSE color_cursor;
        DEALLOCATE color_cursor;
        
        FETCH NEXT FROM size_cursor INTO @SizeId, @SizeName;
    END
    CLOSE size_cursor;
    DEALLOCATE size_cursor;
    
    FETCH NEXT FROM product_cursor INTO @ProductId, @ProductName;
END
CLOSE product_cursor;
DEALLOCATE product_cursor;

-- If we still need more variants, create additional ones
WHILE @VariantCounter < 35
BEGIN
    -- Get random product, size, and color
    SELECT TOP 1 @ProductId = id, @ProductName = name FROM @ProductIds ORDER BY NEWID();
    SELECT TOP 1 @SizeId = id, @SizeName = name FROM @SizeIds ORDER BY NEWID();
    SELECT TOP 1 @ColorId = id, @ColorName = name FROM @ColorIds ORDER BY NEWID();
    
    SET @VariantName = @ProductName + ' - ' + @SizeName + ' - ' + @ColorName;
    
    IF NOT EXISTS (
        SELECT 1 FROM dbo.tbl_variants v
        WHERE v.user_id = @SellerUserId 
        AND v.product_id = @ProductId 
        AND v.name = @VariantName 
        AND v.archived_at IS NULL
    )
    BEGIN
        SET @VariantPrice = 500 + (RAND() * 4500);
        SET @VariantCostPrice = @VariantPrice * (0.4 + RAND() * 0.3);
        
        INSERT INTO dbo.tbl_variants (user_id, product_id, name, price, cost_price, created_at)
        VALUES (@SellerUserId, @ProductId, @VariantName, @VariantPrice, @VariantCostPrice, SYSUTCDATETIME());
        SET @VariantId = SCOPE_IDENTITY();
        
        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_variant_sizes WHERE variant_id = @VariantId AND size_id = @SizeId)
        BEGIN
            INSERT INTO dbo.tbl_variant_sizes (variant_id, size_id, created_at)
            VALUES (@VariantId, @SizeId, SYSUTCDATETIME());
        END
        
        IF NOT EXISTS (SELECT 1 FROM dbo.tbl_variant_colors WHERE variant_id = @VariantId AND color_id = @ColorId)
        BEGIN
            INSERT INTO dbo.tbl_variant_colors (variant_id, color_id, created_at)
            VALUES (@VariantId, @ColorId, SYSUTCDATETIME());
        END
        
        SET @VariantCounter = @VariantCounter + 1;
    END
END

SELECT @VariantCounter = COUNT(*) FROM dbo.tbl_variants WHERE user_id = @SellerUserId AND archived_at IS NULL;
PRINT '  - ' + CAST(@VariantCounter AS NVARCHAR(10)) + ' variants created/verified';

-- Ensure all variants have size and color relationships
DECLARE @MissingSizeCount INT = 0;
DECLARE @MissingColorCount INT = 0;
DECLARE @VariantIdCheck INT;
DECLARE @SizeIdCheck INT;
DECLARE @ColorIdCheck INT;

-- Get size and color IDs for this seller (redeclare for verification step)
DECLARE @SizeIdsCheck TABLE (id INT);
INSERT INTO @SizeIdsCheck
SELECT id FROM dbo.tbl_sizes WHERE user_id = @SellerUserId AND archived_at IS NULL;

DECLARE @ColorIdsCheck TABLE (id INT);
INSERT INTO @ColorIdsCheck
SELECT id FROM dbo.tbl_colors WHERE user_id = @SellerUserId AND archived_at IS NULL;

-- Check for variants missing size relationships
DECLARE variant_size_check CURSOR FOR
SELECT v.id 
FROM dbo.tbl_variants v
WHERE v.user_id = @SellerUserId 
AND v.archived_at IS NULL
AND NOT EXISTS (SELECT 1 FROM dbo.tbl_variant_sizes vs WHERE vs.variant_id = v.id);

OPEN variant_size_check;
FETCH NEXT FROM variant_size_check INTO @VariantIdCheck;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Assign a random size to this variant
    SELECT TOP 1 @SizeIdCheck = id FROM @SizeIdsCheck ORDER BY NEWID();
    IF @SizeIdCheck IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.tbl_variant_sizes WHERE variant_id = @VariantIdCheck AND size_id = @SizeIdCheck)
    BEGIN
        INSERT INTO dbo.tbl_variant_sizes (variant_id, size_id, created_at)
        VALUES (@VariantIdCheck, @SizeIdCheck, SYSUTCDATETIME());
        SET @MissingSizeCount = @MissingSizeCount + 1;
    END
    FETCH NEXT FROM variant_size_check INTO @VariantIdCheck;
END
CLOSE variant_size_check;
DEALLOCATE variant_size_check;

-- Check for variants missing color relationships
DECLARE variant_color_check CURSOR FOR
SELECT v.id 
FROM dbo.tbl_variants v
WHERE v.user_id = @SellerUserId 
AND v.archived_at IS NULL
AND NOT EXISTS (SELECT 1 FROM dbo.tbl_variant_colors vc WHERE vc.variant_id = v.id);

OPEN variant_color_check;
FETCH NEXT FROM variant_color_check INTO @VariantIdCheck;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Assign a random color to this variant
    SELECT TOP 1 @ColorIdCheck = id FROM @ColorIdsCheck ORDER BY NEWID();
    IF @ColorIdCheck IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.tbl_variant_colors WHERE variant_id = @VariantIdCheck AND color_id = @ColorIdCheck)
    BEGIN
        INSERT INTO dbo.tbl_variant_colors (variant_id, color_id, created_at)
        VALUES (@VariantIdCheck, @ColorIdCheck, SYSUTCDATETIME());
        SET @MissingColorCount = @MissingColorCount + 1;
    END
    FETCH NEXT FROM variant_color_check INTO @VariantIdCheck;
END
CLOSE variant_color_check;
DEALLOCATE variant_color_check;

-- Verify and report variant-size and variant-color relationships
DECLARE @VariantSizeCount INT;
DECLARE @VariantColorCount INT;
SELECT @VariantSizeCount = COUNT(*) 
FROM dbo.tbl_variant_sizes vs
INNER JOIN dbo.tbl_variants v ON vs.variant_id = v.id
WHERE v.user_id = @SellerUserId AND v.archived_at IS NULL;

SELECT @VariantColorCount = COUNT(*) 
FROM dbo.tbl_variant_colors vc
INNER JOIN dbo.tbl_variants v ON vc.variant_id = v.id
WHERE v.user_id = @SellerUserId AND v.archived_at IS NULL;

PRINT '  - ' + CAST(@VariantSizeCount AS NVARCHAR(10)) + ' variant-size relationships in tbl_variant_sizes';
PRINT '  - ' + CAST(@VariantColorCount AS NVARCHAR(10)) + ' variant-color relationships in tbl_variant_colors';
IF @MissingSizeCount > 0 OR @MissingColorCount > 0
BEGIN
    PRINT '  - Fixed ' + CAST(@MissingSizeCount AS NVARCHAR(10)) + ' missing size relationships';
    PRINT '  - Fixed ' + CAST(@MissingColorCount AS NVARCHAR(10)) + ' missing color relationships';
END
PRINT '';
GO

-- ========================================
-- Step 6: Insert Suppliers (25 minimum)
-- ========================================
PRINT 'Step 6: Inserting Suppliers (25 minimum)...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @SupplierCounter INT = 0;
DECLARE @SupplierId INT;
DECLARE @CompanyName NVARCHAR(200);
DECLARE @ContactPerson NVARCHAR(150);
DECLARE @Email NVARCHAR(256);
DECLARE @ContactNumber NVARCHAR(30);
DECLARE @Street NVARCHAR(200);
DECLARE @City NVARCHAR(100);
DECLARE @Province NVARCHAR(100);
DECLARE @Zip NVARCHAR(20);
DECLARE @CityIndex INT;
DECLARE @ProvinceIndex INT;

-- Sample supplier company names
DECLARE @SupplierNames TABLE (company_name NVARCHAR(200), contact_person NVARCHAR(150), email_suffix NVARCHAR(50));
INSERT INTO @SupplierNames VALUES 
    ('Global Textiles Inc.', 'John Smith', 'globaltextiles.com'),
    ('Fashion Forward Ltd.', 'Sarah Johnson', 'fashionforward.com'),
    ('Premium Apparel Co.', 'Michael Brown', 'premiumapparel.com'),
    ('Style Solutions', 'Emily Davis', 'stylesolutions.com'),
    ('Elite Clothing Group', 'David Wilson', 'eliteclothing.com'),
    ('Modern Wear Industries', 'Jessica Martinez', 'modernwear.com'),
    ('Trendy Threads Corp.', 'Robert Taylor', 'trendythreads.com'),
    ('Classic Styles LLC', 'Amanda Anderson', 'classicstyles.com'),
    ('Urban Fashion Supply', 'Christopher Lee', 'urbanfashion.com'),
    ('Designer Outfits Inc.', 'Michelle White', 'designeroutfits.com'),
    ('Quality Garments Ltd.', 'Daniel Harris', 'qualitygarments.com'),
    ('Fashion Hub Co.', 'Lisa Thompson', 'fashionhub.com'),
    ('Style Masters', 'James Garcia', 'stylemasters.com'),
    ('Apparel Express', 'Patricia Rodriguez', 'apparelexpress.com'),
    ('Fashion World', 'Matthew Lewis', 'fashionworld.com'),
    ('Textile Traders', 'Jennifer Walker', 'textiletraders.com'),
    ('Style Elite', 'Andrew Hall', 'styleelite.com'),
    ('Fashion Plus', 'Nancy Allen', 'fashionplus.com'),
    ('Premium Threads', 'Kevin Young', 'premiumthreads.com'),
    ('Designer Wear Co.', 'Karen King', 'designerwear.com'),
    ('Style Craft', 'Brian Wright', 'stylecraft.com'),
    ('Fashion First', 'Betty Lopez', 'fashionfirst.com'),
    ('Apparel Masters', 'George Hill', 'apparelmasters.com'),
    ('Trend Setter', 'Dorothy Scott', 'trendsetter.com'),
    ('Fashion Central', 'Edward Green', 'fashioncentral.com');

DECLARE @EmailSuffix NVARCHAR(50);
DECLARE supplier_cursor CURSOR FOR SELECT company_name, contact_person, email_suffix FROM @SupplierNames;
OPEN supplier_cursor;
FETCH NEXT FROM supplier_cursor INTO @CompanyName, @ContactPerson, @EmailSuffix;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.tbl_suppliers WHERE user_id = @SellerUserId AND LOWER(company_name) = LOWER(@CompanyName) AND archived_at IS NULL)
    BEGIN
        SET @Email = LOWER(REPLACE(REPLACE(REPLACE(@CompanyName, ' ', ''), '.', ''), ',', '')) + '@' + @EmailSuffix;
        SET @ContactNumber = '+1-' + CAST(200 + (@SupplierCounter * 7) AS NVARCHAR(3)) + '-' + 
                            CAST(500 + (@SupplierCounter * 13) AS NVARCHAR(3)) + '-' + 
                            CAST(1000 + (@SupplierCounter * 23) AS NVARCHAR(4));
        
        INSERT INTO dbo.tbl_suppliers (user_id, company_name, contact_person, email, contact_number, status, created_at)
        VALUES (@SellerUserId, @CompanyName, @ContactPerson, @Email, @ContactNumber, 'Active', SYSUTCDATETIME());
        SET @SupplierId = SCOPE_IDENTITY();
        
        -- Create address for this supplier
        -- Generate address based on supplier counter
        SET @CityIndex = @SupplierCounter % 10;
        SET @ProvinceIndex = @SupplierCounter % 5;
        
        DECLARE @Cities TABLE (idx INT, name NVARCHAR(100));
        INSERT INTO @Cities VALUES 
            (0, 'Manila'), (1, 'Quezon City'), (2, 'Makati'), (3, 'Pasig'), (4, 'Taguig'),
            (5, 'Mandaluyong'), (6, 'San Juan'), (7, 'Pasay'), (8, 'Paranaque'), (9, 'Las Pinas');
        
        DECLARE @Provinces TABLE (idx INT, name NVARCHAR(100));
        INSERT INTO @Provinces VALUES 
            (0, 'Metro Manila'), (1, 'Cavite'), (2, 'Laguna'), (3, 'Rizal'), (4, 'Bulacan');
        
        SELECT @City = name FROM @Cities WHERE idx = @CityIndex;
        SELECT @Province = name FROM @Provinces WHERE idx = @ProvinceIndex;
        SET @Street = CAST(100 + (@SupplierCounter * 25) AS NVARCHAR(10)) + ' ' + @CompanyName + ' Street';
        SET @Zip = CAST(1000 + (@SupplierCounter * 50) AS NVARCHAR(10));
        
        -- Insert address linked to supplier
        INSERT INTO dbo.tbl_addresses (user_id, supplier_id, street, city, province, zip, created_at)
        VALUES (@SellerUserId, @SupplierId, @Street, @City, @Province, @Zip, SYSUTCDATETIME());
        
        SET @SupplierCounter = @SupplierCounter + 1;
        PRINT '  - Supplier "' + @CompanyName + '" created with address';
    END
    
    FETCH NEXT FROM supplier_cursor INTO @CompanyName, @ContactPerson, @EmailSuffix;
END
CLOSE supplier_cursor;
DEALLOCATE supplier_cursor;

-- Add more suppliers to reach 25 minimum
WHILE @SupplierCounter < 25
BEGIN
    SET @CompanyName = 'Supplier Company ' + CAST(@SupplierCounter + 1 AS NVARCHAR(10));
    IF NOT EXISTS (SELECT 1 FROM dbo.tbl_suppliers WHERE user_id = @SellerUserId AND LOWER(company_name) = LOWER(@CompanyName) AND archived_at IS NULL)
    BEGIN
        SET @ContactPerson = 'Contact Person ' + CAST(@SupplierCounter + 1 AS NVARCHAR(10));
        SET @Email = 'supplier' + CAST(@SupplierCounter + 1 AS NVARCHAR(10)) + '@example.com';
        SET @ContactNumber = '+1-' + CAST(200 + (@SupplierCounter * 7) AS NVARCHAR(3)) + '-' + 
                            CAST(500 + (@SupplierCounter * 13) AS NVARCHAR(3)) + '-' + 
                            CAST(1000 + (@SupplierCounter * 23) AS NVARCHAR(4));
        
        INSERT INTO dbo.tbl_suppliers (user_id, company_name, contact_person, email, contact_number, status, created_at)
        VALUES (@SellerUserId, @CompanyName, @ContactPerson, @Email, @ContactNumber, 'Active', SYSUTCDATETIME());
        SET @SupplierId = SCOPE_IDENTITY();
        
        -- Create address
        SET @CityIndex = @SupplierCounter % 10;
        SET @ProvinceIndex = @SupplierCounter % 5;
        
        DECLARE @Cities2 TABLE (idx INT, name NVARCHAR(100));
        INSERT INTO @Cities2 VALUES 
            (0, 'Manila'), (1, 'Quezon City'), (2, 'Makati'), (3, 'Pasig'), (4, 'Taguig'),
            (5, 'Mandaluyong'), (6, 'San Juan'), (7, 'Pasay'), (8, 'Paranaque'), (9, 'Las Pinas');
        
        DECLARE @Provinces2 TABLE (idx INT, name NVARCHAR(100));
        INSERT INTO @Provinces2 VALUES 
            (0, 'Metro Manila'), (1, 'Cavite'), (2, 'Laguna'), (3, 'Rizal'), (4, 'Bulacan');
        
        SELECT @City = name FROM @Cities2 WHERE idx = @CityIndex;
        SELECT @Province = name FROM @Provinces2 WHERE idx = @ProvinceIndex;
        SET @Street = CAST(100 + (@SupplierCounter * 25) AS NVARCHAR(10)) + ' Supplier Street';
        SET @Zip = CAST(1000 + (@SupplierCounter * 50) AS NVARCHAR(10));
        
        INSERT INTO dbo.tbl_addresses (user_id, supplier_id, street, city, province, zip, created_at)
        VALUES (@SellerUserId, @SupplierId, @Street, @City, @Province, @Zip, SYSUTCDATETIME());
        
        SET @SupplierCounter = @SupplierCounter + 1;
    END
    ELSE
    BEGIN
        SET @SupplierCounter = @SupplierCounter + 1;
    END
END

SELECT @SupplierCounter = COUNT(*) FROM dbo.tbl_suppliers WHERE user_id = @SellerUserId AND archived_at IS NULL;
DECLARE @AddressCount INT;
SELECT @AddressCount = COUNT(*) FROM dbo.tbl_addresses WHERE user_id = @SellerUserId AND supplier_id IS NOT NULL AND archived_at IS NULL;
PRINT '  - ' + CAST(@SupplierCounter AS NVARCHAR(10)) + ' suppliers created/verified';
PRINT '  - ' + CAST(@AddressCount AS NVARCHAR(10)) + ' supplier addresses created/verified';
PRINT '';
GO

PRINT '';
PRINT '========================================';
PRINT 'Seed Data Creation Complete!';
PRINT '========================================';
PRINT '';
PRINT 'Summary for Seller User ID 2:';
PRINT '  - Categories: 2';
PRINT '  - Products: 10 (5 per category)';
PRINT '  - Sizes: 7 (XS, S, M, L, XL, XXL, XXXL)';
PRINT '  - Colors: 35+';
PRINT '  - Variants: 35+';
PRINT '  - Suppliers: 25+';
PRINT '  - Supplier Addresses: 25+';
PRINT '';
GO
