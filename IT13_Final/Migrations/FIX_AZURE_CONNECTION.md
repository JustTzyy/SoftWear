# Fix Azure Connection Issue

If the database works in SSMS but not in your application, check these:

## 1. Verify Server Name

The server name might be different. Check in Azure Portal:
- Go to SQL Server resource
- Look at the exact "Server name" shown
- It might be:
  - `jussstzy.database.windows.net` (double 's')
  - `jusstzy.database.windows.net` (single 's')

## 2. Connection String Format

The connection string in `DatabaseSyncService.cs` now uses the Azure Portal format:
```
Server=tcp:jussstzy.database.windows.net,1433;Initial Catalog=db_SoftWear;Persist Security Info=False;User ID=justin;Password=JussPogi27;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;
```

## 3. If Server Name is Different

If your server name is `jusstzy` (single 's'), update the connection string:

1. Open `Services/Data/DatabaseSyncService.cs`
2. Find line with `_azureConnectionString`
3. Change `jussstzy` to `jusstzy` if needed

## 4. Test Connection

1. Go to Admin → Database Sync
2. Click "Test" on Azure Database
3. Should show green if connection works

## 5. Common Issues

- **DNS Resolution Failed**: Server name might be wrong
- **Connection Timeout**: Firewall might be blocking
- **Authentication Failed**: Username/password incorrect
- **Database Paused**: Resume database in Azure Portal

## Quick Fix

Copy the exact connection string from Azure Portal:
1. Go to Azure Portal → SQL Database → `db_SoftWear`
2. Click "Connection strings" in left menu
3. Select "ADO.NET (SQL authentication)"
4. Copy the connection string
5. Replace `{your_password}` with `JussPogi27`
6. Update `DatabaseSyncService.cs` with the exact string












