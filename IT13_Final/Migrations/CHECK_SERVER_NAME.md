# Check Your Exact Server Name

The connection string in Azure Portal shows `jusstzy` (single 's'), but we're using `jussstzy` (double 's').

## To Fix:

1. **Go to Azure Portal → SQL Database → `db_SoftWear`**
2. **Click "Connection strings" in the left menu**
3. **Select "ADO.NET (SQL authentication)"**
4. **Look at the Server name in the connection string**
   - Is it `tcp:jusstzy.database.windows.net` (single 's')?
   - Or `tcp:jussstzy.database.windows.net` (double 's')?

## Update the Code:

1. Open `Services/Data/DatabaseSyncService.cs`
2. Find line 51 (the `_azureConnectionString`)
3. If your server is `jusstzy` (single 's'), change:
   - `jussstzy` → `jusstzy`

## Current Connection String Format:

The connection string now uses Azure's recommended format:
- `Server=tcp:server,1433` (with tcp: prefix and port)
- `Initial Catalog=db_SoftWear` (not Database=)
- `User ID=justin` (not User Id=)
- `Encrypt=True;TrustServerCertificate=False`

This should match what SSMS uses when connecting successfully.












