SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Migration: Seed Data for Roles and Default Users
-- Description: Inserts default roles and initial admin/seller users
-- Date: Generated seed data migration

PRINT '';
PRINT '========================================';
PRINT 'Seeding Roles and Users Data...';
PRINT '========================================';
GO

-- ========================================
-- Step 1: Insert Roles (if they don't exist)
-- ========================================
PRINT '';
PRINT 'Step 1: Inserting roles...';
GO

-- Insert Admin Role
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'admin')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('admin', 'Full system access: manage users, settings, and reports', SYSUTCDATETIME());
    PRINT 'Admin role inserted.';
END
ELSE
BEGIN
    PRINT 'Admin role already exists.';
END
GO

-- Insert Seller Role
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'seller')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('seller', 'Handles customer orders, quotes, and sales invoices', SYSUTCDATETIME());
    PRINT 'Seller role inserted.';
END
ELSE
BEGIN
    PRINT 'Seller role already exists.';
END
GO

-- Insert Cashier Role
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('cashier', 'Processes payments, receipts, and daily cash balances', SYSUTCDATETIME());
    PRINT 'Cashier role inserted.';
END
ELSE
BEGIN
    PRINT 'Cashier role already exists.';
END
GO

-- Insert Accounting Role
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('accounting', 'Manages ledgers, billing, and financial reconciliation', SYSUTCDATETIME());
    PRINT 'Accounting role inserted.';
END
ELSE
BEGIN
    PRINT 'Accounting role already exists.';
END
GO

-- Insert Stock Clerk Role
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk')
BEGIN
    INSERT INTO dbo.tbl_roles (name, [desc], created_at)
    VALUES ('stockclerk', 'Receives, organizes, and tracks inventory movement', SYSUTCDATETIME());
    PRINT 'Stock Clerk role inserted.';
END
ELSE
BEGIN
    PRINT 'Stock Clerk role already exists.';
END
GO

-- ========================================
-- Step 2: Insert Default Users
-- ========================================
PRINT '';
PRINT 'Step 2: Inserting default users...';
GO

-- Insert Admin User
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'admin@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @AdminRoleId INT;
    SELECT @AdminRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'admin';
    
    IF @AdminRoleId IS NOT NULL
    BEGIN
        -- Password: admin123
        -- Hash: SHA256(Unicode('admin123')) = 9D39DD891B174041B3488557421FAE0F8D551E1F612725717D820BDBB111530F
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
    ELSE
    BEGIN
        PRINT 'ERROR: Admin role not found. Cannot insert admin user.';
    END
END
ELSE
BEGIN
    PRINT 'Admin user already exists.';
END
GO

-- Insert Seller User
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'seller@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerRoleId INT;
    SELECT @SellerRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'seller';
    
    IF @SellerRoleId IS NOT NULL
    BEGIN
        -- Password: seller123
        -- Hash: SHA256(Unicode('seller123')) = 22F286AF25FAC3F3278DDDFEA4315F236AEAA11401F4DC101BD448711E43E878
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
    ELSE
    BEGIN
        PRINT 'ERROR: Seller role not found. Cannot insert seller user.';
    END
END
ELSE
BEGIN
    PRINT 'Seller user already exists.';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Seed data migration completed!';
PRINT '========================================';
PRINT '';
PRINT 'Default Users Created:';
PRINT '  - Admin: admin@SoftWear.com / admin123';
PRINT '  - Seller: seller@SoftWear.com / seller123';
PRINT '';
GO

