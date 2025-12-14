-- Migration: Create Subscription Tables
-- Description: Creates tables for subscription plans and seller subscriptions
-- Date: 2024-12-14

-- =============================================
-- Step 1: Create tbl_subscription_plans table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_subscription_plans') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_subscription_plans (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Plan Information
        name NVARCHAR(50) NOT NULL,                    -- 'Normal', 'Pro', 'Premium'
        code NVARCHAR(20) NOT NULL,                    -- 'NORMAL', 'PRO', 'PREMIUM'
        description NVARCHAR(500) NULL,
        price DECIMAL(10,2) NOT NULL DEFAULT 0,        -- Monthly price (e.g., $2 for Normal)
        admin_fee_percentage DECIMAL(5,2) NOT NULL,    -- e.g., 2.00, 3.50, 5.00
        
        -- Module Access Permissions (JSON or individual columns)
        has_stock_clerk_access BIT NOT NULL DEFAULT 0,
        has_cashier_access BIT NOT NULL DEFAULT 0,
        has_accounting_access BIT NOT NULL DEFAULT 0,
        has_full_reports_access BIT NOT NULL DEFAULT 0,
        
        -- Display Order
        display_order INT NOT NULL DEFAULT 0,
        
        -- Status
        is_active BIT NOT NULL DEFAULT 1,
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_subscription_plans PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes
    CREATE NONCLUSTERED INDEX IX_tbl_subscription_plans_code ON dbo.tbl_subscription_plans(code);
    CREATE NONCLUSTERED INDEX IX_tbl_subscription_plans_is_active ON dbo.tbl_subscription_plans(is_active) WHERE is_active = 1;
    
    -- Add Unique Constraint on code
    ALTER TABLE dbo.tbl_subscription_plans
    ADD CONSTRAINT UQ_tbl_subscription_plans_code UNIQUE (code);
    
    PRINT 'Table dbo.tbl_subscription_plans created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_subscription_plans already exists.';
END
GO

