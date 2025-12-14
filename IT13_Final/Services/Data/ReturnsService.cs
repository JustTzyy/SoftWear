using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class SaleModel
    {
        public int Id { get; set; }
        public string SaleNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime Timestamps { get; set; }
        public string CashierName { get; set; } = string.Empty;
    }

    public class ReturnSaleItemModel
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

    public class ReturnItemModel
    {
        public int SaleItemId { get; set; }
        public int VariantId { get; set; }
        public int? SizeId { get; set; }
        public int? ColorId { get; set; }
        public int Quantity { get; set; }
        public string Condition { get; set; } = "New"; // New, Used, Damaged
    }

    public class CreateReturnModel
    {
        public int SaleId { get; set; }
        public List<ReturnItemModel> Items { get; set; } = new();
        public string? Reason { get; set; }
        public int CashierUserId { get; set; }
    }

    public class ReturnResult
    {
        public int? ReturnId { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
    }

    public class ReturnReportModel
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public int SaleId { get; set; }
        public string SaleNumber { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamps { get; set; }
        public string CashierName { get; set; } = string.Empty;
    }

    public class ReturnReportItemModel
    {
        public int Id { get; set; }
        public int ReturnId { get; set; }
        public int VariantId { get; set; }
        public int? SizeId { get; set; }
        public int? ColorId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHexValue { get; set; }
        public int Quantity { get; set; }
        public string Condition { get; set; } = string.Empty;
    }

    public class DailyReturnsData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public interface IReturnsService
    {
        Task<List<SaleModel>> GetSalesAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetSalesCountAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<SaleModel?> GetSaleDetailsAsync(int saleId, int cashierUserId, CancellationToken ct = default);
        Task<List<ReturnSaleItemModel>> GetSaleItemsAsync(int saleId, CancellationToken ct = default);
        Task<ReturnResult?> CreateReturnAsync(CreateReturnModel returnModel, CancellationToken ct = default);
        Task<string> GenerateReturnNumberAsync(CancellationToken ct = default);
        Task<bool> UpdateReturnStatusAsync(int returnId, string status, int approvedByUserId, CancellationToken ct = default);
        Task<List<ReturnReportModel>> GetReturnsForReportsAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetReturnsCountForReportsAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<ReturnReportModel?> GetReturnForReportAsync(int returnId, int cashierUserId, CancellationToken ct = default);
        Task<List<ReturnReportItemModel>> GetReturnItemsForReportAsync(int returnId, CancellationToken ct = default);
        Task<List<DailyReturnsData>> GetDailyReturnsDataAsync(int cashierUserId, int days = 30, CancellationToken ct = default);
        Task<List<ReturnReportModel>> GetPendingReturnsForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetPendingReturnsCountForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<ReturnReportModel?> GetReturnDetailsForAccountingAsync(int returnId, int sellerUserId, CancellationToken ct = default);
    }

    public class ReturnsService : IReturnsService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<string> GenerateReturnNumberAsync(CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*) + 1 
                FROM dbo.tbl_returns 
                WHERE archives IS NULL 
                AND CAST(timestamps AS DATE) = CAST(SYSUTCDATETIME() AS DATE)";

            using var cmd = new SqlCommand(sql, conn);
            var count = await cmd.ExecuteScalarAsync(ct);
            var sequenceNumber = count != null ? Convert.ToInt32(count) : 1;

            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            return $"RET-{dateStr}-{sequenceNumber:D4}";
        }

        public async Task<List<SaleModel>> GetSalesAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var sales = new List<SaleModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT s.id, s.sale_number, s.amount, s.payment_type, s.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                WHERE s.archives IS NULL AND s.status = 'Completed' AND s.user_id = @CashierUserId";

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
                sales.Add(new SaleModel
                {
                    Id = reader.GetInt32(0),
                    SaleNumber = reader.GetString(1),
                    Amount = reader.GetDecimal(2),
                    PaymentMethod = reader.GetString(3),
                    Timestamps = reader.GetDateTime(4),
                    CashierName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                });
            }

            return sales;
        }

        public async Task<int> GetSalesCountAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_sales s
                WHERE s.archives IS NULL AND s.status = 'Completed' AND s.user_id = @CashierUserId";

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

        public async Task<SaleModel?> GetSaleDetailsAsync(int saleId, int cashierUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT s.id, s.sale_number, s.amount, s.payment_type, s.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                WHERE s.id = @SaleId AND s.user_id = @CashierUserId AND s.archives IS NULL AND s.status = 'Completed'";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SaleId", saleId);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new SaleModel
                {
                    Id = reader.GetInt32(0),
                    SaleNumber = reader.GetString(1),
                    Amount = reader.GetDecimal(2),
                    PaymentMethod = reader.GetString(3),
                    Timestamps = reader.GetDateTime(4),
                    CashierName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                };
            }

            return null;
        }

        public async Task<List<ReturnSaleItemModel>> GetSaleItemsAsync(int saleId, CancellationToken ct = default)
        {
            var items = new List<ReturnSaleItemModel>();

            try
            {
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
                    items.Add(new ReturnSaleItemModel
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSaleItemsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }

            return items;
        }

        private async Task<string> GenerateReturnNumberInTransactionAsync(SqlConnection conn, SqlTransaction transaction, CancellationToken ct = default)
        {
            var sql = @"
                SELECT COUNT(*) + 1 
                FROM dbo.tbl_returns 
                WHERE archives IS NULL 
                AND CAST(timestamps AS DATE) = CAST(SYSUTCDATETIME() AS DATE)";

            using var cmd = new SqlCommand(sql, conn, transaction);
            var count = await cmd.ExecuteScalarAsync(ct);
            var sequenceNumber = count != null ? Convert.ToInt32(count) : 1;

            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            return $"RET-{dateStr}-{sequenceNumber:D4}";
        }

        public async Task<ReturnResult?> CreateReturnAsync(CreateReturnModel returnModel, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var transaction = conn.BeginTransaction();
            try
            {
                // Generate return number within transaction
                var returnNumber = await GenerateReturnNumberInTransactionAsync(conn, transaction, ct);

                // Insert return record
                var returnSql = @"
                    INSERT INTO dbo.tbl_returns (return_number, sale_id, reason, status, user_id, timestamps)
                    VALUES (@ReturnNumber, @SaleId, @Reason, 'Pending', @UserId, SYSUTCDATETIME());
                    SELECT SCOPE_IDENTITY();";

                using var returnCmd = new SqlCommand(returnSql, conn, transaction);
                returnCmd.Parameters.AddWithValue("@ReturnNumber", returnNumber);
                returnCmd.Parameters.AddWithValue("@SaleId", returnModel.SaleId);
                returnCmd.Parameters.AddWithValue("@Reason", (object?)returnModel.Reason ?? DBNull.Value);
                returnCmd.Parameters.AddWithValue("@UserId", returnModel.CashierUserId);

                var returnIdObj = await returnCmd.ExecuteScalarAsync(ct);
                if (returnIdObj == null || returnIdObj == DBNull.Value)
                {
                    transaction.Rollback();
                    return null;
                }

                var returnId = Convert.ToInt32(returnIdObj);

                // Insert return items
                foreach (var item in returnModel.Items)
                {
                    var itemSql = @"
                        INSERT INTO dbo.tbl_return_items (return_id, sale_item_id, variant_id, size_id, color_id, quantity, condition, timestamps)
                        VALUES (@ReturnId, @SaleItemId, @VariantId, @SizeId, @ColorId, @Quantity, @Condition, SYSUTCDATETIME());";

                    using var itemCmd = new SqlCommand(itemSql, conn, transaction);
                    itemCmd.Parameters.AddWithValue("@ReturnId", returnId);
                    itemCmd.Parameters.AddWithValue("@SaleItemId", item.SaleItemId);
                    itemCmd.Parameters.AddWithValue("@VariantId", item.VariantId);
                    itemCmd.Parameters.AddWithValue("@SizeId", (object?)item.SizeId ?? DBNull.Value);
                    itemCmd.Parameters.AddWithValue("@ColorId", (object?)item.ColorId ?? DBNull.Value);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@Condition", item.Condition);

                    await itemCmd.ExecuteNonQueryAsync(ct);

                    // Note: Stock will only be added when the return is approved by the seller
                    // This is handled in UpdateReturnStatusAsync method
                }

                transaction.Commit();
                return new ReturnResult
                {
                    ReturnId = returnId,
                    ReturnNumber = returnNumber
                };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<ReturnReportModel>> GetReturnsForReportsAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var returns = new List<ReturnReportModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT r.id, r.return_number, r.sale_id, s.sale_number, r.reason, r.status, r.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                INNER JOIN dbo.tbl_users u ON r.user_id = u.id
                WHERE r.archives IS NULL AND r.user_id = @CashierUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (r.return_number LIKE @SearchTerm OR s.sale_number LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) <= @EndDate";
            }

            sql += " ORDER BY r.timestamps DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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

            return returns;
        }

        public async Task<int> GetReturnsCountForReportsAsync(int cashierUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                WHERE r.archives IS NULL AND r.user_id = @CashierUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (r.return_number LIKE @SearchTerm OR s.sale_number LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) <= @EndDate";
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

        public async Task<ReturnReportModel?> GetReturnForReportAsync(int returnId, int cashierUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT r.id, r.return_number, r.sale_id, s.sale_number, r.reason, r.status, r.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                INNER JOIN dbo.tbl_users u ON r.user_id = u.id
                WHERE r.id = @ReturnId
                AND (@CashierUserId = 0 OR r.user_id = @CashierUserId)
                AND r.archives IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ReturnId", returnId);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new ReturnReportModel
                {
                    Id = reader.GetInt32(0),
                    ReturnNumber = reader.GetString(1),
                    SaleId = reader.GetInt32(2),
                    SaleNumber = reader.GetString(3),
                    Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Status = reader.GetString(5),
                    Timestamps = reader.GetDateTime(6),
                    CashierName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                };
            }

            return null;
        }

        public async Task<List<ReturnReportItemModel>> GetReturnItemsForReportAsync(int returnId, CancellationToken ct = default)
        {
            var items = new List<ReturnReportItemModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT ri.id, ri.return_id, ri.variant_id, ri.size_id, ri.color_id,
                       p.name as product_name, v.name as variant_name,
                       sz.name as size_name, c.name as color_name, c.hex_value as color_hex,
                       ri.quantity, ri.condition
                FROM dbo.tbl_return_items ri
                INNER JOIN dbo.tbl_variants v ON ri.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                LEFT JOIN dbo.tbl_sizes sz ON ri.size_id = sz.id
                LEFT JOIN dbo.tbl_colors c ON ri.color_id = c.id
                WHERE ri.return_id = @ReturnId AND ri.archives IS NULL
                ORDER BY ri.id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ReturnId", returnId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new ReturnReportItemModel
                {
                    Id = reader.GetInt32(0),
                    ReturnId = reader.GetInt32(1),
                    VariantId = reader.GetInt32(2),
                    SizeId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    ColorId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    ProductName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    VariantName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    SizeName = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ColorName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ColorHexValue = reader.IsDBNull(9) ? null : reader.GetString(9),
                    Quantity = reader.GetInt32(10),
                    Condition = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
                });
            }

            return items;
        }

        public async Task<List<DailyReturnsData>> GetDailyReturnsDataAsync(int cashierUserId, int days = 30, CancellationToken ct = default)
        {
            var data = new List<DailyReturnsData>();
            var startDate = DateTime.Today.AddDays(-days);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT CAST(r.timestamps AS DATE) as return_date,
                       COUNT(*) as return_count
                FROM dbo.tbl_returns r
                WHERE r.archives IS NULL 
                AND r.user_id = @CashierUserId
                AND CAST(r.timestamps AS DATE) >= @StartDate
                GROUP BY CAST(r.timestamps AS DATE)
                ORDER BY return_date";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId);
            cmd.Parameters.AddWithValue("@StartDate", startDate);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                data.Add(new DailyReturnsData
                {
                    Date = reader.GetDateTime(0),
                    Count = reader.GetInt32(1)
                });
            }

            // Fill in missing dates with zero values
            var allDates = new List<DailyReturnsData>();
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var existing = data.FirstOrDefault(d => d.Date.Date == date.Date);
                allDates.Add(existing ?? new DailyReturnsData { Date = date, Count = 0 });
            }

            return allDates;
        }

        public async Task<bool> UpdateReturnStatusAsync(int returnId, string status, int approvedByUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var transaction = conn.BeginTransaction();
            try
            {
                // Get current status
                var getStatusSql = @"
                    SELECT status 
                    FROM dbo.tbl_returns 
                    WHERE id = @ReturnId AND archives IS NULL";

                using var getStatusCmd = new SqlCommand(getStatusSql, conn, transaction);
                getStatusCmd.Parameters.AddWithValue("@ReturnId", returnId);

                var currentStatusObj = await getStatusCmd.ExecuteScalarAsync(ct);
                if (currentStatusObj == null || currentStatusObj == DBNull.Value)
                {
                    transaction.Rollback();
                    return false;
                }

                var currentStatus = currentStatusObj.ToString() ?? string.Empty;

                // Update return status
                var updateSql = @"
                    UPDATE dbo.tbl_returns 
                    SET status = @Status, 
                        approved_by = @ApprovedByUserId,
                        timestamps = SYSUTCDATETIME()
                    WHERE id = @ReturnId AND archives IS NULL";

                using var updateCmd = new SqlCommand(updateSql, conn, transaction);
                updateCmd.Parameters.AddWithValue("@Status", status);
                updateCmd.Parameters.AddWithValue("@ApprovedByUserId", approvedByUserId);
                updateCmd.Parameters.AddWithValue("@ReturnId", returnId);

                await updateCmd.ExecuteNonQueryAsync(ct);

                // If status is being changed to "Approved", add stock back to inventory and reverse fees
                if (status == "Approved" && currentStatus != "Approved")
                {
                    // Get all return items for this return with price info for fee calculation
                    var itemsSql = @"
                        SELECT ri.variant_id, ri.size_id, ri.color_id, ri.quantity, v.user_id, v.cost_price, si.price, si.subtotal
                        FROM dbo.tbl_return_items ri
                        INNER JOIN dbo.tbl_variants v ON ri.variant_id = v.id
                        INNER JOIN dbo.tbl_sales_items si ON ri.sale_item_id = si.id
                        WHERE ri.return_id = @ReturnId AND ri.archives IS NULL";

                    using var itemsCmd = new SqlCommand(itemsSql, conn, transaction);
                    itemsCmd.Parameters.AddWithValue("@ReturnId", returnId);

                    // Read all items into a list first to avoid DataReader conflict
                    var returnItems = new List<(int VariantId, int? SizeId, int? ColorId, int Quantity, int VariantOwnerUserId, decimal CostPrice, decimal Price, decimal OriginalSubtotal)>();
                    
                    using (var reader = await itemsCmd.ExecuteReaderAsync(ct))
                    {
                        while (await reader.ReadAsync(ct))
                        {
                            returnItems.Add((
                                reader.GetInt32(0),
                                reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                reader.GetInt32(3),
                                reader.GetInt32(4),
                                reader.GetDecimal(5),
                                reader.GetDecimal(6),
                                reader.GetDecimal(7)
                            ));
                        }
                    }

                    // Calculate total return amount for fee reversal
                    decimal totalReturnAmount = 0;
                    int sellerUserId = 0;

                    // Now process all items after reader is closed
                    foreach (var item in returnItems)
                    {
                        sellerUserId = item.VariantOwnerUserId;
                        
                        // Calculate the return subtotal based on returned quantity
                        decimal returnSubtotal = item.Price * item.Quantity;
                        totalReturnAmount += returnSubtotal;

                        // Create stock_in record to add inventory back
                        var stockInSql = @"
                            INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
                            VALUES (@UserId, @VariantId, @SizeId, @ColorId, @Quantity, @CostPrice, NULL, SYSUTCDATETIME());";

                        using var stockInCmd = new SqlCommand(stockInSql, conn, transaction);
                        stockInCmd.Parameters.AddWithValue("@UserId", item.VariantOwnerUserId);
                        stockInCmd.Parameters.AddWithValue("@VariantId", item.VariantId);
                        stockInCmd.Parameters.AddWithValue("@SizeId", (object?)item.SizeId ?? DBNull.Value);
                        stockInCmd.Parameters.AddWithValue("@ColorId", (object?)item.ColorId ?? DBNull.Value);
                        stockInCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        stockInCmd.Parameters.AddWithValue("@CostPrice", item.CostPrice);

                        await stockInCmd.ExecuteNonQueryAsync(ct);
                    }

                    // Reverse admin fee for the returned items (create negative admin fee transaction)
                    if (sellerUserId > 0 && totalReturnAmount > 0)
                    {
                        // Get seller's subscription fee percentage
                        var getFeeSql = @"
                            SELECT COALESCE(sp.admin_fee_percentage, 0)
                            FROM dbo.tbl_seller_subscriptions ss
                            INNER JOIN dbo.tbl_subscription_plans sp ON ss.plan_id = sp.id
                            WHERE ss.seller_user_id = @SellerUserId AND ss.status = 'Active'";

                        using var getFeeCmd = new SqlCommand(getFeeSql, conn, transaction);
                        getFeeCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

                        var feePercentageObj = await getFeeCmd.ExecuteScalarAsync(ct);
                        decimal feePercentage = feePercentageObj != null && feePercentageObj != DBNull.Value 
                            ? Convert.ToDecimal(feePercentageObj) 
                            : 0m;

                        if (feePercentage > 0)
                        {
                            // Calculate the admin fee to reverse (negative amount)
                            decimal adminFeeToReverse = totalReturnAmount * (feePercentage / 100m);

                            // Get the return number for reference
                            var getReturnNumSql = "SELECT return_number FROM dbo.tbl_returns WHERE id = @ReturnId";
                            using var getReturnNumCmd = new SqlCommand(getReturnNumSql, conn, transaction);
                            getReturnNumCmd.Parameters.AddWithValue("@ReturnId", returnId);
                            var returnNumberObj = await getReturnNumCmd.ExecuteScalarAsync(ct);
                            string returnNumber = returnNumberObj?.ToString() ?? $"RET-{returnId}";

                            // Get seller's active subscription ID
                            var getSubSql = "SELECT id FROM dbo.tbl_seller_subscriptions WHERE seller_user_id = @SellerUserId AND status = 'Active'";
                            using var getSubCmd = new SqlCommand(getSubSql, conn, transaction);
                            getSubCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                            var subscriptionIdObj = await getSubCmd.ExecuteScalarAsync(ct);
                            int? subscriptionId = subscriptionIdObj != null && subscriptionIdObj != DBNull.Value ? Convert.ToInt32(subscriptionIdObj) : (int?)null;

                            // Insert negative admin fee transaction to reverse the fee
                            var insertFeeSql = @"
                                INSERT INTO dbo.tbl_subscription_transactions 
                                    (seller_user_id, subscription_id, sale_id, return_id, transaction_type, sale_amount, admin_fee_percentage, admin_fee_amount, status, created_at)
                                VALUES 
                                    (@SellerUserId, @SubscriptionId, NULL, @ReturnId, 'AdminFeeReversal', @SaleAmount, @FeePercentage, @FeeAmount, 'Collected', SYSUTCDATETIME())";

                            using var insertFeeCmd = new SqlCommand(insertFeeSql, conn, transaction);
                            insertFeeCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                            insertFeeCmd.Parameters.AddWithValue("@SubscriptionId", (object?)subscriptionId ?? DBNull.Value);
                            insertFeeCmd.Parameters.AddWithValue("@ReturnId", returnId);
                            insertFeeCmd.Parameters.AddWithValue("@SaleAmount", -totalReturnAmount); // Negative amount for return
                            insertFeeCmd.Parameters.AddWithValue("@FeePercentage", feePercentage);
                            insertFeeCmd.Parameters.AddWithValue("@FeeAmount", -adminFeeToReverse); // Negative fee for reversal

                            await insertFeeCmd.ExecuteNonQueryAsync(ct);
                        }
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<ReturnReportModel>> GetPendingReturnsForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var returns = new List<ReturnReportModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Get returns created by cashiers that belong to this seller
            // Returns are filtered by: cashier's user_id = sellerUserId AND return status = 'Pending'
            var sql = @"
                SELECT r.id, r.return_number, r.sale_id, s.sale_number, r.reason, r.status, r.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                INNER JOIN dbo.tbl_users u ON r.user_id = u.id
                WHERE r.archives IS NULL 
                AND r.status = 'Pending'
                AND u.user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (r.return_number LIKE @SearchTerm OR s.sale_number LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) <= @EndDate";
            }

            sql += " ORDER BY r.timestamps DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
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

            return returns;
        }

        public async Task<int> GetPendingReturnsCountForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                INNER JOIN dbo.tbl_users u ON r.user_id = u.id
                WHERE r.archives IS NULL 
                AND r.status = 'Pending'
                AND u.user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (r.return_number LIKE @SearchTerm OR s.sale_number LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) <= @EndDate";
            }

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

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<ReturnReportModel?> GetReturnDetailsForAccountingAsync(int returnId, int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT r.id, r.return_number, r.sale_id, s.sale_number, r.reason, r.status, r.timestamps,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                INNER JOIN dbo.tbl_users u ON r.user_id = u.id
                WHERE r.id = @ReturnId 
                AND r.archives IS NULL 
                AND u.user_id = @SellerUserId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ReturnId", returnId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new ReturnReportModel
                {
                    Id = reader.GetInt32(0),
                    ReturnNumber = reader.GetString(1),
                    SaleId = reader.GetInt32(2),
                    SaleNumber = reader.GetString(3),
                    Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Status = reader.GetString(5),
                    Timestamps = reader.GetDateTime(6),
                    CashierName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                };
            }

            return null;
        }
    }
}

