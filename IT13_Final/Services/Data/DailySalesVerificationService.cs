using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class DailySalesVerificationModel
    {
        public int CashierUserId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalTransactions { get; set; }
        public decimal CashAmount { get; set; }
        public decimal GCashAmount { get; set; }
        public decimal TotalReturns { get; set; }
        public int ReturnCount { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal ActualCash { get; set; }
        public decimal CashDiscrepancy { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedByName { get; set; }
    }

    public class DailySalesVerificationDetailsModel
    {
        public int CashierUserId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalTransactions { get; set; }
        public decimal CashAmount { get; set; }
        public decimal GCashAmount { get; set; }
        public decimal TotalReturns { get; set; }
        public int ReturnCount { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal ActualCash { get; set; }
        public decimal CashDiscrepancy { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedByName { get; set; }
        public List<SaleReportModel> Sales { get; set; } = new();
        public List<ReturnReportModel> Returns { get; set; } = new();
        public List<PaymentMethodBreakdownModel> PaymentBreakdown { get; set; } = new();
    }

    public class PaymentMethodBreakdownModel
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public interface IDailySalesVerificationService
    {
        Task<List<DailySalesVerificationModel>> GetPendingDailySalesForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetPendingDailySalesCountForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<DailySalesVerificationDetailsModel?> GetDailySalesDetailsForAccountingAsync(int cashierUserId, DateTime saleDate, int sellerUserId, CancellationToken ct = default);
        Task<bool> ApproveDailySalesAsync(int cashierUserId, DateTime saleDate, int approvedByUserId, int sellerUserId, CancellationToken ct = default);
        Task<bool> RejectDailySalesAsync(int cashierUserId, DateTime saleDate, int rejectedByUserId, int sellerUserId, CancellationToken ct = default);
        Task<List<DailySalesVerificationModel>> GetAllDailySalesVerificationsForReportAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null, CancellationToken ct = default);
    }

    public class DailySalesVerificationService : IDailySalesVerificationService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<DailySalesVerificationModel>> GetPendingDailySalesForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var reports = new List<DailySalesVerificationModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Get cashiers that belong to this seller (user_id matches sellerUserId)
            var sql = @"
                WITH DailySales AS (
                    SELECT 
                        s.user_id as cashier_user_id,
                        CAST(s.timestamps AS DATE) as sale_date,
                        COUNT(*) as transaction_count,
                        COALESCE(SUM(s.amount), 0) as total_sales,
                        COALESCE(SUM(CASE 
                            WHEN p.payment_method = 'Cash' THEN p.amount_paid - p.change_given
                            WHEN p.payment_method IS NULL AND s.payment_type = 'Cash' THEN s.amount
                            ELSE 0 
                        END), 0) as cash_amount,
                        COALESCE(SUM(CASE 
                            WHEN p.payment_method = 'GCash' THEN s.amount
                            WHEN p.payment_method IS NULL AND s.payment_type = 'GCash' THEN s.amount
                            ELSE 0 
                        END), 0) as gcash_amount
                    FROM dbo.tbl_sales s
                    INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                    LEFT JOIN dbo.tbl_payments p ON s.id = p.sale_id AND p.archives IS NULL
                    WHERE s.archives IS NULL 
                    AND s.status = 'Completed'
                    AND u.user_id = @SellerUserId
                    AND u.archived_at IS NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += @" AND (u.name LIKE @SearchTerm OR u.fname LIKE @SearchTerm OR u.lname LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            sql += @"
                    GROUP BY s.user_id, CAST(s.timestamps AS DATE)
                ),
                DailyReturns AS (
                    SELECT 
                        s.user_id as cashier_user_id,
                        CAST(s.timestamps AS DATE) as sale_date,
                        COUNT(*) as return_count,
                        COALESCE(SUM(ri.quantity * si.price), 0) as total_returns
                    FROM dbo.tbl_returns r
                    INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                    INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                    INNER JOIN dbo.tbl_return_items ri ON r.id = ri.return_id
                    INNER JOIN dbo.tbl_sales_items si ON ri.sale_item_id = si.id
                    WHERE r.archives IS NULL 
                    AND r.status = 'Approved'
                    AND s.archives IS NULL
                    AND ri.archives IS NULL
                    AND si.archives IS NULL
                    AND u.user_id = @SellerUserId
                    AND u.archived_at IS NULL";

            if (startDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            sql += @"
                    GROUP BY s.user_id, CAST(s.timestamps AS DATE)
                )
                SELECT DISTINCT
                    ds.cashier_user_id,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name,
                    ds.sale_date,
                    ds.transaction_count,
                    ds.total_sales,
                    ds.cash_amount,
                    ds.gcash_amount,
                    COALESCE(dr.return_count, 0) as return_count,
                    COALESCE(dr.total_returns, 0.00) as total_returns,
                    CAST(0 AS DECIMAL(18,2)) as total_discounts,
                    ds.cash_amount as expected_cash,
                    CAST(0 AS DECIMAL(18,2)) as actual_cash,
                    ds.cash_amount as cash_discrepancy,
                    COALESCE(dsv.status, 'Pending') as status,
                    dsv.updated_at as verified_at,
                    COALESCE(verifier.name, (LTRIM(RTRIM(ISNULL(verifier.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(verifier.lname,''))))) as verified_by_name
                FROM DailySales ds
                INNER JOIN dbo.tbl_users u ON ds.cashier_user_id = u.id
                LEFT JOIN DailyReturns dr ON ds.cashier_user_id = dr.cashier_user_id AND ds.sale_date = dr.sale_date
                LEFT JOIN dbo.tbl_daily_sales_verifications dsv ON ds.cashier_user_id = dsv.cashier_user_id 
                    AND ds.sale_date = dsv.sale_date 
                    AND dsv.seller_user_id = @SellerUserId
                    AND dsv.archived_at IS NULL
                LEFT JOIN dbo.tbl_users verifier ON dsv.verified_by = verifier.id
                WHERE (dsv.status IS NULL OR dsv.status = 'Pending')";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += @" AND (u.name LIKE @SearchTerm OR u.fname LIKE @SearchTerm OR u.lname LIKE @SearchTerm)";
            }

            sql += @"
                ORDER BY sale_date DESC, cashier_name
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }

            // Debug: First check if there are any sales at all for this seller
            var diagnosticSql = @"
                SELECT 
                    COUNT(*) as total_sales,
                    COUNT(DISTINCT s.user_id) as cashier_count,
                    MIN(s.timestamps) as earliest_sale,
                    MAX(s.timestamps) as latest_sale
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                WHERE s.archives IS NULL 
                AND s.status = 'Completed'
                AND u.user_id = @SellerUserId
                AND u.archived_at IS NULL";

            using (var diagCmd = new SqlCommand(diagnosticSql, conn))
            {
                diagCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                using var diagReader = await diagCmd.ExecuteReaderAsync(ct);
                if (await diagReader.ReadAsync(ct))
                {
                    var totalSales = diagReader.GetInt32(0);
                    var cashierCount = diagReader.GetInt32(1);
                    var earliestSale = diagReader.IsDBNull(2) ? (DateTime?)null : diagReader.GetDateTime(2);
                    var latestSale = diagReader.IsDBNull(3) ? (DateTime?)null : diagReader.GetDateTime(3);
                    
                    System.Diagnostics.Debug.WriteLine($"Diagnostic - SellerUserId: {sellerUserId}, Total Sales: {totalSales}, Cashiers: {cashierCount}, Earliest: {earliestSale}, Latest: {latestSale}");
                }
            }

            // Debug: Check for returns
            var returnsDiagnosticSql = @"
                SELECT 
                    COUNT(*) as total_returns,
                    COUNT(DISTINCT s.user_id) as cashier_count,
                    MIN(s.timestamps) as earliest_return_sale_date,
                    MAX(s.timestamps) as latest_return_sale_date
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                WHERE r.archives IS NULL 
                AND r.status = 'Approved'
                AND s.archives IS NULL
                AND u.user_id = @SellerUserId
                AND u.archived_at IS NULL";

            using (var diagCmd2 = new SqlCommand(returnsDiagnosticSql, conn))
            {
                diagCmd2.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                using var diagReader2 = await diagCmd2.ExecuteReaderAsync(ct);
                if (await diagReader2.ReadAsync(ct))
                {
                    var totalReturns = diagReader2.GetInt32(0);
                    var returnCashierCount = diagReader2.GetInt32(1);
                    var earliestReturnSale = diagReader2.IsDBNull(2) ? (DateTime?)null : diagReader2.GetDateTime(2);
                    var latestReturnSale = diagReader2.IsDBNull(3) ? (DateTime?)null : diagReader2.GetDateTime(3);
                    
                    System.Diagnostics.Debug.WriteLine($"Returns Diagnostic - SellerUserId: {sellerUserId}, Total Returns: {totalReturns}, Cashiers: {returnCashierCount}, Earliest Return Sale Date: {earliestReturnSale}, Latest Return Sale Date: {latestReturnSale}");
                }
            }

            // Debug: Log the query and parameters
            System.Diagnostics.Debug.WriteLine($"Daily Sales Query - SellerUserId: {sellerUserId}, StartDate: {startDate?.ToString() ?? "null"}, EndDate: {endDate?.ToString() ?? "null"}");

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                    reports.Add(new DailySalesVerificationModel
                    {
                        CashierUserId = reader.GetInt32(0),
                        CashierName = reader.GetString(1),
                        SaleDate = reader.GetDateTime(2),
                        TotalTransactions = reader.GetInt32(3),
                        TotalSales = reader.GetDecimal(4),
                        CashAmount = reader.GetDecimal(5),
                        GCashAmount = reader.GetDecimal(6),
                        ReturnCount = reader.GetInt32(7),
                        TotalReturns = reader.GetDecimal(8),
                        TotalDiscounts = reader.GetDecimal(9),
                        ExpectedCash = reader.GetDecimal(10),
                        ActualCash = reader.GetDecimal(11),
                        CashDiscrepancy = reader.GetDecimal(12),
                        Status = reader.GetString(13),
                        VerifiedAt = reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                        VerifiedByName = reader.IsDBNull(15) ? null : reader.GetString(15)
                    });
            }

            return reports;
        }

        public async Task<int> GetPendingDailySalesCountForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                WITH DailySales AS (
                    SELECT 
                        s.user_id as cashier_user_id,
                        CAST(s.timestamps AS DATE) as sale_date
                    FROM dbo.tbl_sales s
                    INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                    WHERE s.archives IS NULL 
                    AND s.status = 'Completed'
                    AND u.user_id = @SellerUserId
                    AND u.archived_at IS NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += @" AND (u.name LIKE @SearchTerm OR u.fname LIKE @SearchTerm OR u.lname LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            sql += @"
                    GROUP BY s.user_id, CAST(s.timestamps AS DATE)
                )
                SELECT COUNT(DISTINCT CONCAT(ds.cashier_user_id, '_', ds.sale_date))
                FROM DailySales ds
                LEFT JOIN dbo.tbl_daily_sales_verifications dsv ON ds.cashier_user_id = dsv.cashier_user_id 
                    AND ds.sale_date = dsv.sale_date 
                    AND dsv.seller_user_id = @SellerUserId
                    AND dsv.archived_at IS NULL
                WHERE (dsv.status IS NULL OR dsv.status = 'Pending')";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<DailySalesVerificationDetailsModel?> GetDailySalesDetailsForAccountingAsync(int cashierUserId, DateTime saleDate, int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Verify cashier belongs to seller
            var verifySql = @"
                SELECT COUNT(*) 
                FROM dbo.tbl_users 
                WHERE id = @CashierUserId 
                AND user_id = @SellerUserId 
                AND archived_at IS NULL";

            using var verifyCmd = new SqlCommand(verifySql, conn);
            verifyCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            verifyCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var verifyResult = await verifyCmd.ExecuteScalarAsync(ct);
            if (verifyResult == null || Convert.ToInt32(verifyResult) == 0)
            {
                return null; // Cashier doesn't belong to this seller
            }

            // Get cashier name
            var cashierSql = @"
                SELECT COALESCE(name, (LTRIM(RTRIM(ISNULL(fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(lname,'')))))
                FROM dbo.tbl_users
                WHERE id = @CashierUserId";

            using var cashierCmd = new SqlCommand(cashierSql, conn);
            cashierCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            var cashierName = await cashierCmd.ExecuteScalarAsync(ct) as string ?? string.Empty;

            // Get daily sales summary
            var summarySql = @"
                SELECT 
                    COUNT(*) as transaction_count,
                    COALESCE(SUM(s.amount), 0) as total_sales,
                    COALESCE(SUM(CASE WHEN p.payment_method = 'Cash' THEN p.amount_paid - p.change_given ELSE 0 END), 0) as cash_amount,
                    COALESCE(SUM(CASE WHEN p.payment_method = 'GCash' THEN s.amount ELSE 0 END), 0) as gcash_amount
                FROM dbo.tbl_sales s
                LEFT JOIN dbo.tbl_payments p ON s.id = p.sale_id AND p.archives IS NULL
                WHERE s.archives IS NULL 
                AND s.status = 'Completed'
                AND s.user_id = @CashierUserId
                AND CAST(s.timestamps AS DATE) = @SaleDate";

            decimal totalSales = 0;
            int totalTransactions = 0;
            decimal cashAmount = 0;
            decimal gcashAmount = 0;

            using (var summaryCmd = new SqlCommand(summarySql, conn))
            {
                summaryCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                summaryCmd.Parameters.AddWithValue("@SaleDate", saleDate.Date);

                using var reader = await summaryCmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    totalTransactions = reader.GetInt32(0);
                    totalSales = reader.GetDecimal(1);
                    cashAmount = reader.GetDecimal(2);
                    gcashAmount = reader.GetDecimal(3);
                }
            }

            // Get returns - match by original sale date, not return date
            var returnsSql = @"
                SELECT 
                    COUNT(DISTINCT r.id) as return_count,
                    COALESCE(SUM(ri.quantity * si.price), 0) as total_returns
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                INNER JOIN dbo.tbl_return_items ri ON r.id = ri.return_id
                INNER JOIN dbo.tbl_sales_items si ON ri.sale_item_id = si.id
                WHERE r.archives IS NULL 
                AND r.status = 'Approved'
                AND s.archives IS NULL
                AND ri.archives IS NULL
                AND si.archives IS NULL
                AND s.user_id = @CashierUserId
                AND CAST(s.timestamps AS DATE) = @SaleDate";

            int returnCount = 0;
            decimal totalReturns = 0;

            using (var returnsCmd = new SqlCommand(returnsSql, conn))
            {
                returnsCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                returnsCmd.Parameters.AddWithValue("@SaleDate", saleDate.Date);

                using var reader = await returnsCmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    returnCount = reader.GetInt32(0);
                    totalReturns = reader.GetDecimal(1);
                }
            }

            // Get payment breakdown
            var paymentBreakdownSql = @"
                SELECT 
                    p.payment_method,
                    COUNT(*) as payment_count,
                    COALESCE(SUM(s.amount), 0) as total_amount
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_payments p ON s.id = p.sale_id
                WHERE s.archives IS NULL 
                AND s.status = 'Completed'
                AND p.archives IS NULL
                AND s.user_id = @CashierUserId
                AND CAST(s.timestamps AS DATE) = @SaleDate
                GROUP BY p.payment_method";

            var paymentBreakdown = new List<PaymentMethodBreakdownModel>();
            using (var breakdownCmd = new SqlCommand(paymentBreakdownSql, conn))
            {
                breakdownCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                breakdownCmd.Parameters.AddWithValue("@SaleDate", saleDate.Date);

                using var reader = await breakdownCmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    paymentBreakdown.Add(new PaymentMethodBreakdownModel
                    {
                        PaymentMethod = reader.GetString(0),
                        Count = reader.GetInt32(1),
                        TotalAmount = reader.GetDecimal(2)
                    });
                }
            }

            // Get individual sales
            var salesSql = @"
                SELECT s.id, s.sale_number, s.amount, s.payment_type, s.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name,
                       p.amount_paid, p.change_given, p.reference_number
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                LEFT JOIN dbo.tbl_payments p ON s.id = p.sale_id
                WHERE s.archives IS NULL 
                AND s.status = 'Completed'
                AND s.user_id = @CashierUserId
                AND CAST(s.timestamps AS DATE) = @SaleDate
                ORDER BY s.timestamps DESC";

            var sales = new List<SaleReportModel>();
            using (var salesCmd = new SqlCommand(salesSql, conn))
            {
                salesCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                salesCmd.Parameters.AddWithValue("@SaleDate", saleDate.Date);

                using var reader = await salesCmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    sales.Add(new SaleReportModel
                    {
                        Id = reader.GetInt32(0),
                        SaleNumber = reader.GetString(1),
                        Amount = reader.GetDecimal(2),
                        PaymentMethod = reader.GetString(3),
                        Timestamps = reader.GetDateTime(4),
                        CashierName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        AmountPaid = reader.IsDBNull(6) ? reader.GetDecimal(2) : reader.GetDecimal(6),
                        ChangeGiven = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                        ReferenceNumber = reader.IsDBNull(8) ? null : reader.GetString(8)
                    });
                }
            }

            // Get individual returns - match by original sale date, not return date
            var returnDetailsSql = @"
                SELECT r.id, r.return_number, r.sale_id, s.sale_number, r.reason, r.status, r.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                WHERE r.archives IS NULL 
                AND r.status = 'Approved'
                AND s.archives IS NULL
                AND s.user_id = @CashierUserId
                AND CAST(s.timestamps AS DATE) = @SaleDate
                ORDER BY r.timestamps DESC";

            var returns = new List<ReturnReportModel>();
            using (var returnDetailsCmd = new SqlCommand(returnDetailsSql, conn))
            {
                returnDetailsCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                returnDetailsCmd.Parameters.AddWithValue("@SaleDate", saleDate.Date);

                using var reader = await returnDetailsCmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    returns.Add(new ReturnReportModel
                    {
                        Id = reader.GetInt32(0),
                        ReturnNumber = reader.GetString(1),
                        SaleId = reader.GetInt32(2),
                        SaleNumber = reader.GetString(3),
                        Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Status = reader.GetString(5),
                        Timestamps = reader.GetDateTime(6),
                        CashierName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                    });
                }
            }

            // Get verification status
            var verificationSql = @"
                SELECT status, updated_at, verified_by
                FROM dbo.tbl_daily_sales_verifications
                WHERE cashier_user_id = @CashierUserId
                AND sale_date = @SaleDate
                AND seller_user_id = @SellerUserId
                AND archived_at IS NULL";

            string status = "Pending";
            DateTime? verifiedAt = null;
            string? verifiedByName = null;

            using (var verificationCmd = new SqlCommand(verificationSql, conn))
            {
                verificationCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                verificationCmd.Parameters.AddWithValue("@SaleDate", saleDate.Date);
                verificationCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

                using var reader = await verificationCmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    status = reader.GetString(0);
                    verifiedAt = reader.IsDBNull(1) ? null : reader.GetDateTime(1);
                    var verifiedByUserId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);

                    if (verifiedByUserId.HasValue)
                    {
                        // Get verifier name
                        var verifierNameSql = @"
                            SELECT COALESCE(name, (LTRIM(RTRIM(ISNULL(fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(lname,'')))))
                            FROM dbo.tbl_users
                            WHERE id = @VerifiedByUserId";

                        using var verifierCmd = new SqlCommand(verifierNameSql, conn);
                        verifierCmd.Parameters.AddWithValue("@VerifiedByUserId", verifiedByUserId.Value);
                        var verifierName = await verifierCmd.ExecuteScalarAsync(ct) as string;
                        verifiedByName = verifierName;
                    }
                }
            }

            return new DailySalesVerificationDetailsModel
            {
                CashierUserId = cashierUserId,
                CashierName = cashierName,
                SaleDate = saleDate,
                TotalSales = totalSales,
                TotalTransactions = totalTransactions,
                CashAmount = cashAmount,
                GCashAmount = gcashAmount,
                TotalReturns = totalReturns,
                ReturnCount = returnCount,
                TotalDiscounts = 0, // Discounts not tracked in current system
                ExpectedCash = cashAmount,
                ActualCash = 0, // This would be entered by cashier when submitting
                CashDiscrepancy = 0, // Calculated as ActualCash - ExpectedCash
                Status = status,
                VerifiedAt = verifiedAt,
                VerifiedByName = verifiedByName,
                Sales = sales,
                Returns = returns,
                PaymentBreakdown = paymentBreakdown
            };
        }

        public async Task<bool> ApproveDailySalesAsync(int cashierUserId, DateTime saleDate, int approvedByUserId, int sellerUserId, CancellationToken ct = default)
        {
            // Verify cashier belongs to seller
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var verifySql = @"
                SELECT COUNT(*) 
                FROM dbo.tbl_users 
                WHERE id = @CashierUserId 
                AND user_id = @SellerUserId 
                AND archived_at IS NULL";

            using var verifyCmd = new SqlCommand(verifySql, conn);
            verifyCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            verifyCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var verifyResult = await verifyCmd.ExecuteScalarAsync(ct);
            if (verifyResult == null || Convert.ToInt32(verifyResult) == 0)
            {
                return false; // Cashier doesn't belong to this seller
            }

            // Insert or update verification record
            var upsertSql = @"
                IF EXISTS (
                    SELECT 1 
                    FROM dbo.tbl_daily_sales_verifications 
                    WHERE cashier_user_id = @CashierUserId 
                    AND sale_date = @SaleDate 
                    AND seller_user_id = @SellerUserId
                    AND archived_at IS NULL
                )
                BEGIN
                    UPDATE dbo.tbl_daily_sales_verifications
                    SET status = 'Approved',
                        verified_by = @ApprovedByUserId,
                        updated_at = SYSUTCDATETIME()
                    WHERE cashier_user_id = @CashierUserId 
                    AND sale_date = @SaleDate 
                    AND seller_user_id = @SellerUserId
                    AND archived_at IS NULL
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.tbl_daily_sales_verifications 
                        (cashier_user_id, sale_date, status, verified_by, seller_user_id, created_at, updated_at)
                    VALUES 
                        (@CashierUserId, @SaleDate, 'Approved', @ApprovedByUserId, @SellerUserId, SYSUTCDATETIME(), SYSUTCDATETIME())
                END";

            using var upsertCmd = new SqlCommand(upsertSql, conn);
            upsertCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            upsertCmd.Parameters.AddWithValue("@SaleDate", saleDate.Date);
            upsertCmd.Parameters.AddWithValue("@ApprovedByUserId", approvedByUserId);
            upsertCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            await upsertCmd.ExecuteNonQueryAsync(ct);
            return true;
        }

        public async Task<bool> RejectDailySalesAsync(int cashierUserId, DateTime saleDate, int rejectedByUserId, int sellerUserId, CancellationToken ct = default)
        {
            // Verify cashier belongs to seller
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var verifySql = @"
                SELECT COUNT(*) 
                FROM dbo.tbl_users 
                WHERE id = @CashierUserId 
                AND user_id = @SellerUserId 
                AND archived_at IS NULL";

            using var verifyCmd = new SqlCommand(verifySql, conn);
            verifyCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            verifyCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var verifyResult = await verifyCmd.ExecuteScalarAsync(ct);
            if (verifyResult == null || Convert.ToInt32(verifyResult) == 0)
            {
                return false; // Cashier doesn't belong to this seller
            }

            // Insert or update verification record
            var upsertSql = @"
                IF EXISTS (
                    SELECT 1 
                    FROM dbo.tbl_daily_sales_verifications 
                    WHERE cashier_user_id = @CashierUserId 
                    AND sale_date = @SaleDate 
                    AND seller_user_id = @SellerUserId
                    AND archived_at IS NULL
                )
                BEGIN
                    UPDATE dbo.tbl_daily_sales_verifications
                    SET status = 'Rejected',
                        verified_by = @RejectedByUserId,
                        updated_at = SYSUTCDATETIME()
                    WHERE cashier_user_id = @CashierUserId 
                    AND sale_date = @SaleDate 
                    AND seller_user_id = @SellerUserId
                    AND archived_at IS NULL
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.tbl_daily_sales_verifications 
                        (cashier_user_id, sale_date, status, verified_by, seller_user_id, created_at, updated_at)
                    VALUES 
                        (@CashierUserId, @SaleDate, 'Rejected', @RejectedByUserId, @SellerUserId, SYSUTCDATETIME(), SYSUTCDATETIME())
                END";

            using var upsertCmd = new SqlCommand(upsertSql, conn);
            upsertCmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            upsertCmd.Parameters.AddWithValue("@SaleDate", saleDate.Date);
            upsertCmd.Parameters.AddWithValue("@RejectedByUserId", rejectedByUserId);
            upsertCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            await upsertCmd.ExecuteNonQueryAsync(ct);
            return true;
        }

        public async Task<List<DailySalesVerificationModel>> GetAllDailySalesVerificationsForReportAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null, CancellationToken ct = default)
        {
            var reports = new List<DailySalesVerificationModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                WITH DailySales AS (
                    SELECT 
                        s.user_id as cashier_user_id,
                        CAST(s.timestamps AS DATE) as sale_date,
                        COUNT(*) as transaction_count,
                        COALESCE(SUM(s.amount), 0) as total_sales,
                        COALESCE(SUM(CASE 
                            WHEN p.payment_method = 'Cash' THEN p.amount_paid - p.change_given
                            WHEN p.payment_method IS NULL AND s.payment_type = 'Cash' THEN s.amount
                            ELSE 0 
                        END), 0) as cash_amount,
                        COALESCE(SUM(CASE 
                            WHEN p.payment_method = 'GCash' THEN s.amount
                            WHEN p.payment_method IS NULL AND s.payment_type = 'GCash' THEN s.amount
                            ELSE 0 
                        END), 0) as gcash_amount
                    FROM dbo.tbl_sales s
                    INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                    LEFT JOIN dbo.tbl_payments p ON s.id = p.sale_id AND p.archives IS NULL
                    WHERE s.archives IS NULL 
                    AND s.status = 'Completed'
                    AND u.user_id = @SellerUserId
                    AND u.archived_at IS NULL";

            if (startDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            sql += @"
                    GROUP BY s.user_id, CAST(s.timestamps AS DATE)
                ),
                DailyReturns AS (
                    SELECT 
                        s.user_id as cashier_user_id,
                        CAST(s.timestamps AS DATE) as sale_date,
                        COUNT(*) as return_count,
                        COALESCE(SUM(ri.quantity * si.price), 0) as total_returns
                    FROM dbo.tbl_returns r
                    INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                    INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                    INNER JOIN dbo.tbl_return_items ri ON r.id = ri.return_id
                    INNER JOIN dbo.tbl_sales_items si ON ri.sale_item_id = si.id
                    WHERE r.archives IS NULL 
                    AND r.status = 'Approved'
                    AND s.archives IS NULL
                    AND ri.archives IS NULL
                    AND si.archives IS NULL
                    AND u.user_id = @SellerUserId
                    AND u.archived_at IS NULL";

            if (startDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            sql += @"
                    GROUP BY s.user_id, CAST(s.timestamps AS DATE)
                )
                SELECT DISTINCT
                    ds.cashier_user_id,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name,
                    ds.sale_date,
                    ds.transaction_count,
                    ds.total_sales,
                    ds.cash_amount,
                    ds.gcash_amount,
                    COALESCE(dr.return_count, 0) as return_count,
                    COALESCE(dr.total_returns, 0.00) as total_returns,
                    CAST(0 AS DECIMAL(18,2)) as total_discounts,
                    ds.cash_amount as expected_cash,
                    CAST(0 AS DECIMAL(18,2)) as actual_cash,
                    ds.cash_amount as cash_discrepancy,
                    COALESCE(dsv.status, 'Pending') as status,
                    dsv.updated_at as verified_at,
                    COALESCE(verifier.name, (LTRIM(RTRIM(ISNULL(verifier.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(verifier.lname,''))))) as verified_by_name
                FROM DailySales ds
                INNER JOIN dbo.tbl_users u ON ds.cashier_user_id = u.id
                LEFT JOIN DailyReturns dr ON ds.cashier_user_id = dr.cashier_user_id AND ds.sale_date = dr.sale_date
                LEFT JOIN dbo.tbl_daily_sales_verifications dsv ON ds.cashier_user_id = dsv.cashier_user_id 
                    AND ds.sale_date = dsv.sale_date 
                    AND dsv.seller_user_id = @SellerUserId
                    AND dsv.archived_at IS NULL
                LEFT JOIN dbo.tbl_users verifier ON dsv.verified_by = verifier.id
                WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += @" AND (u.name LIKE @SearchTerm OR u.fname LIKE @SearchTerm OR u.lname LIKE @SearchTerm)";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND COALESCE(dsv.status, 'Pending') = @Status";
            }

            sql += @"
                ORDER BY ds.sale_date DESC, cashier_name";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@Status", status);
            }

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                reports.Add(new DailySalesVerificationModel
                {
                    CashierUserId = reader.GetInt32(0),
                    CashierName = reader.GetString(1),
                    SaleDate = reader.GetDateTime(2),
                    TotalTransactions = reader.GetInt32(3),
                    TotalSales = reader.GetDecimal(4),
                    CashAmount = reader.GetDecimal(5),
                    GCashAmount = reader.GetDecimal(6),
                    ReturnCount = reader.GetInt32(7),
                    TotalReturns = reader.GetDecimal(8),
                    TotalDiscounts = reader.GetDecimal(9),
                    ExpectedCash = reader.GetDecimal(10),
                    ActualCash = reader.GetDecimal(11),
                    CashDiscrepancy = reader.GetDecimal(12),
                    Status = reader.GetString(13),
                    VerifiedAt = reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                    VerifiedByName = reader.IsDBNull(15) ? null : reader.GetString(15)
                });
            }

            return reports;
        }
    }
}

