-- Migration: Seed Stock Clerk Transactions
-- Description: Creates stock in, stock out, adjustments, and purchase orders for stock clerks
-- Date: 2025-12-08
--
-- This script creates:
--   - 15 stock in transactions (Justin Digal gets 15, others get distributed)
--   - 15 stock out transactions
--   - 15 adjustment transactions
--   - 15 approved PO
--   - 15 completed PO
--   - 15 pending PO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Seeding Stock Clerk Transactions';
PRINT '========================================';
PRINT '';

-- ========================================
-- Step 1: Get Stock Clerks from Screenshot
-- ========================================
PRINT 'Step 1: Getting stock clerks...';

DECLARE @StockClerkIds TABLE (id INT, name NVARCHAR(200), email NVARCHAR(256));
INSERT INTO @StockClerkIds VALUES
    (4, 'Justin Digal', 'JustinDigal@SoftWear.com'),
    (1739, 'Ramon Gonzales Gonzales', 'RamonGonzalesStockClerk11@SoftWear.com'),
    (1738, 'Carlos Ramos Ramos', 'CarlosRamosStockClerk10@SoftWear.com'),
    (1736, 'Roberto Lopez Mendoza', 'RobertoMendozaStockClerk8@SoftWear.com'),
    (1734, 'Fernando Mendoza Fernandez', 'FernandoFernandezStockClerk6@SoftWear.com'),
    (1742, 'Anthony Dela Cruz Dela Cruz', 'AnthonyDelaCruzStockClerk14@SoftWear.com'),
    (1741, 'Vincent Rodriguez Rodriguez', 'VincentRodriguezStockClerk13@SoftWear.com');

-- Verify stock clerks exist
DECLARE @ValidStockClerkIds TABLE (id INT, name NVARCHAR(200));
INSERT INTO @ValidStockClerkIds
SELECT sc.id, sc.name
FROM @StockClerkIds sc
INNER JOIN dbo.tbl_users u ON u.id = sc.id
WHERE u.archived_at IS NULL;

DECLARE @StockClerkCount INT;
SELECT @StockClerkCount = COUNT(*) FROM @ValidStockClerkIds;
PRINT '  - Found ' + CAST(@StockClerkCount AS NVARCHAR(10)) + ' valid stock clerks';
PRINT '';

-- ========================================
-- Step 2: Get Variants and Suppliers for Seller ID 2
-- ========================================
PRINT 'Step 2: Getting variants and suppliers for seller ID 2...';

DECLARE @SellerId INT = 2;
DECLARE @VariantIds TABLE (id INT);
DECLARE @SupplierIds TABLE (id INT);
DECLARE @SizeIds TABLE (id INT);
DECLARE @ColorIds TABLE (id INT);

-- Get variants for seller ID 2
INSERT INTO @VariantIds
SELECT TOP 50 v.id
FROM dbo.tbl_variants v
INNER JOIN dbo.tbl_products p ON p.id = v.product_id
WHERE p.user_id = @SellerId AND v.archived_at IS NULL AND p.archived_at IS NULL
ORDER BY v.id;

-- Get suppliers for seller ID 2
INSERT INTO @SupplierIds
SELECT TOP 25 id
FROM dbo.tbl_suppliers
WHERE user_id = @SellerId AND archived_at IS NULL
ORDER BY id;

-- Get sizes for seller ID 2
INSERT INTO @SizeIds
SELECT id
FROM dbo.tbl_sizes
WHERE user_id = @SellerId AND archived_at IS NULL;

-- Get colors for seller ID 2
INSERT INTO @ColorIds
SELECT TOP 20 id
FROM dbo.tbl_colors
WHERE user_id = @SellerId AND archived_at IS NULL;

DECLARE @VariantCount INT, @SupplierCount INT, @SizeCount INT, @ColorCount INT;
SELECT @VariantCount = COUNT(*) FROM @VariantIds;
SELECT @SupplierCount = COUNT(*) FROM @SupplierIds;
SELECT @SizeCount = COUNT(*) FROM @SizeIds;
SELECT @ColorCount = COUNT(*) FROM @ColorIds;

PRINT '  - Variants: ' + CAST(@VariantCount AS NVARCHAR(10));
PRINT '  - Suppliers: ' + CAST(@SupplierCount AS NVARCHAR(10));
PRINT '  - Sizes: ' + CAST(@SizeCount AS NVARCHAR(10));
PRINT '  - Colors: ' + CAST(@ColorCount AS NVARCHAR(10));
PRINT '';

