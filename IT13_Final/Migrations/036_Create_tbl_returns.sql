-- Migration: Create tbl_returns table
-- Description: Creates the returns table for managing return transactions
-- Date: Generated migration for returns table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_returns table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_returns') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_returns (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Return Fields
        return_number NVARCHAR(50) NOT NULL,
        sale_id INT NOT NULL,
        reason NVARCHAR(1000) NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected, Completed
        
        -- Audit Fields
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        
        -- Foreign Keys
        user_id INT NOT NULL, -- cashier who processed the return
        approved_by INT NULL, -- user who approved the return (manager/admin)
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_returns PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_returns_return_number ON dbo.tbl_returns(return_number);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_sale_id ON dbo.tbl_returns(sale_id);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_status ON dbo.tbl_returns(status);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_user_id ON dbo.tbl_returns(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_approved_by ON dbo.tbl_returns(approved_by);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_archives ON dbo.tbl_returns(archives) WHERE archives IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_returns_timestamps ON dbo.tbl_returns(timestamps);
    
    -- Create unique constraint on return_number
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_returns') AND name = 'UQ_tbl_returns_return_number')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_returns_return_number ON dbo.tbl_returns(return_number) WHERE archives IS NULL;
    END;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_returns
        ADD CONSTRAINT FK_tbl_returns_sale 
        FOREIGN KEY (sale_id) 
        REFERENCES dbo.tbl_sales(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_returns
        ADD CONSTRAINT FK_tbl_returns_user 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
        
        ALTER TABLE dbo.tbl_returns
        ADD CONSTRAINT FK_tbl_returns_approved_by 
        FOREIGN KEY (approved_by) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_returns created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_returns already exists.';
END
GO

