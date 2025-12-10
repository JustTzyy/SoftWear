-- Migration: Delete Seed Data for Seller User ID 2
-- Description: Deletes all seed data created for seller user_id = 2
-- This includes: Categories, Products, Sizes, Colors, Variants, Variant-Size relationships,
--                Variant-Color relationships, Suppliers, and Supplier Addresses
-- Date: 2025-01-23
--
-- Run this script before re-running 048_Seed_Data_For_Seller_2.sql to clean up existing data

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Deleting Seed Data for Seller User ID 2';
PRINT '========================================';
PRINT '';

DECLARE @SellerUserId INT = 2;

-- Verify that user_id = 2 exists
DECLARE @SellerExists INT;
SELECT @SellerExists = COUNT(*) 
FROM dbo.tbl_users 
WHERE id = @SellerUserId AND archived_at IS NULL;

IF @SellerExists = 0
BEGIN
    PRINT 'WARNING: User ID 2 does not exist or is archived!';
    PRINT 'Proceeding with deletion anyway...';
    PRINT '';
END
ELSE
BEGIN
    PRINT 'Seller user ID 2 verified.';
    PRINT '';
END

-- ========================================
-- Step 1: Delete Supplier Addresses
-- ========================================
PRINT 'Step 1: Deleting Supplier Addresses...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @DeletedCount INT;

DELETE FROM dbo.tbl_addresses
WHERE user_id = @SellerUserId 
AND supplier_id IS NOT NULL 
AND archived_at IS NULL;

SET @DeletedCount = @@ROWCOUNT;
PRINT '  - ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' supplier addresses deleted';
PRINT '';
GO

-- ========================================
-- Step 2: Delete Suppliers
-- ========================================
PRINT 'Step 2: Deleting Suppliers...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @DeletedCount INT;

DELETE FROM dbo.tbl_suppliers
WHERE user_id = @SellerUserId 
AND archived_at IS NULL;

SET @DeletedCount = @@ROWCOUNT;
PRINT '  - ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' suppliers deleted';
PRINT '';
GO

-- ========================================
-- Step 3: Delete Variant-Color Relationships
-- ========================================
PRINT 'Step 3: Deleting Variant-Color Relationships...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @DeletedCount INT;

DELETE vc
FROM dbo.tbl_variant_colors vc
INNER JOIN dbo.tbl_variants v ON vc.variant_id = v.id
WHERE v.user_id = @SellerUserId 
AND v.archived_at IS NULL;

SET @DeletedCount = @@ROWCOUNT;
PRINT '  - ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' variant-color relationships deleted';
PRINT '';
GO

-- ========================================
-- Step 4: Delete Variant-Size Relationships
-- ========================================
PRINT 'Step 4: Deleting Variant-Size Relationships...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @DeletedCount INT;

DELETE vs
FROM dbo.tbl_variant_sizes vs
INNER JOIN dbo.tbl_variants v ON vs.variant_id = v.id
WHERE v.user_id = @SellerUserId 
AND v.archived_at IS NULL;

SET @DeletedCount = @@ROWCOUNT;
PRINT '  - ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' variant-size relationships deleted';
PRINT '';
GO

-- ========================================
-- Step 5: Delete Variants
-- ========================================
PRINT 'Step 5: Deleting Variants...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @DeletedCount INT;

DELETE FROM dbo.tbl_variants
WHERE user_id = @SellerUserId 
AND archived_at IS NULL;

SET @DeletedCount = @@ROWCOUNT;
PRINT '  - ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' variants deleted';
PRINT '';
GO

-- ========================================
-- Step 6: Delete Products
-- ========================================
PRINT 'Step 6: Deleting Products...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @DeletedCount INT;

DELETE FROM dbo.tbl_products
WHERE user_id = @SellerUserId 
AND archived_at IS NULL;

SET @DeletedCount = @@ROWCOUNT;
PRINT '  - ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' products deleted';
PRINT '';
GO

-- ========================================
-- Step 7: Delete Colors
-- ========================================
PRINT 'Step 7: Deleting Colors...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @DeletedCount INT;

DELETE FROM dbo.tbl_colors
WHERE user_id = @SellerUserId 
AND archived_at IS NULL;

SET @DeletedCount = @@ROWCOUNT;
PRINT '  - ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' colors deleted';
PRINT '';
GO

-- ========================================
-- Step 8: Delete Sizes
-- ========================================
PRINT 'Step 8: Deleting Sizes...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @DeletedCount INT;

DELETE FROM dbo.tbl_sizes
WHERE user_id = @SellerUserId 
AND archived_at IS NULL;

SET @DeletedCount = @@ROWCOUNT;
PRINT '  - ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' sizes deleted';
PRINT '';
GO

-- ========================================
-- Step 9: Delete Categories
-- ========================================
PRINT 'Step 9: Deleting Categories...';
GO

DECLARE @SellerUserId INT = 2;
DECLARE @DeletedCount INT;

DELETE FROM dbo.tbl_categories
WHERE user_id = @SellerUserId 
AND archived_at IS NULL;

SET @DeletedCount = @@ROWCOUNT;
PRINT '  - ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' categories deleted';
PRINT '';
GO

PRINT '';
PRINT '========================================';
PRINT 'Deletion Complete!';
PRINT '========================================';
PRINT '';
PRINT 'All seed data for Seller User ID 2 has been deleted.';
PRINT 'You can now run 048_Seed_Data_For_Seller_2.sql again.';
PRINT '';
GO













