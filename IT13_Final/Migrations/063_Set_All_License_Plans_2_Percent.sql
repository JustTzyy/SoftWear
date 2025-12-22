-- Migration: Set All Business License Plans to 2% Admin Fee
-- Description: Updates all license plans to use a flat 2% admin fee
-- Date: 2024-12-18

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- =============================================
-- Update ALL Plans to 2% Admin Fee
-- =============================================
UPDATE dbo.tbl_subscription_plans
SET admin_fee_percentage = 2.00,
    updated_at = SYSUTCDATETIME()
WHERE admin_fee_percentage != 2.00;

PRINT 'All license plans updated to 2% admin fee';
GO

-- Update descriptions to reflect flat 2% fee
UPDATE dbo.tbl_subscription_plans
SET description = 'Basic plan with Stock Clerk module access. 2% admin fee per transaction.'
WHERE code = 'NORMAL';

UPDATE dbo.tbl_subscription_plans
SET description = 'Professional plan with Stock Clerk and Cashier module access. 2% admin fee per transaction.'
WHERE code = 'PRO';

UPDATE dbo.tbl_subscription_plans
SET description = 'Full access plan with all modules including Stock Clerk, Cashier, Accounting, and complete system reports. 2% admin fee per transaction.'
WHERE code = 'PREMIUM';

PRINT 'Plan descriptions updated';
GO

PRINT '';
PRINT 'Migration completed: All Business license plans now use flat 2% admin fee.';
GO
