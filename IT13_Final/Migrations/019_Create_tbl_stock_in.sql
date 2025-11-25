-- Migration: Create tbl_stock_in table
-- Description: Creates the stock_in table with all required columns, constraints
-- Date: Generated migration for stock_in table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_stock_in table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_in') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_stock_in (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Stock In Fields
        quantity_added INT NOT NULL,
        cost_price DECIMAL(18,2) NOT NULL,
        
        -- Audit Fields
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        
        -- Foreign Keys
        user_id INT NOT NULL,
        variant_id INT NOT NULL,
        supplier_id INT NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_stock_in PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_user_id ON dbo.tbl_stock_in(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_variant_id ON dbo.tbl_stock_in(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_supplier_id ON dbo.tbl_stock_in(supplier_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_timestamps ON dbo.tbl_stock_in(timestamps DESC);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_archives ON dbo.tbl_stock_in(archives) WHERE archives IS NULL;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_in
        ADD CONSTRAINT FK_tbl_stock_in_user 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_in
        ADD CONSTRAINT FK_tbl_stock_in_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_in
        ADD CONSTRAINT FK_tbl_stock_in_supplier 
        FOREIGN KEY (supplier_id) 
        REFERENCES dbo.tbl_suppliers(id);
    END;
    
    PRINT 'Table dbo.tbl_stock_in created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_stock_in already exists.';
END
GO

