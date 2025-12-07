# PowerShell Script to Sync Data from Local SQL Server to Azure SQL
# This script copies data from local database to Azure SQL Database

param(
    [string]$LocalServer = "localhost\SQLEXPRESS",
    [string]$LocalDatabase = "db_SoftWear",
    [string]$AzureServer = "jussstzy.database.windows.net",
    [string]$AzureDatabase = "db_SoftWear",
    [string]$AzureUsername = "justin",
    [string]$AzurePassword = "JussPogi27",
    [switch]$SkipExisting = $true
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Data Sync: Local to Azure SQL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# List of tables to sync (in dependency order)
$tables = @(
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
    "tbl_return_items"
)

try {
    # Connect to local database
    Write-Host "Connecting to local database..." -ForegroundColor Yellow
    $localConnectionString = "Server=$LocalServer;Database=$LocalDatabase;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=30;"
    $localConnection = New-Object System.Data.SqlClient.SqlConnection($localConnectionString)
    $localConnection.Open()
    Write-Host "Connected to local database successfully!" -ForegroundColor Green
    Write-Host ""
    
    # Connect to Azure database
    Write-Host "Connecting to Azure database..." -ForegroundColor Yellow
    $azureConnectionString = "Server=$AzureServer;Database=$AzureDatabase;User Id=$AzureUsername;Password=$AzurePassword;TrustServerCertificate=True;Connection Timeout=30;"
    $azureConnection = New-Object System.Data.SqlClient.SqlConnection($azureConnectionString)
    $azureConnection.Open()
    Write-Host "Connected to Azure database successfully!" -ForegroundColor Green
    Write-Host ""
    
    $totalRows = 0
    
    foreach ($table in $tables) {
        Write-Host "Syncing table: $table" -ForegroundColor Cyan
        
        try {
            # Check if table exists in local database
            $checkTableSql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$table'"
            $checkCmd = New-Object System.Data.SqlClient.SqlCommand($checkTableSql, $localConnection)
            $tableExists = [int]$checkCmd.ExecuteScalar() -gt 0
            
            if (-not $tableExists) {
                Write-Host "  Table $table does not exist in local database, skipping..." -ForegroundColor Yellow
                continue
            }
            
            # Get row count from local
            $countSql = "SELECT COUNT(*) FROM dbo.$table"
            $countCmd = New-Object System.Data.SqlClient.SqlCommand($countSql, $localConnection)
            $rowCount = [int]$countCmd.ExecuteScalar()
            
            if ($rowCount -eq 0) {
                Write-Host "  No data to sync (0 rows)" -ForegroundColor Gray
                continue
            }
            
            Write-Host "  Found $rowCount rows in local database" -ForegroundColor Gray
            
            # Check if we should skip existing data
            if ($SkipExisting) {
                # Get existing row count in Azure
                $azureCountSql = "SELECT COUNT(*) FROM dbo.$table"
                $azureCountCmd = New-Object System.Data.SqlClient.SqlCommand($azureCountSql, $azureConnection)
                $azureRowCount = [int]$azureCountCmd.ExecuteScalar()
                
                if ($azureRowCount -gt 0) {
                    Write-Host "  Azure database already has $azureRowCount rows, skipping..." -ForegroundColor Yellow
                    continue
                }
            }
            
            # Read all data from local table
            $selectSql = "SELECT * FROM dbo.$table"
            $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($selectSql, $localConnection)
            $dataSet = New-Object System.Data.DataSet
            $adapter.Fill($dataSet, $table)
            $dataTable = $dataSet.Tables[$table]
            
            if ($dataTable.Rows.Count -eq 0) {
                Write-Host "  No data to sync" -ForegroundColor Gray
                continue
            }
            
            # Get column names
            $columns = $dataTable.Columns | ForEach-Object { $_.ColumnName }
            $columnList = ($columns | ForEach-Object { "[$_]" }) -join ", "
            $parameterList = ($columns | ForEach-Object { "@$_" }) -join ", "
            
            # Prepare insert statement
            $insertSql = "INSERT INTO dbo.$table ($columnList) VALUES ($parameterList)"
            
            # Insert data into Azure
            $insertedRows = 0
            foreach ($row in $dataTable.Rows) {
                try {
                    $insertCmd = New-Object System.Data.SqlClient.SqlCommand($insertSql, $azureConnection)
                    
                    foreach ($column in $columns) {
                        $value = $row[$column]
                        if ([DBNull]::Value.Equals($value)) {
                            $insertCmd.Parameters.AddWithValue("@$column", [DBNull]::Value) | Out-Null
                        } else {
                            $insertCmd.Parameters.AddWithValue("@$column", $value) | Out-Null
                        }
                    }
                    
                    $insertCmd.ExecuteNonQuery() | Out-Null
                    $insertedRows++
                }
                catch {
                    # Skip duplicate key errors or other constraint violations
                    if ($_.Exception.Message -notmatch "Violation of PRIMARY KEY|Violation of UNIQUE KEY") {
                        Write-Host "    Warning: Error inserting row: $($_.Exception.Message)" -ForegroundColor Yellow
                    }
                }
            }
            
            Write-Host "  Synced $insertedRows rows to Azure" -ForegroundColor Green
            $totalRows += $insertedRows
        }
        catch {
            Write-Host "  Error syncing table $table : $($_.Exception.Message)" -ForegroundColor Red
        }
        
        Write-Host ""
    }
    
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Data sync completed!" -ForegroundColor Green
    Write-Host "Total rows synced: $totalRows" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Green
    
    $localConnection.Close()
    $azureConnection.Close()
}
catch {
    Write-Host ""
    Write-Host "Error during data sync:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check:" -ForegroundColor Yellow
    Write-Host "1. Local SQL Server is running and accessible" -ForegroundColor Gray
    Write-Host "2. Azure SQL Server connection details are correct" -ForegroundColor Gray
    Write-Host "3. Firewall rules allow your IP address" -ForegroundColor Gray
    exit 1
}












