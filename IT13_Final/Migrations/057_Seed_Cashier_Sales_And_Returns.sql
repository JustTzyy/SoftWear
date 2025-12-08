-- Migration: Seed Cashier Sales and Returns
-- Description: Creates sales and returns for cashiers with proper stock management
-- Date: 2025-12-08
--
-- This script creates:
--   - 100 sales transactions (distributed, Michael Kevin Hernandez gets at least 20)
--   - 20 pending returns
--   - 20 approved returns (adds stock back)
--   - 15 canceled returns
-- All sales only use items with available stock and properly deduct inventory

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Seeding Cashier Sales and Returns';
PRINT '========================================';
PRINT '';

-- ========================================
-- Step 1: Get Cashiers from Screenshot
-- ========================================
PRINT 'Step 1: Getting cashiers...';

DECLARE @CashierIds TABLE (id INT, name NVARCHAR(200), email NVARCHAR(256));
INSERT INTO @CashierIds VALUES
    (5, 'Michael Kevin Hernandez', 'MichaelKevinHernandez@SoftWear.com'),
    (1724, 'Vincent Gonzales Gonzales', 'VincentGonzalesCashier11@SoftWear.com'),
    (1723, 'Ramon Ramos Ramos', 'RamonRamosCashier10@SoftWear.com'),
    (1722, 'Andres Villanueva Villanueva', 'AndresVillanuevaCashier9@SoftWear.com'),
    (1721, 'Pedro Lopez Mendoza', 'PedroMendozaCashier8@SoftWear.com'),
    (1719, 'Ricardo Mendoza Fernandez', 'RicardoFernandezCashier6@SoftWear.com'),
    (1728, 'Brian Bautista Bautista', 'BrianBautistaCashier15@SoftWear.com'),
    (1727, 'Edward Dela Cruz Dela Cruz', 'EdwardDelaCruzCashier14@SoftWear.com'),
    (1726, 'Ronald Rodriguez Rodriguez', 'RonaldRodriguezCashier13@SoftWear.com'),
    (1725, 'Anthony Martinez Martinez', 'AnthonyMartinezCashier12@SoftWear.com');

-- Verify cashiers exist
DECLARE @ValidCashierIds TABLE (id INT, name NVARCHAR(200));
INSERT INTO @ValidCashierIds
SELECT c.id, c.name
FROM @CashierIds c
INNER JOIN dbo.tbl_users u ON u.id = c.id
WHERE u.archived_at IS NULL;

DECLARE @CashierCount INT;
SELECT @CashierCount = COUNT(*) FROM @ValidCashierIds;
PRINT '  - Found ' + CAST(@CashierCount AS NVARCHAR(10)) + ' valid cashiers';
PRINT '';

-- ========================================
-- Step 2: Get Variants with Stock for Seller ID 2
-- ========================================
PRINT 'Step 2: Getting variants with available stock for seller ID 2...';

DECLARE @SellerId INT = 2;
DECLARE @VariantsWithStock TABLE (
    variant_id INT, 
    size_id INT, 
    color_id INT, 
    current_stock INT,
    variant_user_id INT,
    price DECIMAL(18,2),
    cost_price DECIMAL(18,2)
);

-- Calculate current stock from stock_in, stock_out, and adjustments
INSERT INTO @VariantsWithStock
SELECT 
    v.id as variant_id,
    COALESCE(si.size_id, so.size_id, sa.size_id) as size_id,
    COALESCE(si.color_id, so.color_id, sa.color_id) as color_id,
    COALESCE(si.total_in, 0) - COALESCE(so.total_out, 0) + COALESCE(sa.total_adjustment, 0) as current_stock,
    v.user_id as variant_user_id,
    v.price,
    v.cost_price
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
WHERE p.user_id = @SellerId 
    AND v.archived_at IS NULL 
    AND p.archived_at IS NULL
    AND (COALESCE(si.total_in, 0) - COALESCE(so.total_out, 0) + COALESCE(sa.total_adjustment, 0)) > 0;

