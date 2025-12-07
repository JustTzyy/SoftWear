-- Migration: Create tbl_expenses table
-- Description: Creates the expenses table for tracking business expenses (utilities, supplies, salaries, repairs, etc.)
-- Date: 2025-01-23

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_expenses table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_expenses') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_expenses (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Expense Fields
        expense_type NVARCHAR(100) NOT NULL, -- Utilities, Ingredients/Supplies, Salaries/Allowances, Repairs, Other Expenses
        amount DECIMAL(18,2) NOT NULL,
        description NVARCHAR(1000) NULL,
        expense_date DATE NOT NULL,
        receipt_image NVARCHAR(MAX) NULL, -- Base64 encoded image or file path
        receipt_content_type NVARCHAR(100) NULL, -- image/jpeg, image/png, etc.
        
        -- Audit Fields
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        
        -- Foreign Keys
        created_by INT NOT NULL, -- The accounting user (user.id) who created this expense
        seller_user_id INT NOT NULL, -- The seller (user.user_id) for filtering
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_expenses PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_expense_type ON dbo.tbl_expenses(expense_type);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_expense_date ON dbo.tbl_expenses(expense_date);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_seller_user_id ON dbo.tbl_expenses(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_created_by ON dbo.tbl_expenses(created_by);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_archived_at ON dbo.tbl_expenses(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table tbl_expenses created successfully.';
END
ELSE
BEGIN
    PRINT 'Table tbl_expenses already exists.';
END
GO




