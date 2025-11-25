SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Migration: Add user_id columns to products, variants, colors, sizes, and categories tables
-- Description: Associates each record with a specific seller/user
-- Date: Add user_id foreign key columns

PRINT '';
PRINT '========================================';
PRINT 'Adding user_id columns to tables...';
PRINT '========================================';
GO

-- ========================================
-- Step 1: Add user_id to tbl_colors
-- ========================================
PRINT '';
PRINT 'Step 1: Adding user_id to tbl_colors...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_colors
    ADD user_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_colors_user_id ON dbo.tbl_colors(user_id);
    
    -- Add foreign key constraint
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_colors
        ADD CONSTRAINT FK_tbl_colors_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END
    
    PRINT 'user_id column added to tbl_colors.';
END
ELSE
BEGIN
    PRINT 'user_id column already exists in tbl_colors.';
END
GO

-- ========================================
-- Step 2: Add user_id to tbl_sizes
-- ========================================
PRINT '';
PRINT 'Step 2: Adding user_id to tbl_sizes...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_sizes
    ADD user_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_sizes_user_id ON dbo.tbl_sizes(user_id);
    
    -- Add foreign key constraint
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sizes
        ADD CONSTRAINT FK_tbl_sizes_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END
    
    PRINT 'user_id column added to tbl_sizes.';
END
ELSE
BEGIN
    PRINT 'user_id column already exists in tbl_sizes.';
END
GO

-- ========================================
-- Step 3: Add user_id to tbl_categories
-- ========================================
PRINT '';
PRINT 'Step 3: Adding user_id to tbl_categories...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_categories') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_categories
    ADD user_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_categories_user_id ON dbo.tbl_categories(user_id);
    
    -- Add foreign key constraint
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_categories
        ADD CONSTRAINT FK_tbl_categories_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END
    
    PRINT 'user_id column added to tbl_categories.';
END
ELSE
BEGIN
    PRINT 'user_id column already exists in tbl_categories.';
END
GO

-- ========================================
-- Step 4: Add user_id to tbl_products
-- ========================================
PRINT '';
PRINT 'Step 4: Adding user_id to tbl_products...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_products') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_products
    ADD user_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_products_user_id ON dbo.tbl_products(user_id);
    
    -- Add foreign key constraint
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_products
        ADD CONSTRAINT FK_tbl_products_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END
    
    PRINT 'user_id column added to tbl_products.';
END
ELSE
BEGIN
    PRINT 'user_id column already exists in tbl_products.';
END
GO

-- ========================================
-- Step 5: Add user_id to tbl_variants
-- ========================================
PRINT '';
PRINT 'Step 5: Adding user_id to tbl_variants...';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_variants
    ADD user_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_variants_user_id ON dbo.tbl_variants(user_id);
    
    -- Add foreign key constraint
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_variants
        ADD CONSTRAINT FK_tbl_variants_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END
    
    PRINT 'user_id column added to tbl_variants.';
END
ELSE
BEGIN
    PRINT 'user_id column already exists in tbl_variants.';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'user_id columns added successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Note: Existing records will have NULL user_id.';
PRINT 'You may want to update existing records with appropriate user_id values.';
PRINT '';
GO

