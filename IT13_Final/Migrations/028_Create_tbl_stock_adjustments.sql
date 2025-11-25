-- Migration: Create tbl_stock_adjustments table
-- Description: Creates the stock adjustments table for correcting inventory discrepancies
-- Date: Create stock adjustments table

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_stock_adjustments table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_adjustments') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_stock_adjustments (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Adjustment Fields
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        adjustment_type NVARCHAR(20) NOT NULL, -- 'Increase' or 'Decrease'
        quantity_adjusted INT NOT NULL, -- Always positive, type determines if increase or decrease
        reason NVARCHAR(500) NULL, -- Reason for adjustment (Miscount, Theft, Human error, System mismatch, etc.)
        
        -- Audit Fields
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        
        -- Foreign Keys
        user_id INT NOT NULL, -- Who made the adjustment
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_stock_adjustments PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_variant_id ON dbo.tbl_stock_adjustments(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_size_id ON dbo.tbl_stock_adjustments(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_color_id ON dbo.tbl_stock_adjustments(color_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_user_id ON dbo.tbl_stock_adjustments(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_adjustment_type ON dbo.tbl_stock_adjustments(adjustment_type);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_timestamps ON dbo.tbl_stock_adjustments(timestamps DESC);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_archives ON dbo.tbl_stock_adjustments(archives) WHERE archives IS NULL;
    
    -- Add Foreign Key Constraints
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_adjustments
        ADD CONSTRAINT FK_tbl_stock_adjustments_user 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_adjustments
        ADD CONSTRAINT FK_tbl_stock_adjustments_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_adjustments
        ADD CONSTRAINT FK_tbl_stock_adjustments_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_adjustments
        ADD CONSTRAINT FK_tbl_stock_adjustments_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'Table dbo.tbl_stock_adjustments created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_stock_adjustments already exists.';
END
GO

