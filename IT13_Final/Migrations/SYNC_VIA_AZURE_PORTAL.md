# Sync Data via Azure Portal Query Editor

Since the direct connection from your application is failing (likely due to DNS/network issues or server being paused), you can sync data directly through Azure Portal Query Editor.

## Method: Use Azure Portal Query Editor

### Step 1: Ensure Database is Running

1. Go to Azure Portal
2. Navigate to your SQL Database: `db_SoftWear`
3. Check the **Status** - it should be "Online"
4. If it shows "Paused", click **"Resume"** and wait for it to start

### Step 2: Open Query Editor

1. In your SQL Database resource, click **"Query editor (preview)"** in the left menu
2. Login with:
   - **Login:** `justin`
   - **Password:** `JussPogi27`
3. Click **"OK"**

### Step 3: Run Sync Script

1. Open the file: `C:\Github\SoftWear\IT13_Final\Migrations\Sync_Data_To_Azure.ps1`
2. Copy the PowerShell script logic, OR
3. Use the SQL script below to sync data table by table

## Alternative: Use SQL Scripts for Each Table

For each table, you can run a script like this in Azure Portal Query Editor:

```sql
-- Example: Sync tbl_roles
-- First, check what's in Azure
SELECT COUNT(*) as AzureCount FROM dbo.tbl_roles;

-- Then, from your local SSMS, export data and import to Azure
-- Or use the INSERT statements generated from local database
```

## Recommended: Use SQL Server Management Studio (SSMS)

1. **Connect to Local Database:**
   - Server: `localhost\SQLEXPRESS`
   - Database: `db_SoftWear`
   - Authentication: Windows Authentication

2. **Connect to Azure Database:**
   - Server: `jussstzy.database.windows.net`
   - Database: `db_SoftWear`
   - Authentication: SQL Server Authentication
   - Login: `justin`
   - Password: `JussPogi27`

3. **Use SSMS Import/Export Wizard:**
   - Right-click on `db_SoftWear` (local) → Tasks → Export Data
   - Source: Local SQL Server
   - Destination: Azure SQL Database
   - Select tables to export
   - Run the wizard

## Quick Fix: Resume Database if Paused

If your database shows as "Paused" in Azure Portal:

1. Go to Azure Portal → SQL Database → `db_SoftWear`
2. Click **"Resume"** button at the top
3. Wait 1-2 minutes for database to start
4. Try the connection test again in your application

## Check Firewall Rules

Make sure your IP is allowed:
1. Go to Azure Portal → SQL Server → `jussstzy`
2. Click **"Networking"** or **"Firewalls and virtual networks"**
3. Verify your IP `175.176.91.199` is in the list
4. Click **"Save"** if you made changes

## Test Connection from Application

After ensuring database is online and firewall is configured:
1. Go to Admin → Database Sync
2. Click **"Test"** on Azure Database
3. If it succeeds (green), you can use "Sync All Tables"












