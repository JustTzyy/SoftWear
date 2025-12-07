# Create Database - Step by Step Instructions

Since direct connection from your machine is blocked, here are 3 ways to create the database:

## Method 1: Azure Portal (Easiest) ⭐

### Step 1: Create the Database
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your **SQL Server** resource (`jussstzy`)
3. Click **"+ New database"** or **"Create database"** button
4. Fill in:
   - **Database name:** `db_SoftWear`
   - **Compute + storage:** Choose Basic or S0 (cheapest option)
   - Click **"Create"**
5. Wait 1-2 minutes for database to be created

### Step 2: Run Setup Script
1. Once database is created, go to the **database** resource (not the server)
2. Click **"Query editor (preview)"** in left menu
3. Login: `justin` / `JussPogi27`
4. Open `C:\Github\SoftWear\IT13_Final\Migrations\Azure_SQL_Setup.sql`
5. Copy entire contents and paste into Query Editor
6. Click **"Run"**

---

## Method 2: Azure CLI (If you have it installed)

Open PowerShell or Command Prompt and run:

```bash
# Login to Azure
az login

# Create the database
az sql db create \
  --resource-group softwear-rg \
  --server jussstzy \
  --name db_SoftWear \
  --service-objective Basic \
  --backup-storage-redundancy Local
```

Then run the setup script via Azure Portal Query Editor.

---

## Method 3: SQL Script via Azure Portal Query Editor

If the database already exists but you want to verify, or if you can access Query Editor:

1. Go to Azure Portal → Your SQL Server (`jussstzy`)
2. Look for **"Query editor"** or **"SQL query editor"** option
3. If available, login and run this:

```sql
-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'db_SoftWear')
BEGIN
    CREATE DATABASE db_SoftWear
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Database created successfully.';
END
ELSE
BEGIN
    PRINT 'Database already exists.';
END
```

---

## After Database is Created

Once `db_SoftWear` exists:

1. Go to the **database** resource in Azure Portal
2. Click **"Query editor (preview)"**
3. Login with `justin` / `JussPogi27`
4. Copy and paste the entire contents of `Azure_SQL_Setup.sql`
5. Click **"Run"** to create all tables

---

## Quick Check: Does Database Exist?

To check if database already exists:
1. Go to Azure Portal → SQL Server (`jussstzy`)
2. Click **"Databases"** in left menu
3. Look for `db_SoftWear` in the list

If it's there, skip to "After Database is Created" section above.