IF @VariantCount = 0 OR @SupplierCount = 0
BEGIN
    PRINT 'ERROR: No variants or suppliers found for seller ID 2!';
    PRINT 'Please ensure seller ID 2 has variants and suppliers before running this script.';
    RETURN;
END

-- ========================================
-- Step 3: Create Stock In Transactions (15 total, Justin Digal gets 15)
-- ========================================
PRINT 'Step 3: Creating stock in transactions...';

DECLARE @StockInCount INT = 0;
DECLARE @StockClerkId INT;
DECLARE @StockClerkName NVARCHAR(200);
DECLARE @VariantId INT;
DECLARE @SupplierId INT;
DECLARE @SizeId INT;
DECLARE @ColorId INT;
DECLARE @Quantity INT;
DECLARE @CostPrice DECIMAL(18,2);
DECLARE @TransactionDate DATETIME2(0);
DECLARE @DaysAgo INT;

-- Justin Digal (ID 4) gets 15 stock in transactions
SET @StockClerkId = 4;
SET @StockClerkName = 'Justin Digal';

DECLARE stock_in_cursor CURSOR FOR SELECT TOP 15 id FROM @VariantIds ORDER BY NEWID();
OPEN stock_in_cursor;
FETCH NEXT FROM stock_in_cursor INTO @VariantId;

WHILE @@FETCH_STATUS = 0 AND @StockInCount < 15
BEGIN
    -- Get random supplier, size, color
    SELECT TOP 1 @SupplierId = id FROM @SupplierIds ORDER BY NEWID();
    SELECT TOP 1 @SizeId = id FROM @SizeIds ORDER BY NEWID();
    SELECT TOP 1 @ColorId = id FROM @ColorIds ORDER BY NEWID();
    
    SET @Quantity = 10 + (ABS(CHECKSUM(NEWID())) % 90); -- 10-100
    SET @CostPrice = 100.00 + (ABS(CHECKSUM(NEWID())) % 900); -- 100-1000
    SET @DaysAgo = @StockInCount;
    SET @TransactionDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    
    INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
    VALUES (@StockClerkId, @VariantId, @SizeId, @ColorId, @Quantity, @CostPrice, @SupplierId, @TransactionDate);
    
    SET @StockInCount = @StockInCount + 1;
    FETCH NEXT FROM stock_in_cursor INTO @VariantId;
END

CLOSE stock_in_cursor;
DEALLOCATE stock_in_cursor;

PRINT '  - Created ' + CAST(@StockInCount AS NVARCHAR(10)) + ' stock in transactions for ' + @StockClerkName;
PRINT '';

-- ========================================
-- Step 4: Create Stock Out Transactions (15 total, distributed)
-- ========================================
PRINT 'Step 4: Creating stock out transactions...';

DECLARE @StockOutCount INT = 0;
DECLARE @Reason NVARCHAR(500);

DECLARE stock_out_cursor CURSOR FOR 
SELECT TOP 15 id FROM @VariantIds ORDER BY NEWID();
OPEN stock_out_cursor;
FETCH NEXT FROM stock_out_cursor INTO @VariantId;

WHILE @@FETCH_STATUS = 0 AND @StockOutCount < 15
BEGIN
    -- Rotate through stock clerks
    SELECT TOP 1 @StockClerkId = id FROM @ValidStockClerkIds ORDER BY (id + @StockOutCount) % @StockClerkCount;
    
    SELECT TOP 1 @SizeId = id FROM @SizeIds ORDER BY NEWID();
    SELECT TOP 1 @ColorId = id FROM @ColorIds ORDER BY NEWID();
    
    SET @Quantity = 1 + (ABS(CHECKSUM(NEWID())) % 20); -- 1-20
    SET @DaysAgo = @StockOutCount;
    SET @TransactionDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    SET @Reason = 'Stock out transaction #' + CAST(@StockOutCount + 1 AS NVARCHAR(10));
    
    INSERT INTO dbo.tbl_stock_out (user_id, variant_id, size_id, color_id, quantity_removed, reason, timestamps)
    VALUES (@StockClerkId, @VariantId, @SizeId, @ColorId, @Quantity, @Reason, @TransactionDate);
    
    SET @StockOutCount = @StockOutCount + 1;
    FETCH NEXT FROM stock_out_cursor INTO @VariantId;
END