-- =============================================
-- Step 2: Create tbl_seller_subscriptions table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_seller_subscriptions') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_seller_subscriptions (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Foreign Keys
        seller_user_id INT NOT NULL,                   -- References tbl_users (seller)
        plan_id INT NOT NULL,                          -- References tbl_subscription_plans
        
        -- Subscription Dates
        start_date DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        end_date DATETIME2(0) NULL,                    -- NULL means ongoing
        
        -- Status
        status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- 'Active', 'Cancelled', 'Expired', 'Pending'
        
        -- Payment Information
        last_payment_date DATETIME2(0) NULL,
        next_billing_date DATETIME2(0) NULL,
        
        -- Upgrade/Downgrade Tracking
        previous_plan_id INT NULL,                     -- Track plan changes
        plan_changed_at DATETIME2(0) NULL,
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_seller_subscriptions PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes
    CREATE NONCLUSTERED INDEX IX_tbl_seller_subscriptions_seller_user_id ON dbo.tbl_seller_subscriptions(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_seller_subscriptions_plan_id ON dbo.tbl_seller_subscriptions(plan_id);
    CREATE NONCLUSTERED INDEX IX_tbl_seller_subscriptions_status ON dbo.tbl_seller_subscriptions(status) WHERE status = 'Active';
    
    -- Add Foreign Key Constraints
    ALTER TABLE dbo.tbl_seller_subscriptions
    ADD CONSTRAINT FK_tbl_seller_subscriptions_seller_user_id 
    FOREIGN KEY (seller_user_id) 
    REFERENCES dbo.tbl_users(id);
    
    ALTER TABLE dbo.tbl_seller_subscriptions
    ADD CONSTRAINT FK_tbl_seller_subscriptions_plan_id 
    FOREIGN KEY (plan_id) 
    REFERENCES dbo.tbl_subscription_plans(id);
    
    ALTER TABLE dbo.tbl_seller_subscriptions
    ADD CONSTRAINT FK_tbl_seller_subscriptions_previous_plan_id 
    FOREIGN KEY (previous_plan_id) 
    REFERENCES dbo.tbl_subscription_plans(id);
    
    PRINT 'Table dbo.tbl_seller_subscriptions created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_seller_subscriptions already exists.';
END
GO

-- =============================================
-- Step 3: Create tbl_subscription_transactions table (for admin fee tracking)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_subscription_transactions') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_subscription_transactions (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Foreign Keys
        seller_user_id INT NOT NULL,
        subscription_id INT NOT NULL,
        sale_id INT NULL,                              -- References tbl_sales (if applicable)
        
        -- Transaction Details
        transaction_type NVARCHAR(30) NOT NULL,        -- 'AdminFee', 'SubscriptionPayment', 'Refund'
        sale_amount DECIMAL(18,2) NULL,                -- Original sale amount
        admin_fee_percentage DECIMAL(5,2) NOT NULL,    -- Fee percentage at time of transaction
        admin_fee_amount DECIMAL(18,2) NOT NULL,       -- Calculated fee amount
        
        -- Status
        status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Collected', 'Waived'
        collected_at DATETIME2(0) NULL,
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_subscription_transactions PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes
    CREATE NONCLUSTERED INDEX IX_tbl_subscription_transactions_seller_user_id ON dbo.tbl_subscription_transactions(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_subscription_transactions_subscription_id ON dbo.tbl_subscription_transactions(subscription_id);
    CREATE NONCLUSTERED INDEX IX_tbl_subscription_transactions_sale_id ON dbo.tbl_subscription_transactions(sale_id);
    CREATE NONCLUSTERED INDEX IX_tbl_subscription_transactions_status ON dbo.tbl_subscription_transactions(status);
    CREATE NONCLUSTERED INDEX IX_tbl_subscription_transactions_created_at ON dbo.tbl_subscription_transactions(created_at);
    
    -- Add Foreign Key Constraints
    ALTER TABLE dbo.tbl_subscription_transactions
    ADD CONSTRAINT FK_tbl_subscription_transactions_seller_user_id 
    FOREIGN KEY (seller_user_id) 
    REFERENCES dbo.tbl_users(id);
    
    ALTER TABLE dbo.tbl_subscription_transactions
    ADD CONSTRAINT FK_tbl_subscription_transactions_subscription_id 
    FOREIGN KEY (subscription_id) 
    REFERENCES dbo.tbl_seller_subscriptions(id);
    
    -- Note: FK to tbl_sales is optional since sale_id can be NULL
    
    PRINT 'Table dbo.tbl_subscription_transactions created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_subscription_transactions already exists.';
END
GO

-- =============================================
-- Step 4: Seed default subscription plans
-- =============================================
IF NOT EXISTS (SELECT * FROM dbo.tbl_subscription_plans WHERE code = 'NORMAL')
BEGIN
    INSERT INTO dbo.tbl_subscription_plans 
        (name, code, description, price, admin_fee_percentage, has_stock_clerk_access, has_cashier_access, has_accounting_access, has_full_reports_access, display_order, is_active)
    VALUES 
        ('Normal Plan', 'NORMAL', 'Basic plan with Stock Clerk module access. 2% admin fee per transaction.', 2.00, 2.00, 1, 0, 0, 0, 1, 1);
    
    PRINT 'Normal Plan seeded successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM dbo.tbl_subscription_plans WHERE code = 'PRO')
BEGIN
    INSERT INTO dbo.tbl_subscription_plans 
        (name, code, description, price, admin_fee_percentage, has_stock_clerk_access, has_cashier_access, has_accounting_access, has_full_reports_access, display_order, is_active)
    VALUES 
        ('Pro Plan', 'PRO', 'Professional plan with Cashier module access. 3.5% admin fee per transaction.', 0.00, 3.50, 0, 1, 0, 0, 2, 1);
    
    PRINT 'Pro Plan seeded successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM dbo.tbl_subscription_plans WHERE code = 'PREMIUM')
BEGIN
    INSERT INTO dbo.tbl_subscription_plans 
        (name, code, description, price, admin_fee_percentage, has_stock_clerk_access, has_cashier_access, has_accounting_access, has_full_reports_access, display_order, is_active)
    VALUES 
        ('Premium Plan', 'PREMIUM', 'Full access plan with all modules including Stock Clerk, Cashier, Accounting, and complete system reports. 5% admin fee per transaction.', 0.00, 5.00, 1, 1, 1, 1, 3, 1);
    
    PRINT 'Premium Plan seeded successfully.';
END
GO

PRINT '';
PRINT 'Migration completed: Subscription tables created and default plans seeded.';
GO
