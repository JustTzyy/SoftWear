using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class SaleItemModel
    {
        public int VariantId { get; set; }
        public int? SizeId { get; set; }
        public int? ColorId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class SaleReportModel
    {
        public int Id { get; set; }
        public string SaleNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal ChangeGiven { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime Timestamps { get; set; }
        public string CashierName { get; set; } = string.Empty;
    }

    public class SaleReportItemModel
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int VariantId { get; set; }
        public int? SizeId { get; set; }
        public int? ColorId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHexValue { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CreateSaleModel
    {
        public List<SaleItemModel> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal ChangeGiven { get; set; }
        public string? ReferenceNumber { get; set; } // For GCash transaction reference
        public int CashierUserId { get; set; }
    }

    public class SaleResult
    {
        public int? SaleId { get; set; }
        public string SaleNumber { get; set; } = string.Empty;
    }

    public class DashboardStatsModel
    {
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalReturns { get; set; }
        public int TodaySales { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TodayReturns { get; set; }
    }

    public class DailySalesData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class TopSellingProductModel
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public int SaleCount { get; set; }
    }

    public class PaymentMethodStatsModel
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RecentTransactionModel
    {
        public int Id { get; set; }
        public string SaleNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime Timestamps { get; set; }
    }

    public class HourlySalesData
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    public interface ISalesService
    {
        Task<SaleResult?> CreateSaleAsync(CreateSaleModel sale, CancellationToken ct = default);
        Task<string> GenerateSaleNumberAsync(CancellationToken ct = default);
        Task<List<SaleReportModel>> GetSalesForReportsAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, bool onlyApprovedDays = false, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetSalesCountForReportsAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, bool onlyApprovedDays = false, CancellationToken ct = default);
        Task<SaleReportModel?> GetSaleForReportAsync(int saleId, int cashierUserId, CancellationToken ct = default);
        Task<List<SaleReportItemModel>> GetSaleItemsForReportAsync(int saleId, CancellationToken ct = default);
        Task<DashboardStatsModel> GetDashboardStatsAsync(int cashierUserId, CancellationToken ct = default);
        Task<List<DailySalesData>> GetDailySalesDataAsync(int cashierUserId, int days = 30, CancellationToken ct = default);
        Task<List<TopSellingProductModel>> GetTopSellingProductsAsync(int cashierUserId, int topCount = 10, CancellationToken ct = default);
        Task<List<PaymentMethodStatsModel>> GetPaymentMethodStatsAsync(int cashierUserId, CancellationToken ct = default);
        Task<List<RecentTransactionModel>> GetRecentTransactionsAsync(int cashierUserId, int count = 5, CancellationToken ct = default);
        Task<List<HourlySalesData>> GetHourlySalesDataAsync(int cashierUserId, CancellationToken ct = default);
        Task<decimal> GetAverageTransactionValueAsync(int cashierUserId, CancellationToken ct = default);
    }

    public class SalesService : ISalesService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<string> GenerateSaleNumberAsync(CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*) + 1 
                FROM dbo.tbl_sales 
                WHERE archives IS NULL 
                AND CAST(timestamps AS DATE) = CAST(SYSUTCDATETIME() AS DATE)";

            using var cmd = new SqlCommand(sql, conn);
            var count = await cmd.ExecuteScalarAsync(ct);
            var sequenceNumber = count != null ? Convert.ToInt32(count) : 1;

            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            return $"SALE-{dateStr}-{sequenceNumber:D4}";
        }

        private async Task<string> GenerateSaleNumberInTransactionAsync(SqlConnection conn, SqlTransaction transaction, CancellationToken ct = default)
        {
            var sql = @"
                SELECT COUNT(*) + 1 
                FROM dbo.tbl_sales 
                WHERE archives IS NULL 
                AND CAST(timestamps AS DATE) = CAST(SYSUTCDATETIME() AS DATE)";

            using var cmd = new SqlCommand(sql, conn, transaction);
            var count = await cmd.ExecuteScalarAsync(ct);
            var sequenceNumber = count != null ? Convert.ToInt32(count) : 1;

            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            return $"SALE-{dateStr}-{sequenceNumber:D4}";
        }

        public async Task<SaleResult?> CreateSaleAsync(CreateSaleModel sale, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var transaction = conn.BeginTransaction();
            try
            {
                // Generate sale number within transaction
                var saleNumber = await GenerateSaleNumberInTransactionAsync(conn, transaction, ct);

                // Insert sale record
                var saleSql = @"
                    INSERT INTO dbo.tbl_sales (sale_number, amount, payment_type, status, user_id, timestamps)
                    VALUES (@SaleNumber, @Amount, @PaymentMethod, 'Completed', @UserId, SYSUTCDATETIME());
                    SELECT SCOPE_IDENTITY();";

                using var saleCmd = new SqlCommand(saleSql, conn, transaction);
                saleCmd.Parameters.AddWithValue("@SaleNumber", saleNumber);
                saleCmd.Parameters.AddWithValue("@Amount", sale.TotalAmount);
                saleCmd.Parameters.AddWithValue("@PaymentMethod", sale.PaymentMethod);
                saleCmd.Parameters.AddWithValue("@UserId", sale.CashierUserId);

                var saleIdObj = await saleCmd.ExecuteScalarAsync(ct);
                if (saleIdObj == null || saleIdObj == DBNull.Value)
                {
                    transaction.Rollback();
                    return null;
                }

                var saleId = Convert.ToInt32(saleIdObj);

                // Insert sale items
                foreach (var item in sale.Items)
                {
                    var itemSql = @"
                        INSERT INTO dbo.tbl_sales_items (sale_id, variant_id, size_id, color_id, quantity, price, subtotal, timestamps)
                        VALUES (@SaleId, @VariantId, @SizeId, @ColorId, @Quantity, @Price, @Subtotal, SYSUTCDATETIME());";

                    using var itemCmd = new SqlCommand(itemSql, conn, transaction);
                    itemCmd.Parameters.AddWithValue("@SaleId", saleId);
                    itemCmd.Parameters.AddWithValue("@VariantId", item.VariantId);
                    itemCmd.Parameters.AddWithValue("@SizeId", (object?)item.SizeId ?? DBNull.Value);
                    itemCmd.Parameters.AddWithValue("@ColorId", (object?)item.ColorId ?? DBNull.Value);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@Price", item.Price);
                    itemCmd.Parameters.AddWithValue("@Subtotal", item.Subtotal);

                    await itemCmd.ExecuteNonQueryAsync(ct);

                    // Create stock_out record to reduce inventory
                    var stockOutSql = @"
                        INSERT INTO dbo.tbl_stock_out (user_id, variant_id, size_id, color_id, quantity_removed, reason, timestamps)
                        VALUES (
                            (SELECT user_id FROM dbo.tbl_variants WHERE id = @VariantId),
                            @VariantId, 
                            @SizeId, 
                            @ColorId, 
                            @Quantity, 
                            'Sale: ' + @SaleNumber,
                            SYSUTCDATETIME()
                        );";

                    using var stockOutCmd = new SqlCommand(stockOutSql, conn, transaction);
                    stockOutCmd.Parameters.AddWithValue("@VariantId", item.VariantId);
                    stockOutCmd.Parameters.AddWithValue("@SizeId", (object?)item.SizeId ?? DBNull.Value);
                    stockOutCmd.Parameters.AddWithValue("@ColorId", (object?)item.ColorId ?? DBNull.Value);
                    stockOutCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    stockOutCmd.Parameters.AddWithValue("@SaleNumber", saleNumber);

                    await stockOutCmd.ExecuteNonQueryAsync(ct);
                }

                // Insert payment record
                var paymentSql = @"
                    INSERT INTO dbo.tbl_payments (sale_id, amount_paid, payment_method, change_given, reference_number, timestamps)
                    VALUES (@SaleId, @AmountPaid, @PaymentMethod, @ChangeGiven, @ReferenceNumber, SYSUTCDATETIME());";

                using var paymentCmd = new SqlCommand(paymentSql, conn, transaction);
                paymentCmd.Parameters.AddWithValue("@SaleId", saleId);
                paymentCmd.Parameters.AddWithValue("@AmountPaid", sale.AmountPaid);
                paymentCmd.Parameters.AddWithValue("@PaymentMethod", sale.PaymentMethod);
                paymentCmd.Parameters.AddWithValue("@ChangeGiven", sale.ChangeGiven);
                paymentCmd.Parameters.AddWithValue("@ReferenceNumber", (object?)sale.ReferenceNumber ?? DBNull.Value);

                await paymentCmd.ExecuteNonQueryAsync(ct);

                transaction.Commit();
                return new SaleResult
                {
                    SaleId = saleId,
                    SaleNumber = saleNumber
                };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<SaleReportModel>> GetSalesForReportsAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, bool onlyApprovedDays = false, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var sales = new List<SaleReportModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT s.id, s.sale_number, s.amount, s.payment_type, s.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name,
                       p.amount_paid, p.change_given, p.reference_number
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                LEFT JOIN dbo.tbl_payments p ON s.id = p.sale_id
                WHERE s.archives IS NULL AND s.status = 'Completed'
                AND (@CashierUserId = 0 OR s.user_id = @CashierUserId)";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (s.sale_number LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            // Filter by approved daily sales verification if requested
            if (onlyApprovedDays)
            {
                sql += @"
                    AND EXISTS (
                        SELECT 1 FROM dbo.tbl_daily_sales_verifications dsv
                        WHERE dsv.cashier_user_id = s.user_id
                        AND CAST(dsv.sale_date AS DATE) = CAST(s.timestamps AS DATE)
                        AND dsv.status = 'Approved'
                        AND dsv.archived_at IS NULL)";
            }

            sql += " ORDER BY s.timestamps DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
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
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
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

            return sales;
        }

        public async Task<int> GetSalesCountForReportsAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, bool onlyApprovedDays = false, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_sales s
                WHERE s.archives IS NULL AND s.status = 'Completed'
                AND (@CashierUserId = 0 OR s.user_id = @CashierUserId)";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (s.sale_number LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            // Filter by approved daily sales verification if requested
            if (onlyApprovedDays)
            {
                sql += @"
                    AND EXISTS (
                        SELECT 1 FROM dbo.tbl_daily_sales_verifications dsv
                        WHERE dsv.cashier_user_id = s.user_id
                        AND CAST(dsv.sale_date AS DATE) = CAST(s.timestamps AS DATE)
                        AND dsv.status = 'Approved'
                        AND dsv.archived_at IS NULL)";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
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

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<SaleReportModel?> GetSaleForReportAsync(int saleId, int cashierUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT s.id, s.sale_number, s.amount, s.payment_type, s.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name,
                       p.amount_paid, p.change_given, p.reference_number
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                LEFT JOIN dbo.tbl_payments p ON s.id = p.sale_id
                WHERE s.id = @SaleId
                AND (@CashierUserId = 0 OR s.user_id = @CashierUserId)
                AND s.archives IS NULL AND s.status = 'Completed'";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SaleId", saleId);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new SaleReportModel
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
                };
            }

            return null;
        }

        public async Task<List<SaleReportItemModel>> GetSaleItemsForReportAsync(int saleId, CancellationToken ct = default)
        {
            var items = new List<SaleReportItemModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT si.id, si.sale_id, si.variant_id, si.size_id, si.color_id,
                       p.name as product_name, v.name as variant_name,
                       sz.name as size_name, c.name as color_name, c.hex_value as color_hex,
                       si.quantity, si.price, si.subtotal
                FROM dbo.tbl_sales_items si
                INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                LEFT JOIN dbo.tbl_sizes sz ON si.size_id = sz.id
                LEFT JOIN dbo.tbl_colors c ON si.color_id = c.id
                WHERE si.sale_id = @SaleId AND si.archives IS NULL
                ORDER BY si.id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SaleId", saleId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new SaleReportItemModel
                {
                    Id = reader.GetInt32(0),
                    SaleId = reader.GetInt32(1),
                    VariantId = reader.GetInt32(2),
                    SizeId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    ColorId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    ProductName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    VariantName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    SizeName = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ColorName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ColorHexValue = reader.IsDBNull(9) ? null : reader.GetString(9),
                    Quantity = reader.GetInt32(10),
                    Price = reader.GetDecimal(11),
                    Subtotal = reader.GetDecimal(12)
                });
            }

            return items;
        }

        public async Task<DashboardStatsModel> GetDashboardStatsAsync(int cashierUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var today = DateTime.Today;

            // Get total sales count and revenue
            var totalSalesSql = @"
                SELECT COUNT(*), COALESCE(SUM(s.amount), 0)
                FROM dbo.tbl_sales s
                WHERE s.archives IS NULL AND s.status = 'Completed' AND s.user_id = @CashierUserId";

            // Get today's sales count and revenue
            var todaySalesSql = @"
                SELECT COUNT(*), COALESCE(SUM(s.amount), 0)
                FROM dbo.tbl_sales s
                WHERE s.archives IS NULL AND s.status = 'Completed' AND s.user_id = @CashierUserId
                AND CAST(s.timestamps AS DATE) = @Today";

            // Get total returns count
            var totalReturnsSql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_returns r
                WHERE r.archives IS NULL AND r.user_id = @CashierUserId";

            // Get today's returns count
            var todayReturnsSql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_returns r
                WHERE r.archives IS NULL AND r.user_id = @CashierUserId
                AND CAST(r.timestamps AS DATE) = @Today";

            var stats = new DashboardStatsModel();

            // Get total sales
            using (var cmd = new SqlCommand(totalSalesSql, conn))
            {
                cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    stats.TotalSales = reader.GetInt32(0);
                    stats.TotalRevenue = reader.GetDecimal(1);
                }
            }

            // Get today's sales
            using (var cmd = new SqlCommand(todaySalesSql, conn))
            {
                cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                cmd.Parameters.AddWithValue("@Today", today);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    stats.TodaySales = reader.GetInt32(0);
                    stats.TodayRevenue = reader.GetDecimal(1);
                }
            }

            // Get total returns
            using (var cmd = new SqlCommand(totalReturnsSql, conn))
            {
                cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                var result = await cmd.ExecuteScalarAsync(ct);
                stats.TotalReturns = result != null ? Convert.ToInt32(result) : 0;
            }

            // Get today's returns
            using (var cmd = new SqlCommand(todayReturnsSql, conn))
            {
                cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
                cmd.Parameters.AddWithValue("@Today", today);
                var result = await cmd.ExecuteScalarAsync(ct);
                stats.TodayReturns = result != null ? Convert.ToInt32(result) : 0;
            }

            return stats;
        }

        public async Task<List<DailySalesData>> GetDailySalesDataAsync(int cashierUserId, int days = 30, CancellationToken ct = default)
        {
            var data = new List<DailySalesData>();
            var startDate = DateTime.Today.AddDays(-days);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT CAST(s.timestamps AS DATE) as sale_date,
                       COUNT(*) as sale_count,
                       COALESCE(SUM(s.amount), 0) as total_amount
                FROM dbo.tbl_sales s
                WHERE s.archives IS NULL 
                AND s.status = 'Completed' 
                AND s.user_id = @CashierUserId
                AND CAST(s.timestamps AS DATE) >= @StartDate
                GROUP BY CAST(s.timestamps AS DATE)
                ORDER BY sale_date";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            cmd.Parameters.AddWithValue("@StartDate", startDate);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                data.Add(new DailySalesData
                {
                    Date = reader.GetDateTime(0),
                    Count = reader.GetInt32(1),
                    Amount = reader.GetDecimal(2)
                });
            }

            // Fill in missing dates with zero values
            var allDates = new List<DailySalesData>();
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var existing = data.FirstOrDefault(d => d.Date.Date == date.Date);
                allDates.Add(existing ?? new DailySalesData { Date = date, Count = 0, Amount = 0 });
            }

            return allDates;
        }

        public async Task<List<TopSellingProductModel>> GetTopSellingProductsAsync(int cashierUserId, int topCount = 10, CancellationToken ct = default)
        {
            var products = new List<TopSellingProductModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT TOP (@TopCount)
                       v.product_id, si.variant_id,
                       p.name as product_name, v.name as variant_name,
                       SUM(si.quantity) as total_quantity,
                       SUM(si.subtotal) as total_revenue,
                       COUNT(DISTINCT si.sale_id) as sale_count
                FROM dbo.tbl_sales_items si
                INNER JOIN dbo.tbl_sales s ON si.sale_id = s.id
                INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                WHERE s.archives IS NULL 
                AND s.status = 'Completed' 
                AND s.user_id = @CashierUserId
                AND si.archives IS NULL
                GROUP BY v.product_id, si.variant_id, p.name, v.name
                ORDER BY total_quantity DESC, total_revenue DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            cmd.Parameters.AddWithValue("@TopCount", topCount);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                products.Add(new TopSellingProductModel
                {
                    ProductId = reader.GetInt32(0),
                    VariantId = reader.GetInt32(1),
                    ProductName = reader.GetString(2),
                    VariantName = reader.GetString(3),
                    TotalQuantity = reader.GetInt32(4),
                    TotalRevenue = reader.GetDecimal(5),
                    SaleCount = reader.GetInt32(6)
                });
            }

            return products;
        }

        public async Task<List<PaymentMethodStatsModel>> GetPaymentMethodStatsAsync(int cashierUserId, CancellationToken ct = default)
        {
            var stats = new List<PaymentMethodStatsModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT s.payment_type,
                       COUNT(*) as transaction_count,
                       COALESCE(SUM(s.amount), 0) as total_amount
                FROM dbo.tbl_sales s
                WHERE s.archives IS NULL 
                AND s.status = 'Completed' 
                AND s.user_id = @CashierUserId
                GROUP BY s.payment_type";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);

            decimal totalAmount = 0;
            using (var reader = await cmd.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    var stat = new PaymentMethodStatsModel
                    {
                        PaymentMethod = reader.GetString(0),
                        Count = reader.GetInt32(1),
                        TotalAmount = reader.GetDecimal(2)
                    };
                    stats.Add(stat);
                    totalAmount += stat.TotalAmount;
                }
            }

            // Calculate percentages
            if (totalAmount > 0)
            {
                foreach (var stat in stats)
                {
                    stat.Percentage = (stat.TotalAmount / totalAmount) * 100;
                }
            }

            return stats.OrderByDescending(s => s.TotalAmount).ToList();
        }

        public async Task<List<RecentTransactionModel>> GetRecentTransactionsAsync(int cashierUserId, int count = 5, CancellationToken ct = default)
        {
            var transactions = new List<RecentTransactionModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT TOP (@Count)
                       s.id, s.sale_number, s.amount, s.payment_type, s.timestamps
                FROM dbo.tbl_sales s
                WHERE s.archives IS NULL 
                AND s.status = 'Completed' 
                AND s.user_id = @CashierUserId
                ORDER BY s.timestamps DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                transactions.Add(new RecentTransactionModel
                {
                    Id = reader.GetInt32(0),
                    SaleNumber = reader.GetString(1),
                    Amount = reader.GetDecimal(2),
                    PaymentMethod = reader.GetString(3),
                    Timestamps = reader.GetDateTime(4)
                });
            }

            return transactions;
        }

        public async Task<List<HourlySalesData>> GetHourlySalesDataAsync(int cashierUserId, CancellationToken ct = default)
        {
            var data = new List<HourlySalesData>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT DATEPART(HOUR, s.timestamps) as sale_hour,
                       COUNT(*) as sale_count,
                       COALESCE(SUM(s.amount), 0) as total_amount
                FROM dbo.tbl_sales s
                WHERE s.archives IS NULL 
                AND s.status = 'Completed' 
                AND s.user_id = @CashierUserId
                GROUP BY DATEPART(HOUR, s.timestamps)
                ORDER BY sale_hour";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                data.Add(new HourlySalesData
                {
                    Hour = reader.GetInt32(0),
                    Count = reader.GetInt32(1),
                    Amount = reader.GetDecimal(2)
                });
            }

            // Fill in missing hours with zero values
            var allHours = new List<HourlySalesData>();
            for (int hour = 0; hour < 24; hour++)
            {
                var existing = data.FirstOrDefault(d => d.Hour == hour);
                allHours.Add(existing ?? new HourlySalesData { Hour = hour, Count = 0, Amount = 0 });
            }

            return allHours;
        }

        public async Task<decimal> GetAverageTransactionValueAsync(int cashierUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COALESCE(AVG(s.amount), 0)
                FROM dbo.tbl_sales s
                WHERE s.archives IS NULL 
                AND s.status = 'Completed' 
                AND s.user_id = @CashierUserId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null ? Convert.ToDecimal(result) : 0;
        }
    }
}

