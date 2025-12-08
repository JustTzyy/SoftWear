-- Migration: Seed Admin Permission Requests
-- Description: Adds 15 admin permission requests for sellers (personal information changes)
-- Date: 2025-12-08
--
-- This script:
--   1. Selects 15 active sellers from the database
--   2. Creates permission requests with type "personal_info" for each seller
--   3. Sets request status to "pending"
--   4. Sets different request dates for variety

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Seeding Admin Permission Requests';
PRINT '========================================';
PRINT '';

-- ========================================
-- Step 1: Verify Seller Role Exists
-- ========================================
PRINT 'Step 1: Verifying seller role exists...';

DECLARE @SellerRoleId INT;
SELECT @SellerRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'seller';

IF @SellerRoleId IS NULL
BEGIN
    PRINT 'ERROR: Seller role not found!';
    PRINT 'Please ensure the seller role exists before running this script.';
    RETURN;
END

PRINT '  - Seller role verified';
PRINT '';

-- ========================================
-- Step 2: Get 15 Active Sellers
-- ========================================
PRINT 'Step 2: Selecting 15 active sellers...';

DECLARE @SellerIds TABLE (id INT, name NVARCHAR(200), email NVARCHAR(256), fname NVARCHAR(100), lname NVARCHAR(100), contact_no NVARCHAR(30), bday DATE, age INT, sex INT);
DECLARE @RequestCount INT = 0;

-- Get sellers that don't already have a pending permission request
INSERT INTO @SellerIds
SELECT TOP 15 
    u.id,
    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
    u.email,
    u.fname,
    u.lname,
    u.contact_no,
    u.bday,
    u.age,
    u.sex
FROM dbo.tbl_users u
WHERE u.role_id = @SellerRoleId
    AND u.archived_at IS NULL
    AND (u.permission_request_status IS NULL OR u.permission_request_status != 'pending')
ORDER BY u.id;

DECLARE @SellerCount INT;
SELECT @SellerCount = COUNT(*) FROM @SellerIds;

IF @SellerCount < 15
BEGIN
    PRINT '  - WARNING: Only ' + CAST(@SellerCount AS NVARCHAR(10)) + ' sellers found without pending requests';
    PRINT '  - Will create requests for available sellers';
END
ELSE
BEGIN
    PRINT '  - Found ' + CAST(@SellerCount AS NVARCHAR(10)) + ' sellers';
END
PRINT '';

-- ========================================
-- Step 3: Create Permission Requests
-- ========================================
PRINT 'Step 3: Creating permission requests...';
PRINT '';

DECLARE @SellerId INT;
DECLARE @SellerName NVARCHAR(200);
DECLARE @SellerEmail NVARCHAR(256);
DECLARE @SellerFname NVARCHAR(100);
DECLARE @SellerLname NVARCHAR(100);
DECLARE @SellerContact NVARCHAR(30);
DECLARE @SellerBday DATE;
DECLARE @SellerAge INT;
DECLARE @SellerSex INT;
DECLARE @RequestDate DATETIME2(0);
DECLARE @RequestDataJson NVARCHAR(MAX);
DECLARE @DaysAgo INT;

DECLARE seller_cursor CURSOR FOR 
SELECT id, name, email, fname, lname, contact_no, bday, age, sex FROM @SellerIds;
OPEN seller_cursor;
FETCH NEXT FROM seller_cursor INTO @SellerId, @SellerName, @SellerEmail, @SellerFname, @SellerLname, @SellerContact, @SellerBday, @SellerAge, @SellerSex;

WHILE @@FETCH_STATUS = 0 AND @RequestCount < 15
BEGIN
    -- Generate request date (varying from 1 to 15 days ago)
    SET @DaysAgo = @RequestCount + 1;
    SET @RequestDate = DATEADD(DAY, -@DaysAgo, SYSUTCDATETIME());
    
    -- Create personal info request data with modified values
    -- This simulates sellers requesting changes to their personal information
    DECLARE @NewFname NVARCHAR(100) = @SellerFname + ' Updated';
    DECLARE @NewLname NVARCHAR(100) = @SellerLname;
    DECLARE @NewContact NVARCHAR(30);
    
    -- Generate a new contact number (increment last digit)
    IF @SellerContact IS NOT NULL AND LEN(@SellerContact) > 0
    BEGIN
        SET @NewContact = LEFT(@SellerContact, LEN(@SellerContact) - 1) + CAST((CAST(RIGHT(@SellerContact, 1) AS INT) + 1) % 10 AS NVARCHAR(1));
    END
    ELSE
    BEGIN
        SET @NewContact = '0917123456' + CAST(@RequestCount AS NVARCHAR(1));
    END
    
    -- Create JSON request data
    SET @RequestDataJson = '{"FirstName":"' + REPLACE(@NewFname, '"', '\"') + 
                          '","MiddleName":"","LastName":"' + REPLACE(@NewLname, '"', '\"') + 
                          '","Contact":"' + REPLACE(@NewContact, '"', '\"') + 
                          '","Birthday":"' + CASE WHEN @SellerBday IS NOT NULL THEN CONVERT(NVARCHAR(10), @SellerBday, 120) ELSE '' END + 
                          '","Age":' + CASE WHEN @SellerAge IS NOT NULL THEN CAST(@SellerAge AS NVARCHAR(10)) ELSE 'NULL' END + 
                          ',"Sex":"' + CASE WHEN @SellerSex = 0 THEN 'Male' WHEN @SellerSex = 1 THEN 'Female' ELSE '' END + '"}';
    
    -- Update seller with permission request
    UPDATE dbo.tbl_users
    SET permission_request_type = 'personal_info',
        permission_request_data = @RequestDataJson,
        permission_request_status = 'pending',
        permission_request_date = @RequestDate
    WHERE id = @SellerId;
    
    SET @RequestCount = @RequestCount + 1;
    PRINT '  - Request ' + CAST(@RequestCount AS NVARCHAR(10)) + ': ' + @SellerName + ' (' + @SellerEmail + ') - ' + CONVERT(NVARCHAR(20), @RequestDate, 120);
    
    FETCH NEXT FROM seller_cursor INTO @SellerId, @SellerName, @SellerEmail, @SellerFname, @SellerLname, @SellerContact, @SellerBday, @SellerAge, @SellerSex;
END

CLOSE seller_cursor;
DEALLOCATE seller_cursor;

PRINT '';
PRINT '========================================';
PRINT 'Summary: ' + CAST(@RequestCount AS NVARCHAR(10)) + ' permission requests created';
PRINT '========================================';
PRINT '';
PRINT 'All requests:';
PRINT '  - Type: personal_info (Personal Information)';
PRINT '  - Status: pending';
PRINT '  - Request dates: Varying from 1 to ' + CAST(@RequestCount AS NVARCHAR(10)) + ' days ago';
PRINT '';
PRINT 'Migration completed successfully!';
GO