DECLARE @VariantCount INT;
SELECT @VariantCount = COUNT(*) FROM @VariantsWithStock;
PRINT '  - Found ' + CAST(@VariantCount AS NVARCHAR(10)) + ' variants with available stock';
PRINT '';

IF @VariantCount = 0
BEGIN
    PRINT 'ERROR: No variants with available stock found for seller ID 2!';
    PRINT 'Please ensure there are stock_in transactions before running this script.';
    RETURN;
END

-- ========================================
-- Step 3: Create Sales Transactions (100 total)
-- ========================================
PRINT 'Step 3: Creating sales transactions...';

DECLARE @SaleCount INT = 0;
DECLARE @CashierId INT;
DECLARE @CashierName NVARCHAR(200);
DECLARE @SaleId INT;
DECLARE @SaleNumber NVARCHAR(50);
DECLARE @SaleAmount DECIMAL(18,2);
DECLARE @PaymentMethod NVARCHAR(50);
DECLARE @AmountPaid DECIMAL(18,2);
DECLARE @ChangeGiven DECIMAL(18,2);
DECLARE @SaleDate DATETIME2(0);
DECLARE @DaysAgo INT;
DECLARE @BaseSaleNumber INT = 10000;

-- Variables for sale items
DECLARE @VariantId INT;
DECLARE @SizeId INT;
DECLARE @ColorId INT;
DECLARE @ItemQuantity INT;
DECLARE @ItemPrice DECIMAL(18,2);
DECLARE @ItemSubtotal DECIMAL(18,2);
DECLARE @SaleItemId INT;
DECLARE @ItemCount INT;
DECLARE @CurrentStock INT;
DECLARE @VariantUserId INT;

-- Michael Kevin Hernandez (ID 5) gets at least 20 sales
DECLARE @MichaelSalesCount INT = 0;
SET @CashierId = 5;
SET @CashierName = 'Michael Kevin Hernandez';

WHILE @MichaelSalesCount < 20 AND @SaleCount < 100
BEGIN
    SET @SaleNumber = 'SALE-' + FORMAT(GETDATE() - @SaleCount, 'yyyyMMdd') + '-' + RIGHT('0000' + CAST(@BaseSaleNumber + @SaleCount AS NVARCHAR(10)), 4);
    SET @SaleAmount = 0;
    SET @ItemCount = 1 + (ABS(CHECKSUM(NEWID())) % 3); -- 1-3 items per sale
    SET @DaysAgo = @SaleCount;
    SET @SaleDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    SET @PaymentMethod = CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN 'Cash' ELSE 'GCash' END;
    
    -- Create sale
    INSERT INTO dbo.tbl_sales (sale_number, amount, payment_type, status, user_id, timestamps)
    VALUES (@SaleNumber, @SaleAmount, @PaymentMethod, 'Completed', @CashierId, @SaleDate);
    
    SET @SaleId = SCOPE_IDENTITY();
    
    -- Add items to sale
    DECLARE @ItemCounter INT = 0;
    WHILE @ItemCounter < @ItemCount
    BEGIN
        -- Get random variant with stock
        SELECT TOP 1 
            @VariantId = variant_id,
            @SizeId = size_id,
            @ColorId = color_id,
            @CurrentStock = current_stock,
            @VariantUserId = variant_user_id,
            @ItemPrice = price
        FROM @VariantsWithStock
        WHERE current_stock > 0
        ORDER BY NEWID();
        
        IF @VariantId IS NULL
            BREAK;
        
        -- Quantity should not exceed available stock
        SET @ItemQuantity = 1 + (ABS(CHECKSUM(NEWID())) % CASE WHEN @CurrentStock > 5 THEN 5 ELSE @CurrentStock END);
        SET @ItemSubtotal = @ItemQuantity * @ItemPrice;
        SET @SaleAmount = @SaleAmount + @ItemSubtotal;
        
        -- Insert sale item
        INSERT INTO dbo.tbl_sales_items (sale_id, variant_id, size_id, color_id, quantity, price, subtotal, timestamps)
        VALUES (@SaleId, @VariantId, @SizeId, @ColorId, @ItemQuantity, @ItemPrice, @ItemSubtotal, @SaleDate);
        
        SET @SaleItemId = SCOPE_IDENTITY();
        
        -- Create stock_out to deduct inventory
        INSERT INTO dbo.tbl_stock_out (user_id, variant_id, size_id, color_id, quantity_removed, reason, timestamps)
        VALUES (@VariantUserId, @VariantId, @SizeId, @ColorId, @ItemQuantity, 'Sale: ' + @SaleNumber, @SaleDate);
        
        -- Update stock in our temp table
        UPDATE @VariantsWithStock
        SET current_stock = current_stock - @ItemQuantity
        WHERE variant_id = @VariantId
            AND (size_id = @SizeId OR (size_id IS NULL AND @SizeId IS NULL))
            AND (color_id = @ColorId OR (color_id IS NULL AND @ColorId IS NULL));
        
        SET @ItemCounter = @ItemCounter + 1;
    END
    
    -- Update sale amount
    UPDATE dbo.tbl_sales SET amount = @SaleAmount WHERE id = @SaleId;
    
    -- Create payment record
    SET @AmountPaid = @SaleAmount + (ABS(CHECKSUM(NEWID())) % 100); -- Add some change
    SET @ChangeGiven = @AmountPaid - @SaleAmount;
    
    INSERT INTO dbo.tbl_payments (sale_id, amount_paid, payment_method, change_given, timestamps)
    VALUES (@SaleId, @AmountPaid, @PaymentMethod, @ChangeGiven, @SaleDate);
    
    SET @SaleCount = @SaleCount + 1;
    SET @MichaelSalesCount = @MichaelSalesCount + 1;
