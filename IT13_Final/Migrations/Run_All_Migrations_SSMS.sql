-- Migration Runner Script for SQL Server Management Studio (SSMS)
-- This script contains all migration SQL in one file for easy execution in SSMS
-- Database: db_SoftWear
-- Server: localhost\SQLEXPRESS

USE db_SoftWear;
GO

PRINT '========================================';
PRINT 'Running All Database Migrations';
PRINT '========================================';
PRINT '';

-- ========================================
-- Step 1: Create tbl_roles table
-- ========================================
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

-- ========================================
-- Step 2: Create tbl_users table
-- ========================================
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
        CONSTRAINT PK_tbl_users PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_users_email ON dbo.tbl_users(email);
    CREATE NONCLUSTERED INDEX IX_tbl_users_role_id ON dbo.tbl_users(role_id);
    CREATE NONCLUSTERED INDEX IX_tbl_users_archived_at ON dbo.tbl_users(archived_at) WHERE archived_at IS NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_users_user_id ON dbo.tbl_users(user_id);
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_roles') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_users
        ADD CONSTRAINT FK_tbl_users_role_id 
        FOREIGN KEY (role_id) 
        REFERENCES dbo.tbl_roles(id);
    END;
    
    -- Add self-referential foreign key constraint
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

-- ========================================
-- Step 3: Create tbl_histories table
-- ========================================
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

-- ========================================
-- Step 4: Create tbl_addresses table
-- ========================================
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
        CONSTRAINT PK_tbl_addresses PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_addresses_user_id ON dbo.tbl_addresses(user_id);
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

-- ========================================
-- Step 5: Add user_id to tbl_users table (self-referential foreign key)
-- ========================================
PRINT '';
PRINT 'Step 5: Adding user_id column to tbl_users table...';

-- Add the user_id column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.tbl_users') 
    AND name = 'user_id'
)
BEGIN
    ALTER TABLE dbo.tbl_users
    ADD user_id INT NULL;
    
    PRINT 'Column user_id added to tbl_users table.';
END
ELSE
BEGIN
    PRINT 'Column user_id already exists in tbl_users table.';
END
GO

-- Create index on user_id for better performance
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE object_id = OBJECT_ID(N'dbo.tbl_users') 
    AND name = 'IX_tbl_users_user_id'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_tbl_users_user_id 
    ON dbo.tbl_users(user_id);
    
    PRINT 'Index IX_tbl_users_user_id created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_tbl_users_user_id already exists.';
END
GO

-- Add foreign key constraint (self-referential)
IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE object_id = OBJECT_ID(N'dbo.FK_tbl_users_user_id') 
    AND parent_object_id = OBJECT_ID(N'dbo.tbl_users')
)
BEGIN
    ALTER TABLE dbo.tbl_users
    ADD CONSTRAINT FK_tbl_users_user_id 
    FOREIGN KEY (user_id) 
    REFERENCES dbo.tbl_users(id);
    
    PRINT 'Foreign key constraint FK_tbl_users_user_id created successfully.';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_tbl_users_user_id already exists.';
END
GO

