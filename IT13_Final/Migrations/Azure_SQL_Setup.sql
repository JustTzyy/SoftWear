-- ============================================
-- Azure SQL Database Setup Script
-- Server: jussstzy.database.windows.net
-- Database: db_SoftWear
-- Admin: justin
-- ============================================
-- This script creates the database and all tables
-- Run this script on the master database first to create db_SoftWear
-- Then run the rest on db_SoftWear
-- ============================================

-- Step 1: Create Database (Run this on master database)
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'db_SoftWear')
BEGIN
    CREATE DATABASE db_SoftWear;
    PRINT 'Database db_SoftWear created successfully.';
END
ELSE
BEGIN
    PRINT 'Database db_SoftWear already exists.';
END
GO

-- Switch to the database
USE db_SoftWear;
GO

PRINT '';
PRINT '========================================';
PRINT 'Running All Database Migrations';
PRINT '========================================';
PRINT '';

-- ============================================
-- Step 2: Create tbl_roles table
-- ============================================
PRINT 'Step 1: Creating tbl_roles table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_roles') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_roles (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(100) NOT NULL,
        [desc] NVARCHAR(500) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_roles PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_roles_name ON dbo.tbl_roles(name);
    
    ALTER TABLE dbo.tbl_roles
    ADD CONSTRAINT UQ_tbl_roles_name UNIQUE (name);
    
    PRINT 'Table dbo.tbl_roles created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_roles already exists.';
END
GO

-- ============================================
-- Step 3: Create tbl_users table
-- ============================================
PRINT '';
PRINT 'Step 2: Creating tbl_users table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_users (
        id INT IDENTITY(1,1) NOT NULL,
        email NVARCHAR(256) NOT NULL,
        pwd_hash NVARCHAR(256) NOT NULL,
        name NVARCHAR(150) NULL,
        fname NVARCHAR(100) NULL,
        mname NVARCHAR(100) NULL,
        lname NVARCHAR(100) NULL,
        contact_no NVARCHAR(30) NULL,
        bday DATE NULL,
        age INT NULL,
        sex TINYINT NULL,
        is_active BIT NOT NULL DEFAULT 1,
        must_change_pw BIT NOT NULL DEFAULT 0,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        role_id INT NULL,
        user_id INT NULL,
        permission_request_type NVARCHAR(50) NULL,
        permission_request_data NVARCHAR(MAX) NULL,
        permission_request_status NVARCHAR(20) NULL,
        permission_request_date DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_users PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_users_email ON dbo.tbl_users(email);
    CREATE NONCLUSTERED INDEX IX_tbl_users_role_id ON dbo.tbl_users(role_id);
    CREATE NONCLUSTERED INDEX IX_tbl_users_archived_at ON dbo.tbl_users(archived_at) WHERE archived_at IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_users_user_id ON dbo.tbl_users(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_users_permission_request_status ON dbo.tbl_users(permission_request_status) WHERE permission_request_status = 'pending';
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_roles') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_users
        ADD CONSTRAINT FK_tbl_users_role_id 
        FOREIGN KEY (role_id) 
        REFERENCES dbo.tbl_roles(id);
    END;
    
    ALTER TABLE dbo.tbl_users
    ADD CONSTRAINT FK_tbl_users_user_id 
    FOREIGN KEY (user_id) 
    REFERENCES dbo.tbl_users(id);
    
    PRINT 'Table dbo.tbl_users created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_users already exists.';
END
GO

-- ============================================
-- Step 4: Create tbl_histories table
-- ============================================
PRINT '';
PRINT 'Step 3: Creating tbl_histories table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_histories') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_histories (
        id INT IDENTITY(1,1) NOT NULL,
        user_id INT NOT NULL,
        status NVARCHAR(32) NOT NULL,
        module NVARCHAR(64) NOT NULL,
        description NVARCHAR(256) NULL,
        ts DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_tbl_histories PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_histories_user_id ON dbo.tbl_histories(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_histories_ts ON dbo.tbl_histories(ts DESC);
    CREATE NONCLUSTERED INDEX IX_tbl_histories_status ON dbo.tbl_histories(status);
    CREATE NONCLUSTERED INDEX IX_tbl_histories_module ON dbo.tbl_histories(module);
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_histories
        ADD CONSTRAINT FK_tbl_histories_user_id 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_histories created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_histories already exists.';
END
GO

-- ============================================
-- Step 5: Create tbl_addresses table
-- ============================================
PRINT '';
PRINT 'Step 4: Creating tbl_addresses table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_addresses') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_addresses (
        id INT IDENTITY(1,1) NOT NULL,
        street NVARCHAR(200) NULL,
        city NVARCHAR(100) NULL,
        province NVARCHAR(100) NULL,
        zip NVARCHAR(20) NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        user_id INT NOT NULL,
        supplier_id INT NULL,
        CONSTRAINT PK_tbl_addresses PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_addresses_user_id ON dbo.tbl_addresses(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_addresses_supplier_id ON dbo.tbl_addresses(supplier_id);
    CREATE NONCLUSTERED INDEX IX_tbl_addresses_archived_at ON dbo.tbl_addresses(archived_at) WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_addresses
        ADD CONSTRAINT FK_tbl_addresses_user_id 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_addresses created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_addresses already exists.';
END
GO

-- ============================================
-- Step 6: Create tbl_colors table
-- ============================================
PRINT '';
PRINT 'Step 5: Creating tbl_colors table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_colors (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(100) NOT NULL,
        hex_value NVARCHAR(7) NOT NULL,
        description NVARCHAR(500) NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        user_id INT NULL,
        CONSTRAINT PK_tbl_colors PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_colors_name ON dbo.tbl_colors(name);
    CREATE NONCLUSTERED INDEX IX_tbl_colors_user_id ON dbo.tbl_colors(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_colors_archived_at ON dbo.tbl_colors(archived_at) WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_colors
        ADD CONSTRAINT FK_tbl_colors_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_colors created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_colors already exists.';
END
GO

-- ============================================
-- Step 7: Create tbl_sizes table
-- ============================================
PRINT '';
PRINT 'Step 6: Creating tbl_sizes table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_sizes (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        user_id INT NULL,
        CONSTRAINT PK_tbl_sizes PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_sizes_name ON dbo.tbl_sizes(name);
    CREATE NONCLUSTERED INDEX IX_tbl_sizes_user_id ON dbo.tbl_sizes(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sizes_archived_at ON dbo.tbl_sizes(archived_at) WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sizes
        ADD CONSTRAINT FK_tbl_sizes_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_sizes created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_sizes already exists.';
END
GO

-- ============================================
-- Step 8: Create tbl_categories table
-- ============================================
PRINT '';
PRINT 'Step 7: Creating tbl_categories table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_categories') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_categories (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        user_id INT NULL,
        CONSTRAINT PK_tbl_categories PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_categories_name ON dbo.tbl_categories(name);
    CREATE NONCLUSTERED INDEX IX_tbl_categories_user_id ON dbo.tbl_categories(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_categories_archived_at ON dbo.tbl_categories(archived_at) WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_categories
        ADD CONSTRAINT FK_tbl_categories_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_categories created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_categories already exists.';
END
GO

-- ============================================
-- Step 9: Create tbl_products table
-- ============================================
PRINT '';
PRINT 'Step 8: Creating tbl_products table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_products') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_products (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(200) NOT NULL,
        description NVARCHAR(1000) NULL,
        category_id INT NOT NULL,
        image VARBINARY(MAX) NULL,
        image_content_type NVARCHAR(100) NULL,
        status NVARCHAR(20) NOT NULL DEFAULT 'Active',
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        user_id INT NULL,
        CONSTRAINT PK_tbl_products PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_products_category FOREIGN KEY (category_id) REFERENCES dbo.tbl_categories(id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_products_name ON dbo.tbl_products(name);
    CREATE NONCLUSTERED INDEX IX_tbl_products_category_id ON dbo.tbl_products(category_id);
    CREATE NONCLUSTERED INDEX IX_tbl_products_user_id ON dbo.tbl_products(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_products_archived_at ON dbo.tbl_products(archived_at) WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_products
        ADD CONSTRAINT FK_tbl_products_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_products created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_products already exists.';
END
GO

-- ============================================
-- Step 10: Create tbl_variants table
-- ============================================
PRINT '';
PRINT 'Step 9: Creating tbl_variants table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_variants (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(200) NOT NULL,
        price DECIMAL(18,2) NOT NULL,
        cost_price DECIMAL(18,2) NULL,
        product_id INT NOT NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        user_id INT NULL,
        CONSTRAINT PK_tbl_variants PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_variants_product FOREIGN KEY (product_id) REFERENCES dbo.tbl_products(id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_variants_name ON dbo.tbl_variants(name);
    CREATE NONCLUSTERED INDEX IX_tbl_variants_product_id ON dbo.tbl_variants(product_id);
    CREATE NONCLUSTERED INDEX IX_tbl_variants_user_id ON dbo.tbl_variants(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_variants_archived_at ON dbo.tbl_variants(archived_at) WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_variants
        ADD CONSTRAINT FK_tbl_variants_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_variants created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_variants already exists.';
END
GO

-- ============================================
-- Step 11: Create tbl_variant_sizes table
-- ============================================
PRINT '';
PRINT 'Step 10: Creating tbl_variant_sizes table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variant_sizes') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_variant_sizes (
        id INT IDENTITY(1,1) NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NOT NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_tbl_variant_sizes PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_variant_sizes_variant FOREIGN KEY (variant_id) REFERENCES dbo.tbl_variants(id) ON DELETE CASCADE,
        CONSTRAINT FK_tbl_variant_sizes_size FOREIGN KEY (size_id) REFERENCES dbo.tbl_sizes(id),
        CONSTRAINT UQ_tbl_variant_sizes_variant_size UNIQUE (variant_id, size_id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_variant_sizes_variant_id ON dbo.tbl_variant_sizes(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_variant_sizes_size_id ON dbo.tbl_variant_sizes(size_id);
    
    PRINT 'Table dbo.tbl_variant_sizes created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_variant_sizes already exists.';
END
GO

-- ============================================
-- Step 12: Create tbl_variant_colors table
-- ============================================
PRINT '';
PRINT 'Step 11: Creating tbl_variant_colors table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variant_colors') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_variant_colors (
        id INT IDENTITY(1,1) NOT NULL,
        variant_id INT NOT NULL,
        color_id INT NOT NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_tbl_variant_colors PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_variant_colors_variant FOREIGN KEY (variant_id) REFERENCES dbo.tbl_variants(id) ON DELETE CASCADE,
        CONSTRAINT FK_tbl_variant_colors_color FOREIGN KEY (color_id) REFERENCES dbo.tbl_colors(id),
        CONSTRAINT UQ_tbl_variant_colors_variant_color UNIQUE (variant_id, color_id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_variant_colors_variant_id ON dbo.tbl_variant_colors(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_variant_colors_color_id ON dbo.tbl_variant_colors(color_id);
    
    PRINT 'Table dbo.tbl_variant_colors created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_variant_colors already exists.';
END
GO

-- ============================================
-- Step 13: Create tbl_suppliers table
-- ============================================
PRINT '';
PRINT 'Step 12: Creating tbl_suppliers table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_suppliers (
        id INT IDENTITY(1,1) NOT NULL,
        company_name NVARCHAR(200) NOT NULL,
        contact_person NVARCHAR(150) NULL,
        email NVARCHAR(256) NULL,
        contact_number NVARCHAR(30) NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'Active',
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        user_id INT NULL,
        CONSTRAINT PK_tbl_suppliers PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_suppliers_company_name ON dbo.tbl_suppliers(company_name);
    CREATE NONCLUSTERED INDEX IX_tbl_suppliers_user_id ON dbo.tbl_suppliers(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_suppliers_archived_at ON dbo.tbl_suppliers(archived_at) WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_suppliers
        ADD CONSTRAINT FK_tbl_suppliers_user 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_suppliers created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_suppliers already exists.';
END
GO

-- Add supplier_id foreign key to tbl_addresses
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_tbl_addresses_supplier')
    BEGIN
        ALTER TABLE dbo.tbl_addresses
        ADD CONSTRAINT FK_tbl_addresses_supplier FOREIGN KEY (supplier_id) REFERENCES dbo.tbl_suppliers(id);
    END
END
GO

-- ============================================
-- Step 14: Create tbl_inventories table
-- ============================================
PRINT '';
PRINT 'Step 13: Creating tbl_inventories table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_inventories (
        id INT IDENTITY(1,1) NOT NULL,
        current_stock INT NOT NULL DEFAULT 0,
        reorder_level INT NOT NULL DEFAULT 0,
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        user_id INT NULL,
        CONSTRAINT PK_tbl_inventories PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_variant_id ON dbo.tbl_inventories(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_size_id ON dbo.tbl_inventories(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_color_id ON dbo.tbl_inventories(color_id);
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_user_id ON dbo.tbl_inventories(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_archives ON dbo.tbl_inventories(archives) WHERE archives IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_inventories
        ADD CONSTRAINT FK_tbl_inventories_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_inventories
        ADD CONSTRAINT FK_tbl_inventories_size FOREIGN KEY (size_id) REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_inventories
        ADD CONSTRAINT FK_tbl_inventories_color FOREIGN KEY (color_id) REFERENCES dbo.tbl_colors(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_inventories
        ADD CONSTRAINT FK_tbl_inventories_user 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_inventories_variant_size_color 
    ON dbo.tbl_inventories(variant_id, size_id, color_id) 
    WHERE archives IS NULL;
    
    PRINT 'Table dbo.tbl_inventories created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_inventories already exists.';
END
GO

-- ============================================
-- Step 15: Create tbl_stock_in table
-- ============================================
PRINT '';
PRINT 'Step 14: Creating tbl_stock_in table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_in') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_stock_in (
        id INT IDENTITY(1,1) NOT NULL,
        quantity_added INT NOT NULL,
        cost_price DECIMAL(18,2) NOT NULL,
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        user_id INT NOT NULL,
        variant_id INT NOT NULL,
        supplier_id INT NULL,
        size_id INT NULL,
        color_id INT NULL,
        CONSTRAINT PK_tbl_stock_in PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_user_id ON dbo.tbl_stock_in(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_variant_id ON dbo.tbl_stock_in(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_supplier_id ON dbo.tbl_stock_in(supplier_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_size_id ON dbo.tbl_stock_in(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_color_id ON dbo.tbl_stock_in(color_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_timestamps ON dbo.tbl_stock_in(timestamps DESC);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_archives ON dbo.tbl_stock_in(archives) WHERE archives IS NULL;
    
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
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_in
        ADD CONSTRAINT FK_tbl_stock_in_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_in
        ADD CONSTRAINT FK_tbl_stock_in_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'Table dbo.tbl_stock_in created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_stock_in already exists.';
END
GO

-- ============================================
-- Step 16: Create tbl_stock_out table
-- ============================================
PRINT '';
PRINT 'Step 15: Creating tbl_stock_out table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_out') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_stock_out (
        id INT IDENTITY(1,1) NOT NULL,
        quantity_removed INT NOT NULL,
        reason NVARCHAR(500) NULL,
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        user_id INT NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        CONSTRAINT PK_tbl_stock_out PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_user_id ON dbo.tbl_stock_out(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_variant_id ON dbo.tbl_stock_out(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_size_id ON dbo.tbl_stock_out(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_color_id ON dbo.tbl_stock_out(color_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_timestamps ON dbo.tbl_stock_out(timestamps DESC);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_archives ON dbo.tbl_stock_out(archives) WHERE archives IS NULL;
    
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
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_out
        ADD CONSTRAINT FK_tbl_stock_out_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_stock_out
        ADD CONSTRAINT FK_tbl_stock_out_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'Table dbo.tbl_stock_out created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_stock_out already exists.';
END
GO

-- ============================================
-- Step 17: Create tbl_stock_adjustments table
-- ============================================
PRINT '';
PRINT 'Step 16: Creating tbl_stock_adjustments table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_adjustments') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_stock_adjustments (
        id INT IDENTITY(1,1) NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        adjustment_type NVARCHAR(20) NOT NULL,
        quantity_adjusted INT NOT NULL,
        reason NVARCHAR(500) NULL,
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        user_id INT NOT NULL,
        CONSTRAINT PK_tbl_stock_adjustments PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_variant_id ON dbo.tbl_stock_adjustments(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_size_id ON dbo.tbl_stock_adjustments(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_color_id ON dbo.tbl_stock_adjustments(color_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_user_id ON dbo.tbl_stock_adjustments(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_adjustment_type ON dbo.tbl_stock_adjustments(adjustment_type);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_timestamps ON dbo.tbl_stock_adjustments(timestamps DESC);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_adjustments_archives ON dbo.tbl_stock_adjustments(archives) WHERE archives IS NULL;
    
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

-- ============================================
-- Step 18: Create tbl_purchase_orders table
-- ============================================
PRINT '';
PRINT 'Step 17: Creating tbl_purchase_orders table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_purchase_orders (
        id INT IDENTITY(1,1) NOT NULL,
        po_number NVARCHAR(50) NOT NULL,
        supplier_id INT NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        total_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        notes NVARCHAR(1000) NULL,
        expected_delivery_date DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        created_by INT NOT NULL,
        updated_by INT NULL,
        CONSTRAINT PK_tbl_purchase_orders PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_po_number ON dbo.tbl_purchase_orders(po_number);
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_supplier_id ON dbo.tbl_purchase_orders(supplier_id);
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_status ON dbo.tbl_purchase_orders(status);
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_created_by ON dbo.tbl_purchase_orders(created_by);
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_updated_by ON dbo.tbl_purchase_orders(updated_by);
    CREATE NONCLUSTERED INDEX IX_tbl_purchase_orders_archived_at ON dbo.tbl_purchase_orders(archived_at) WHERE archived_at IS NULL;
    
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_purchase_orders_po_number ON dbo.tbl_purchase_orders(po_number) WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_purchase_orders
        ADD CONSTRAINT FK_tbl_purchase_orders_supplier 
        FOREIGN KEY (supplier_id) 
        REFERENCES dbo.tbl_suppliers(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_purchase_orders
        ADD CONSTRAINT FK_tbl_purchase_orders_created_by 
        FOREIGN KEY (created_by) 
        REFERENCES dbo.tbl_users(id);
        
        ALTER TABLE dbo.tbl_purchase_orders
        ADD CONSTRAINT FK_tbl_purchase_orders_updated_by 
        FOREIGN KEY (updated_by) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_purchase_orders created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_purchase_orders already exists.';
END
GO

-- ============================================
-- Step 19: Create tbl_po_items table
-- ============================================
PRINT '';
PRINT 'Step 18: Creating tbl_po_items table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_po_items') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_po_items (
        id INT IDENTITY(1,1) NOT NULL,
        po_id INT NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        quantity INT NOT NULL DEFAULT 1,
        unit_price DECIMAL(18,2) NOT NULL,
        total_price DECIMAL(18,2) NOT NULL,
        received_quantity INT NOT NULL DEFAULT 0,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_po_items PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_po_items_po_id ON dbo.tbl_po_items(po_id);
    CREATE NONCLUSTERED INDEX IX_tbl_po_items_variant_id ON dbo.tbl_po_items(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_po_items_size_id ON dbo.tbl_po_items(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_po_items_color_id ON dbo.tbl_po_items(color_id);
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_po_items
        ADD CONSTRAINT FK_tbl_po_items_purchase_order 
        FOREIGN KEY (po_id) 
        REFERENCES dbo.tbl_purchase_orders(id) ON DELETE CASCADE;
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_po_items
        ADD CONSTRAINT FK_tbl_po_items_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_po_items
        ADD CONSTRAINT FK_tbl_po_items_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_po_items
        ADD CONSTRAINT FK_tbl_po_items_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'Table dbo.tbl_po_items created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_po_items already exists.';
END
GO

-- ============================================
-- Step 20: Create tbl_sales table
-- ============================================
PRINT '';
PRINT 'Step 19: Creating tbl_sales table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_sales (
        id INT IDENTITY(1,1) NOT NULL,
        sale_number NVARCHAR(50) NOT NULL,
        amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        payment_type NVARCHAR(50) NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        user_id INT NOT NULL,
        CONSTRAINT PK_tbl_sales PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_sales_sale_number ON dbo.tbl_sales(sale_number);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_status ON dbo.tbl_sales(status);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_user_id ON dbo.tbl_sales(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_archives ON dbo.tbl_sales(archives) WHERE archives IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_sales_timestamps ON dbo.tbl_sales(timestamps);
    
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_sales_sale_number ON dbo.tbl_sales(sale_number) WHERE archives IS NULL;
    
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

-- ============================================
-- Step 21: Create tbl_sales_items table
-- ============================================
PRINT '';
PRINT 'Step 20: Creating tbl_sales_items table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales_items') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_sales_items (
        id INT IDENTITY(1,1) NOT NULL,
        sale_id INT NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        quantity INT NOT NULL DEFAULT 1,
        price DECIMAL(18,2) NOT NULL,
        subtotal DECIMAL(18,2) NOT NULL,
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_sales_items PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_sale_id ON dbo.tbl_sales_items(sale_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_variant_id ON dbo.tbl_sales_items(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_size_id ON dbo.tbl_sales_items(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_color_id ON dbo.tbl_sales_items(color_id);
    CREATE NONCLUSTERED INDEX IX_tbl_sales_items_archives ON dbo.tbl_sales_items(archives) WHERE archives IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sales_items
        ADD CONSTRAINT FK_tbl_sales_items_sale 
        FOREIGN KEY (sale_id) 
        REFERENCES dbo.tbl_sales(id) ON DELETE CASCADE;
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sales_items
        ADD CONSTRAINT FK_tbl_sales_items_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sales_items
        ADD CONSTRAINT FK_tbl_sales_items_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_sales_items
        ADD CONSTRAINT FK_tbl_sales_items_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'Table dbo.tbl_sales_items created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_sales_items already exists.';
END
GO

-- ============================================
-- Step 22: Create tbl_payments table
-- ============================================
PRINT '';
PRINT 'Step 21: Creating tbl_payments table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_payments') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_payments (
        id INT IDENTITY(1,1) NOT NULL,
        sale_id INT NOT NULL,
        amount_paid DECIMAL(18,2) NOT NULL,
        payment_method NVARCHAR(50) NOT NULL,
        change_given DECIMAL(18,2) NOT NULL DEFAULT 0,
        reference_number NVARCHAR(100) NULL,
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_payments PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_payments_sale_id ON dbo.tbl_payments(sale_id);
    CREATE NONCLUSTERED INDEX IX_tbl_payments_payment_method ON dbo.tbl_payments(payment_method);
    CREATE NONCLUSTERED INDEX IX_tbl_payments_reference_number ON dbo.tbl_payments(reference_number) WHERE reference_number IS NOT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_payments_archives ON dbo.tbl_payments(archives) WHERE archives IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_payments_timestamps ON dbo.tbl_payments(timestamps);
    
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

-- ============================================
-- Step 23: Create tbl_returns table
-- ============================================
PRINT '';
PRINT 'Step 22: Creating tbl_returns table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_returns') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_returns (
        id INT IDENTITY(1,1) NOT NULL,
        return_number NVARCHAR(50) NOT NULL,
        sale_id INT NOT NULL,
        reason NVARCHAR(1000) NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        user_id INT NOT NULL,
        approved_by INT NULL,
        CONSTRAINT PK_tbl_returns PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_returns_return_number ON dbo.tbl_returns(return_number);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_sale_id ON dbo.tbl_returns(sale_id);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_status ON dbo.tbl_returns(status);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_user_id ON dbo.tbl_returns(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_approved_by ON dbo.tbl_returns(approved_by);
    CREATE NONCLUSTERED INDEX IX_tbl_returns_archives ON dbo.tbl_returns(archives) WHERE archives IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_returns_timestamps ON dbo.tbl_returns(timestamps);
    
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_returns_return_number ON dbo.tbl_returns(return_number) WHERE archives IS NULL;
    
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

-- ============================================
-- Step 24: Create tbl_return_items table
-- ============================================
PRINT '';
PRINT 'Step 23: Creating tbl_return_items table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_return_items') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_return_items (
        id INT IDENTITY(1,1) NOT NULL,
        return_id INT NOT NULL,
        sale_item_id INT NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NULL,
        color_id INT NULL,
        quantity INT NOT NULL DEFAULT 1,
        [condition] NVARCHAR(50) NULL,
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_return_items PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_return_id ON dbo.tbl_return_items(return_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_sale_item_id ON dbo.tbl_return_items(sale_item_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_variant_id ON dbo.tbl_return_items(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_size_id ON dbo.tbl_return_items(size_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_color_id ON dbo.tbl_return_items(color_id);
    CREATE NONCLUSTERED INDEX IX_tbl_return_items_archives ON dbo.tbl_return_items(archives) WHERE archives IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_returns') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_return 
        FOREIGN KEY (return_id) 
        REFERENCES dbo.tbl_returns(id) ON DELETE CASCADE;
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sales_items') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_sale_item 
        FOREIGN KEY (sale_item_id) 
        REFERENCES dbo.tbl_sales_items(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_size 
        FOREIGN KEY (size_id) 
        REFERENCES dbo.tbl_sizes(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_return_items
        ADD CONSTRAINT FK_tbl_return_items_color 
        FOREIGN KEY (color_id) 
        REFERENCES dbo.tbl_colors(id);
    END;
    
    PRINT 'Table dbo.tbl_return_items created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_return_items already exists.';
END
GO

-- ============================================
-- Step 25: Create tbl_daily_sales_verifications table
-- ============================================
PRINT '';
PRINT 'Step 24: Creating tbl_daily_sales_verifications table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_daily_sales_verifications') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_daily_sales_verifications (
        id INT IDENTITY(1,1) NOT NULL,
        cashier_user_id INT NOT NULL,
        sale_date DATE NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        notes NVARCHAR(1000) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        verified_by INT NULL,
        seller_user_id INT NOT NULL,
        CONSTRAINT PK_tbl_daily_sales_verifications PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_cashier_date ON dbo.tbl_daily_sales_verifications(cashier_user_id, sale_date);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_seller_date ON dbo.tbl_daily_sales_verifications(seller_user_id, sale_date);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_status ON dbo.tbl_daily_sales_verifications(status);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_verified_by ON dbo.tbl_daily_sales_verifications(verified_by);
    CREATE NONCLUSTERED INDEX IX_tbl_daily_sales_verifications_archived_at ON dbo.tbl_daily_sales_verifications(archived_at) WHERE archived_at IS NULL;
    
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_daily_sales_verifications_cashier_date_seller 
    ON dbo.tbl_daily_sales_verifications(cashier_user_id, sale_date, seller_user_id) 
    WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_daily_sales_verifications created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_daily_sales_verifications already exists.';
END
GO

-- ============================================
-- Step 26: Create tbl_expenses table
-- ============================================
PRINT '';
PRINT 'Step 25: Creating tbl_expenses table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_expenses') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_expenses (
        id INT IDENTITY(1,1) NOT NULL,
        expense_type NVARCHAR(100) NOT NULL,
        amount DECIMAL(18,2) NOT NULL,
        description NVARCHAR(1000) NULL,
        expense_date DATE NOT NULL,
        receipt_image NVARCHAR(MAX) NULL,
        receipt_content_type NVARCHAR(100) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        created_by INT NOT NULL,
        seller_user_id INT NOT NULL,
        CONSTRAINT PK_tbl_expenses PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_expense_type ON dbo.tbl_expenses(expense_type);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_expense_date ON dbo.tbl_expenses(expense_date);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_seller_user_id ON dbo.tbl_expenses(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_created_by ON dbo.tbl_expenses(created_by);
    CREATE NONCLUSTERED INDEX IX_tbl_expenses_archived_at ON dbo.tbl_expenses(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_expenses created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_expenses already exists.';
END
GO

-- ============================================
-- Step 27: Create tbl_supplier_invoices table
-- ============================================
PRINT '';
PRINT 'Step 26: Creating tbl_supplier_invoices table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_invoices') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_supplier_invoices (
        id INT IDENTITY(1,1) NOT NULL,
        invoice_number NVARCHAR(100) NOT NULL,
        supplier_id INT NOT NULL,
        invoice_date DATE NOT NULL,
        total_amount DECIMAL(18,2) NOT NULL,
        description NVARCHAR(1000) NULL,
        source_type NVARCHAR(50) NOT NULL DEFAULT 'Manual',
        stock_in_id INT NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        created_by INT NOT NULL,
        seller_user_id INT NOT NULL,
        CONSTRAINT PK_tbl_supplier_invoices PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_invoice_number ON dbo.tbl_supplier_invoices(invoice_number);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_supplier_id ON dbo.tbl_supplier_invoices(supplier_id);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_invoice_date ON dbo.tbl_supplier_invoices(invoice_date);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_seller_user_id ON dbo.tbl_supplier_invoices(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_invoices_archived_at ON dbo.tbl_supplier_invoices(archived_at) WHERE archived_at IS NULL;
    
    CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_supplier_invoices_invoice_number_seller 
    ON dbo.tbl_supplier_invoices(invoice_number, seller_user_id) 
    WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_invoices
        ADD CONSTRAINT FK_tbl_supplier_invoices_supplier 
        FOREIGN KEY (supplier_id) 
        REFERENCES dbo.tbl_suppliers(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_invoices
        ADD CONSTRAINT FK_tbl_supplier_invoices_created_by 
        FOREIGN KEY (created_by) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_stock_in') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_invoices
        ADD CONSTRAINT FK_tbl_supplier_invoices_stock_in 
        FOREIGN KEY (stock_in_id) 
        REFERENCES dbo.tbl_stock_in(id);
    END;
    
    PRINT 'Table dbo.tbl_supplier_invoices created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_supplier_invoices already exists.';
END
GO

-- ============================================
-- Step 28: Create tbl_supplier_payments table
-- ============================================
PRINT '';
PRINT 'Step 27: Creating tbl_supplier_payments table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_payments') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_supplier_payments (
        id INT IDENTITY(1,1) NOT NULL,
        invoice_id INT NULL,
        po_id INT NULL,
        amount_paid DECIMAL(18,2) NOT NULL,
        payment_method NVARCHAR(50) NOT NULL,
        payment_date DATE NOT NULL,
        reference_number NVARCHAR(100) NULL,
        notes NVARCHAR(1000) NULL,
        receipt_image_base64 NVARCHAR(MAX) NULL,
        receipt_image_content_type NVARCHAR(50) NULL,
        stock_in_group_key NVARCHAR(100) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        archived_at DATETIME2(0) NULL,
        created_by INT NOT NULL,
        seller_user_id INT NOT NULL,
        CONSTRAINT PK_tbl_supplier_payments PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_invoice_id ON dbo.tbl_supplier_payments(invoice_id) WHERE invoice_id IS NOT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_po_id ON dbo.tbl_supplier_payments(po_id) WHERE po_id IS NOT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_payment_date ON dbo.tbl_supplier_payments(payment_date);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_seller_user_id ON dbo.tbl_supplier_payments(seller_user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_archived_at ON dbo.tbl_supplier_payments(archived_at) WHERE archived_at IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_supplier_payments_stock_in_group_key ON dbo.tbl_supplier_payments(stock_in_group_key) WHERE stock_in_group_key IS NOT NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_supplier_invoices') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_payments
        ADD CONSTRAINT FK_tbl_supplier_payments_invoice 
        FOREIGN KEY (invoice_id) 
        REFERENCES dbo.tbl_supplier_invoices(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_purchase_orders') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_payments
        ADD CONSTRAINT FK_tbl_supplier_payments_po 
        FOREIGN KEY (po_id) 
        REFERENCES dbo.tbl_purchase_orders(id);
    END;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_supplier_payments
        ADD CONSTRAINT FK_tbl_supplier_payments_created_by 
        FOREIGN KEY (created_by) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    PRINT 'Table dbo.tbl_supplier_payments created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_supplier_payments already exists.';
END
GO

-- ============================================
-- Step 29: Create Triggers
-- ============================================
PRINT '';
PRINT 'Step 28: Creating triggers...';

-- Trigger: Update inventory on Stock In
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TR_tbl_stock_in_UpdateInventory') AND type = 'TR')
BEGIN
    DROP TRIGGER dbo.TR_tbl_stock_in_UpdateInventory;
END
GO

CREATE TRIGGER TR_tbl_stock_in_UpdateInventory
ON dbo.tbl_stock_in
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    SET ANSI_NULLS ON;
    SET QUOTED_IDENTIFIER ON;
    SET ARITHABORT ON;
    
    BEGIN TRY
        MERGE dbo.tbl_inventories AS target
        USING (
            SELECT 
                si.variant_id,
                si.size_id,
                si.color_id,
                SUM(si.quantity_added) as total_quantity
            FROM inserted si
            WHERE si.archives IS NULL
            GROUP BY si.variant_id, si.size_id, si.color_id
        ) AS source
        ON target.variant_id = source.variant_id 
            AND (target.size_id = source.size_id OR (target.size_id IS NULL AND source.size_id IS NULL))
            AND (target.color_id = source.color_id OR (target.color_id IS NULL AND source.color_id IS NULL))
            AND target.archives IS NULL
        WHEN MATCHED THEN
            UPDATE SET 
                current_stock = target.current_stock + source.total_quantity,
                timestamps = SYSUTCDATETIME()
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (variant_id, size_id, color_id, current_stock, reorder_level, timestamps)
            VALUES (
                source.variant_id, 
                source.size_id, 
                source.color_id, 
                source.total_quantity, 
                0, 
                SYSUTCDATETIME()
            );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        PRINT 'Error in TR_tbl_stock_in_UpdateInventory: ' + @ErrorMessage;
        
        IF @ErrorSeverity > 16
        BEGIN
            RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        END
    END CATCH
END
GO

-- Trigger: Update inventory on Stock Out
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TR_tbl_stock_out_UpdateInventory') AND type = 'TR')
BEGIN
    DROP TRIGGER dbo.TR_tbl_stock_out_UpdateInventory;
END
GO

CREATE TRIGGER TR_tbl_stock_out_UpdateInventory
ON dbo.tbl_stock_out
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE i
    SET 
        current_stock = i.current_stock - so.total_quantity,
        timestamps = SYSUTCDATETIME()
    FROM dbo.tbl_inventories i
    INNER JOIN (
        SELECT variant_id, size_id, color_id, SUM(quantity_removed) as total_quantity
        FROM inserted
        WHERE archives IS NULL
        GROUP BY variant_id, size_id, color_id
    ) so ON i.variant_id = so.variant_id
        AND (i.size_id = so.size_id OR (i.size_id IS NULL AND so.size_id IS NULL))
        AND (i.color_id = so.color_id OR (i.color_id IS NULL AND so.color_id IS NULL))
    WHERE i.archives IS NULL;
END
GO

-- Trigger: Update inventory on Stock Adjustment
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TR_tbl_stock_adjustments_UpdateInventory') AND type = 'TR')
BEGIN
    DROP TRIGGER dbo.TR_tbl_stock_adjustments_UpdateInventory;
END
GO

CREATE TRIGGER TR_tbl_stock_adjustments_UpdateInventory
ON dbo.tbl_stock_adjustments
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    SET ANSI_NULLS ON;
    SET QUOTED_IDENTIFIER ON;
    SET ARITHABORT ON;
    
    BEGIN TRY
        MERGE dbo.tbl_inventories AS target
        USING (
            SELECT 
                sa.variant_id,
                sa.size_id,
                sa.color_id,
                SUM(CASE 
                    WHEN sa.adjustment_type = 'Increase' THEN sa.quantity_adjusted
                    WHEN sa.adjustment_type = 'Decrease' THEN -sa.quantity_adjusted
                    ELSE 0
                END) as net_adjustment
            FROM inserted sa
            WHERE sa.archives IS NULL
            GROUP BY sa.variant_id, sa.size_id, sa.color_id
        ) AS source
        ON target.variant_id = source.variant_id 
            AND (target.size_id = source.size_id OR (target.size_id IS NULL AND source.size_id IS NULL))
            AND (target.color_id = source.color_id OR (target.color_id IS NULL AND source.color_id IS NULL))
            AND target.archives IS NULL
        WHEN MATCHED THEN
            UPDATE SET 
                current_stock = CASE 
                    WHEN (target.current_stock + source.net_adjustment) < 0 THEN 0
                    ELSE target.current_stock + source.net_adjustment
                END,
                timestamps = SYSUTCDATETIME()
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (variant_id, size_id, color_id, current_stock, reorder_level, timestamps)
            VALUES (
                source.variant_id, 
                source.size_id, 
                source.color_id, 
                CASE 
                    WHEN source.net_adjustment < 0 THEN 0
                    ELSE source.net_adjustment
                END, 
                0, 
                SYSUTCDATETIME()
            );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage2 NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity2 INT = ERROR_SEVERITY();
        DECLARE @ErrorState2 INT = ERROR_STATE();
        
        PRINT 'Error in TR_tbl_stock_adjustments_UpdateInventory: ' + @ErrorMessage2;
        
        IF @ErrorSeverity2 > 16
        BEGIN
            RAISERROR(@ErrorMessage2, @ErrorSeverity2, @ErrorState2);
        END
    END CATCH
END
GO

PRINT 'Triggers created successfully.';
GO

-- ============================================
-- Step 26: Seed Data for Roles and Users
-- ============================================
PRINT '';
PRINT 'Step 25: Seeding roles and users data...';

-- Insert Roles
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'admin')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('admin', 'Full system access: manage users, settings, and reports', SYSUTCDATETIME());
    PRINT 'Admin role inserted.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'seller')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('seller', 'Handles customer orders, quotes, and sales invoices', SYSUTCDATETIME());
    PRINT 'Seller role inserted.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('cashier', 'Processes payments, receipts, and daily cash balances', SYSUTCDATETIME());
    PRINT 'Cashier role inserted.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('accounting', 'Manages ledgers, billing, and financial reconciliation', SYSUTCDATETIME());
    PRINT 'Accounting role inserted.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('stockclerk', 'Receives, organizes, and tracks inventory movement', SYSUTCDATETIME());
    PRINT 'Stock Clerk role inserted.';
END
GO

-- Insert Admin User
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'admin@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @AdminRoleId INT;
    SELECT @AdminRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'admin';
    
    IF @AdminRoleId IS NOT NULL
    BEGIN
        INSERT INTO dbo.tbl_users (
            email, 
            pwd_hash, 
            name, 
            fname, 
            lname, 
            role_id, 
            is_active, 
            must_change_pw, 
            created_at
        )
        VALUES (
            'admin@SoftWear.com',
            '9D39DD891B174041B3488557421FAE0F8D551E1F612725717D820BDBB111530F',
            'Admin User',
            'Admin',
            'User',
            @AdminRoleId,
            1,
            0,
            SYSUTCDATETIME()
        );
        PRINT 'Admin user inserted (email: admin@SoftWear.com, password: admin123).';
    END
END
GO

-- Insert Seller User
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'seller@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerRoleId INT;
    SELECT @SellerRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'seller';
    
    IF @SellerRoleId IS NOT NULL
    BEGIN
        INSERT INTO dbo.tbl_users (
            email, 
            pwd_hash, 
            name, 
            fname, 
            lname, 
            role_id, 
            is_active, 
            must_change_pw, 
            created_at
        )
        VALUES (
            'seller@SoftWear.com',
            '22F286AF25FAC3F3278DDDFEA4315F236AEAA11401F4DC101BD448711E43E878',
            'Seller User',
            'Seller',
            'User',
            @SellerRoleId,
            1,
            0,
            SYSUTCDATETIME()
        );
        PRINT 'Seller user inserted (email: seller@SoftWear.com, password: seller123).';
    END
END
GO

PRINT '';
PRINT '========================================';
PRINT 'All migrations completed!';
PRINT '========================================';
PRINT '';
PRINT 'Default Users Created:';
PRINT '  - Admin: admin@SoftWear.com / admin123';
PRINT '  - Seller: seller@SoftWear.com / seller123';
PRINT '';
PRINT 'Database: db_SoftWear';
PRINT 'Server: jussstzy.database.windows.net';
PRINT '';
GO












