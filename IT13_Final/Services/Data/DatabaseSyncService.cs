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

        private void AddParameterToCommand(SqlCommand cmd, string column, object value, Dictionary<string, string> columnTypes, DataTable dataTable, string? primaryKeyColumn, List<string> allColumns)
        {
            // Check if this is a binary column based on database schema
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
                    var binaryParam = new SqlParameter($"@{column}", SqlDbType.VarBinary, -1);
                    binaryParam.Value = DBNull.Value;
                    cmd.Parameters.Add(binaryParam);
                }
                else
                {
                    cmd.Parameters.AddWithValue($"@{column}", DBNull.Value);
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
                        }
                        catch { }
                    }
                    
                    var binaryParam = new SqlParameter($"@{column}", SqlDbType.VarBinary, -1);
                    binaryParam.Value = (object?)byteValue ?? DBNull.Value;
                    cmd.Parameters.Add(binaryParam);
                }
                else
                {
                    cmd.Parameters.AddWithValue($"@{column}", value);
                }
            }
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

                // Read all data from local table (including archived records)
                // Sync ALL data to ensure complete synchronization
                string selectSql = $"SELECT * FROM dbo.{tableName}";
                
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

                // Get primary key column for MERGE statement
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
                    }
                    else
                    {
                        // Fallback: assume 'id' is the primary key
                        if (allColumns.Contains("id", StringComparer.OrdinalIgnoreCase))
                        {
                            primaryKeyColumn = allColumns.First(c => c.Equals("id", StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If we can't get primary keys, try to use 'id' as fallback
                    if (allColumns.Contains("id", StringComparer.OrdinalIgnoreCase))
                    {
                        primaryKeyColumn = allColumns.First(c => c.Equals("id", StringComparison.OrdinalIgnoreCase));
                    }
                    result.Errors.Add($"Warning: Could not determine primary key for {tableName}: {ex.Message}");
                }

                // Use all columns for MERGE (including IDENTITY columns)
                var insertColumns = allColumns.ToList();
                var columnList = string.Join(", ", insertColumns.Select(c => $"[{c}]"));
                var parameterList = string.Join(", ", insertColumns.Select(c => $"@{c}"));

                // Check if we need IDENTITY_INSERT
                bool needsIdentityInsert = identityColumns.Any();

                int insertedRows = 0;
                int updatedRows = 0;
                int skippedRows = 0;
                
                // Track which specific rows are being skipped for better reporting
                var skippedRowDetails = new List<string>();

                // Enable IDENTITY_INSERT if needed (once for all operations)
                if (needsIdentityInsert)
                {
                    var enableSql = $"SET IDENTITY_INSERT dbo.{tableName} ON;";
                    await using var enableCmd = new SqlCommand(enableSql, azureConn);
                    await enableCmd.ExecuteNonQueryAsync(ct);
                }

                try
                {
                    // Build UPDATE and INSERT statements for UPSERT (UPDATE if exists, INSERT if not)
                    string? updateSql = null;
                    string insertSql = $"INSERT INTO dbo.{tableName} ({columnList}) VALUES ({parameterList})";
                    
                    if (!string.IsNullOrEmpty(primaryKeyColumn) && allColumns.Contains(primaryKeyColumn))
                    {
                        // Build UPDATE SET clause (all columns except primary key)
                        var updateColumns = insertColumns.Where(c => !c.Equals(primaryKeyColumn, StringComparison.OrdinalIgnoreCase)).ToList();
                        var updateSetClause = string.Join(", ", updateColumns.Select(c => $"[{c}] = @{c}"));
                        updateSql = $"UPDATE dbo.{tableName} SET {updateSetClause} WHERE [{primaryKeyColumn}] = @{primaryKeyColumn}";
                    }

                    foreach (DataRow row in dataTable.Rows)
                    {
                        try
                        {
                            // Try UPDATE first if we have a primary key and UPDATE statement
                            bool wasUpdated = false;
                            if (!string.IsNullOrEmpty(updateSql) && !string.IsNullOrEmpty(primaryKeyColumn) && allColumns.Contains(primaryKeyColumn))
                            {
                                await using var updateCmd = new SqlCommand(updateSql, azureConn);
                                
                                // Add parameters for UPDATE
                                foreach (var column in insertColumns)
                                {
                                    var value = row[column];
                                    AddParameterToCommand(updateCmd, column, value, columnTypes, dataTable, primaryKeyColumn, allColumns);
                                }
                                
                                var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
                                if (rowsAffected > 0)
                                {
                                    updatedRows++;
                                    wasUpdated = true;
                                    continue; // Skip INSERT since UPDATE succeeded
                                }
                            }
                            
                            // If UPDATE didn't affect any rows (or no UPDATE statement), do INSERT
                            if (!wasUpdated)
                            {
                                await using var insertCmd = new SqlCommand(insertSql, azureConn);
                                
                                // Add parameters for INSERT
                                foreach (var column in insertColumns)
                                {
                                    var value = row[column];
                                    AddParameterToCommand(insertCmd, column, value, columnTypes, dataTable, primaryKeyColumn, allColumns);
                                }
                                
                                await insertCmd.ExecuteNonQueryAsync(ct);
                                insertedRows++;
                            }
                        }
                        catch (SqlException ex)
                        {
                            // Handle constraint violations
                            if (ex.Number == 544) // Cannot insert explicit value for identity column
                            {
                                skippedRows++;
                                result.Errors.Add($"Skipped row with identity conflict in {tableName}: {ex.Message}");
                            }
                            else if (ex.Number == 547) // Foreign key constraint violation
                            {
                                skippedRows++;
                                if (primaryKeyColumn != null && allColumns.Contains(primaryKeyColumn))
                                {
                                    var pkValue = row[primaryKeyColumn];
                                    result.Errors.Add($"Skipped row in {tableName} with {primaryKeyColumn}={pkValue} due to foreign key constraint: {ex.Message}");
                                }
                                else
                                {
                                    result.Errors.Add($"Skipped row in {tableName} due to foreign key constraint: {ex.Message}");
                                }
                            }
                            else
                            {
                                skippedRows++;
                                result.Errors.Add($"Error syncing row in {tableName}: {ex.Message} (Error #{ex.Number})");
                            }
                        }
                        catch (Exception ex)
                        {
                            skippedRows++;
                            result.Errors.Add($"Error syncing row in {tableName}: {ex.Message}");
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
                result.RowsSynced = insertedRows + updatedRows;
                result.RowsSkipped = skippedRows;
                
                // Build detailed message
                var messageParts = new List<string>();
                
                if (insertedRows > 0 && updatedRows > 0)
                {
                    messageParts.Add($"{insertedRows} inserted, {updatedRows} updated");
                }
                else if (insertedRows > 0)
                {
                    messageParts.Add($"{insertedRows} inserted");
                }
                else if (updatedRows > 0)
                {
                    messageParts.Add($"{updatedRows} updated");
                }
                
                if (skippedRows > 0)
                {
                    messageParts.Add($"{skippedRows} skipped");
                    
                    // Add error details if any
                    if (result.Errors.Any())
                    {
                        var errorDetails = result.Errors.Take(3).ToList();
                        messageParts.Add($"Errors: {string.Join("; ", errorDetails)}");
                    }
                }
                
                if (messageParts.Any())
                {
                    result.Message = $"Synced {tableName}: {string.Join(", ", messageParts)}.";
                }
                else
                {
                    result.Message = $"No changes needed for {tableName}.";
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
                        
                        // Sync ALL records including archived ones
                        selectSql = $"SELECT * FROM dbo.{tableName}";
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