-- ========================================
-- Step 6: Create tbl_colors table
-- ========================================
PRINT '';
PRINT 'Step 6: Creating tbl_colors table...';

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
        CONSTRAINT PK_tbl_colors PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_colors_name ON dbo.tbl_colors(name);
    CREATE NONCLUSTERED INDEX IX_tbl_colors_archived_at ON dbo.tbl_colors(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_colors created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_colors already exists.';
END
GO

-- ========================================
-- Step 7: Create tbl_sizes table
-- ========================================
PRINT '';
PRINT 'Step 7: Creating tbl_sizes table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_sizes (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_sizes PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_sizes_name ON dbo.tbl_sizes(name);
    CREATE NONCLUSTERED INDEX IX_tbl_sizes_archived_at ON dbo.tbl_sizes(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_sizes created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_sizes already exists.';
END
GO

-- ========================================
-- Step 8: Create tbl_categories table
-- ========================================
PRINT '';
PRINT 'Step 8: Creating tbl_categories table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_categories') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_categories (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_categories PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_categories_name ON dbo.tbl_categories(name);
    CREATE NONCLUSTERED INDEX IX_tbl_categories_archived_at ON dbo.tbl_categories(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_categories created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_categories already exists.';
END
GO

-- ========================================
-- Step 9: Create tbl_products
-- ========================================
PRINT '';
PRINT 'Step 9: Creating tbl_products...';
GO

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
        CONSTRAINT PK_tbl_products PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_products_category FOREIGN KEY (category_id) REFERENCES dbo.tbl_categories(id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_products_name ON dbo.tbl_products(name);
    CREATE NONCLUSTERED INDEX IX_tbl_products_category_id ON dbo.tbl_products(category_id);
    CREATE NONCLUSTERED INDEX IX_tbl_products_archived_at ON dbo.tbl_products(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_products created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_products already exists.';
END
GO

-- ========================================
-- Step 10: Create tbl_variants
-- ========================================
PRINT '';
PRINT 'Step 10: Creating tbl_variants...';
GO

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
        CONSTRAINT PK_tbl_variants PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_variants_product FOREIGN KEY (product_id) REFERENCES dbo.tbl_products(id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_variants_name ON dbo.tbl_variants(name);
    CREATE NONCLUSTERED INDEX IX_tbl_variants_product_id ON dbo.tbl_variants(product_id);
    CREATE NONCLUSTERED INDEX IX_tbl_variants_archived_at ON dbo.tbl_variants(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_variants created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_variants already exists.';
END
GO

-- ========================================
-- Step 11: Create tbl_variant_sizes
-- ========================================
PRINT '';
PRINT 'Step 11: Creating tbl_variant_sizes...';
GO

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

-- ========================================
-- Step 12: Create tbl_variant_colors
-- ========================================
PRINT '';
PRINT 'Step 12: Creating tbl_variant_colors...';
GO

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

-- ========================================
-- Step 13: Seed Data for Roles and Users
-- ========================================
PRINT '';
PRINT 'Step 13: Seeding roles and users data...';
GO

-- Insert Roles (if they don't exist)
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'admin')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('admin', 'Full system access: manage users, settings, and reports', SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'seller')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('seller', 'Handles customer orders, quotes, and sales invoices', SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('cashier', 'Processes payments, receipts, and daily cash balances', SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('accounting', 'Manages ledgers, billing, and financial reconciliation', SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('stockclerk', 'Receives, organizes, and tracks inventory movement', SYSUTCDATETIME());
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

-- ========================================
-- Step 14: Add user_id columns to tables
-- ========================================
PRINT '';
PRINT 'Step 14: Adding user_id columns to tables...';
GO

-- Add user_id to tbl_colors
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_colors ADD user_id INT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_colors_user_id ON dbo.tbl_colors(user_id);
    ALTER TABLE dbo.tbl_colors ADD CONSTRAINT FK_tbl_colors_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
END
GO

-- Add user_id to tbl_sizes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_sizes ADD user_id INT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_sizes_user_id ON dbo.tbl_sizes(user_id);
    ALTER TABLE dbo.tbl_sizes ADD CONSTRAINT FK_tbl_sizes_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
END
GO

-- Add user_id to tbl_categories
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_categories') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_categories ADD user_id INT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_categories_user_id ON dbo.tbl_categories(user_id);
    ALTER TABLE dbo.tbl_categories ADD CONSTRAINT FK_tbl_categories_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
END
GO

-- Add user_id to tbl_products
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_products') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_products ADD user_id INT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_products_user_id ON dbo.tbl_products(user_id);
    ALTER TABLE dbo.tbl_products ADD CONSTRAINT FK_tbl_products_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
END
GO

-- Add user_id to tbl_variants
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND name = 'user_id')
BEGIN
    ALTER TABLE dbo.tbl_variants ADD user_id INT NULL;
    CREATE NONCLUSTERED INDEX IX_tbl_variants_user_id ON dbo.tbl_variants(user_id);
    ALTER TABLE dbo.tbl_variants ADD CONSTRAINT FK_tbl_variants_user FOREIGN KEY (user_id) REFERENCES dbo.tbl_users(id);
END
GO

-- ========================================
-- Step 16: Create tbl_suppliers table
-- ========================================
PRINT '';
PRINT 'Step 16: Creating tbl_suppliers table...';
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

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
    CREATE NONCLUSTERED INDEX IX_tbl_suppliers_archived_at ON dbo.tbl_suppliers(archived_at) WHERE archived_at IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_users') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_suppliers
        ADD CONSTRAINT FK_tbl_suppliers_user 
        FOREIGN KEY (user_id) 
        REFERENCES dbo.tbl_users(id);
    END;
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND name = 'IX_tbl_suppliers_user_id')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_tbl_suppliers_user_id ON dbo.tbl_suppliers(user_id);
    END;
    
    PRINT 'Table dbo.tbl_suppliers created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_suppliers already exists.';
END
GO

-- ========================================
-- Step 17: Add supplier_id to tbl_addresses
-- ========================================
PRINT '';
PRINT 'Step 17: Adding supplier_id to tbl_addresses...';
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tbl_addresses') AND name = 'supplier_id')
BEGIN
    ALTER TABLE dbo.tbl_addresses
    ADD supplier_id INT NULL;
    
    CREATE NONCLUSTERED INDEX IX_tbl_addresses_supplier_id ON dbo.tbl_addresses(supplier_id);
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_suppliers') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_addresses
        ADD CONSTRAINT FK_tbl_addresses_supplier FOREIGN KEY (supplier_id) REFERENCES dbo.tbl_suppliers(id);
    END
    
    PRINT 'supplier_id column added to tbl_addresses.';
END
ELSE
BEGIN
    PRINT 'supplier_id column already exists in tbl_addresses.';
END
GO

-- ========================================
-- Step 18: Create tbl_inventories table
-- ========================================
PRINT '';
PRINT 'Step 18: Creating tbl_inventories table...';
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_inventories (
        id INT IDENTITY(1,1) NOT NULL,
        current_stock INT NOT NULL DEFAULT 0,
        reorder_level INT NOT NULL DEFAULT 0,
        timestamps DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        archives DATETIME2(0) NULL,
        variant_id INT NOT NULL,
        CONSTRAINT PK_tbl_inventories PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_variant_id ON dbo.tbl_inventories(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_inventories_archives ON dbo.tbl_inventories(archives) WHERE archives IS NULL;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
    BEGIN
        ALTER TABLE dbo.tbl_inventories
        ADD CONSTRAINT FK_tbl_inventories_variant 
        FOREIGN KEY (variant_id) 
        REFERENCES dbo.tbl_variants(id);
    END;
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_inventories') AND name = 'UQ_tbl_inventories_variant_id')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UQ_tbl_inventories_variant_id ON dbo.tbl_inventories(variant_id) WHERE archives IS NULL;
    END;
    
    PRINT 'Table dbo.tbl_inventories created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_inventories already exists.';
END
GO

-- ========================================
-- Step 19: Create tbl_stock_in table
-- ========================================
PRINT '';
PRINT 'Step 19: Creating tbl_stock_in table...';
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

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
        CONSTRAINT PK_tbl_stock_in PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_user_id ON dbo.tbl_stock_in(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_variant_id ON dbo.tbl_stock_in(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_in_supplier_id ON dbo.tbl_stock_in(supplier_id);
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
    
    PRINT 'Table dbo.tbl_stock_in created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_stock_in already exists.';
END
GO

-- ========================================
-- Step 20: Create tbl_stock_out table
-- ========================================
PRINT '';
PRINT 'Step 20: Creating tbl_stock_out table...';
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

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
        CONSTRAINT PK_tbl_stock_out PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_user_id ON dbo.tbl_stock_out(user_id);
    CREATE NONCLUSTERED INDEX IX_tbl_stock_out_variant_id ON dbo.tbl_stock_out(variant_id);
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
    
    PRINT 'Table dbo.tbl_stock_out created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_stock_out already exists.';
END
GO

-- ========================================
-- Step 21: Create triggers to update inventory
-- ========================================
PRINT '';
PRINT 'Step 21: Creating triggers to update inventory...';
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

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
    
    MERGE dbo.tbl_inventories AS target
    USING (
        SELECT variant_id, SUM(quantity_added) as total_quantity
        FROM inserted
        WHERE archives IS NULL
        GROUP BY variant_id
    ) AS source
    ON target.variant_id = source.variant_id AND target.archives IS NULL
    WHEN MATCHED THEN
        UPDATE SET 
            current_stock = target.current_stock + source.total_quantity,
            timestamps = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (variant_id, current_stock, reorder_level, timestamps)
        VALUES (source.variant_id, source.total_quantity, 0, SYSUTCDATETIME());
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
        SELECT variant_id, SUM(quantity_removed) as total_quantity
        FROM inserted
        WHERE archives IS NULL
        GROUP BY variant_id
    ) so ON i.variant_id = so.variant_id
    WHERE i.archives IS NULL;
END
GO

PRINT 'Triggers for automatic inventory updates created successfully.';
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
GO



