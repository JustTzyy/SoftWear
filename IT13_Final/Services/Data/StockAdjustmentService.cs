using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class StockAdjustmentModel
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHexValue { get; set; }
        public string AdjustmentType { get; set; } = string.Empty; // "Increase" or "Decrease"
        public int QuantityAdjusted { get; set; }
        public string? Reason { get; set; }
        public DateTime Timestamps { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class StockAdjustmentDetailsModel
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHexValue { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public int QuantityAdjusted { get; set; }
        public string? Reason { get; set; }
        public DateTime Timestamps { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public interface IStockAdjustmentService
    {
        Task<List<StockAdjustmentModel>> GetStockAdjustmentsAsync(int userId, string? searchTerm = null, string? adjustmentType = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetStockAdjustmentsCountAsync(int userId, string? searchTerm = null, string? adjustmentType = null, CancellationToken ct = default);
        Task<StockAdjustmentDetailsModel?> GetStockAdjustmentDetailsAsync(int adjustmentId, int userId, CancellationToken ct = default);
        Task<int?> CreateStockAdjustmentAsync(int sellerUserId, int createdByUserId, int variantId, int? sizeId, int? colorId, string adjustmentType, int quantityAdjusted, string? reason, CancellationToken ct = default);
    }

    public class StockAdjustmentService : IStockAdjustmentService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<StockAdjustmentModel>> GetStockAdjustmentsAsync(int userId, string? searchTerm = null, string? adjustmentType = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var adjustments = new List<StockAdjustmentModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT sa.id, sa.variant_id, v.name as variant_name, p.name as product_name,
                       sa.size_id, sz.name as size_name,
                       sa.color_id, c.name as color_name, c.hex_value as color_hex,
                       sa.adjustment_type, sa.quantity_adjusted, sa.reason, sa.timestamps,
                       sa.user_id, COALESCE(u.name, u.fname + ' ' + u.lname, 'N/A') as user_name
                FROM dbo.tbl_stock_adjustments sa
                INNER JOIN dbo.tbl_variants v ON sa.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                INNER JOIN dbo.tbl_users u ON sa.user_id = u.id
                LEFT JOIN dbo.tbl_sizes sz ON sa.size_id = sz.id
                LEFT JOIN dbo.tbl_colors c ON sa.color_id = c.id
                WHERE sa.archives IS NULL AND v.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm OR sa.reason LIKE @SearchTerm)";
            }

            if (!string.IsNullOrWhiteSpace(adjustmentType))
            {
                sql += " AND sa.adjustment_type = @AdjustmentType";
            }

            sql += " ORDER BY sa.timestamps DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (!string.IsNullOrWhiteSpace(adjustmentType))
            {
                cmd.Parameters.AddWithValue("@AdjustmentType", adjustmentType);
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                adjustments.Add(new StockAdjustmentModel
                {
                    Id = reader.GetInt32(0),
                    VariantId = reader.GetInt32(1),
                    VariantName = reader.GetString(2),
                    ProductName = reader.GetString(3),
                    SizeId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    SizeName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ColorId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    ColorName = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ColorHexValue = reader.IsDBNull(8) ? null : reader.GetString(8),
                    AdjustmentType = reader.GetString(9),
                    QuantityAdjusted = reader.GetInt32(10),
                    Reason = reader.IsDBNull(11) ? null : reader.GetString(11),
                    Timestamps = reader.GetDateTime(12),
                    UserId = reader.GetInt32(13),
                    UserName = reader.IsDBNull(14) ? "N/A" : reader.GetString(14)
                });
            }

            return adjustments;
        }

        public async Task<int> GetStockAdjustmentsCountAsync(int userId, string? searchTerm = null, string? adjustmentType = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_stock_adjustments sa
                INNER JOIN dbo.tbl_variants v ON sa.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                WHERE sa.archives IS NULL AND v.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm OR sa.reason LIKE @SearchTerm)";
            }

            if (!string.IsNullOrWhiteSpace(adjustmentType))
            {
                sql += " AND sa.adjustment_type = @AdjustmentType";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (!string.IsNullOrWhiteSpace(adjustmentType))
            {
                cmd.Parameters.AddWithValue("@AdjustmentType", adjustmentType);
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<StockAdjustmentDetailsModel?> GetStockAdjustmentDetailsAsync(int adjustmentId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT sa.id, sa.variant_id, v.name as variant_name, p.name as product_name,
                       sa.size_id, sz.name as size_name,
                       sa.color_id, c.name as color_name, c.hex_value as color_hex,
                       sa.adjustment_type, sa.quantity_adjusted, sa.reason, sa.timestamps,
                       sa.user_id, COALESCE(u.name, u.fname + ' ' + u.lname, 'N/A') as user_name
                FROM dbo.tbl_stock_adjustments sa
                INNER JOIN dbo.tbl_variants v ON sa.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                INNER JOIN dbo.tbl_users u ON sa.user_id = u.id
                LEFT JOIN dbo.tbl_sizes sz ON sa.size_id = sz.id
                LEFT JOIN dbo.tbl_colors c ON sa.color_id = c.id
                WHERE sa.id = @AdjustmentId AND sa.archives IS NULL AND v.user_id = @UserId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@AdjustmentId", adjustmentId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new StockAdjustmentDetailsModel
                {
                    Id = reader.GetInt32(0),
                    VariantId = reader.GetInt32(1),
                    VariantName = reader.GetString(2),
                    ProductName = reader.GetString(3),
                    SizeId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    SizeName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ColorId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    ColorName = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ColorHexValue = reader.IsDBNull(8) ? null : reader.GetString(8),
                    AdjustmentType = reader.GetString(9),
                    QuantityAdjusted = reader.GetInt32(10),
                    Reason = reader.IsDBNull(11) ? null : reader.GetString(11),
                    Timestamps = reader.GetDateTime(12),
                    UserId = reader.GetInt32(13),
                    UserName = reader.IsDBNull(14) ? "N/A" : reader.GetString(14)
                };
            }

            return null;
        }

        public async Task<int?> CreateStockAdjustmentAsync(int sellerUserId, int createdByUserId, int variantId, int? sizeId, int? colorId, string adjustmentType, int quantityAdjusted, string? reason, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            try
            {
                // Verify variant belongs to seller
                var verifySql = "SELECT COUNT(*) FROM dbo.tbl_variants WHERE id = @VariantId AND user_id = @SellerUserId AND archived_at IS NULL";
                using var verifyCmd = new SqlCommand(verifySql, conn);
                verifyCmd.Parameters.AddWithValue("@VariantId", variantId);
                verifyCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                var count = await verifyCmd.ExecuteScalarAsync(ct);
                if (count == null || Convert.ToInt32(count) == 0)
                {
                    return null;
                }

                // Validate adjustment type
                if (adjustmentType != "Increase" && adjustmentType != "Decrease")
                {
                    return null;
                }

                // Validate quantity
                if (quantityAdjusted <= 0)
                {
                    return null;
                }

                var sql = @"
                    INSERT INTO dbo.tbl_stock_adjustments (variant_id, size_id, color_id, adjustment_type, quantity_adjusted, reason, user_id)
                    VALUES (@VariantId, @SizeId, @ColorId, @AdjustmentType, @QuantityAdjusted, @Reason, @CreatedByUserId);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@VariantId", variantId);
                cmd.Parameters.AddWithValue("@SizeId", (object?)sizeId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ColorId", (object?)colorId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AdjustmentType", adjustmentType);
                cmd.Parameters.AddWithValue("@QuantityAdjusted", quantityAdjusted);
                cmd.Parameters.AddWithValue("@Reason", (object?)reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedByUserId", createdByUserId);

                var result = await cmd.ExecuteScalarAsync(ct);
                return result != null ? Convert.ToInt32(result) : null;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error creating stock adjustment: {ex.Message}");
                return null;
            }
        }
    }
}

