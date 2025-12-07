# Azure SQL Database Setup Guide

This guide will help you connect your SoftWear application to Azure SQL Database and sync your data.

## Prerequisites

1. Azure SQL Database server: `jussstzy.database.windows.net`
2. Database name: `db_SoftWear`
3. Username: `justin`
4. Password: `JussPogi27`
5. PowerShell (for running setup scripts)
6. SQL Server Management Studio (SSMS) - Optional, for manual execution

## Step 1: Configure Azure SQL Firewall

Before connecting, you need to allow your IP address in Azure SQL Database firewall:

1. Go to Azure Portal
2. Navigate to your SQL Server resource
3. Go to "Networking" or "Firewalls and virtual networks"
4. Add your current IP address to the firewall rules
5. Or enable "Allow Azure services and resources to access this server" for testing

## Step 2: Create Database and Tables

### Option A: Using PowerShell Script (Recommended)

1. Open PowerShell
2. Navigate to the project directory
3. Run the setup script:

```powershell
cd Migrations
.\Setup_Azure_SQL.ps1
```

The script will:
- Connect to Azure SQL Database
- Create the `db_SoftWear` database if it doesn't exist
- Create all tables, indexes, constraints, and triggers
- Seed default roles and users

### Option B: Using SQL Server Management Studio (SSMS)

1. Open SQL Server Management Studio
2. Connect to: `jussstzy.database.windows.net`
   - Authentication: SQL Server Authentication
   - Login: `justin`
   - Password: `JussPogi27`
3. Open the file `Migrations\Azure_SQL_Setup.sql`
4. Execute the script (F5)

## Step 3: Sync Data from Local Database (Optional)

If you have existing data in your local SQL Server database that you want to copy to Azure:

1. Make sure your local SQL Server is running
2. Run the data sync script:

```powershell
cd Migrations
.\Sync_Data_To_Azure.ps1
```

The script will:
- Connect to both local and Azure databases
- Copy all data from local tables to Azure tables
- Skip tables that already have data in Azure (by default)
- Preserve all relationships and foreign keys

### Sync Script Parameters

You can customize the sync script with parameters:

```powershell
.\Sync_Data_To_Azure.ps1 `
    -LocalServer "localhost\SQLEXPRESS" `
    -LocalDatabase "db_SoftWear" `
    -AzureServer "jussstzy.database.windows.net" `
    -AzureDatabase "db_SoftWear" `
    -AzureUsername "justin" `
    -AzurePassword "JussPogi27" `
    -SkipExisting:$false  # Set to $false to overwrite existing data
```

## Step 4: Verify Connection

All service files have been updated with the Azure SQL connection string:

```
Server=jussstzy.database.windows.net;Database=db_SoftWear;User Id=justin;Password=JussPogi27;TrustServerCertificate=True;Connection Timeout=30;
```

You can test the connection by running your application.

## Default Users

After setup, the following default users are created:

- **Admin User**
  - Email: `admin@SoftWear.com`
  - Password: `admin123`
  - Role: Admin

- **Seller User**
  - Email: `seller@SoftWear.com`
  - Password: `seller123`
  - Role: Seller

## Troubleshooting

### Connection Timeout

If you get connection timeout errors:
1. Check Azure SQL firewall rules
2. Verify your IP address is allowed
3. Check if the SQL Server is running

### Authentication Failed

If authentication fails:
1. Verify username and password are correct
2. Check if the SQL Server authentication is enabled
3. Ensure the user has proper permissions

### Script Execution Errors

If PowerShell scripts fail:
1. Make sure you're running PowerShell as Administrator
2. Check execution policy: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`
3. Verify all required modules are installed

### Data Sync Issues

If data sync fails:
1. Verify both databases are accessible
2. Check that tables exist in both databases
3. Ensure foreign key relationships are maintained
4. Review error messages for specific table issues

## Connection String Format

All services now use this connection string format:

```
Server=jussstzy.database.windows.net;
Database=db_SoftWear;
User Id=justin;
Password=JussPogi27;
TrustServerCertificate=True;
Connection Timeout=30;
```

## Files Created

1. **Azure_SQL_Setup.sql** - Complete database schema and migration script
2. **Setup_Azure_SQL.ps1** - PowerShell script to automate setup
3. **Sync_Data_To_Azure.ps1** - PowerShell script to sync data from local to Azure

## Next Steps

1. Run the setup script to create the database and tables
2. (Optional) Sync existing data from local database
3. Test your application with the new Azure SQL connection
4. Update any environment-specific configurations if needed

## Security Notes

⚠️ **Important**: The password is currently hardcoded in connection strings. For production:

1. Use Azure Key Vault or secure configuration
2. Store credentials in environment variables
3. Use managed identities where possible
4. Enable Azure AD authentication for better security

## Support

If you encounter any issues:
1. Check Azure SQL Database logs
2. Review PowerShell script output for detailed error messages
3. Verify network connectivity to Azure
4. Check Azure SQL Database service status