CLOSE stock_out_cursor;
DEALLOCATE stock_out_cursor;

PRINT '  - Created ' + CAST(@StockOutCount AS NVARCHAR(10)) + ' stock out transactions';
PRINT '';

-- ========================================
-- Step 5: Create Adjustment Transactions (15 total, distributed)
-- ========================================
PRINT 'Step 5: Creating adjustment transactions...';

DECLARE @AdjustmentCount INT = 0;
DECLARE @AdjustmentType NVARCHAR(20);
DECLARE @ReasonText NVARCHAR(500);

DECLARE adjustment_cursor CURSOR FOR 
SELECT TOP 15 id FROM @VariantIds ORDER BY NEWID();
OPEN adjustment_cursor;
FETCH NEXT FROM adjustment_cursor INTO @VariantId;

WHILE @@FETCH_STATUS = 0 AND @AdjustmentCount < 15
BEGIN
    -- Rotate through stock clerks
    SELECT TOP 1 @StockClerkId = id FROM @ValidStockClerkIds ORDER BY (id + @AdjustmentCount) % @StockClerkCount;
    
    SELECT TOP 1 @SizeId = id FROM @SizeIds ORDER BY NEWID();
    SELECT TOP 1 @ColorId = id FROM @ColorIds ORDER BY NEWID();
    
    SET @Quantity = 1 + (ABS(CHECKSUM(NEWID())) % 10); -- 1-10
    SET @AdjustmentType = CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN 'Increase' ELSE 'Decrease' END;
    SET @DaysAgo = @AdjustmentCount;
    SET @TransactionDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    SET @ReasonText = CASE 
        WHEN @AdjustmentType = 'Increase' THEN 'Inventory count correction - increase'
        ELSE 'Inventory count correction - decrease'
    END;
    
    INSERT INTO dbo.tbl_stock_adjustments (user_id, variant_id, size_id, color_id, adjustment_type, quantity_adjusted, reason, timestamps)
    VALUES (@StockClerkId, @VariantId, @SizeId, @ColorId, @AdjustmentType, @Quantity, @ReasonText, @TransactionDate);
    
    SET @AdjustmentCount = @AdjustmentCount + 1;
    FETCH NEXT FROM adjustment_cursor INTO @VariantId;
END

CLOSE adjustment_cursor;
DEALLOCATE adjustment_cursor;

PRINT '  - Created ' + CAST(@AdjustmentCount AS NVARCHAR(10)) + ' adjustment transactions';
PRINT '';

-- ========================================
-- Step 6: Create Purchase Orders (45 total: 15 Pending, 15 Approved, 15 Completed)
-- ========================================
PRINT 'Step 6: Creating purchase orders...';

DECLARE @POCount INT = 0;
DECLARE @POId INT;
DECLARE @PONumber NVARCHAR(50);
DECLARE @POStatus NVARCHAR(50);
DECLARE @TotalAmount DECIMAL(18,2);
DECLARE @ExpectedDeliveryDate DATETIME2(0);
DECLARE @UnitPrice DECIMAL(18,2);
DECLARE @ItemQuantity INT;
DECLARE @ItemTotalPrice DECIMAL(18,2);
DECLARE @POItemCount INT;
DECLARE @BasePONumber INT = 1000;

-- Create 15 Pending POs
SET @POStatus = 'Pending';
DECLARE pending_po_cursor CURSOR FOR SELECT TOP 15 id FROM @SupplierIds ORDER BY NEWID();
OPEN pending_po_cursor;
FETCH NEXT FROM pending_po_cursor INTO @SupplierId;

