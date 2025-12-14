-- Migration: Assign default Normal subscription to all existing sellers who don't have one
-- This ensures existing sellers can use the system and admin fees are tracked

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

-- Get the Normal plan ID (default plan for existing sellers)
DECLARE @NormalPlanId INT;
SELECT @NormalPlanId = id FROM dbo.tbl_subscription_plans WHERE code = 'NORMAL' AND is_active = 1;

IF @NormalPlanId IS NULL
BEGIN
    PRINT 'ERROR: Normal subscription plan not found. Please run migration 059 first.';
    RETURN;
END

PRINT 'Normal Plan ID: ' + CAST(@NormalPlanId AS VARCHAR(10));

-- Insert subscriptions for sellers who don't have one
-- Seller role_id = 2
INSERT INTO dbo.tbl_seller_subscriptions (seller_user_id, plan_id, start_date, status, created_at)
SELECT 
    u.id,
    @NormalPlanId,
    SYSUTCDATETIME(),
    'Active',
    SYSUTCDATETIME()
FROM dbo.tbl_users u
WHERE u.role_id = 2
AND u.archived_at IS NULL
AND NOT EXISTS (
    SELECT 1 FROM dbo.tbl_seller_subscriptions ss 
    WHERE ss.seller_user_id = u.id 
    AND ss.status = 'Active'
    AND ss.archived_at IS NULL
);

DECLARE @SellersUpdated INT = @@ROWCOUNT;
PRINT 'Assigned Normal subscription to ' + CAST(@SellersUpdated AS VARCHAR(10)) + ' existing sellers.';

-- Now backfill admin fee transactions for existing approved sales
-- This creates transaction records for historical sales so admin can see fee data
PRINT 'Backfilling admin fee transactions for existing approved sales...';

INSERT INTO dbo.tbl_subscription_transactions 
    (seller_user_id, subscription_id, sale_id, transaction_type, sale_amount, 
     admin_fee_percentage, admin_fee_amount, status, created_at)
SELECT DISTINCT
    p.user_id as seller_user_id,
    ss.id as subscription_id,
    s.id as sale_id,
    'AdminFee' as transaction_type,
    sale_totals.total_amount as sale_amount,
    sp.admin_fee_percentage,
    ROUND(sale_totals.total_amount * (sp.admin_fee_percentage / 100.0), 2) as admin_fee_amount,
    'Pending' as status,
    s.timestamps as created_at
FROM dbo.tbl_sales s
INNER JOIN dbo.tbl_sales_items si ON s.id = si.sale_id
INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
INNER JOIN dbo.tbl_products p ON v.product_id = p.id
INNER JOIN dbo.tbl_seller_subscriptions ss ON p.user_id = ss.seller_user_id AND ss.status = 'Active'
INNER JOIN dbo.tbl_subscription_plans sp ON ss.plan_id = sp.id
CROSS APPLY (
    SELECT SUM(si2.subtotal) as total_amount
    FROM dbo.tbl_sales_items si2
    WHERE si2.sale_id = s.id AND si2.archives IS NULL
) sale_totals
WHERE s.archives IS NULL
AND s.status = 'Completed'
AND p.user_id IS NOT NULL
-- Only include sales that have been approved in Daily Sales Verification
AND EXISTS (
    SELECT 1 FROM dbo.tbl_daily_sales_verifications dsv
    WHERE dsv.cashier_user_id = s.user_id
    AND dsv.sale_date = CAST(s.timestamps AS DATE)
    AND dsv.seller_user_id = p.user_id
    AND dsv.status = 'Approved'
    AND dsv.archived_at IS NULL
)
-- Don't duplicate existing transactions
AND NOT EXISTS (
    SELECT 1 FROM dbo.tbl_subscription_transactions st
    WHERE st.sale_id = s.id
    AND st.seller_user_id = p.user_id
    AND st.transaction_type = 'AdminFee'
);

DECLARE @TransactionsCreated INT = @@ROWCOUNT;
PRINT 'Created ' + CAST(@TransactionsCreated AS VARCHAR(10)) + ' admin fee transaction records for existing sales.';

PRINT 'Migration completed successfully.';
