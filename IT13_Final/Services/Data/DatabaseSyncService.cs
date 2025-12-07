using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public interface IDatabaseSyncService
    {
        // Push: Local → Azure
        Task<SyncResult> SyncTableAsync(string tableName, CancellationToken ct = default);
        Task<SyncResult> SyncAllTablesAsync(CancellationToken ct = default);
        Task<SyncResult> SyncAllTablesAsync(IProgress<SyncProgress>? progress, CancellationToken ct = default);
        
        // Pull: Azure → Local
        Task<SyncResult> PullTableAsync(string tableName, CancellationToken ct = default);
        Task<SyncResult> PullAllTablesAsync(CancellationToken ct = default);
        Task<SyncResult> PullAllTablesAsync(IProgress<SyncProgress>? progress, CancellationToken ct = default);
        
        // Info
        Task<List<TableInfo>> GetLocalTableInfoAsync(CancellationToken ct = default);
        Task<List<TableInfo>> GetAzureTableInfoAsync(CancellationToken ct = default);
        Task<bool> TestLocalConnectionAsync(CancellationToken ct = default);
        Task<bool> TestAzureConnectionAsync(CancellationToken ct = default);
    }

    public class SyncProgress
    {
        public int CurrentTable { get; set; }
        public int TotalTables { get; set; }
        public string CurrentTableName { get; set; } = string.Empty;
        public int RowsSynced { get; set; }
        public int RowsSkipped { get; set; }
        public int Percentage => TotalTables > 0 ? (CurrentTable * 100 / TotalTables) : 0;
    }

    public class SyncResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RowsSynced { get; set; }
        public int RowsSkipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class TableInfo
    {
        public string TableName { get; set; } = string.Empty;
        public int RowCount { get; set; }
        public DateTime? LastSync { get; set; }
    }

    public sealed class DatabaseSyncService : IDatabaseSyncService
    {
        private readonly string _localConnectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=30;";

        // Azure connection string - using exact format from Azure Portal
        // Server name: jusstzy.database.windows.net (single 's')
        private readonly string _azureConnectionString =
            "Server=tcp:jusstzy.database.windows.net,1433;Initial Catalog=db_SoftWear;Persist Security Info=False;User ID=justin;Password=JussPogi27;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;";

        // Tables in dependency order
        private readonly string[] _tables = new[]
        {
            "tbl_roles",
            "tbl_users",
            "tbl_histories",
            "tbl_addresses",
            "tbl_colors",
            "tbl_sizes",
            "tbl_categories",
            "tbl_products",
            "tbl_variants",
            "tbl_variant_sizes",
            "tbl_variant_colors",
            "tbl_suppliers",
            "tbl_inventories",
            "tbl_stock_in",
            "tbl_stock_out",
            "tbl_stock_adjustments",
            "tbl_purchase_orders",
            "tbl_po_items",
            "tbl_sales",
            "tbl_sales_items",
            "tbl_payments",
            "tbl_returns",
            "tbl_return_items",
            "tbl_daily_sales_verifications",
            "tbl_expenses",
            "tbl_supplier_invoices",
            "tbl_supplier_payments"
        };

        public async Task<bool> TestLocalConnectionAsync(CancellationToken ct = default)
        {
            try
            {
                await using var conn = new SqlConnection(_localConnectionString);
                await conn.OpenAsync(ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TestAzureConnectionAsync(CancellationToken ct = default)
        {
            try
            {
                await using var conn = new SqlConnection(_azureConnectionString);
                await conn.OpenAsync(ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<TableInfo>> GetLocalTableInfoAsync(CancellationToken ct = default)
        {
            var tables = new List<TableInfo>();
            
            try
            {
                await using var conn = new SqlConnection(_localConnectionString);
                await conn.OpenAsync(ct);

                foreach (var table in _tables)
                {
                    try
                    {
                        var countSql = $"SELECT COUNT(*) FROM dbo.{table}";
                        await using var cmd = new SqlCommand(countSql, conn);
                        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
                        
                        tables.Add(new TableInfo
                        {
                            TableName = table,
                            RowCount = count
                        });
                    }
                    catch
                    {
                        // Table might not exist
                        tables.Add(new TableInfo
                        {
                            TableName = table,
                            RowCount = 0
                        });
                    }
                }
            }
            catch
            {
                // Connection failed
            }

            return tables;
        }

        public async Task<List<TableInfo>> GetAzureTableInfoAsync(CancellationToken ct = default)
        {
            var tables = new List<TableInfo>();
            
            try
            {
                await using var conn = new SqlConnection(_azureConnectionString);
                await conn.OpenAsync(ct);

                foreach (var table in _tables)
                {
                    try
                    {
                        var countSql = $"SELECT COUNT(*) FROM dbo.{table}";
                        await using var cmd = new SqlCommand(countSql, conn);
                        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
                        
                        tables.Add(new TableInfo
                        {
                            TableName = table,
                            RowCount = count
                        });
                    }
                    catch
                    {
                        // Table might not exist
                        tables.Add(new TableInfo
                        {
                            TableName = table,
                            RowCount = 0
                        });
                    }
                }
            }
            catch
            {
                // Connection failed
            }

            return tables;
        }

        public async Task<SyncResult> SyncTableAsync(string tableName, CancellationToken ct = default)
        {
            var result = new SyncResult { Success = false };

            try
            {
                // Connect to local database
                await using var localConn = new SqlConnection(_localConnectionString);
                await localConn.OpenAsync(ct);

                // Check if table exists in local
                var checkTableSql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
                await using var checkCmd = new SqlCommand(checkTableSql, localConn);
                checkCmd.Parameters.AddWithValue("@tableName", tableName);
                var tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct)) > 0;

                if (!tableExists)
                {
                    result.Message = $"Table {tableName} does not exist in local database.";
                    return result;
                }

                // Get row count from local
                var countSql = $"SELECT COUNT(*) FROM dbo.{tableName}";
                await using var countCmd = new SqlCommand(countSql, localConn);
                var rowCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct));

                if (rowCount == 0)
                {
                    result.Success = true;
                    result.Message = $"Table {tableName} has no data to sync.";
                    return result;
                }

                // Get column data types from local database (to identify binary columns)
                var columnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    var typeSql = @"
                        SELECT 
                            COLUMN_NAME,
                            DATA_TYPE
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = @tableName AND TABLE_SCHEMA = 'dbo'
                        ORDER BY ORDINAL_POSITION";
                    await using var typeCmd = new SqlCommand(typeSql, localConn);
                    typeCmd.Parameters.AddWithValue("@tableName", tableName);
                    await using var typeReader = await typeCmd.ExecuteReaderAsync(ct);
                    while (await typeReader.ReadAsync(ct))
                    {
                        var colName = typeReader.GetString(0);
                        var dataType = typeReader.GetString(1);
                        columnTypes[colName] = dataType;
                    }
                    await typeReader.CloseAsync();
                }
                catch
                {
                    // If we can't get column types, continue without this info
                }

                // Read all data from local table
                // For tables with archived_at or archives column, only sync non-archived records
                string selectSql;
                try
                {
                    // Check if table has archived_at or archives column
                    var checkArchivedSql = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = @tableName 
                        AND TABLE_SCHEMA = 'dbo'
                        AND (COLUMN_NAME = 'archived_at' OR COLUMN_NAME = 'archives')";
                    await using var checkArchivedCmd = new SqlCommand(checkArchivedSql, localConn);
                    checkArchivedCmd.Parameters.AddWithValue("@tableName", tableName);
                    var hasArchivedColumn = Convert.ToInt32(await checkArchivedCmd.ExecuteScalarAsync(ct)) > 0;
                    
                    if (hasArchivedColumn)
                    {
                        // Check which archived column exists
                        var checkArchivedAtSql = @"
                            SELECT COUNT(*) 
                            FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = @tableName 
                            AND TABLE_SCHEMA = 'dbo'
                            AND COLUMN_NAME = 'archived_at'";
                        await using var checkArchivedAtCmd = new SqlCommand(checkArchivedAtSql, localConn);
                        checkArchivedAtCmd.Parameters.AddWithValue("@tableName", tableName);
                        var hasArchivedAt = Convert.ToInt32(await checkArchivedAtCmd.ExecuteScalarAsync(ct)) > 0;
                        
                        if (hasArchivedAt)
                        {
                            selectSql = $"SELECT * FROM dbo.{tableName} WHERE archived_at IS NULL";
                        }
                        else
                        {
                            selectSql = $"SELECT * FROM dbo.{tableName} WHERE archives IS NULL";
                        }
                    }
                    else
                    {
                        selectSql = $"SELECT * FROM dbo.{tableName}";
                    }
                }
                catch
                {
                    // If we can't check, use all rows
                    selectSql = $"SELECT * FROM dbo.{tableName}";
                }
                
                var adapter = new SqlDataAdapter(selectSql, localConn);
                var dataSet = new DataSet();
                adapter.Fill(dataSet, tableName);
                var dataTable = dataSet.Tables[tableName];

                if (dataTable.Rows.Count == 0)
                {
                    result.Success = true;
                    result.Message = $"No data to sync from {tableName}.";
                    return result;
                }

                // Connect to Azure database
                await using var azureConn = new SqlConnection(_azureConnectionString);
                await azureConn.OpenAsync(ct);

                // Get column names (exclude IDENTITY columns for insert)
                var allColumns = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                
                // Check which columns are IDENTITY in Azure
                var identityColumns = new List<string>();
                try
                {
                    var identitySql = @"
                        SELECT COLUMN_NAME 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = @tableName 
                        AND TABLE_SCHEMA = 'dbo'
                        AND COLUMNPROPERTY(OBJECT_ID('dbo.' + @tableName), COLUMN_NAME, 'IsIdentity') = 1";
                    await using var identityCmd = new SqlCommand(identitySql, azureConn);
                    identityCmd.Parameters.AddWithValue("@tableName", tableName);
                    await using var identityReader = await identityCmd.ExecuteReaderAsync(ct);
                    while (await identityReader.ReadAsync(ct))
                    {
                        identityColumns.Add(identityReader.GetString(0));
                    }
                    await identityReader.CloseAsync();
                }
                catch
                {
                    // If we can't determine identity columns, assume 'id' is identity
                    if (allColumns.Contains("id", StringComparer.OrdinalIgnoreCase))
                    {
                        identityColumns.Add("id");
                    }
                }

                // Get existing primary key values from Azure to avoid duplicates
                var existingKeys = new HashSet<string>();
                string? primaryKeyColumn = null;
                
                try
                {
                    // Try to get primary key column
                    var pkSql = @"
                        SELECT COLUMN_NAME 
                        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                        WHERE TABLE_NAME = @tableName 
                        AND TABLE_SCHEMA = 'dbo'
                        AND CONSTRAINT_NAME LIKE 'PK_%'
                        ORDER BY ORDINAL_POSITION";
                    await using var pkCmd = new SqlCommand(pkSql, azureConn);
                    pkCmd.Parameters.AddWithValue("@tableName", tableName);
                    await using var pkReader = await pkCmd.ExecuteReaderAsync(ct);
                    var pkColumns = new List<string>();
                    while (await pkReader.ReadAsync(ct))
                    {
                        pkColumns.Add(pkReader.GetString(0));
                    }
                    await pkReader.CloseAsync();

                    // Use first primary key column (most tables have single-column PK)
                    if (pkColumns.Any())
                    {
                        primaryKeyColumn = pkColumns[0];
                        
                        // Get existing primary key values
                        var existingSql = $"SELECT [{primaryKeyColumn}] FROM dbo.{tableName}";
                        await using var existingCmd = new SqlCommand(existingSql, azureConn);
                        await using var existingReader = await existingCmd.ExecuteReaderAsync(ct);
                        while (await existingReader.ReadAsync(ct))
                        {
                            if (!existingReader.IsDBNull(0))
                            {
                                var keyValue = existingReader.GetValue(0).ToString();
                                if (keyValue != null)
                                {
                                    existingKeys.Add(keyValue);
                                }
                            }
                        }
                        await existingReader.CloseAsync();
                    }
                    else
                    {
                        // Fallback: assume 'id' is the primary key
                        if (allColumns.Contains("id", StringComparer.OrdinalIgnoreCase))
                        {
                            primaryKeyColumn = allColumns.First(c => c.Equals("id", StringComparison.OrdinalIgnoreCase));
                            var existingSql = $"SELECT [id] FROM dbo.{tableName}";
                            await using var existingCmd = new SqlCommand(existingSql, azureConn);
                            await using var existingReader = await existingCmd.ExecuteReaderAsync(ct);
                            while (await existingReader.ReadAsync(ct))
                            {
                                if (!existingReader.IsDBNull(0))
                                {
                                    var keyValue = existingReader.GetValue(0).ToString();
                                    if (keyValue != null)
                                    {
                                        existingKeys.Add(keyValue);
                                    }
                                }
                            }
                            await existingReader.CloseAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If we can't get primary keys, continue without duplicate checking
                    result.Errors.Add($"Warning: Could not check existing keys for {tableName}: {ex.Message}");
                }

                // Use all columns for insert (including IDENTITY columns)
                var insertColumns = allColumns.ToList();
                var columnList = string.Join(", ", insertColumns.Select(c => $"[{c}]"));
                var parameterList = string.Join(", ", insertColumns.Select(c => $"@{c}"));

                // Check if we need IDENTITY_INSERT
                bool needsIdentityInsert = identityColumns.Any();

                int insertedRows = 0;
                int skippedRows = 0;
                
                // Track which specific rows are being skipped for better reporting
                var skippedRowDetails = new List<string>();

                // Enable IDENTITY_INSERT if needed (once for all inserts)
                if (needsIdentityInsert)
                {
                    var enableSql = $"SET IDENTITY_INSERT dbo.{tableName} ON;";
                    await using var enableCmd = new SqlCommand(enableSql, azureConn);
                    await enableCmd.ExecuteNonQueryAsync(ct);
                }

                try
                {
                    var insertSql = $"INSERT INTO dbo.{tableName} ({columnList}) VALUES ({parameterList})";

                    foreach (DataRow row in dataTable.Rows)
                    {
                        try
                        {
                            // Check if this row already exists (by primary key)
                            bool shouldSkip = false;
                            string rowInfo = "";
                            if (existingKeys.Count > 0 && !string.IsNullOrEmpty(primaryKeyColumn))
                            {
                                if (allColumns.Contains(primaryKeyColumn))
                                {
                                    var pkValue = row[primaryKeyColumn];
                                    if (pkValue != DBNull.Value)
                                    {
                                        var rowKey = pkValue.ToString();
                                        if (rowKey != null && existingKeys.Contains(rowKey))
                                        {
                                            skippedRows++;
                                            shouldSkip = true;
                                            // Track which specific row was skipped
                                            rowInfo = $"{primaryKeyColumn}={rowKey}";
                                            // Try to get a name or identifier column for better reporting
                                            if (allColumns.Contains("name") && row["name"] != DBNull.Value)
                                            {
                                                rowInfo += $" (name='{row["name"]}')";
                                            }
                                        }
                                    }
                                }
                            }

                            if (shouldSkip)
                            {
                                if (!string.IsNullOrEmpty(rowInfo))
                                {
                                    skippedRowDetails.Add(rowInfo);
                                }
                                continue;
                            }

                            await using var insertCmd = new SqlCommand(insertSql, azureConn);
                            
                            foreach (var column in insertColumns)
                            {
                                var value = row[column];
                                
                                // Check if this is a binary column based on database schema
                                // Also check by column name (common binary column names)
                                bool isBinaryColumn = false;
                                if (columnTypes.TryGetValue(column, out var dbDataType))
                                {
                                    isBinaryColumn = dbDataType.Equals("varbinary", StringComparison.OrdinalIgnoreCase) ||
                                                     dbDataType.Equals("binary", StringComparison.OrdinalIgnoreCase) ||
                                                     dbDataType.Equals("image", StringComparison.OrdinalIgnoreCase);
                                }
                                
                                // Fallback 1: check by column name (common binary column names)
                                if (!isBinaryColumn && (
                                    column.Equals("image", StringComparison.OrdinalIgnoreCase) ||
                                    column.Equals("photo", StringComparison.OrdinalIgnoreCase) ||
                                    column.Equals("picture", StringComparison.OrdinalIgnoreCase) ||
                                    column.Equals("data", StringComparison.OrdinalIgnoreCase) ||
                                    column.Equals("content", StringComparison.OrdinalIgnoreCase) ||
                                    column.EndsWith("_image", StringComparison.OrdinalIgnoreCase) ||
                                    column.EndsWith("_data", StringComparison.OrdinalIgnoreCase)))
                                {
                                    isBinaryColumn = true;
                                }
                                
                                // Fallback 2: check DataTable column DataType
                                if (!isBinaryColumn && dataTable.Columns.Contains(column))
                                {
                                    var colType = dataTable.Columns[column].DataType;
                                    isBinaryColumn = colType == typeof(byte[]) || 
                                                     colType == typeof(System.Data.SqlTypes.SqlBinary);
                                }
                                
                                if (value == DBNull.Value)
                                {
                                    if (isBinaryColumn)
                                    {
                                        // Even for NULL, use proper binary parameter type
                                        var binaryParam = new SqlParameter($"@{column}", SqlDbType.VarBinary, -1);
                                        binaryParam.Value = DBNull.Value;
                                        insertCmd.Parameters.Add(binaryParam);
                                    }
                                    else
                                    {
                                        insertCmd.Parameters.AddWithValue($"@{column}", DBNull.Value);
                                    }
                                }
                                else
                                {
                                    if (isBinaryColumn)
                                    {
                                        // Handle binary data properly
                                        byte[]? byteValue = null;
                                        
                                        if (value is byte[] bytes)
                                        {
                                            byteValue = bytes;
                                        }
                                        else if (value is string strValue && !string.IsNullOrEmpty(strValue))
                                        {
                                            // SqlDataAdapter might read VARBINARY as string (hex representation)
                                            // Try to convert from hex string
                                            try
                                            {
                                                // Remove spaces and convert hex string to bytes
                                                var hexString = strValue.Replace(" ", "").Replace("-", "");
                                                if (hexString.Length % 2 == 0)
                                                {
                                                    byteValue = new byte[hexString.Length / 2];
                                                    for (int i = 0; i < byteValue.Length; i++)
                                                    {
                                                        byteValue[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                                                    }
                                                }
                                                else
                                                {
                                                    // If not hex, try base64
                                                    byteValue = Convert.FromBase64String(strValue);
                                                }
                                            }
                                            catch
                                            {
                                                // If conversion fails, use UTF8 encoding as last resort
                                                byteValue = System.Text.Encoding.UTF8.GetBytes(strValue);
                                            }
                                        }
                                        else if (value != null)
                                        {
                                            // Try direct cast or conversion
                                            try
                                            {
                                                if (value is System.Data.SqlTypes.SqlBinary sqlBinary)
                                                {
                                                    byteValue = sqlBinary.Value;
                                                }
                                                else
                                                {
                                                    // Try to convert using Convert.ChangeType or direct cast
                                                    var valueType = value.GetType();
                                                    if (valueType == typeof(byte[]))
                                                    {
                                                        byteValue = (byte[])value;
                                                    }
                                                    else
                                                    {
                                                        // If we can't convert, log error and skip
                                                        result.Errors.Add($"Cannot convert {column} value from {valueType.Name} to byte[] for product ID {row[primaryKeyColumn ?? "id"]}");
                                                        byteValue = null; // Will be set to DBNull
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // If all else fails, log and use null
                                                result.Errors.Add($"Error converting {column} to byte[]: {ex.Message}");
                                                byteValue = null;
                                            }
                                        }
                                        
                                        // Create proper VARBINARY parameter
                                        var binaryParam = new SqlParameter($"@{column}", SqlDbType.VarBinary, -1);
                                        binaryParam.Value = (object?)byteValue ?? DBNull.Value;
                                        insertCmd.Parameters.Add(binaryParam);
                                    }
                                    else
                                    {
                                        // For non-binary columns, use AddWithValue
                                        insertCmd.Parameters.AddWithValue($"@{column}", value);
                                    }
                                }
                            }

                            await insertCmd.ExecuteNonQueryAsync(ct);
                            insertedRows++;
                        }
                        catch (SqlException ex)
                        {
                            // Skip duplicate key errors or other constraint violations
                            if (ex.Number == 2627 || ex.Number == 2601) // Primary key or unique constraint violation
                            {
                                skippedRows++;
                                // Add detail about which row was skipped
                                if (primaryKeyColumn != null && allColumns.Contains(primaryKeyColumn))
                                {
                                    var pkValue = row[primaryKeyColumn];
                                    result.Errors.Add($"Skipped duplicate {tableName} with {primaryKeyColumn}={pkValue} (already exists in Azure)");
                                }
                                else
                                {
                                    result.Errors.Add($"Skipped duplicate row in {tableName} (already exists in Azure)");
                                }
                            }
                            else if (ex.Number == 544) // Cannot insert explicit value for identity column
                            {
                                skippedRows++;
                                result.Errors.Add($"Skipped row with identity conflict in {tableName}: {ex.Message}");
                            }
                            else if (ex.Number == 547) // Foreign key constraint violation
                            {
                                skippedRows++;
                                result.Errors.Add($"Skipped row in {tableName} due to foreign key constraint: {ex.Message}");
                            }
                            else
                            {
                                result.Errors.Add($"Error inserting row in {tableName}: {ex.Message} (Error #{ex.Number})");
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Error inserting row in {tableName}: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    // Disable IDENTITY_INSERT if it was enabled
                    if (needsIdentityInsert)
                    {
                        var disableSql = $"SET IDENTITY_INSERT dbo.{tableName} OFF;";
                        await using var disableCmd = new SqlCommand(disableSql, azureConn);
                        await disableCmd.ExecuteNonQueryAsync(ct);
                    }
                }

                result.Success = true;
                result.RowsSynced = insertedRows;
                result.RowsSkipped = skippedRows;
                
                // Build detailed message about skipped rows
                if (skippedRows > 0)
                {
                    var skipDetails = new List<string>();
                    
                    // Add skipped row details (from duplicate check)
                    if (skippedRowDetails.Any())
                    {
                        var uniqueSkips = skippedRowDetails.Distinct().Take(5).ToList();
                        skipDetails.Add($"Skipped existing: {string.Join(", ", uniqueSkips)}");
                        if (skippedRowDetails.Count > 5)
                        {
                            skipDetails.Add($"... and {skippedRowDetails.Count - 5} more");
                        }
                    }
                    
                    // Add error details (from insert failures)
                    if (result.Errors.Any())
                    {
                        var errorDetails = result.Errors.Take(3).ToList();
                        skipDetails.Add($"Errors: {string.Join("; ", errorDetails)}");
                    }
                    
                    if (skipDetails.Any())
                    {
                        result.Message = $"Synced {insertedRows} rows from {tableName}. {skippedRows} rows skipped. " +
                                       string.Join(". ", skipDetails);
                    }
                    else
                    {
                        result.Message = $"Synced {insertedRows} rows from {tableName}. {skippedRows} rows skipped (duplicates).";
                    }
                }
                else
                {
                    result.Message = $"Synced {insertedRows} rows from {tableName}.";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error syncing {tableName}: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }


        public async Task<SyncResult> SyncAllTablesAsync(CancellationToken ct = default)
        {
            return await SyncAllTablesAsync(null, ct);
        }

        public async Task<SyncResult> SyncAllTablesAsync(IProgress<SyncProgress>? progress, CancellationToken ct = default)
        {
            var result = new SyncResult 
            { 
                Success = true,
                Message = "Starting full database sync...",
                Errors = new List<string>()
            };

            int totalSynced = 0;
            int totalSkipped = 0;
            int tablesProcessed = 0;

            for (int i = 0; i < _tables.Length; i++)
            {
                var table = _tables[i];
                
                // Report progress
                progress?.Report(new SyncProgress
                {
                    CurrentTable = i + 1,
                    TotalTables = _tables.Length,
                    CurrentTableName = table,
                    RowsSynced = totalSynced,
                    RowsSkipped = totalSkipped
                });

                try
                {
                    var tableResult = await SyncTableAsync(table, ct);
                    
                    if (tableResult.Success)
                    {
                        totalSynced += tableResult.RowsSynced;
                        totalSkipped += tableResult.RowsSkipped;
                        tablesProcessed++;
                    }
                    else
                    {
                        result.Errors.AddRange(tableResult.Errors);
                        if (tableResult.Errors.Count > 0)
                        {
                            result.Success = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add($"Error processing {table}: {ex.Message}");
                }
            }

            // Final progress report
            progress?.Report(new SyncProgress
            {
                CurrentTable = _tables.Length,
                TotalTables = _tables.Length,
                CurrentTableName = "Completed",
                RowsSynced = totalSynced,
                RowsSkipped = totalSkipped
            });

            result.RowsSynced = totalSynced;
            result.RowsSkipped = totalSkipped;
            result.Message = $"Sync completed. Processed {tablesProcessed}/{_tables.Length} tables. Synced {totalSynced} rows, skipped {totalSkipped} rows.";

            return result;
        }

        // ============================================
        // PULL METHODS: Azure → Local
        // ============================================

        public async Task<SyncResult> PullTableAsync(string tableName, CancellationToken ct = default)
        {
            var result = new SyncResult { Success = false };

            try
            {
                // Connect to Azure database (source)
                await using var azureConn = new SqlConnection(_azureConnectionString);
                await azureConn.OpenAsync(ct);

                // Check if table exists in Azure
                var checkTableSql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
                await using var checkCmd = new SqlCommand(checkTableSql, azureConn);
                checkCmd.Parameters.AddWithValue("@tableName", tableName);
                var tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct)) > 0;

                if (!tableExists)
                {
                    result.Message = $"Table {tableName} does not exist in Azure database.";
                    return result;
                }

                // Get row count from Azure
                var countSql = $"SELECT COUNT(*) FROM dbo.{tableName}";
                await using var countCmd = new SqlCommand(countSql, azureConn);
                var rowCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct));

                if (rowCount == 0)
                {
                    result.Success = true;
                    result.Message = $"Table {tableName} has no data to pull from Azure.";
                    return result;
                }

                // Get column data types from Azure database (to identify binary columns)
                var columnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    var typeSql = @"
                        SELECT 
                            COLUMN_NAME,
                            DATA_TYPE
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = @tableName AND TABLE_SCHEMA = 'dbo'
                        ORDER BY ORDINAL_POSITION";
                    await using var typeCmd = new SqlCommand(typeSql, azureConn);
                    typeCmd.Parameters.AddWithValue("@tableName", tableName);
                    await using var typeReader = await typeCmd.ExecuteReaderAsync(ct);
                    while (await typeReader.ReadAsync(ct))
                    {
                        var colName = typeReader.GetString(0);
                        var dataType = typeReader.GetString(1);
                        columnTypes[colName] = dataType;
                    }
                    await typeReader.CloseAsync();
                }
                catch
                {
                    // If we can't get column types, continue without this info
                }

                // Read all data from Azure table (same logic as push for archived filtering)
                string selectSql;
                try
                {
                    var checkArchivedSql = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = @tableName 
                        AND TABLE_SCHEMA = 'dbo'
                        AND (COLUMN_NAME = 'archived_at' OR COLUMN_NAME = 'archives')";
                    await using var checkArchivedCmd = new SqlCommand(checkArchivedSql, azureConn);
                    checkArchivedCmd.Parameters.AddWithValue("@tableName", tableName);
                    var hasArchivedColumn = Convert.ToInt32(await checkArchivedCmd.ExecuteScalarAsync(ct)) > 0;
                    
                    if (hasArchivedColumn)
                    {
                        var checkArchivedAtSql = @"
                            SELECT COUNT(*) 
                            FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = @tableName 
                            AND TABLE_SCHEMA = 'dbo'
                            AND COLUMN_NAME = 'archived_at'";
                        await using var checkArchivedAtCmd = new SqlCommand(checkArchivedAtSql, azureConn);
                        checkArchivedAtCmd.Parameters.AddWithValue("@tableName", tableName);
                        var hasArchivedAt = Convert.ToInt32(await checkArchivedAtCmd.ExecuteScalarAsync(ct)) > 0;
                        
                        if (hasArchivedAt)
                        {
                            selectSql = $"SELECT * FROM dbo.{tableName} WHERE archived_at IS NULL";
                        }
                        else
                        {
                            selectSql = $"SELECT * FROM dbo.{tableName} WHERE archives IS NULL";
                        }
                    }
                    else
                    {
                        selectSql = $"SELECT * FROM dbo.{tableName}";
                    }
                }
                catch
                {
                    selectSql = $"SELECT * FROM dbo.{tableName}";
                }
                
                var adapter = new SqlDataAdapter(selectSql, azureConn);
                var dataSet = new DataSet();
                adapter.Fill(dataSet, tableName);
                var dataTable = dataSet.Tables[tableName];

                if (dataTable.Rows.Count == 0)
                {
                    result.Success = true;
                    result.Message = $"No data to pull from {tableName}.";
                    return result;
                }

                // Connect to Local database (destination)
                await using var localConn = new SqlConnection(_localConnectionString);
                await localConn.OpenAsync(ct);

                // Get column names
                var allColumns = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                
                // Check which columns are IDENTITY in Local
                var identityColumns = new List<string>();
                try
                {
                    var identitySql = @"
                        SELECT COLUMN_NAME
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = @tableName 
                        AND TABLE_SCHEMA = 'dbo'
                        AND COLUMNPROPERTY(OBJECT_ID('dbo.' + @tableName), COLUMN_NAME, 'IsIdentity') = 1";
                    await using var identityCmd = new SqlCommand(identitySql, localConn);
                    identityCmd.Parameters.AddWithValue("@tableName", tableName);
                    await using var identityReader = await identityCmd.ExecuteReaderAsync(ct);
                    while (await identityReader.ReadAsync(ct))
                    {
                        identityColumns.Add(identityReader.GetString(0));
                    }
                    await identityReader.CloseAsync();
                }
                catch { }

                // Get existing primary keys from Local to avoid duplicates
                HashSet<string> existingKeys = new HashSet<string>();
                string? primaryKeyColumn = null;
                try
                {
                    var pkSql = @"
                        SELECT COLUMN_NAME
                        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                        WHERE TABLE_NAME = @tableName 
                        AND TABLE_SCHEMA = 'dbo'
                        AND CONSTRAINT_NAME LIKE 'PK_%'";
                    await using var pkCmd = new SqlCommand(pkSql, localConn);
                    pkCmd.Parameters.AddWithValue("@tableName", tableName);
                    await using var pkReader = await pkCmd.ExecuteReaderAsync(ct);
                    var pkColumns = new List<string>();
                    while (await pkReader.ReadAsync(ct))
                    {
                        pkColumns.Add(pkReader.GetString(0));
                    }
                    await pkReader.CloseAsync();

                    if (pkColumns.Count > 0)
                    {
                        primaryKeyColumn = pkColumns[0];
                        var existingSql = $"SELECT [{primaryKeyColumn}] FROM dbo.{tableName}";
                        await using var existingCmd = new SqlCommand(existingSql, localConn);
                        await using var existingReader = await existingCmd.ExecuteReaderAsync(ct);
                        while (await existingReader.ReadAsync(ct))
                        {
                            if (!existingReader.IsDBNull(0))
                            {
                                var keyValue = existingReader.GetValue(0).ToString();
                                if (keyValue != null)
                                {
                                    existingKeys.Add(keyValue);
                                }
                            }
                        }
                        await existingReader.CloseAsync();
                    }
                    else if (allColumns.Contains("id", StringComparer.OrdinalIgnoreCase))
                    {
                        primaryKeyColumn = allColumns.First(c => c.Equals("id", StringComparison.OrdinalIgnoreCase));
                        var existingSql = $"SELECT [id] FROM dbo.{tableName}";
                        await using var existingCmd = new SqlCommand(existingSql, localConn);
                        await using var existingReader = await existingCmd.ExecuteReaderAsync(ct);
                        while (await existingReader.ReadAsync(ct))
                        {
                            if (!existingReader.IsDBNull(0))
                            {
                                var keyValue = existingReader.GetValue(0).ToString();
                                if (keyValue != null)
                                {
                                    existingKeys.Add(keyValue);
                                }
                            }
                        }
                        await existingReader.CloseAsync();
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Warning: Could not check existing keys for {tableName}: {ex.Message}");
                }

                // Use all columns for insert (including IDENTITY columns)
                var insertColumns = allColumns.ToList();
                var columnList = string.Join(", ", insertColumns.Select(c => $"[{c}]"));
                var parameterList = string.Join(", ", insertColumns.Select(c => $"@{c}"));

                bool needsIdentityInsert = identityColumns.Any();
                int insertedRows = 0;
                int skippedRows = 0;
                var skippedRowDetails = new List<string>();

                // Enable IDENTITY_INSERT if needed
                if (needsIdentityInsert)
                {
                    var enableSql = $"SET IDENTITY_INSERT dbo.{tableName} ON;";
                    await using var enableCmd = new SqlCommand(enableSql, localConn);
                    await enableCmd.ExecuteNonQueryAsync(ct);
                }

                try
                {
                    var insertSql = $"INSERT INTO dbo.{tableName} ({columnList}) VALUES ({parameterList})";

                    foreach (DataRow row in dataTable.Rows)
                    {
                        try
                        {
                            // Check if this row already exists
                            bool shouldSkip = false;
                            string rowInfo = "";
                            if (existingKeys.Count > 0 && !string.IsNullOrEmpty(primaryKeyColumn))
                            {
                                if (allColumns.Contains(primaryKeyColumn))
                                {
                                    var pkValue = row[primaryKeyColumn];
                                    if (pkValue != DBNull.Value)
                                    {
                                        var rowKey = pkValue.ToString();
                                        if (rowKey != null && existingKeys.Contains(rowKey))
                                        {
                                            skippedRows++;
                                            shouldSkip = true;
                                            rowInfo = $"{primaryKeyColumn}={rowKey}";
                                            if (allColumns.Contains("name") && row["name"] != DBNull.Value)
                                            {
                                                rowInfo += $" (name='{row["name"]}')";
                                            }
                                        }
                                    }
                                }
                            }

                            if (shouldSkip)
                            {
                                if (!string.IsNullOrEmpty(rowInfo))
                                {
                                    skippedRowDetails.Add(rowInfo);
                                }
                                continue;
                            }

                            await using var insertCmd = new SqlCommand(insertSql, localConn);
                            
                            foreach (var column in insertColumns)
                            {
                                var value = row[column];
                                
                                // Check if this is a binary column (same logic as push)
                                bool isBinaryColumn = false;
                                if (columnTypes.TryGetValue(column, out var dbDataType))
                                {
                                    isBinaryColumn = dbDataType.Equals("varbinary", StringComparison.OrdinalIgnoreCase) ||
                                                     dbDataType.Equals("binary", StringComparison.OrdinalIgnoreCase) ||
                                                     dbDataType.Equals("image", StringComparison.OrdinalIgnoreCase);
                                }
                                
                                if (!isBinaryColumn && (
                                    column.Equals("image", StringComparison.OrdinalIgnoreCase) ||
                                    column.Equals("photo", StringComparison.OrdinalIgnoreCase) ||
                                    column.Equals("picture", StringComparison.OrdinalIgnoreCase) ||
                                    column.Equals("data", StringComparison.OrdinalIgnoreCase) ||
                                    column.Equals("content", StringComparison.OrdinalIgnoreCase) ||
                                    column.EndsWith("_image", StringComparison.OrdinalIgnoreCase) ||
                                    column.EndsWith("_data", StringComparison.OrdinalIgnoreCase)))
                                {
                                    isBinaryColumn = true;
                                }
                                
                                if (!isBinaryColumn && dataTable.Columns.Contains(column))
                                {
                                    var colType = dataTable.Columns[column].DataType;
                                    isBinaryColumn = colType == typeof(byte[]) || 
                                                     colType == typeof(System.Data.SqlTypes.SqlBinary);
                                }
                                
                                if (value == DBNull.Value)
                                {
                                    if (isBinaryColumn)
                                    {
                                        var binaryParam = new SqlParameter($"@{column}", SqlDbType.VarBinary, -1);
                                        binaryParam.Value = DBNull.Value;
                                        insertCmd.Parameters.Add(binaryParam);
                                    }
                                    else
                                    {
                                        insertCmd.Parameters.AddWithValue($"@{column}", DBNull.Value);
                                    }
                                }
                                else
                                {
                                    if (isBinaryColumn)
                                    {
                                        byte[]? byteValue = null;
                                        
                                        if (value is byte[] bytes)
                                        {
                                            byteValue = bytes;
                                        }
                                        else if (value is string strValue && !string.IsNullOrEmpty(strValue))
                                        {
                                            try
                                            {
                                                var hexString = strValue.Replace(" ", "").Replace("-", "");
                                                if (hexString.Length % 2 == 0)
                                                {
                                                    byteValue = new byte[hexString.Length / 2];
                                                    for (int i = 0; i < byteValue.Length; i++)
                                                    {
                                                        byteValue[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                                                    }
                                                }
                                                else
                                                {
                                                    byteValue = Convert.FromBase64String(strValue);
                                                }
                                            }
                                            catch
                                            {
                                                byteValue = System.Text.Encoding.UTF8.GetBytes(strValue);
                                            }
                                        }
                                        else if (value != null)
                                        {
                                            try
                                            {
                                                if (value is System.Data.SqlTypes.SqlBinary sqlBinary)
                                                {
                                                    byteValue = sqlBinary.Value;
                                                }
                                                else if (value.GetType() == typeof(byte[]))
                                                {
                                                    byteValue = (byte[])value;
                                                }
                                                else
                                                {
                                                    result.Errors.Add($"Cannot convert {column} value from {value.GetType().Name} to byte[]");
                                                    byteValue = null;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                result.Errors.Add($"Error converting {column} to byte[]: {ex.Message}");
                                                byteValue = null;
                                            }
                                        }
                                        
                                        var binaryParam = new SqlParameter($"@{column}", SqlDbType.VarBinary, -1);
                                        binaryParam.Value = (object?)byteValue ?? DBNull.Value;
                                        insertCmd.Parameters.Add(binaryParam);
                                    }
                                    else
                                    {
                                        insertCmd.Parameters.AddWithValue($"@{column}", value);
                                    }
                                }
                            }

                            await insertCmd.ExecuteNonQueryAsync(ct);
                            insertedRows++;
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Number == 2627 || ex.Number == 2601)
                            {
                                skippedRows++;
                                if (primaryKeyColumn != null && allColumns.Contains(primaryKeyColumn))
                                {
                                    var pkValue = row[primaryKeyColumn];
                                    result.Errors.Add($"Skipped duplicate {tableName} with {primaryKeyColumn}={pkValue} (already exists in Local)");
                                }
                            }
                            else if (ex.Number == 544 || ex.Number == 547)
                            {
                                skippedRows++;
                                result.Errors.Add($"Skipped row in {tableName}: {ex.Message}");
                            }
                            else
                            {
                                result.Errors.Add($"Error inserting row in {tableName}: {ex.Message} (Error #{ex.Number})");
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Error inserting row in {tableName}: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    if (needsIdentityInsert)
                    {
                        var disableSql = $"SET IDENTITY_INSERT dbo.{tableName} OFF;";
                        await using var disableCmd = new SqlCommand(disableSql, localConn);
                        await disableCmd.ExecuteNonQueryAsync(ct);
                    }
                }

                result.Success = true;
                result.RowsSynced = insertedRows;
                result.RowsSkipped = skippedRows;
                
                if (skippedRows > 0)
                {
                    var skipDetails = new List<string>();
                    if (skippedRowDetails.Any())
                    {
                        var uniqueSkips = skippedRowDetails.Distinct().Take(5).ToList();
                        skipDetails.Add($"Skipped existing: {string.Join(", ", uniqueSkips)}");
                        if (skippedRowDetails.Count > 5)
                        {
                            skipDetails.Add($"... and {skippedRowDetails.Count - 5} more");
                        }
                    }
                    if (result.Errors.Any())
                    {
                        var errorDetails = result.Errors.Take(3).ToList();
                        skipDetails.Add($"Errors: {string.Join("; ", errorDetails)}");
                    }
                    
                    if (skipDetails.Any())
                    {
                        result.Message = $"Pulled {insertedRows} rows from Azure to Local for {tableName}. {skippedRows} rows skipped. " +
                                       string.Join(". ", skipDetails);
                    }
                    else
                    {
                        result.Message = $"Pulled {insertedRows} rows from Azure to Local for {tableName}. {skippedRows} rows skipped (duplicates).";
                    }
                }
                else
                {
                    result.Message = $"Pulled {insertedRows} rows from Azure to Local for {tableName}.";
                }
            }
            catch (Exception ex)
            {
                result.Message = $"Error pulling {tableName}: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<SyncResult> PullAllTablesAsync(CancellationToken ct = default)
        {
            return await PullAllTablesAsync(null, ct);
        }

        public async Task<SyncResult> PullAllTablesAsync(IProgress<SyncProgress>? progress, CancellationToken ct = default)
        {
            var result = new SyncResult 
            { 
                Success = true,
                Message = "Starting pull from Azure to Local...",
                Errors = new List<string>()
            };

            int totalSynced = 0;
            int totalSkipped = 0;
            int tablesProcessed = 0;

            for (int i = 0; i < _tables.Length; i++)
            {
                var table = _tables[i];
                
                progress?.Report(new SyncProgress
                {
                    CurrentTable = i + 1,
                    TotalTables = _tables.Length,
                    CurrentTableName = table,
                    RowsSynced = totalSynced,
                    RowsSkipped = totalSkipped
                });

                try
                {
                    var tableResult = await PullTableAsync(table, ct);
                    
                    if (tableResult.Success)
                    {
                        totalSynced += tableResult.RowsSynced;
                        totalSkipped += tableResult.RowsSkipped;
                        tablesProcessed++;
                    }
                    else
                    {
                        result.Errors.AddRange(tableResult.Errors);
                        if (tableResult.Errors.Count > 0)
                        {
                            result.Success = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add($"Error processing {table}: {ex.Message}");
                }
            }

            progress?.Report(new SyncProgress
            {
                CurrentTable = _tables.Length,
                TotalTables = _tables.Length,
                CurrentTableName = "Completed",
                RowsSynced = totalSynced,
                RowsSkipped = totalSkipped
            });

            result.RowsSynced = totalSynced;
            result.RowsSkipped = totalSkipped;
            result.Message = $"Pull completed: {totalSynced} rows pulled from Azure to Local, {totalSkipped} rows skipped across {tablesProcessed} tables.";
            
            return result;
        }
    }
}

