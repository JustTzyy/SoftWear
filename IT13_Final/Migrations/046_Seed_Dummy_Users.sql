-- Migration: Seed Dummy Users
-- Description: Inserts the specified users with proper relationships
-- Date: 2025-01-23
-- Users:
--   - JohnPaul@SoftWear.com (admin)
--   - JackieClaire@SoftWear.com (Seller)
--   - RechelleAnn@SoftWear.com (Accounting) -> connected to seller
--   - JustinDigal@SoftWear.com (Stock clerk) -> connected to seller
--   - MichaelKevinHernandez@SoftWear.com (cashier) -> connected to seller

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Seeding Dummy Users...';
PRINT '========================================';
PRINT '';

-- ========================================
-- Step 1: Verify roles exist
-- ========================================
PRINT '';
PRINT 'Step 1: Verifying roles exist...';

DECLARE @AdminRoleId INT;
DECLARE @SellerRoleId INT;
DECLARE @AccountingRoleId INT;
DECLARE @StockClerkRoleId INT;
DECLARE @CashierRoleId INT;

SELECT @AdminRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'admin';
SELECT @SellerRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'seller';
SELECT @AccountingRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting';
SELECT @StockClerkRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk';
SELECT @CashierRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier';

IF @AdminRoleId IS NULL
BEGIN
    PRINT 'ERROR: Admin role not found!';
    RETURN;
END

IF @SellerRoleId IS NULL
BEGIN
    PRINT 'ERROR: Seller role not found!';
    RETURN;
END

IF @AccountingRoleId IS NULL
BEGIN
    PRINT 'ERROR: Accounting role not found!';
    RETURN;
END

IF @StockClerkRoleId IS NULL
BEGIN
    PRINT 'ERROR: Stock Clerk role not found!';
    RETURN;
END

IF @CashierRoleId IS NULL
BEGIN
    PRINT 'ERROR: Cashier role not found!';
    RETURN;
END

PRINT '  - All roles found successfully';
PRINT '';

-- ========================================
-- Step 2: Insert Admin User
-- ========================================
PRINT 'Step 2: Inserting admin user...';

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JohnPaul@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @AdminRoleId2 INT;
    SELECT @AdminRoleId2 = id FROM dbo.tbl_roles WHERE LOWER(name) = 'admin';
    
    -- Password: password123
    -- Hash: SHA256(Unicode('password123')) - converted to uppercase hex string
    DECLARE @AdminPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
    
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
        'JohnPaul@SoftWear.com',
        @AdminPasswordHash,
        'John Paul',
        'John',
        'Paul',
        @AdminRoleId2,
        1,
        0,
        SYSUTCDATETIME()
    );
    PRINT '  - Admin user inserted: JohnPaul@SoftWear.com (password: password123)';
END
ELSE
BEGIN
    PRINT '  - Admin user already exists';
END
GO

-- ========================================
-- Step 3: Insert Seller User
-- ========================================
PRINT '';
PRINT 'Step 3: Inserting seller user...';

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JackieClaire@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerRoleId2 INT;
    SELECT @SellerRoleId2 = id FROM dbo.tbl_roles WHERE LOWER(name) = 'seller';
    
    DECLARE @SellerPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
    
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
        'JackieClaire@SoftWear.com',
        @SellerPasswordHash,
        'Jackie Claire',
        'Jackie',
        'Claire',
        @SellerRoleId2,
        1,
        0,
        SYSUTCDATETIME()
    );
    PRINT '  - Seller user inserted: JackieClaire@SoftWear.com (password: password123)';
END
ELSE
BEGIN
    PRINT '  - Seller user already exists';
END
GO

-- ========================================
-- Step 4: Insert Accounting User (connected to seller)
-- ========================================
PRINT '';
PRINT 'Step 4: Inserting accounting user (connected to seller)...';

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'RechelleAnn@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerIdForAccounting INT;
    SELECT @SellerIdForAccounting = id FROM dbo.tbl_users WHERE email = 'JackieClaire@SoftWear.com' AND archived_at IS NULL;
    
    IF @SellerIdForAccounting IS NULL
    BEGIN
        PRINT '  - ERROR: Seller user not found. Cannot connect accounting user.';
    END
    ELSE
    BEGIN
        DECLARE @AccountingRoleId2 INT;
        SELECT @AccountingRoleId2 = id FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting';
        
        DECLARE @AccountingPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
        
        INSERT INTO dbo.tbl_users (
            email, 
            pwd_hash, 
            name, 
            fname, 
            lname, 
            role_id, 
            user_id,
            is_active, 
            must_change_pw, 
            created_at
        )
        VALUES (
            'RechelleAnn@SoftWear.com',
            @AccountingPasswordHash,
            'Rechelle Ann',
            'Rechelle',
            'Ann',
            @AccountingRoleId2,
            @SellerIdForAccounting,
            1,
            0,
            SYSUTCDATETIME()
        );
        PRINT '  - Accounting user inserted: RechelleAnn@SoftWear.com (password: password123, connected to seller)';
    END
