using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class StockOutModel
    {
        public int Id { get; set; }
        public int QuantityRemoved { get; set; }
        public string? Reason { get; set; }
        public DateTime Timestamps { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    public class StockOutDetailsModel
    {
        public int Id { get; set; }
        public int QuantityRemoved { get; set; }
        public string? Reason { get; set; }
        public DateTime Timestamps { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHexValue { get; set; }
    }

    public class DailyStockOutData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int Quantity { get; set; }
    }

    public interface IStockOutService
    {
        Task<List<StockOutModel>> GetStockOutsAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetStockOutsCountAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<StockOutDetailsModel?> GetStockOutDetailsAsync(int stockOutId, int userId, CancellationToken ct = default);
        Task<int?> CreateStockOutAsync(int userId, int variantId, int sizeId, int colorId, int quantityRemoved, string? reason, CancellationToken ct = default);
        Task<List<DailyStockOutData>> GetDailyStockOutDataAsync(int userId, int days = 30, CancellationToken ct = default);
    }

    public class StockOutService : IStockOutService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<StockOutModel>> GetStockOutsAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var stockOuts = new List<StockOutModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT so.id, so.quantity_removed, so.reason, so.timestamps, so.variant_id, 
                       v.name as variant_name, p.name as product_name, u.name as user_name
                FROM dbo.tbl_stock_out so
                INNER JOIN dbo.tbl_variants v ON so.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                INNER JOIN dbo.tbl_users u ON so.user_id = u.id
                WHERE so.archives IS NULL AND so.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm OR so.reason LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(so.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(so.timestamps AS DATE) <= @EndDate";
            }

            sql += @" ORDER BY so.timestamps DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
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
                stockOuts.Add(new StockOutModel
                {
                    Id = reader.GetInt32(0),
                    QuantityRemoved = reader.GetInt32(1),
                    Reason = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Timestamps = reader.GetDateTime(3),
                    VariantId = reader.GetInt32(4),
                    VariantName = reader.GetString(5),
                    ProductName = reader.GetString(6),
                    UserName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                });
            }

            return stockOuts;
        }

        public async Task<int> GetStockOutsCountAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_stock_out so
                INNER JOIN dbo.tbl_variants v ON so.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                WHERE so.archives IS NULL AND so.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm OR so.reason LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(so.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(so.timestamps AS DATE) <= @EndDate";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
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

        public async Task<StockOutDetailsModel?> GetStockOutDetailsAsync(int stockOutId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT so.id, so.quantity_removed, so.reason, so.timestamps, so.variant_id, 
                       v.name as variant_name, p.name as product_name, u.name as user_name,
                       so.size_id, sz.name as size_name, so.color_id, c.name as color_name, c.hex_value as color_hex
                FROM dbo.tbl_stock_out so
                INNER JOIN dbo.tbl_variants v ON so.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                INNER JOIN dbo.tbl_users u ON so.user_id = u.id
                LEFT JOIN dbo.tbl_sizes sz ON so.size_id = sz.id
                LEFT JOIN dbo.tbl_colors c ON so.color_id = c.id
                WHERE so.id = @StockOutId
                AND (@UserId = 0 OR so.user_id = @UserId)
                AND so.archives IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StockOutId", stockOutId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new StockOutDetailsModel
                {
                    Id = reader.GetInt32(0),
                    QuantityRemoved = reader.GetInt32(1),
                    Reason = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Timestamps = reader.GetDateTime(3),
                    VariantId = reader.GetInt32(4),
                    VariantName = reader.GetString(5),
                    ProductName = reader.GetString(6),
                    UserName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    SizeId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                    SizeName = reader.IsDBNull(9) ? null : reader.GetString(9),
                    ColorId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                    ColorName = reader.IsDBNull(11) ? null : reader.GetString(11),
                    ColorHexValue = reader.IsDBNull(12) ? null : reader.GetString(12)
                };
            }

            return null;
        }

        public async Task<int?> CreateStockOutAsync(int userId, int variantId, int sizeId, int colorId, int quantityRemoved, string? reason, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            try
            {
                var sql = @"
                    INSERT INTO dbo.tbl_stock_out (user_id, variant_id, size_id, color_id, quantity_removed, reason, timestamps)
                    VALUES (@UserId, @VariantId, @SizeId, @ColorId, @QuantityRemoved, @Reason, SYSUTCDATETIME());
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@VariantId", variantId);
                cmd.Parameters.AddWithValue("@SizeId", sizeId);
                cmd.Parameters.AddWithValue("@ColorId", colorId);
                cmd.Parameters.AddWithValue("@QuantityRemoved", quantityRemoved);
                cmd.Parameters.AddWithValue("@Reason", (object?)reason ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync(ct);
                return result != null ? Convert.ToInt32(result) : (int?)null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<DailyStockOutData>> GetDailyStockOutDataAsync(int userId, int days = 30, CancellationToken ct = default)
        {
            var data = new List<DailyStockOutData>();
            var startDate = DateTime.Today.AddDays(-days);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT CAST(so.timestamps AS DATE) as date, COUNT(*) as count, COALESCE(SUM(so.quantity_removed), 0) as quantity
                FROM dbo.tbl_stock_out so
                INNER JOIN dbo.tbl_variants v ON so.variant_id = v.id
                WHERE so.archives IS NULL AND v.user_id = @UserId
                    AND CAST(so.timestamps AS DATE) >= @StartDate
                GROUP BY CAST(so.timestamps AS DATE)
                ORDER BY date";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@StartDate", startDate);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                data.Add(new DailyStockOutData
                {
                    Date = reader.GetDateTime(0),
                    Count = reader.GetInt32(1),
                    Quantity = reader.GetInt32(2)
                });
            }

            // Fill in missing dates with zero values
            var allDates = new List<DailyStockOutData>();
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var existing = data.FirstOrDefault(d => d.Date.Date == date.Date);
                allDates.Add(existing ?? new DailyStockOutData { Date = date, Count = 0, Quantity = 0 });
            }

            return allDates;
        }
    }
}