END

PRINT '  - Created ' + CAST(@MichaelSalesCount AS NVARCHAR(10)) + ' sales for ' + @CashierName;

-- Create remaining sales for other cashiers
WHILE @SaleCount < 100
BEGIN
    -- Rotate through cashiers
    SELECT TOP 1 @CashierId = id, @CashierName = name 
    FROM @ValidCashierIds 
    ORDER BY (id + @SaleCount) % @CashierCount;
    
    SET @SaleNumber = 'SALE-' + FORMAT(GETDATE() - @SaleCount, 'yyyyMMdd') + '-' + RIGHT('0000' + CAST(@BaseSaleNumber + @SaleCount AS NVARCHAR(10)), 4);
    SET @SaleAmount = 0;
    SET @ItemCount = 1 + (ABS(CHECKSUM(NEWID())) % 3);
    SET @DaysAgo = @SaleCount;
    SET @SaleDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    SET @PaymentMethod = CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN 'Cash' ELSE 'GCash' END;
    
    -- Create sale
    INSERT INTO dbo.tbl_sales (sale_number, amount, payment_type, status, user_id, timestamps)
    VALUES (@SaleNumber, @SaleAmount, @PaymentMethod, 'Completed', @CashierId, @SaleDate);
    
    SET @SaleId = SCOPE_IDENTITY();
    
    -- Add items to sale
    SET @ItemCounter = 0;
    WHILE @ItemCounter < @ItemCount
    BEGIN
        SELECT TOP 1 
            @VariantId = variant_id,
            @SizeId = size_id,
            @ColorId = color_id,
            @CurrentStock = current_stock,
            @VariantUserId = variant_user_id,
            @ItemPrice = price
        FROM @VariantsWithStock
        WHERE current_stock > 0
        ORDER BY NEWID();
        
        IF @VariantId IS NULL
            BREAK;
        
        SET @ItemQuantity = 1 + (ABS(CHECKSUM(NEWID())) % CASE WHEN @CurrentStock > 5 THEN 5 ELSE @CurrentStock END);
        SET @ItemSubtotal = @ItemQuantity * @ItemPrice;
        SET @SaleAmount = @SaleAmount + @ItemSubtotal;
        
        INSERT INTO dbo.tbl_sales_items (sale_id, variant_id, size_id, color_id, quantity, price, subtotal, timestamps)
        VALUES (@SaleId, @VariantId, @SizeId, @ColorId, @ItemQuantity, @ItemPrice, @ItemSubtotal, @SaleDate);
        
        SET @SaleItemId = SCOPE_IDENTITY();
        
        INSERT INTO dbo.tbl_stock_out (user_id, variant_id, size_id, color_id, quantity_removed, reason, timestamps)
        VALUES (@VariantUserId, @VariantId, @SizeId, @ColorId, @ItemQuantity, 'Sale: ' + @SaleNumber, @SaleDate);
        
        UPDATE @VariantsWithStock
        SET current_stock = current_stock - @ItemQuantity
        WHERE variant_id = @VariantId
            AND (size_id = @SizeId OR (size_id IS NULL AND @SizeId IS NULL))
            AND (color_id = @ColorId OR (color_id IS NULL AND @ColorId IS NULL));
        
        SET @ItemCounter = @ItemCounter + 1;
    END
    
    UPDATE dbo.tbl_sales SET amount = @SaleAmount WHERE id = @SaleId;
    
    SET @AmountPaid = @SaleAmount + (ABS(CHECKSUM(NEWID())) % 100);
    SET @ChangeGiven = @AmountPaid - @SaleAmount;
    
    INSERT INTO dbo.tbl_payments (sale_id, amount_paid, payment_method, change_given, timestamps)
    VALUES (@SaleId, @AmountPaid, @PaymentMethod, @ChangeGiven, @SaleDate);
    
    SET @SaleCount = @SaleCount + 1;
