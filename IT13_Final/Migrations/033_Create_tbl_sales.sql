-- Migration: Create tbl_sales table
-- Description: Creates the sales table for managing sales transactions
-- Date: Generated migration for sales table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_sales table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_sales (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Sales Fields
        sale_number NVARCHAR(50) NOT NULL,
        amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        payment_type NVARCHAR(50) NOT NULL, -- Cash, Credit Card, Debit Card, etc.
        status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Completed, Cancelled
        
        -- Audit Fields
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        
        -- Foreign Keys
        user_id INT NOT NULL, -- cashier who processed the sale
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_sales PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_sales_sale_number ON dbo.tbl_sales(sale_number);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_status ON dbo.tbl_sales(status);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_user_id ON dbo.tbl_sales(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_archives ON dbo.tbl_sales(archives) WHERE archives IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_sales_timestamps ON dbo.tbl_sales(timestamps);
    
    -- Create unique constraint on sale_number
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_sales') AND name = 'UQ_tbl_sales_sale_number')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_sales_sale_number ON dbo.tbl_sales(sale_number) WHERE archives IS NULL;
    END;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sales
        ADD CONSTRAINT FK_tbl_sales_user 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_sales created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_sales already exists.';
END
GO

