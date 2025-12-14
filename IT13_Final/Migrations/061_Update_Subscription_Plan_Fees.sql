-- Migration: Update Subscription Plan Fees
-- Description: Updates admin fee percentages - Normal (0%), Pro (2%), Premium (5%)
-- Date: 2024-12-14

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- =============================================
-- Update Normal Plan: 2% -> 0%
-- =============================================
UPDATE dbo.tbl_subscription_plans
SET admin_fee_percentage = 0.00,
    description = 'Basic plan with Stock Clerk module access. No admin fee.',
    updated_at = SYSUTCDATETIME()
WHERE code = 'NORMAL';

PRINT 'Normal Plan updated: 0% admin fee';
GO

-- =============================================
-- Update Pro Plan: 3.5% -> 2%, add Stock Clerk access
-- =============================================
UPDATE dbo.tbl_subscription_plans
SET admin_fee_percentage = 2.00,
    has_stock_clerk_access = 1,
    description = 'Professional plan with Stock Clerk and Cashier module access. 2% admin fee per transaction.',
    updated_at = SYSUTCDATETIME()
WHERE code = 'PRO';

PRINT 'Pro Plan updated: 2% admin fee';
GO

-- =============================================
-- Premium Plan stays at 5% (no change needed)
-- =============================================
UPDATE dbo.tbl_subscription_plans
SET description = 'Full access plan with all modules including Stock Clerk, Cashier, Accounting, and complete system reports. 5% admin fee per transaction.',
    updated_at = SYSUTCDATETIME()
WHERE code = 'PREMIUM';

PRINT 'Premium Plan confirmed: 5% admin fee';
GO

PRINT '';
PRINT 'Migration completed: Subscription plan fees updated.';
PRINT '  - Normal Plan: 0%';
PRINT '  - Pro Plan: 2%';
PRINT '  - Premium Plan: 5%';
GO
