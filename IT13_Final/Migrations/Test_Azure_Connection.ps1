# Test Azure SQL Connection
# This script tests the connection to Azure SQL Database

param(
    [string]$ServerName = "jussstzy.database.windows.net",
    [string]$DatabaseName = "db_SoftWear",
    [string]$Username = "justin",
    [string]$Password = "JussPogi27"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Azure SQL Database Connection" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test DNS resolution
Write-Host "Step 1: Testing DNS resolution..." -ForegroundColor Yellow
try {
    $dnsResult = Resolve-DnsName -Name $ServerName -ErrorAction Stop
    Write-Host "  DNS Resolution: SUCCESS" -ForegroundColor Green
    Write-Host "  Resolved to: $($dnsResult[0].IPAddress)" -ForegroundColor Gray
}
catch {
    Write-Host "  DNS Resolution: FAILED" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Possible issues:" -ForegroundColor Yellow
    Write-Host "  1. Server name might be incorrect" -ForegroundColor Gray
    Write-Host "  2. Server might be paused or deleted" -ForegroundColor Gray
    Write-Host "  3. Network connectivity issues" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Please verify the server name in Azure Portal." -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test TCP connection
Write-Host "Step 2: Testing TCP connection to port 1433..." -ForegroundColor Yellow
try {
    $tcpTest = Test-NetConnection -ComputerName $ServerName -Port 1433 -WarningAction SilentlyContinue
    if ($tcpTest.TcpTestSucceeded) {
        Write-Host "  TCP Connection: SUCCESS" -ForegroundColor Green
    } else {
        Write-Host "  TCP Connection: FAILED" -ForegroundColor Red
        Write-Host "  This usually means firewall rules are blocking access" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To fix this:" -ForegroundColor Yellow
        Write-Host "  1. Go to Azure Portal" -ForegroundColor Gray
        Write-Host "  2. Navigate to your SQL Server resource" -ForegroundColor Gray
        Write-Host "  3. Go to 'Networking' or 'Firewalls and virtual networks'" -ForegroundColor Gray
        Write-Host "  4. Add your current IP address" -ForegroundColor Gray
        Write-Host "  5. Or enable 'Allow Azure services and resources'" -ForegroundColor Gray
        exit 1
    }
}
catch {
    Write-Host "  TCP Connection: FAILED" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test SQL connection
Write-Host "Step 3: Testing SQL authentication..." -ForegroundColor Yellow
try {
    $connectionString = "Server=$ServerName;Database=master;User Id=$Username;Password=$Password;TrustServerCertificate=True;Connection Timeout=10;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "  SQL Authentication: SUCCESS" -ForegroundColor Green
    Write-Host "  Connected to: $($connection.Database)" -ForegroundColor Gray
    $connection.Close()
}
catch {
    Write-Host "  SQL Authentication: FAILED" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Possible issues:" -ForegroundColor Yellow
    Write-Host "  1. Username or password is incorrect" -ForegroundColor Gray
    Write-Host "  2. SQL Server authentication is not enabled" -ForegroundColor Gray
    Write-Host "  3. User doesn't have permission to connect" -ForegroundColor Gray
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All connection tests passed!" -ForegroundColor Green
Write-Host "You can now run the setup script." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green