WHILE @@FETCH_STATUS = 0 AND @POCount < 15
BEGIN
    -- Rotate through stock clerks
    SELECT TOP 1 @StockClerkId = id FROM @ValidStockClerkIds ORDER BY (id + @POCount) % @StockClerkCount;
    
    SET @PONumber = 'PO-' + CAST(@BasePONumber + @POCount AS NVARCHAR(10));
    SET @TotalAmount = 0;
    SET @DaysAgo = @POCount;
    SET @TransactionDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    SET @ExpectedDeliveryDate = DATEADD(DAY, 7, @TransactionDate);
    
    -- Create PO
    INSERT INTO dbo.tbl_purchase_orders (po_number, supplier_id, status, total_amount, notes, expected_delivery_date, created_by, created_at)
    VALUES (@PONumber, @SupplierId, @POStatus, @TotalAmount, 'Pending purchase order #' + CAST(@POCount + 1 AS NVARCHAR(10)), @ExpectedDeliveryDate, @StockClerkId, @TransactionDate);
    
    SET @POId = SCOPE_IDENTITY();
    
    -- Add 1-3 items to PO
    SET @POItemCount = 1 + (ABS(CHECKSUM(NEWID())) % 3);
    DECLARE @ItemCounter INT = 0;
    
    WHILE @ItemCounter < @POItemCount
    BEGIN
        SELECT TOP 1 @VariantId = id FROM @VariantIds ORDER BY NEWID();
        SELECT TOP 1 @SizeId = id FROM @SizeIds ORDER BY NEWID();
        SELECT TOP 1 @ColorId = id FROM @ColorIds ORDER BY NEWID();
        
        SET @ItemQuantity = 5 + (ABS(CHECKSUM(NEWID())) % 45); -- 5-50
        SET @UnitPrice = 100.00 + (ABS(CHECKSUM(NEWID())) % 400); -- 100-500
        SET @ItemTotalPrice = @ItemQuantity * @UnitPrice;
        SET @TotalAmount = @TotalAmount + @ItemTotalPrice;
        
        INSERT INTO dbo.tbl_po_items (po_id, variant_id, size_id, color_id, quantity, unit_price, total_price, created_at)
        VALUES (@POId, @VariantId, @SizeId, @ColorId, @ItemQuantity, @UnitPrice, @ItemTotalPrice, @TransactionDate);
        
        SET @ItemCounter = @ItemCounter + 1;
    END
    
    -- Update PO total amount
    UPDATE dbo.tbl_purchase_orders SET total_amount = @TotalAmount WHERE id = @POId;
    
    SET @POCount = @POCount + 1;
    FETCH NEXT FROM pending_po_cursor INTO @SupplierId;
END

CLOSE pending_po_cursor;
DEALLOCATE pending_po_cursor;

PRINT '  - Created 15 pending purchase orders';

-- Create 15 Approved POs
SET @POStatus = 'Approved';
DECLARE approved_po_cursor CURSOR FOR SELECT TOP 15 id FROM @SupplierIds ORDER BY NEWID();
OPEN approved_po_cursor;
FETCH NEXT FROM approved_po_cursor INTO @SupplierId;

WHILE @@FETCH_STATUS = 0 AND @POCount < 30
BEGIN
    -- Rotate through stock clerks
    SELECT TOP 1 @StockClerkId = id FROM @ValidStockClerkIds ORDER BY (id + @POCount) % @StockClerkCount;
    
    SET @PONumber = 'PO-' + CAST(@BasePONumber + @POCount AS NVARCHAR(10));
    SET @TotalAmount = 0;
    SET @DaysAgo = @POCount;
    SET @TransactionDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    SET @ExpectedDeliveryDate = DATEADD(DAY, 7, @TransactionDate);
    
    -- Create PO
    INSERT INTO dbo.tbl_purchase_orders (po_number, supplier_id, status, total_amount, notes, expected_delivery_date, created_by, created_at, updated_at)
    VALUES (@PONumber, @SupplierId, @POStatus, @TotalAmount, 'Approved purchase order #' + CAST(@POCount - 14 AS NVARCHAR(10)), @ExpectedDeliveryDate, @StockClerkId, @TransactionDate, DATEADD(DAY, 1, @TransactionDate));
    
    SET @POId = SCOPE_IDENTITY();
    
    -- Add 1-3 items to PO
    SET @POItemCount = 1 + (ABS(CHECKSUM(NEWID())) % 3);
    SET @ItemCounter = 0;
    
    WHILE @ItemCounter < @POItemCount
    BEGIN
        SELECT TOP 1 @VariantId = id FROM @VariantIds ORDER BY NEWID();
        SELECT TOP 1 @SizeId = id FROM @SizeIds ORDER BY NEWID();
        SELECT TOP 1 @ColorId = id FROM @ColorIds ORDER BY NEWID();
        
        SET @ItemQuantity = 5 + (ABS(CHECKSUM(NEWID())) % 45);
        SET @UnitPrice = 100.00 + (ABS(CHECKSUM(NEWID())) % 400);
        SET @ItemTotalPrice = @ItemQuantity * @UnitPrice;
        SET @TotalAmount = @TotalAmount + @ItemTotalPrice;
        
        INSERT INTO dbo.tbl_po_items (po_id, variant_id, size_id, color_id, quantity, unit_price, total_price, created_at)
        VALUES (@POId, @VariantId, @SizeId, @ColorId, @ItemQuantity, @UnitPrice, @ItemTotalPrice, @TransactionDate);
        
        SET @ItemCounter = @ItemCounter + 1;
    END
    
    -- Update PO total amount
    UPDATE dbo.tbl_purchase_orders SET total_amount = @TotalAmount WHERE id = @POId;
    
    SET @POCount = @POCount + 1;
    FETCH NEXT FROM approved_po_cursor INTO @SupplierId;
