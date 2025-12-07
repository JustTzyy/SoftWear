# Manual Azure SQL Setup Instructions

Since the automated script cannot connect (DNS resolution failed), please follow these manual steps:

## Step 1: Verify Server Name in Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your SQL Server resource
3. Check the **Server name** - it should be something like: `jussstzy.database.windows.net`
4. If the server name is different, update it in the connection strings

## Step 2: Configure Firewall Rules

1. In Azure Portal, go to your SQL Server resource
2. Click on **"Networking"** or **"Firewalls and virtual networks"**
3. Click **"Add client IPv4 address"** to add your current IP
4. Or enable **"Allow Azure services and resources to access this server"**
5. Click **"Save"**

## Step 3: Verify Server is Running

1. In Azure Portal, check if your SQL Server shows as **"Running"**
2. If it's paused, click **"Resume"** to start it
3. Wait a few minutes for the server to start

## Step 4: Setup Database Using SQL Server Management Studio (SSMS)

### Option A: Using SSMS (Recommended)

1. **Download and install SQL Server Management Studio (SSMS)** if you don't have it:
   - Download from: https://aka.ms/ssmsfullsetup

2. **Connect to Azure SQL Database:**
   - Server name: `jussstzy.database.windows.net` (or your actual server name)
   - Authentication: **SQL Server Authentication**
   - Login: `justin`
   - Password: `JussPogi27`
   - Click **"Connect"**

3. **Run the Setup Script:**
   - In SSMS, click **"File"** → **"Open"** → **"File"**
   - Navigate to: `C:\Github\SoftWear\IT13_Final\Migrations\Azure_SQL_Setup.sql`
   - Open the file
   - Click **"Execute"** (F5) or press **F5**

4. **Verify Setup:**
   - In Object Explorer, expand your server
   - Expand **"Databases"**
   - You should see `db_SoftWear` database
   - Expand `db_SoftWear` → **"Tables"**
   - You should see all the tables created

### Option B: Using Azure Portal Query Editor

1. Go to Azure Portal
2. Navigate to your SQL Database resource
3. Click **"Query editor"** in the left menu
4. Login with:
   - Login: `justin`
   - Password: `JussPogi27`
5. Copy and paste the contents of `Azure_SQL_Setup.sql`
6. Click **"Run"**

### Option C: Using Azure CLI

If you have Azure CLI installed:

```bash
az sql db create \
  --resource-group softwear-rg \
  --server jussstzy \
  --name db_SoftWear \
  --service-objective Basic

# Then run the SQL script using sqlcmd or Azure Portal Query Editor
```

## Step 5: Sync Data (Optional)

If you have existing data in your local database:

1. **Using SSMS:**
   - Connect to both local and Azure databases
   - Use SSMS **"Import/Export Data"** wizard
   - Or manually copy data using SQL scripts

2. **Using PowerShell Script:**
   - Once connection is working, run:
   ```powershell
   cd C:\Github\SoftWear\IT13_Final\Migrations
   .\Sync_Data_To_Azure.ps1
   ```

## Troubleshooting

### "Cannot connect to server"
- Verify server name is correct
- Check firewall rules allow your IP
- Ensure server is running (not paused)

### "Login failed"
- Verify username and password
- Check if SQL authentication is enabled
- Ensure user has proper permissions

### "Database does not exist"
- The setup script creates the database automatically
- If it fails, create it manually first:
  ```sql
  CREATE DATABASE db_SoftWear;
  ```

## Verify Connection String

After setup, verify your connection string in the code:
```
Server=jussstzy.database.windows.net;Database=db_SoftWear;User Id=justin;Password=JussPogi27;TrustServerCertificate=True;Connection Timeout=30;
```

**Note:** If your server name is different, update all connection strings in:
- `Services/Auth/AuthService.cs`
- `Services/Data/*.cs` (all data services)
- `Services/Audit/HistoryService.cs`

## Test Connection

You can test the connection using the test script:
```powershell
cd C:\Github\SoftWear\IT13_Final\Migrations
.\Test_Azure_Connection.ps1
```

Once the connection test passes, you can run the setup script:
```powershell
.\Setup_Azure_SQL.ps1
```