END

PRINT '  - Created ' + CAST(@SaleCount AS NVARCHAR(10)) + ' total sales';
PRINT '';

-- ========================================
-- Step 4: Create Returns (20 Pending, 20 Approved, 15 Canceled)
-- ========================================
PRINT 'Step 4: Creating returns...';

DECLARE @ReturnCount INT = 0;
DECLARE @ReturnId INT;
DECLARE @ReturnNumber NVARCHAR(50);
DECLARE @ReturnStatus NVARCHAR(50);
DECLARE @ReturnReason NVARCHAR(1000);
DECLARE @ReturnDate DATETIME2(0);
DECLARE @BaseReturnNumber INT = 20000;
DECLARE @ReturnItemId INT;
DECLARE @ReturnQuantity INT;
DECLARE @ReturnCondition NVARCHAR(50);
DECLARE @ApprovedByUserId INT = 2; -- Seller ID 2 approves returns

-- Get sales that have items for returns
DECLARE @SalesForReturns TABLE (
    sale_id INT,
    sale_number_for_return NVARCHAR(50),
    cashier_id INT,
    sale_date DATETIME2(0)
);

INSERT INTO @SalesForReturns
SELECT TOP 55 s.id, s.sale_number, s.user_id, s.timestamps
FROM dbo.tbl_sales s
WHERE s.archives IS NULL AND s.status = 'Completed'
ORDER BY s.timestamps DESC;

-- Create 20 Pending Returns
SET @ReturnStatus = 'Pending';
DECLARE @SaleNumberForReturn NVARCHAR(50);
DECLARE pending_return_cursor CURSOR FOR SELECT TOP 20 sale_id, sale_number_for_return, cashier_id, sale_date FROM @SalesForReturns ORDER BY NEWID();
OPEN pending_return_cursor;
FETCH NEXT FROM pending_return_cursor INTO @SaleId, @SaleNumberForReturn, @CashierId, @SaleDate;

