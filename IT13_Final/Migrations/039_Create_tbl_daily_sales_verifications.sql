-- Migration: Create tbl_daily_sales_verifications table
-- Description: Creates the daily sales verifications table for tracking accounting approval/rejection of daily sales reports
-- Date: 2025-01-23

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_daily_sales_verifications table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_daily_sales_verifications') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_daily_sales_verifications (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Verification Fields
        cashier_user_id INT NOT NULL, -- The cashier (user.id) whose sales are being verified
        sale_date DATE NOT NULL, -- The date of the sales being verified
        status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected
        notes NVARCHAR(1000) NULL, -- Optional notes from accounting
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        
        -- Foreign Keys
        verified_by INT NULL, -- The accounting user (user.id) who verified this
        seller_user_id INT NOT NULL, -- The seller (user.user_id) for filtering
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_daily_sales_verifications PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_cashier_date ON dbo.tbl_daily_sales_verifications(cashier_user_id, sale_date);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_seller_date ON dbo.tbl_daily_sales_verifications(seller_user_id, sale_date);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_status ON dbo.tbl_daily_sales_verifications(status);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_verified_by ON dbo.tbl_daily_sales_verifications(verified_by);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_archived_at ON dbo.tbl_daily_sales_verifications(archived_at) WHERE archived_at IS NULL;
    
    -- Create unique constraint on cashier_user_id + sale_date + seller_user_id (one verification per cashier per day per seller)
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_daily_sales_verifications') AND name = 'UQ_tbl_daily_sales_verifications_cashier_date_seller')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_daily_sales_verifications_cashier_date_seller 
        ON dbo.tbl_daily_sales_verifications(cashier_user_id, sale_date, seller_user_id) 
        WHERE archived_at IS NULL;
    END
    
    PRINT 'Table tbl_daily_sales_verifications created successfully.';
END
ELSE
BEGIN
    PRINT 'Table tbl_daily_sales_verifications already exists.';
END
GO




