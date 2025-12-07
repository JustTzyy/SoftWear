# PowerShell Script to Setup Azure SQL Database
# This script connects to Azure SQL and runs the migration script

param(
    [string]$ServerName = "jussstzy.database.windows.net",
    [string]$DatabaseName = "db_SoftWear",
    [string]$Username = "justin",
    [string]$Password = "JussPogi27",
    [string]$SqlScriptPath = ".\Migrations\Azure_SQL_Setup.sql"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Azure SQL Database Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if SQL script exists
if (-not (Test-Path $SqlScriptPath)) {
    Write-Host "Error: SQL script not found at $SqlScriptPath" -ForegroundColor Red
    exit 1
}

# Read the SQL script
$sqlScript = Get-Content $SqlScriptPath -Raw

Write-Host "Connecting to Azure SQL Database..." -ForegroundColor Yellow
Write-Host "Server: $ServerName" -ForegroundColor Gray
Write-Host "Database: $DatabaseName" -ForegroundColor Gray
Write-Host ""

try {
    # Create connection string
    $connectionString = "Server=$ServerName;Database=master;User Id=$Username;Password=$Password;TrustServerCertificate=True;Connection Timeout=30;"
    
    # Create SQL connection
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    Write-Host ""
    
    # Split script by GO statements and execute
    $commands = $sqlScript -split "GO\s*\r?\n" | Where-Object { $_.Trim() -ne "" }
    
    $commandCount = $commands.Count
    Write-Host "Executing $commandCount SQL commands..." -ForegroundColor Yellow
    Write-Host ""
    
    $currentCommand = 0
    foreach ($command in $commands) {
        $currentCommand++
        $command = $command.Trim()
        
        if ($command -ne "") {
            try {
                # Check if this is a USE statement - if so, switch database
                if ($command -match "USE\s+(\w+)") {
                    $dbName = $matches[1]
                    $connection.ChangeDatabase($dbName)
                    Write-Host "[$currentCommand/$commandCount] Switching to database: $dbName" -ForegroundColor Cyan
                } else {
                    $sqlCommand = New-Object System.Data.SqlClient.SqlCommand($command, $connection)
                    $sqlCommand.CommandTimeout = 300  # 5 minutes timeout
                    
                    # Execute command
                    $result = $sqlCommand.ExecuteNonQuery()
                    
                    # Try to get messages if any
                    Write-Host "[$currentCommand/$commandCount] Command executed" -ForegroundColor Gray
                }
            }
            catch {
                $errorMessage = $_.Exception.Message
                Write-Host "[$currentCommand/$commandCount] Error: $errorMessage" -ForegroundColor Red
                # Continue with next command
            }
        }
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Setup completed!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Database: $DatabaseName" -ForegroundColor Cyan
    Write-Host "Server: $ServerName" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "You can now update your connection strings to:" -ForegroundColor Yellow
    Write-Host "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;TrustServerCertificate=True;" -ForegroundColor Gray
    
    $connection.Close()
}
catch {
    Write-Host ""
    Write-Host "Error connecting to Azure SQL Database:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check:" -ForegroundColor Yellow
    Write-Host "1. Server name is correct" -ForegroundColor Gray
    Write-Host "2. Username and password are correct" -ForegroundColor Gray
    Write-Host "3. Firewall rules allow your IP address" -ForegroundColor Gray
    Write-Host "4. SQL Server is running and accessible" -ForegroundColor Gray
    exit 1
}