WHILE @@FETCH_STATUS = 0 AND @ReturnCount < 20
BEGIN
    SET @ReturnNumber = 'RET-' + FORMAT(GETDATE() - @ReturnCount, 'yyyyMMdd') + '-' + RIGHT('0000' + CAST(@BaseReturnNumber + @ReturnCount AS NVARCHAR(10)), 4);
    SET @ReturnReason = 'Customer return request #' + CAST(@ReturnCount + 1 AS NVARCHAR(10));
    SET @ReturnDate = DATEADD(DAY, -@ReturnCount, SYSUTCDATETIME());
    
    INSERT INTO dbo.tbl_returns (return_number, sale_id, reason, status, user_id, timestamps)
    VALUES (@ReturnNumber, @SaleId, @ReturnReason, @ReturnStatus, @CashierId, @ReturnDate);
    
    SET @ReturnId = SCOPE_IDENTITY();
    
    -- Get a sale item for this return
    SELECT TOP 1 
        @SaleItemId = si.id,
        @VariantId = si.variant_id,
        @SizeId = si.size_id,
        @ColorId = si.color_id,
        @ReturnQuantity = si.quantity
    FROM dbo.tbl_sales_items si
    WHERE si.sale_id = @SaleId
    ORDER BY NEWID();
    
    IF @SaleItemId IS NOT NULL
    BEGIN
        SET @ReturnQuantity = 1 + (ABS(CHECKSUM(NEWID())) % @ReturnQuantity); -- Return 1 to full quantity
        SET @ReturnCondition = CASE (ABS(CHECKSUM(NEWID())) % 3)
            WHEN 0 THEN 'New'
            WHEN 1 THEN 'Used'
            ELSE 'Damaged'
        END;
        
        INSERT INTO dbo.tbl_return_items (return_id, sale_item_id, variant_id, size_id, color_id, quantity, condition, timestamps)
        VALUES (@ReturnId, @SaleItemId, @VariantId, @SizeId, @ColorId, @ReturnQuantity, @ReturnCondition, @ReturnDate);
    END
    
    SET @ReturnCount = @ReturnCount + 1;
    FETCH NEXT FROM pending_return_cursor INTO @SaleId, @SaleNumberForReturn, @CashierId, @SaleDate;
END

CLOSE pending_return_cursor;
DEALLOCATE pending_return_cursor;

PRINT '  - Created 20 pending returns';

-- Create 20 Approved Returns (adds stock back)
SET @ReturnStatus = 'Approved';
DECLARE approved_return_cursor CURSOR FOR SELECT TOP 20 sale_id, sale_number_for_return, cashier_id, sale_date FROM @SalesForReturns WHERE sale_id NOT IN (SELECT sale_id FROM dbo.tbl_returns WHERE status = 'Pending') ORDER BY NEWID();
OPEN approved_return_cursor;
FETCH NEXT FROM approved_return_cursor INTO @SaleId, @SaleNumberForReturn, @CashierId, @SaleDate;

WHILE @@FETCH_STATUS = 0 AND @ReturnCount < 40
BEGIN
    SET @ReturnNumber = 'RET-' + FORMAT(GETDATE() - @ReturnCount, 'yyyyMMdd') + '-' + RIGHT('0000' + CAST(@BaseReturnNumber + @ReturnCount AS NVARCHAR(10)), 4);
    SET @ReturnReason = 'Approved customer return #' + CAST(@ReturnCount - 19 AS NVARCHAR(10));
    SET @ReturnDate = DATEADD(DAY, -@ReturnCount, SYSUTCDATETIME());
    
    INSERT INTO dbo.tbl_returns (return_number, sale_id, reason, status, user_id, approved_by, timestamps)
    VALUES (@ReturnNumber, @SaleId, @ReturnReason, @ReturnStatus, @CashierId, @ApprovedByUserId, @ReturnDate);
    
    SET @ReturnId = SCOPE_IDENTITY();
    
    SELECT TOP 1 
        @SaleItemId = si.id,
        @VariantId = si.variant_id,
        @SizeId = si.size_id,
        @ColorId = si.color_id,
        @ReturnQuantity = si.quantity,
        @VariantUserId = v.user_id,
        @ItemPrice = v.cost_price
    FROM dbo.tbl_sales_items si
    INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
    WHERE si.sale_id = @SaleId
    ORDER BY NEWID();
    
    IF @SaleItemId IS NOT NULL
    BEGIN
        SET @ReturnQuantity = 1 + (ABS(CHECKSUM(NEWID())) % @ReturnQuantity);
        SET @ReturnCondition = CASE (ABS(CHECKSUM(NEWID())) % 3)
            WHEN 0 THEN 'New'
            WHEN 1 THEN 'Used'
            ELSE 'Damaged'
        END;
        
        INSERT INTO dbo.tbl_return_items (return_id, sale_item_id, variant_id, size_id, color_id, quantity, condition, timestamps)
        VALUES (@ReturnId, @SaleItemId, @VariantId, @SizeId, @ColorId, @ReturnQuantity, @ReturnCondition, @ReturnDate);
        
        -- Add stock back via stock_in (approved returns add inventory)
        INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
        VALUES (@VariantUserId, @VariantId, @SizeId, @ColorId, @ReturnQuantity, @ItemPrice, NULL, @ReturnDate);
    END
    
    SET @ReturnCount = @ReturnCount + 1;
    FETCH NEXT FROM approved_return_cursor INTO @SaleId, @SaleNumberForReturn, @CashierId, @SaleDate;
