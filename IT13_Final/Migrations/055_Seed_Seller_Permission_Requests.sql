-- Migration: Seed Seller Permission Requests
-- Description: Adds 15 seller permission requests for staff (cashier/accounting/stock clerk) linked to seller ID 2
-- Date: 2025-12-08
--
-- This script:
--   1. Selects 15 active staff members (cashier/accounting/stock clerk) linked to seller ID 2
--   2. Creates permission requests with type "personal_info" for each staff member
--   3. Sets request status to "pending"
--   4. Sets different request dates for variety

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Seeding Seller Permission Requests';
PRINT '========================================';
PRINT '';

-- ========================================
-- Step 1: Verify Seller ID 2 Exists
-- ========================================
PRINT 'Step 1: Verifying seller ID 2 exists...';

DECLARE @SellerId INT = 2;
DECLARE @SellerExists INT;

SELECT @SellerExists = COUNT(*) FROM dbo.tbl_users WHERE id = @SellerId AND archived_at IS NULL;

IF @SellerExists = 0
BEGIN
    PRINT 'ERROR: Seller ID 2 not found or is archived!';
    PRINT 'Please ensure seller ID 2 exists before running this script.';
    RETURN;
END

PRINT '  - Seller ID 2 verified';
PRINT '';

-- ========================================
-- Step 2: Verify Roles Exist
-- ========================================
PRINT 'Step 2: Verifying roles exist...';

DECLARE @AccountingRoleId INT;
DECLARE @CashierRoleId INT;
DECLARE @StockClerkRoleId INT;

SELECT @AccountingRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting';
SELECT @CashierRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier';
SELECT @StockClerkRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk';

IF @AccountingRoleId IS NULL OR @CashierRoleId IS NULL OR @StockClerkRoleId IS NULL
BEGIN
    PRINT 'ERROR: One or more roles not found!';
    PRINT 'Please ensure all roles exist before running this script.';
    RETURN;
END

PRINT '  - All roles verified';
PRINT '';

-- ========================================
-- Step 3: Get 15 Active Staff Members
-- ========================================
PRINT 'Step 3: Selecting 15 active staff members...';

DECLARE @StaffIds TABLE (id INT, name NVARCHAR(200), email NVARCHAR(256), fname NVARCHAR(100), lname NVARCHAR(100), contact_no NVARCHAR(30), bday DATE, age INT, sex INT, role_name NVARCHAR(50));
DECLARE @RequestCount INT = 0;

-- Get staff that don't already have a pending permission request
INSERT INTO @StaffIds
SELECT TOP 15 
    u.id,
    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
    u.email,
    u.fname,
    u.lname,
    u.contact_no,
    u.bday,
    u.age,
    u.sex,
    r.name AS role_name
FROM dbo.tbl_users u
JOIN dbo.tbl_roles r ON r.id = u.role_id
WHERE u.user_id = @SellerId
    AND u.archived_at IS NULL
    AND LOWER(r.name) IN ('cashier', 'stock clerk', 'accounting')
    AND (u.permission_request_status IS NULL OR u.permission_request_status != 'pending')
ORDER BY u.id;

DECLARE @StaffCount INT;
SELECT @StaffCount = COUNT(*) FROM @StaffIds;

IF @StaffCount < 15
BEGIN
    PRINT '  - WARNING: Only ' + CAST(@StaffCount AS NVARCHAR(10)) + ' staff members found without pending requests';
    PRINT '  - Will create requests for available staff members';
END
ELSE
BEGIN
    PRINT '  - Found ' + CAST(@StaffCount AS NVARCHAR(10)) + ' staff members';
END
PRINT '';

-- ========================================
-- Step 4: Create Permission Requests
-- ========================================
PRINT 'Step 4: Creating permission requests...';
PRINT '';

DECLARE @StaffId INT;
DECLARE @StaffName NVARCHAR(200);
DECLARE @StaffEmail NVARCHAR(256);
DECLARE @StaffFname NVARCHAR(100);
DECLARE @StaffLname NVARCHAR(100);
DECLARE @StaffContact NVARCHAR(30);
DECLARE @StaffBday DATE;
DECLARE @StaffAge INT;
DECLARE @StaffSex INT;
DECLARE @StaffRoleName NVARCHAR(50);
DECLARE @RequestDate DATETIME2(0);
DECLARE @RequestDataJson NVARCHAR(MAX);
DECLARE @DaysAgo INT;

