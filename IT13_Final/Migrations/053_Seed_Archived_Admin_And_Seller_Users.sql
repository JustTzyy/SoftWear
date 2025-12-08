-- Migration: Seed Archived Admin and Seller Users
-- Description: Inserts 15 archived admin users and 15 archived seller users with addresses
-- Date: 2025-12-08
-- All users have password: password123
--
-- This script:
--   1. Inserts 15 archived admin users into tbl_users (with archived_at set)
--   2. Inserts addresses for each archived admin
--   3. Inserts 15 archived seller users into tbl_users (with archived_at set)
--   4. Inserts addresses for each archived seller

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '';
PRINT '========================================';
PRINT 'Seeding Archived Admin and Seller Users';
PRINT '========================================';
PRINT '';

-- ========================================
-- Step 1: Verify Roles Exist
-- ========================================
PRINT 'Step 1: Verifying roles exist...';

DECLARE @AdminRoleId INT;
DECLARE @SellerRoleId INT;

SELECT @AdminRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'admin';
SELECT @SellerRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'seller';

IF @AdminRoleId IS NULL OR @SellerRoleId IS NULL
BEGIN
    PRINT 'ERROR: One or more roles not found!';
    PRINT 'Please ensure all roles exist before running this script.';
    RETURN;
END

PRINT '  - All roles verified';
PRINT '';

-- ========================================
-- Step 2: Insert Archived Admin Users
-- ========================================
PRINT 'Step 2: Inserting archived admin users...';
PRINT '';

DECLARE @PasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));
DECLARE @UserId INT;
DECLARE @AdminCount INT = 0;
DECLARE @SellerCount INT = 0;

