# Quick Setup Using SQL Server Management Studio (SSMS)

Since the PowerShell script is having DNS issues, here's the fastest way to set up your database:

## Steps:

1. **Open SQL Server Management Studio (SSMS)**
   - If you don't have it, download from: https://aka.ms/ssmsfullsetup

2. **Connect to Azure SQL:**
   - Click "Connect" → "Database Engine"
   - **Server name:** `jussstzy.database.windows.net`
     - ⚠️ **IMPORTANT:** If this doesn't work, check your Azure Portal for the exact server name
     - It might be something like: `jussstzy.database.windows.net` or just `jussstzy`
   - **Authentication:** SQL Server Authentication
   - **Login:** `justin`
   - **Password:** `JussPogi27`
   - Click "Connect"

3. **Run the Setup Script:**
   - In SSMS, press `Ctrl+O` or go to File → Open → File
   - Navigate to: `C:\Github\SoftWear\IT13_Final\Migrations\Azure_SQL_Setup.sql`
   - Open it
   - Press `F5` to execute

4. **Verify:**
   - In Object Explorer, expand your server
   - Expand "Databases"
   - You should see `db_SoftWear`
   - Expand it → Tables → You should see all 23 tables

## If Connection Fails:

1. **Verify Server Name:**
   - Go to Azure Portal
   - Find your SQL Server resource
   - Copy the exact "Server name" shown there
   - Use that exact name in SSMS

2. **Check Firewall:**
   - Your IP `175.176.91.199` is already added ✅
   - Make sure the rule is saved

3. **Check Server Status:**
   - Make sure the SQL Server is "Running" (not paused)

## Alternative: Use Azure Portal Query Editor

1. Go to Azure Portal
2. Navigate to your SQL Database (not the server)
3. Click "Query editor" in the left menu
4. Login with `justin` / `JussPogi27`
5. Copy the entire contents of `Azure_SQL_Setup.sql`
6. Paste and click "Run"

This method doesn't require DNS resolution and works directly through Azure Portal.












