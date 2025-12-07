# Database Reset and User Seed Scripts

This folder contains scripts to reset your database (delete all data except roles) and seed dummy users.

## Scripts Available

1. **045_Delete_All_Data_Except_Roles.sql** - Deletes all data from all tables except `tbl_roles`
2. **046_Seed_Dummy_Users.sql** - Seeds the specified dummy users
3. **047_Reset_Database_And_Seed_Users.sql** - Combined script that does both operations in sequence (RECOMMENDED)

## Users That Will Be Created

All users have the password: **password123**

- **JohnPaul@SoftWear.com** (Admin) - No connection to seller
- **JackieClaire@SoftWear.com** (Seller) - No connection to seller
- **RechelleAnn@SoftWear.com** (Accounting) - Connected to seller (JackieClaire)
- **JustinDigal@SoftWear.com** (Stock Clerk) - Connected to seller (JackieClaire)
- **MichaelKevinHernandez@SoftWear.com** (Cashier) - Connected to seller (JackieClaire)

## How to Run

### Option 1: Using the Combined Script (Recommended)

Run **047_Reset_Database_And_Seed_Users.sql** on both your local and Azure databases.

#### For Local Database:
1. Open SQL Server Management Studio (SSMS)
2. Connect to your local SQL Server instance
3. Select the `db_SoftWear` database
4. Open and execute `047_Reset_Database_And_Seed_Users.sql`

#### For Azure Database:
1. Open SQL Server Management Studio (SSMS) or Azure Portal Query Editor
2. Connect to your Azure SQL Database (jussstzy.database.windows.net)
3. Select the `db_SoftWear` database
4. Open and execute `047_Reset_Database_And_Seed_Users.sql`

### Option 2: Using Separate Scripts

If you prefer to run the scripts separately:

1. First, run **045_Delete_All_Data_Except_Roles.sql** on both databases
2. Then, run **046_Seed_Dummy_Users.sql** on both databases

## Important Notes

⚠️ **WARNING**: These scripts will DELETE ALL DATA from your database except the roles table. Make sure you have a backup if you need to restore any data.

✅ **Roles are preserved**: The `tbl_roles` table and its data will NOT be deleted.

✅ **Works for both Local and Azure**: The scripts are designed to work on both local SQL Server and Azure SQL Database.

✅ **Idempotent**: The scripts check if users already exist before inserting, so you can run them multiple times safely.

## Verification

After running the scripts, verify the users were created:

```sql
SELECT 
    u.id,
    u.email,
    u.name,
    r.name AS role_name,
    u.user_id AS connected_to_seller_id,
    seller.email AS connected_to_seller_email
FROM dbo.tbl_users u
LEFT JOIN dbo.tbl_roles r ON r.id = u.role_id
LEFT JOIN dbo.tbl_users seller ON seller.id = u.user_id
WHERE u.archived_at IS NULL
ORDER BY u.id;
```

## Troubleshooting

### Error: "Role not found"
- Make sure you have run the role seed script (014_Seed_Data_Roles_And_Users.sql) first
- Verify that all required roles exist: admin, seller, accounting, stockclerk, cashier

### Error: "Seller user not found" (for connected users)
- Make sure the seller user (JackieClaire@SoftWear.com) is created before the connected users
- The script creates users in the correct order, but if you run 046 separately, make sure 045 was run first

### Password Hash Issues
- The scripts use SQL Server's HASHBYTES function with SHA2_256
- The hash is converted to uppercase hex string to match C# implementation
- If authentication fails, verify the hash format matches your C# code

## Next Steps

After running these scripts:
1. Test login with each user account
2. Verify user relationships (connected users should show seller connection)
3. Proceed with your application testing