-- Archived Admin 1: James Mitchell
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JamesMitchell@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('JamesMitchell@SoftWear.com', @PasswordHash, 'James Mitchell', 'James', 'Robert', 'Mitchell', '09171234501', '1980-03-15', 44, 0, @AdminRoleId, 1, 0, '2023-01-15 10:30:00', '2024-06-20 14:25:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '245 Oak Street', 'Manila', 'Metro Manila', '1000', '2023-01-15 10:30:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 1: James Mitchell';
END

-- Archived Admin 2: Sarah Johnson
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'SarahJohnson@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('SarahJohnson@SoftWear.com', @PasswordHash, 'Sarah Johnson', 'Sarah', 'Elizabeth', 'Johnson', '09172345602', '1982-07-22', 42, 1, @AdminRoleId, 1, 0, '2023-02-10 09:15:00', '2024-07-15 11:40:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '378 Maple Avenue', 'Quezon City', 'Metro Manila', '1100', '2023-02-10 09:15:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 2: Sarah Johnson';
END

-- Archived Admin 3: Michael Chen
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'MichaelChen@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('MichaelChen@SoftWear.com', @PasswordHash, 'Michael Chen', 'Michael', 'David', 'Chen', '09173456703', '1979-11-08', 45, 0, @AdminRoleId, 1, 0, '2023-03-05 14:20:00', '2024-08-10 16:55:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '512 Pine Road', 'Makati', 'Metro Manila', '1200', '2023-03-05 14:20:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 3: Michael Chen';
END

-- Archived Admin 4: Emily Rodriguez
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'EmilyRodriguez@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('EmilyRodriguez@SoftWear.com', @PasswordHash, 'Emily Rodriguez', 'Emily', 'Marie', 'Rodriguez', '09174567804', '1981-05-14', 43, 1, @AdminRoleId, 1, 0, '2023-04-12 11:45:00', '2024-09-05 13:30:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '689 Elm Boulevard', 'Pasig', 'Metro Manila', '1600', '2023-04-12 11:45:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 4: Emily Rodriguez';
END

-- Archived Admin 5: David Kim
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'DavidKim@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('DavidKim@SoftWear.com', @PasswordHash, 'David Kim', 'David', 'James', 'Kim', '09175678905', '1983-09-30', 41, 0, @AdminRoleId, 1, 0, '2023-05-20 08:30:00', '2024-10-12 15:20:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '821 Cedar Lane', 'Mandaluyong', 'Metro Manila', '1550', '2023-05-20 08:30:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 5: David Kim';
END

-- Archived Admin 6: Jennifer Williams
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JenniferWilliams@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('JenniferWilliams@SoftWear.com', @PasswordHash, 'Jennifer Williams', 'Jennifer', 'Ann', 'Williams', '09176789006', '1984-12-25', 40, 1, @AdminRoleId, 1, 0, '2023-06-08 12:15:00', '2024-11-18 10:45:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '934 Birch Street', 'Taguig', 'Metro Manila', '1630', '2023-06-08 12:15:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 6: Jennifer Williams';
END

-- Archived Admin 7: Christopher Brown
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'ChristopherBrown@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('ChristopherBrown@SoftWear.com', @PasswordHash, 'Christopher Brown', 'Christopher', 'Michael', 'Brown', '09177890107', '1978-04-18', 46, 0, @AdminRoleId, 1, 0, '2023-07-14 16:40:00', '2024-12-01 09:30:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '145 Willow Drive', 'Las Pinas', 'Metro Manila', '1740', '2023-07-14 16:40:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 7: Christopher Brown';
END

-- Archived Admin 8: Amanda Davis
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'AmandaDavis@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('AmandaDavis@SoftWear.com', @PasswordHash, 'Amanda Davis', 'Amanda', 'Rose', 'Davis', '09178901208', '1985-08-03', 39, 1, @AdminRoleId, 1, 0, '2023-08-22 13:25:00', '2025-01-10 14:15:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '267 Spruce Court', 'Muntinlupa', 'Metro Manila', '1770', '2023-08-22 13:25:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 8: Amanda Davis';
END

-- Archived Admin 9: Robert Taylor
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'RobertTaylor@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('RobertTaylor@SoftWear.com', @PasswordHash, 'Robert Taylor', 'Robert', 'William', 'Taylor', '09179012309', '1982-01-12', 42, 0, @AdminRoleId, 1, 0, '2023-09-30 10:50:00', '2025-02-15 11:20:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '389 Ash Avenue', 'Marikina', 'Metro Manila', '1800', '2023-09-30 10:50:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 9: Robert Taylor';
END

-- Archived Admin 10: Jessica Anderson
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JessicaAnderson@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('JessicaAnderson@SoftWear.com', @PasswordHash, 'Jessica Anderson', 'Jessica', 'Lynn', 'Anderson', '09170123410', '1986-06-20', 38, 1, @AdminRoleId, 1, 0, '2023-10-18 15:10:00', '2025-03-22 16:40:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '451 Hickory Way', 'Pasay', 'Metro Manila', '1300', '2023-10-18 15:10:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 10: Jessica Anderson';
END

-- Archived Admin 11: Daniel Martinez
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'DanielMartinez@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('DanielMartinez@SoftWear.com', @PasswordHash, 'Daniel Martinez', 'Daniel', 'Joseph', 'Martinez', '09171234511', '1981-10-28', 43, 0, @AdminRoleId, 1, 0, '2023-11-25 09:35:00', '2025-04-05 12:55:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '573 Poplar Street', 'Quezon City', 'Metro Manila', '1103', '2023-11-25 09:35:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 11: Daniel Martinez';
END

-- Archived Admin 12: Nicole Thompson
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'NicoleThompson@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('NicoleThompson@SoftWear.com', @PasswordHash, 'Nicole Thompson', 'Nicole', 'Grace', 'Thompson', '09172345612', '1983-02-16', 41, 1, @AdminRoleId, 1, 0, '2023-12-10 14:20:00', '2025-05-12 08:25:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '695 Sycamore Road', 'Makati', 'Metro Manila', '1226', '2023-12-10 14:20:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 12: Nicole Thompson';
END

-- Archived Admin 13: Matthew White
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'MatthewWhite@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('MatthewWhite@SoftWear.com', @PasswordHash, 'Matthew White', 'Matthew', 'Thomas', 'White', '09173456713', '1979-07-07', 45, 0, @AdminRoleId, 1, 0, '2024-01-15 11:00:00', '2025-06-18 13:50:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '817 Magnolia Lane', 'Pasig', 'Metro Manila', '1600', '2024-01-15 11:00:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 13: Matthew White';
END

-- Archived Admin 14: Lauren Harris
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'LaurenHarris@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('LaurenHarris@SoftWear.com', @PasswordHash, 'Lauren Harris', 'Lauren', 'Nicole', 'Harris', '09174567814', '1984-03-13', 40, 1, @AdminRoleId, 1, 0, '2024-02-20 16:45:00', '2025-07-25 10:15:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '939 Cherry Boulevard', 'Mandaluyong', 'Metro Manila', '1550', '2024-02-20 16:45:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 14: Lauren Harris';
END

-- Archived Admin 15: Kevin Lee
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'KevinLee@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('KevinLee@SoftWear.com', @PasswordHash, 'Kevin Lee', 'Kevin', 'Andrew', 'Lee', '09175678915', '1980-11-23', 44, 0, @AdminRoleId, 1, 0, '2024-03-28 08:55:00', '2025-08-30 15:30:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '152 Walnut Street', 'Taguig', 'Metro Manila', '1634', '2024-03-28 08:55:00');
    SET @AdminCount = @AdminCount + 1;
    PRINT '  - Archived Admin 15: Kevin Lee';
END

-- ========================================
-- Step 3: Insert Archived Seller Users
-- ========================================
PRINT '';
PRINT 'Step 3: Inserting archived seller users...';
PRINT '';

-- Archived Seller 1: Thomas Anderson
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'ThomasAnderson@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('ThomasAnderson@SoftWear.com', @PasswordHash, 'Thomas Anderson', 'Thomas', 'Edward', 'Anderson', '09201234501', '1981-04-10', 43, 0, @SellerRoleId, 1, 0, '2023-01-20 10:00:00', '2024-05-15 14:30:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '264 Oakwood Drive', 'Manila', 'Metro Manila', '1004', '2023-01-20 10:00:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 1: Thomas Anderson';
END

-- Archived Seller 2: Rachel Green
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'RachelGreen@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('RachelGreen@SoftWear.com', @PasswordHash, 'Rachel Green', 'Rachel', 'Michelle', 'Green', '09202345602', '1983-08-17', 41, 1, @SellerRoleId, 1, 0, '2023-02-14 09:30:00', '2024-06-22 11:45:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '376 Riverside Avenue', 'Quezon City', 'Metro Manila', '1108', '2023-02-14 09:30:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 2: Rachel Green';
END

-- Archived Seller 3: Brian Wilson
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'BrianWilson@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('BrianWilson@SoftWear.com', @PasswordHash, 'Brian Wilson', 'Brian', 'Patrick', 'Wilson', '09203456703', '1980-12-05', 44, 0, @SellerRoleId, 1, 0, '2023-03-10 13:15:00', '2024-07-28 16:20:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '488 Parkview Street', 'Makati', 'Metro Manila', '1224', '2023-03-10 13:15:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 3: Brian Wilson';
END

-- Archived Seller 4: Stephanie Moore
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'StephanieMoore@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('StephanieMoore@SoftWear.com', @PasswordHash, 'Stephanie Moore', 'Stephanie', 'Renee', 'Moore', '09204567804', '1982-06-21', 42, 1, @SellerRoleId, 1, 0, '2023-04-18 11:50:00', '2024-08-10 09:35:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '592 Hillcrest Road', 'Pasig', 'Metro Manila', '1600', '2023-04-18 11:50:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 4: Stephanie Moore';
END

-- Archived Seller 5: Ryan Clark
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'RyanClark@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('RyanClark@SoftWear.com', @PasswordHash, 'Ryan Clark', 'Ryan', 'Christopher', 'Clark', '09205678905', '1984-10-12', 40, 0, @SellerRoleId, 1, 0, '2023-05-25 08:20:00', '2024-09-18 12:50:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '704 Sunset Boulevard', 'Mandaluyong', 'Metro Manila', '1550', '2023-05-25 08:20:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 5: Ryan Clark';
END

-- Archived Seller 6: Michelle Lewis
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'MichelleLewis@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('MichelleLewis@SoftWear.com', @PasswordHash, 'Michelle Lewis', 'Michelle', 'Ann', 'Lewis', '09206789006', '1985-01-28', 39, 1, @SellerRoleId, 1, 0, '2023-06-12 14:40:00', '2024-10-25 15:10:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '816 Meadow Lane', 'Taguig', 'Metro Manila', '1630', '2023-06-12 14:40:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 6: Michelle Lewis';
END

-- Archived Seller 7: Jonathan Walker
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JonathanWalker@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('JonathanWalker@SoftWear.com', @PasswordHash, 'Jonathan Walker', 'Jonathan', 'Paul', 'Walker', '09207890107', '1979-05-16', 45, 0, @SellerRoleId, 1, 0, '2023-07-20 10:15:00', '2024-11-05 08:45:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '928 Forest Avenue', 'Las Pinas', 'Metro Manila', '1740', '2023-07-20 10:15:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 7: Jonathan Walker';
END

-- Archived Seller 8: Ashley Hall
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'AshleyHall@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('AshleyHall@SoftWear.com', @PasswordHash, 'Ashley Hall', 'Ashley', 'Marie', 'Hall', '09208901208', '1986-09-04', 38, 1, @SellerRoleId, 1, 0, '2023-08-28 12:30:00', '2024-12-12 14:20:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '1040 Lakeview Drive', 'Muntinlupa', 'Metro Manila', '1770', '2023-08-28 12:30:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 8: Ashley Hall';
END

-- Archived Seller 9: Brandon Young
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'BrandonYoung@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('BrandonYoung@SoftWear.com', @PasswordHash, 'Brandon Young', 'Brandon', 'Scott', 'Young', '09209012309', '1983-02-19', 41, 0, @SellerRoleId, 1, 0, '2023-09-15 15:55:00', '2025-01-20 11:30:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '1152 Valley Road', 'Marikina', 'Metro Manila', '1800', '2023-09-15 15:55:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 9: Brandon Young';
END

-- Archived Seller 10: Brittany King
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'BrittanyKing@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('BrittanyKing@SoftWear.com', @PasswordHash, 'Brittany King', 'Brittany', 'Nicole', 'King', '09200123410', '1987-07-11', 37, 1, @SellerRoleId, 1, 0, '2023-10-22 09:10:00', '2025-02-28 16:40:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '1264 Mountain View', 'Pasay', 'Metro Manila', '1300', '2023-10-22 09:10:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 10: Brittany King';
END

-- Archived Seller 11: Nathan Wright
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'NathanWright@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('NathanWright@SoftWear.com', @PasswordHash, 'Nathan Wright', 'Nathan', 'Alexander', 'Wright', '09201234511', '1982-11-29', 42, 0, @SellerRoleId, 1, 0, '2023-11-30 13:25:00', '2025-03-15 10:15:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '1376 Ocean Drive', 'Quezon City', 'Metro Manila', '1109', '2023-11-30 13:25:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 11: Nathan Wright';
END

-- Archived Seller 12: Samantha Lopez
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'SamanthaLopez@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('SamanthaLopez@SoftWear.com', @PasswordHash, 'Samantha Lopez', 'Samantha', 'Isabel', 'Lopez', '09202345612', '1984-03-24', 40, 1, @SellerRoleId, 1, 0, '2023-12-18 11:40:00', '2025-04-22 13:50:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '1488 Garden Street', 'Makati', 'Metro Manila', '1209', '2023-12-18 11:40:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 12: Samantha Lopez';
END

-- Archived Seller 13: Justin Hill
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'JustinHill@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('JustinHill@SoftWear.com', @PasswordHash, 'Justin Hill', 'Justin', 'Michael', 'Hill', '09203456713', '1981-08-08', 43, 0, @SellerRoleId, 1, 0, '2024-01-25 14:50:00', '2025-05-30 09:25:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '1600 Bridge Road', 'Pasig', 'Metro Manila', '1600', '2024-01-25 14:50:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 13: Justin Hill';
END

-- Archived Seller 14: Megan Scott
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'MeganScott@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('MeganScott@SoftWear.com', @PasswordHash, 'Megan Scott', 'Megan', 'Elizabeth', 'Scott', '09204567814', '1985-12-14', 39, 1, @SellerRoleId, 1, 0, '2024-02-10 08:35:00', '2025-06-18 15:45:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '1712 Harbor View', 'Mandaluyong', 'Metro Manila', '1550', '2024-02-10 08:35:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 14: Megan Scott';
END

-- Archived Seller 15: Tyler Adams
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_users WHERE email = 'TylerAdams@SoftWear.com')
BEGIN
    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, is_active, must_change_pw, created_at, archived_at)
    VALUES ('TylerAdams@SoftWear.com', @PasswordHash, 'Tyler Adams', 'Tyler', 'Ryan', 'Adams', '09205678915', '1980-06-26', 44, 0, @SellerRoleId, 1, 0, '2024-03-18 12:20:00', '2025-07-25 11:10:00');
    SET @UserId = SCOPE_IDENTITY();
    INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
    VALUES (@UserId, '1824 Bay Street', 'Taguig', 'Metro Manila', '1634', '2024-03-18 12:20:00');
    SET @SellerCount = @SellerCount + 1;
    PRINT '  - Archived Seller 15: Tyler Adams';
END

PRINT '';
PRINT '========================================';
PRINT 'Summary: ' + CAST(@AdminCount AS NVARCHAR(10)) + ' archived admin users inserted';
PRINT '         ' + CAST(@SellerCount AS NVARCHAR(10)) + ' archived seller users inserted';
PRINT '========================================';
PRINT '';
PRINT 'All users have:';
PRINT '  - Password: password123';
PRINT '  - Status: Active (but archived)';
PRINT '  - Complete address information';
PRINT '  - Archived dates set to various past dates';
PRINT '';
PRINT 'Migration completed successfully!';
GO