END
ELSE
BEGIN
    PRINT '  - Accounting user already exists';
END
GO

-- ========================================
-- Step 5: Insert Stock Clerk User (connected to seller)
-- ========================================
PRINT '';
PRINT 'Step 5: Inserting stock clerk user (connected to seller)...';

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JustinDigal@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerIdForStockClerk INT;
    SELECT @SellerIdForStockClerk = id FROM dbo.tbl_users WHERE email = 'JackieClaire@SoftWear.com' AND archived_at IS NULL;
    
    IF @SellerIdForStockClerk IS NULL
    BEGIN
        PRINT '  - ERROR: Seller user not found. Cannot connect stock clerk user.';
    END
    ELSE
    BEGIN
        DECLARE @StockClerkRoleId2 INT;
        SELECT @StockClerkRoleId2 = id FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk';
        
        DECLARE @StockClerkPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
        
        INSERT INTO dbo.tbl_users (
            email, 
            pwd_hash, 
            name, 
            fname, 
            lname, 
            role_id, 
            user_id,
            is_active, 
            must_change_pw, 
            created_at
        )
        VALUES (
            'JustinDigal@SoftWear.com',
            @StockClerkPasswordHash,
            'Justin Digal',
            'Justin',
            'Digal',
            @StockClerkRoleId2,
            @SellerIdForStockClerk,
            1,
            0,
            SYSUTCDATETIME()
        );
        PRINT '  - Stock Clerk user inserted: JustinDigal@SoftWear.com (password: password123, connected to seller)';
    END
END
ELSE
BEGIN
    PRINT '  - Stock Clerk user already exists';
END
GO

-- ========================================
-- Step 6: Insert Cashier User (connected to seller)
-- ========================================
PRINT '';
PRINT 'Step 6: Inserting cashier user (connected to seller)...';

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'MichaelKevinHernandez@SoftWear.com' AND archived_at IS NULL)
BEGIN
    DECLARE @SellerIdForCashier INT;
    SELECT @SellerIdForCashier = id FROM dbo.tbl_users WHERE email = 'JackieClaire@SoftWear.com' AND archived_at IS NULL;
    
    IF @SellerIdForCashier IS NULL
    BEGIN
        PRINT '  - ERROR: Seller user not found. Cannot connect cashier user.';
    END
    ELSE
    BEGIN
        DECLARE @CashierRoleId2 INT;
        SELECT @CashierRoleId2 = id FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier';
        
        DECLARE @CashierPasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
        
        INSERT INTO dbo.tbl_users (
            email, 
            pwd_hash, 
            name, 
            fname, 
            lname, 
            role_id, 
            user_id,
            is_active, 
            must_change_pw, 
            created_at
        )
        VALUES (
            'MichaelKevinHernandez@SoftWear.com',
            @CashierPasswordHash,
            'Michael Kevin Hernandez',
            'Michael Kevin',
            'Hernandez',
            @CashierRoleId2,
            @SellerIdForCashier,
            1,
            0,
            SYSUTCDATETIME()
        );
        PRINT '  - Cashier user inserted: MichaelKevinHernandez@SoftWear.com (password: password123, connected to seller)';
    END
END
ELSE
BEGIN
    PRINT '  - Cashier user already exists';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Dummy users seed completed!';
PRINT '========================================';
PRINT '';
PRINT 'Users Created:';
PRINT '  - Admin: JohnPaul@SoftWear.com / password123';
PRINT '  - Seller: JackieClaire@SoftWear.com / password123';
PRINT '  - Accounting: RechelleAnn@SoftWear.com / password123 (connected to seller)';
PRINT '  - Stock Clerk: JustinDigal@SoftWear.com / password123 (connected to seller)';
PRINT '  - Cashier: MichaelKevinHernandez@SoftWear.com / password123 (connected to seller)';
PRINT '';
GO