DECLARE staff_cursor CURSOR FOR 
SELECT id, name, email, fname, lname, contact_no, bday, age, sex, role_name FROM @StaffIds;
OPEN staff_cursor;
FETCH NEXT FROM staff_cursor INTO @StaffId, @StaffName, @StaffEmail, @StaffFname, @StaffLname, @StaffContact, @StaffBday, @StaffAge, @StaffSex, @StaffRoleName;

WHILE @@FETCH_STATUS = 0 AND @RequestCount < 15
BEGIN
    -- Generate request date (varying from 1 to 15 days ago)
    SET @DaysAgo = @RequestCount + 1;
    SET @RequestDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    
    -- Create personal info request data with modified values
    -- This simulates staff members requesting changes to their personal information
    DECLARE @NewFname NVARCHAR(100) = @StaffFname + ' Updated';
    DECLARE @NewLname NVARCHAR(100) = @StaffLname;
    DECLARE @NewContact NVARCHAR(30);
    
    -- Generate a new contact number (increment last digit)
    IF @StaffContact IS NOT NULL AND LEN(@StaffContact) > 0
    BEGIN
        DECLARE @LastDigit INT = CAST(RIGHT(@StaffContact, 1) AS INT);
        SET @NewContact = LEFT(@StaffContact, LEN(@StaffContact) - 1) + CAST((@LastDigit + 1) % 10 AS NVARCHAR(1));
    END
    ELSE
    BEGIN
        SET @NewContact = '0917123456' + CAST(@RequestCount AS NVARCHAR(1));
    END
    
    -- Create JSON request data
    DECLARE @BdayStr NVARCHAR(10) = CASE WHEN @StaffBday IS NOT NULL THEN CONVERT(NVARCHAR(10), @StaffBday, 120) ELSE '' END;
    DECLARE @AgeStr NVARCHAR(10) = CASE WHEN @StaffAge IS NOT NULL THEN CAST(@StaffAge AS NVARCHAR(10)) ELSE 'NULL' END;
    DECLARE @SexStr NVARCHAR(10) = CASE WHEN @StaffSex = 0 THEN 'Male' WHEN @StaffSex = 1 THEN 'Female' ELSE '' END;
    
    SET @RequestDataJson = '{"FirstName":"' + REPLACE(@NewFname, '"', '\"') + 
                          '","MiddleName":"","LastName":"' + REPLACE(@NewLname, '"', '\"') + 
                          '","Contact":"' + REPLACE(@NewContact, '"', '\"') + 
                          '","Birthday":"' + @BdayStr + 
                          '","Age":' + @AgeStr + 
                          ',"Sex":"' + @SexStr + '"}';
    
    -- Update staff member with permission request
    UPDATE dbo.tbl_users
    SET permission_request_type = 'personal_info',
        permission_request_data = @RequestDataJson,
        permission_request_status = 'pending',
        permission_request_date = @RequestDate
    WHERE id = @StaffId;
    
    SET @RequestCount = @RequestCount + 1;
    PRINT '  - Request ' + CAST(@RequestCount AS NVARCHAR(10)) + ': ' + @StaffName + ' (' + @StaffRoleName + ') - ' + @StaffEmail + ' - ' + CONVERT(NVARCHAR(20), @RequestDate, 120);
    
    FETCH NEXT FROM staff_cursor INTO @StaffId, @StaffName, @StaffEmail, @StaffFname, @StaffLname, @StaffContact, @StaffBday, @StaffAge, @StaffSex, @StaffRoleName;
END

CLOSE staff_cursor;
DEALLOCATE staff_cursor;

PRINT '';
PRINT '========================================';
PRINT 'Summary: ' + CAST(@RequestCount AS NVARCHAR(10)) + ' permission requests created';
PRINT '========================================';
PRINT '';
PRINT 'All requests:';
PRINT '  - Type: personal_info (Personal Information)';
PRINT '  - Status: pending';
PRINT '  - Staff linked to: Seller ID 2';
PRINT '  - Request dates: Varying from 1 to ' + CAST(@RequestCount AS NVARCHAR(10)) + ' days ago';
PRINT '';
PRINT 'These requests will appear in the Seller Permission Requests page for seller ID 2.';
PRINT '';
PRINT 'Migration completed successfully!';
GO




