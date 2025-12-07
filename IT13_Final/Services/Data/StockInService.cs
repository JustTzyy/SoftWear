using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class StockInModel
    {
        public int Id { get; set; }
        public int QuantityAdded { get; set; }
        public decimal CostPrice { get; set; }
        public DateTime Timestamps { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class StockInDetailsModel
    {
        public int Id { get; set; }
        public int QuantityAdded { get; set; }
        public decimal CostPrice { get; set; }
        public DateTime Timestamps { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierContactPerson { get; set; }
        public string? SupplierEmail { get; set; }
        public string? SupplierContactNumber { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHexValue { get; set; }
    }

    public class DailyStockInData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int Quantity { get; set; }
    }

    public interface IStockInService
    {
        Task<List<StockInModel>> GetStockInsAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetStockInsCountAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<StockInDetailsModel?> GetStockInDetailsAsync(int stockInId, int userId, CancellationToken ct = default);
        Task<int?> CreateStockInAsync(int userId, int variantId, int sizeId, int colorId, int quantityAdded, decimal costPrice, int? supplierId, CancellationToken ct = default);
        Task<List<DailyStockInData>> GetDailyStockInDataAsync(int userId, int days = 30, CancellationToken ct = default);
    }

    public class StockInService : IStockInService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<StockInModel>> GetStockInsAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var stockIns = new List<StockInModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT si.id, si.quantity_added, si.cost_price, si.timestamps, si.variant_id, 
                       v.name as variant_name, p.name as product_name, si.supplier_id, 
                       s.company_name as supplier_name, u.name as user_name
                FROM dbo.tbl_stock_in si
                INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                INNER JOIN dbo.tbl_users u ON si.user_id = u.id
                LEFT JOIN dbo.tbl_suppliers s ON si.supplier_id = s.id
                WHERE si.archives IS NULL
                AND (@UserId = 0 OR si.user_id = @UserId)";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(si.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(si.timestamps AS DATE) <= @EndDate";
            }

            sql += @" ORDER BY si.timestamps DESC
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
                stockIns.Add(new StockInModel
                {
                    Id = reader.GetInt32(0),
                    QuantityAdded = reader.GetInt32(1),
                    CostPrice = reader.GetDecimal(2),
                    Timestamps = reader.GetDateTime(3),
                    VariantId = reader.GetInt32(4),
                    VariantName = reader.GetString(5),
                    ProductName = reader.GetString(6),
                    SupplierId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    SupplierName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    UserName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
                });
            }

            return stockIns;
        }

        public async Task<int> GetStockInsCountAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_stock_in si
                INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                LEFT JOIN dbo.tbl_suppliers s ON si.supplier_id = s.id
                WHERE si.archives IS NULL
                AND (@UserId = 0 OR si.user_id = @UserId)";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(si.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(si.timestamps AS DATE) <= @EndDate";
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

        public async Task<StockInDetailsModel?> GetStockInDetailsAsync(int stockInId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT si.id, si.quantity_added, si.cost_price, si.timestamps, si.variant_id, 
                       v.name as variant_name, p.name as product_name, si.supplier_id, 
                       s.company_name as supplier_name, s.contact_person as supplier_contact_person,
                       s.email as supplier_email, s.contact_number as supplier_contact_number,
                       u.name as user_name, si.size_id, sz.name as size_name,
                       si.color_id, c.name as color_name, c.hex_value as color_hex
                FROM dbo.tbl_stock_in si
                INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                INNER JOIN dbo.tbl_users u ON si.user_id = u.id
                LEFT JOIN dbo.tbl_suppliers s ON si.supplier_id = s.id
                LEFT JOIN dbo.tbl_sizes sz ON si.size_id = sz.id
                LEFT JOIN dbo.tbl_colors c ON si.color_id = c.id
                WHERE si.id = @StockInId
                AND (@UserId = 0 OR si.user_id = @UserId)
                AND si.archives IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StockInId", stockInId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new StockInDetailsModel
                {
                    Id = reader.GetInt32(0),
                    QuantityAdded = reader.GetInt32(1),
                    CostPrice = reader.GetDecimal(2),
                    Timestamps = reader.GetDateTime(3),
                    VariantId = reader.GetInt32(4),
                    VariantName = reader.GetString(5),
                    ProductName = reader.GetString(6),
                    SupplierId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    SupplierName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    SupplierContactPerson = reader.IsDBNull(9) ? null : reader.GetString(9),
                    SupplierEmail = reader.IsDBNull(10) ? null : reader.GetString(10),
                    SupplierContactNumber = reader.IsDBNull(11) ? null : reader.GetString(11),
                    UserName = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    SizeId = reader.IsDBNull(13) ? null : reader.GetInt32(13),
                    SizeName = reader.IsDBNull(14) ? null : reader.GetString(14),
                    ColorId = reader.IsDBNull(15) ? null : reader.GetInt32(15),
                    ColorName = reader.IsDBNull(16) ? null : reader.GetString(16),
                    ColorHexValue = reader.IsDBNull(17) ? null : reader.GetString(17)
                };
            }

            return null;
        }

        public async Task<int?> CreateStockInAsync(int userId, int variantId, int sizeId, int colorId, int quantityAdded, decimal costPrice, int? supplierId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            try
            {
                var sql = @"
                    INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
                    VALUES (@UserId, @VariantId, @SizeId, @ColorId, @QuantityAdded, @CostPrice, @SupplierId, SYSUTCDATETIME());
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@VariantId", variantId);
                cmd.Parameters.AddWithValue("@SizeId", sizeId);
                cmd.Parameters.AddWithValue("@ColorId", colorId);
                cmd.Parameters.AddWithValue("@QuantityAdded", quantityAdded);
                cmd.Parameters.AddWithValue("@CostPrice", costPrice);
                cmd.Parameters.AddWithValue("@SupplierId", (object?)supplierId ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync(ct);
                return result != null ? Convert.ToInt32(result) : (int?)null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<DailyStockInData>> GetDailyStockInDataAsync(int userId, int days = 30, CancellationToken ct = default)
        {
            var data = new List<DailyStockInData>();
            var startDate = DateTime.Today.AddDays(-days);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT CAST(si.timestamps AS DATE) as date, COUNT(*) as count, COALESCE(SUM(si.quantity_added), 0) as quantity
                FROM dbo.tbl_stock_in si
                INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                WHERE si.archives IS NULL AND v.user_id = @UserId
                    AND CAST(si.timestamps AS DATE) >= @StartDate
                GROUP BY CAST(si.timestamps AS DATE)
                ORDER BY date";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@StartDate", startDate);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                data.Add(new DailyStockInData
                {
                    Date = reader.GetDateTime(0),
                    Count = reader.GetInt32(1),
                    Quantity = reader.GetInt32(2)
                });
            }

            // Fill in missing dates with zero values
            var allDates = new List<DailyStockInData>();
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var existing = data.FirstOrDefault(d => d.Date.Date == date.Date);
                allDates.Add(existing ?? new DailyStockInData { Date = date, Count = 0, Quantity = 0 });
            }

            return allDates;
        }
    }
}

