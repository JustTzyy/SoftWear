-- Migration: Create tbl_stock_out table
-- Description: Creates the stock_out table with all required columns, constraints
-- Date: Generated migration for stock_out table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_stock_out table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_out') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_stock_out (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Stock Out Fields
        quantity_removed INT NOT NULL,
        reason NVARCHAR(500) NULL,
        
        -- Audit Fields
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        
        -- Foreign Keys
        user_id INT NOT NULL,
        variant_id INT NOT NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_stock_out PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_user_id ON dbo.tbl_stock_out(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_variant_id ON dbo.tbl_stock_out(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_timestamps ON dbo.tbl_stock_out(timestamps DESC);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_archives ON dbo.tbl_stock_out(archives) WHERE archives IS NULL;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_out
        ADD CONSTRAINT FK_tbl_stock_out_user 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_out
        ADD CONSTRAINT FK_tbl_stock_out_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    PRINT 'Table dbo.tbl_stock_out created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_stock_out already exists.';
END
GO