END

CLOSE approved_return_cursor;
DEALLOCATE approved_return_cursor;

PRINT '  - Created 20 approved returns (stock added back)';

-- Create 15 Canceled Returns
SET @ReturnStatus = 'Cancelled';
DECLARE canceled_return_cursor CURSOR FOR SELECT TOP 15 sale_id, sale_number_for_return, cashier_id, sale_date FROM @SalesForReturns WHERE sale_id NOT IN (SELECT sale_id FROM dbo.tbl_returns) ORDER BY NEWID();
OPEN canceled_return_cursor;
FETCH NEXT FROM canceled_return_cursor INTO @SaleId, @SaleNumberForReturn, @CashierId, @SaleDate;

WHILE @@FETCH_STATUS = 0 AND @ReturnCount < 55
BEGIN
    SET @ReturnNumber = 'RET-' + FORMAT(GETDATE() - @ReturnCount, 'yyyyMMdd') + '-' + RIGHT('0000' + CAST(@BaseReturnNumber + @ReturnCount AS NVARCHAR(10)), 4);
    SET @ReturnReason = 'Canceled return request #' + CAST(@ReturnCount - 39 AS NVARCHAR(10));
    SET @ReturnDate = DATEADD(DAY, -@ReturnCount, SYSUTCDATETIME());
    
    INSERT INTO dbo.tbl_returns (return_number, sale_id, reason, status, user_id, approved_by, timestamps)
    VALUES (@ReturnNumber, @SaleId, @ReturnReason, @ReturnStatus, @CashierId, @ApprovedByUserId, @ReturnDate);
    
    SET @ReturnId = SCOPE_IDENTITY();
    
    SELECT TOP 1 
        @SaleItemId = si.id,
        @VariantId = si.variant_id,
        @SizeId = si.size_id,
        @ColorId = si.color_id,
        @ReturnQuantity = si.quantity
    FROM dbo.tbl_sales_items si
    WHERE si.sale_id = @SaleId
    ORDER BY NEWID();
    
    IF @SaleItemId IS NOT NULL
    BEGIN
        SET @ReturnQuantity = 1 + (ABS(CHECKSUM(NEWID())) % @ReturnQuantity);
        SET @ReturnCondition = 'N/A';
        
        INSERT INTO dbo.tbl_return_items (return_id, sale_item_id, variant_id, size_id, color_id, quantity, condition, timestamps)
        VALUES (@ReturnId, @SaleItemId, @VariantId, @SizeId, @ColorId, @ReturnQuantity, @ReturnCondition, @ReturnDate);
    END
    
    SET @ReturnCount = @ReturnCount + 1;
    FETCH NEXT FROM canceled_return_cursor INTO @SaleId, @SaleNumberForReturn, @CashierId, @SaleDate;
END

CLOSE canceled_return_cursor;
DEALLOCATE canceled_return_cursor;

PRINT '  - Created 15 canceled returns';
PRINT '';

PRINT '========================================';
PRINT 'Summary:';
PRINT '  - Sales: 100 transactions';
PRINT '    * Michael Kevin Hernandez: 20+ sales';
PRINT '    * Other cashiers: distributed';
PRINT '  - Returns: 55 total';
PRINT '    * Pending: 20';
PRINT '    * Approved: 20 (stock added back)';
PRINT '    * Canceled: 15';
PRINT '  - All sales deduct stock via stock_out';
PRINT '  - Approved returns add stock back via stock_in';
PRINT '========================================';
PRINT '';
PRINT 'Migration completed successfully!';
GO