END

CLOSE approved_po_cursor;
DEALLOCATE approved_po_cursor;

PRINT '  - Created 15 approved purchase orders';

-- Create 15 Completed POs
SET @POStatus = 'Completed';
DECLARE completed_po_cursor CURSOR FOR SELECT TOP 15 id FROM @SupplierIds ORDER BY NEWID();
OPEN completed_po_cursor;
FETCH NEXT FROM completed_po_cursor INTO @SupplierId;

WHILE @@FETCH_STATUS = 0 AND @POCount < 45
BEGIN
    -- Rotate through stock clerks
    SELECT TOP 1 @StockClerkId = id FROM @ValidStockClerkIds ORDER BY (id + @POCount) % @StockClerkCount;
    
    SET @PONumber = 'PO-' + CAST(@BasePONumber + @POCount AS NVARCHAR(10));
    SET @TotalAmount = 0;
    SET @DaysAgo = @POCount;
    SET @TransactionDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    SET @ExpectedDeliveryDate = DATEADD(DAY, 5, @TransactionDate);
    
    -- Create PO
    INSERT INTO dbo.tbl_purchase_orders (po_number, supplier_id, status, total_amount, notes, expected_delivery_date, created_by, created_at, updated_at)
    VALUES (@PONumber, @SupplierId, @POStatus, @TotalAmount, 'Completed purchase order #' + CAST(@POCount - 29 AS NVARCHAR(10)), @ExpectedDeliveryDate, @StockClerkId, @TransactionDate, DATEADD(DAY, 3, @TransactionDate));
    
    SET @POId = SCOPE_IDENTITY();
    
    -- Add 1-3 items to PO
    SET @POItemCount = 1 + (ABS(CHECKSUM(NEWID())) % 3);
    SET @ItemCounter = 0;
    
    WHILE @ItemCounter < @POItemCount
    BEGIN
        SELECT TOP 1 @VariantId = id FROM @VariantIds ORDER BY NEWID();
        SELECT TOP 1 @SizeId = id FROM @SizeIds ORDER BY NEWID();
        SELECT TOP 1 @ColorId = id FROM @ColorIds ORDER BY NEWID();
        
        SET @ItemQuantity = 5 + (ABS(CHECKSUM(NEWID())) % 45);
        SET @UnitPrice = 100.00 + (ABS(CHECKSUM(NEWID())) % 400);
        SET @ItemTotalPrice = @ItemQuantity * @UnitPrice;
        SET @TotalAmount = @TotalAmount + @ItemTotalPrice;
        
        INSERT INTO dbo.tbl_po_items (po_id, variant_id, size_id, color_id, quantity, unit_price, total_price, created_at)
        VALUES (@POId, @VariantId, @SizeId, @ColorId, @ItemQuantity, @UnitPrice, @ItemTotalPrice, @TransactionDate);
        
        SET @ItemCounter = @ItemCounter + 1;
    END
    
    -- Update PO total amount
    UPDATE dbo.tbl_purchase_orders SET total_amount = @TotalAmount WHERE id = @POId;
    
    SET @POCount = @POCount + 1;
    FETCH NEXT FROM completed_po_cursor INTO @SupplierId;
END

CLOSE completed_po_cursor;
DEALLOCATE completed_po_cursor;

PRINT '  - Created 15 completed purchase orders';
PRINT '';

PRINT '========================================';
PRINT 'Summary:';
PRINT '  - Stock In: 15 transactions (all for Justin Digal)';
PRINT '  - Stock Out: 15 transactions (distributed)';
PRINT '  - Adjustments: 15 transactions (distributed)';
PRINT '  - Purchase Orders: 45 total';
PRINT '    * Pending: 15';
PRINT '    * Approved: 15';
PRINT '    * Completed: 15';
PRINT '========================================';
PRINT '';
PRINT 'Migration completed successfully!';
GO








