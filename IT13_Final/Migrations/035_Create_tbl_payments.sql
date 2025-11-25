-- Migration: Create tbl_payments table
-- Description: Creates the payments table for managing payment records for sales
-- Date: Generated migration for payments table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_payments table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_payments') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_payments (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Payment Fields
        sale_id INT NOT NULL,
        amount_paid DECIMAL(18,2) NOT NULL,
        payment_method NVARCHAR(50) NOT NULL, -- Cash, Credit Card, Debit Card, etc.
        change_given DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        -- Audit Fields
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_payments PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_payments_sale_id ON dbo.tbl_payments(sale_id);
    CREATE NONCLUSTERED INDEX IX_tbl_payments_payment_method ON dbo.tbl_payments(payment_method);
    CREATE NONCLUSTERED INDEX IX_tbl_payments_archives ON dbo.tbl_payments(archives) WHERE archives IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_payments_timestamps ON dbo.tbl_payments(timestamps);
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_payments
        ADD CONSTRAINT FK_tbl_payments_sale 
        FOREIGN KEY (sale_id) 
        REFERENCES dbo.tbl_sales(id) ON DELETE CASCADE;
    END;
    
    PRINT 'Table dbo.tbl_payments created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_payments already exists.';
END
GO

