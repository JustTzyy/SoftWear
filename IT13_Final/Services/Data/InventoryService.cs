using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class InventoryModel
    {
        public int VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHexValue { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime LastUpdated { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public decimal Price { get; set; } // Added for POS functionality
        public decimal? CostPrice { get; set; } // Added for POS cost price
        public string? ProductImageBase64 { get; set; } // Added for POS image display
        public string? ImageContentType { get; set; } // Added for image content type
        public int CategoryId { get; set; } // Added for category filtering
        public string CategoryName { get; set; } = string.Empty; // Added for category filtering
    }

    public class InventoryDashboardStatsModel
    {
        public int TotalStockIn { get; set; }
        public int TotalStockOut { get; set; }
        public int TotalAdjustments { get; set; }
        public int TodayStockIn { get; set; }
        public int TodayStockOut { get; set; }
        public int TodayAdjustments { get; set; }
        public int LowStockItems { get; set; }
        public int TotalInventoryValue { get; set; }
        public int TotalProducts { get; set; }
    }

    public class LowStockItemModel
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
    }

    public interface IInventoryService
    {
        Task<List<InventoryModel>> GetInventoriesAsync(int userId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetInventoriesCountAsync(int userId, string? searchTerm = null, CancellationToken ct = default);
        Task<InventoryModel?> GetInventoryDetailsAsync(int variantId, int? sizeId, int? colorId, int userId, CancellationToken ct = default);
        Task<bool> UpdateReorderLevelAsync(int variantId, int? sizeId, int? colorId, int reorderLevel, int sellerUserId, int updatedByUserId, CancellationToken ct = default);
        Task<InventoryDashboardStatsModel> GetDashboardStatsAsync(int userId, CancellationToken ct = default);
        Task<List<LowStockItemModel>> GetLowStockItemsAsync(int userId, int topCount = 10, CancellationToken ct = default);
    }

    public class InventoryService : IInventoryService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<InventoryModel>> GetInventoriesAsync(int userId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var inventories = new List<InventoryModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Aggregate inventory from stock_in, stock_out, and stock_adjustments grouped by variant, size, and color
            var sql = @"
                WITH StockInAggregated AS (
                    SELECT 
                        si.variant_id,
                        si.size_id,
                        si.color_id,
                        SUM(si.quantity_added) as total_in
                    FROM dbo.tbl_stock_in si
                    INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                    WHERE si.archives IS NULL AND v.user_id = @UserId
                    GROUP BY si.variant_id, si.size_id, si.color_id
                ),
                StockOutAggregated AS (
                    SELECT 
                        so.variant_id,
                        so.size_id,
                        so.color_id,
                        SUM(so.quantity_removed) as total_out
                    FROM dbo.tbl_stock_out so
                    INNER JOIN dbo.tbl_variants v ON so.variant_id = v.id
                    WHERE so.archives IS NULL AND v.user_id = @UserId
                    GROUP BY so.variant_id, so.size_id, so.color_id
                ),
                StockAdjustmentsAggregated AS (
                    SELECT 
                        sa.variant_id,
                        sa.size_id,
                        sa.color_id,
                        SUM(CASE 
                            WHEN sa.adjustment_type = 'Increase' THEN sa.quantity_adjusted
                            WHEN sa.adjustment_type = 'Decrease' THEN -sa.quantity_adjusted
                            ELSE 0
                        END) as total_adjustment
                    FROM dbo.tbl_stock_adjustments sa
                    INNER JOIN dbo.tbl_variants v ON sa.variant_id = v.id
                    WHERE sa.archives IS NULL AND v.user_id = @UserId
                    GROUP BY sa.variant_id, sa.size_id, sa.color_id
                ),
                InventoryCombined AS (
                    SELECT 
                        COALESCE(si.variant_id, COALESCE(so.variant_id, sa.variant_id)) as variant_id,
                        COALESCE(si.size_id, COALESCE(so.size_id, sa.size_id)) as size_id,
                        COALESCE(si.color_id, COALESCE(so.color_id, sa.color_id)) as color_id,
                        COALESCE(si.total_in, 0) - COALESCE(so.total_out, 0) + COALESCE(sa.total_adjustment, 0) as current_stock
                    FROM StockInAggregated si
                    FULL OUTER JOIN StockOutAggregated so 
                        ON si.variant_id = so.variant_id 
                        AND (si.size_id = so.size_id OR (si.size_id IS NULL AND so.size_id IS NULL))
                        AND (si.color_id = so.color_id OR (si.color_id IS NULL AND so.color_id IS NULL))
                    FULL OUTER JOIN StockAdjustmentsAggregated sa
                        ON COALESCE(si.variant_id, so.variant_id) = sa.variant_id
                        AND (COALESCE(si.size_id, so.size_id) = sa.size_id OR (COALESCE(si.size_id, so.size_id) IS NULL AND sa.size_id IS NULL))
                        AND (COALESCE(si.color_id, so.color_id) = sa.color_id OR (COALESCE(si.color_id, so.color_id) IS NULL AND sa.color_id IS NULL))
                )
                SELECT DISTINCT
                    ic.variant_id,
                    v.name as variant_name,
                    p.name as product_name,
                    ic.size_id,
                    sz.name as size_name,
                    ic.color_id,
                    c.name as color_name,
                    c.hex_value as color_hex,
                    ic.current_stock,
                    COALESCE(i.reorder_level, 0) as reorder_level,
                    COALESCE(i.timestamps, GETDATE()) as last_updated,
                    i.user_id,
                    COALESCE(u.name, u.fname + ' ' + u.lname, 'N/A') as user_name,
                    v.price,
                    v.cost_price,
                    p.image,
                    p.image_content_type,
                    p.category_id,
                    cat.name as category_name
                FROM InventoryCombined ic
                INNER JOIN dbo.tbl_variants v ON ic.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                LEFT JOIN dbo.tbl_categories cat ON p.category_id = cat.id
                LEFT JOIN dbo.tbl_sizes sz ON ic.size_id = sz.id
                LEFT JOIN dbo.tbl_colors c ON ic.color_id = c.id
                LEFT JOIN dbo.tbl_inventories i ON ic.variant_id = i.variant_id 
                    AND (ic.size_id = i.size_id OR (ic.size_id IS NULL AND i.size_id IS NULL))
                    AND (ic.color_id = i.color_id OR (ic.color_id IS NULL AND i.color_id IS NULL))
                    AND i.archives IS NULL
                LEFT JOIN dbo.tbl_users u ON i.user_id = u.id
                WHERE v.user_id = @UserId AND v.archived_at IS NULL AND ic.current_stock > 0
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm OR sz.name LIKE @SearchTerm OR c.name LIKE @SearchTerm)") + @"
                ORDER BY p.name, v.name, sz.name, c.name
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                inventories.Add(new InventoryModel
                {
                    VariantId = reader.GetInt32(0),
                    VariantName = reader.GetString(1),
                    ProductName = reader.GetString(2),
                    SizeId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    SizeName = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ColorId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    ColorName = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ColorHexValue = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CurrentStock = reader.GetInt32(8),
                    ReorderLevel = reader.GetInt32(9),
                    LastUpdated = reader.GetDateTime(10),
                    UserId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    UserName = reader.IsDBNull(12) ? null : reader.GetString(12),
                    Price = reader.GetDecimal(13),
                    CostPrice = reader.IsDBNull(14) ? null : reader.GetDecimal(14),
                    ProductImageBase64 = reader.IsDBNull(15) ? null : Convert.ToBase64String((byte[])reader.GetValue(15)),
                    ImageContentType = reader.IsDBNull(16) ? null : reader.GetString(16),
                    CategoryId = reader.GetInt32(17),
                    CategoryName = reader.IsDBNull(18) ? string.Empty : reader.GetString(18)
                });
            }

            return inventories;
        }

        public async Task<int> GetInventoriesCountAsync(int userId, string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                WITH StockInAggregated AS (
                    SELECT 
                        si.variant_id,
                        si.size_id,
                        si.color_id,
                        SUM(si.quantity_added) as total_in
                    FROM dbo.tbl_stock_in si
                    INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                    WHERE si.archives IS NULL AND v.user_id = @UserId
                    GROUP BY si.variant_id, si.size_id, si.color_id
                ),
                StockOutAggregated AS (
                    SELECT 
                        so.variant_id,
                        so.size_id,
                        so.color_id,
                        SUM(so.quantity_removed) as total_out
                    FROM dbo.tbl_stock_out so
                    INNER JOIN dbo.tbl_variants v ON so.variant_id = v.id
                    WHERE so.archives IS NULL AND v.user_id = @UserId
                    GROUP BY so.variant_id, so.size_id, so.color_id
                ),
                StockAdjustmentsAggregated AS (
                    SELECT 
                        sa.variant_id,
                        sa.size_id,
                        sa.color_id,
                        SUM(CASE 
                            WHEN sa.adjustment_type = 'Increase' THEN sa.quantity_adjusted
                            WHEN sa.adjustment_type = 'Decrease' THEN -sa.quantity_adjusted
                            ELSE 0
                        END) as total_adjustment
                    FROM dbo.tbl_stock_adjustments sa
                    INNER JOIN dbo.tbl_variants v ON sa.variant_id = v.id
                    WHERE sa.archives IS NULL AND v.user_id = @UserId
                    GROUP BY sa.variant_id, sa.size_id, sa.color_id
                ),
                InventoryCombined AS (
                    SELECT 
                        COALESCE(si.variant_id, COALESCE(so.variant_id, sa.variant_id)) as variant_id,
                        COALESCE(si.size_id, COALESCE(so.size_id, sa.size_id)) as size_id,
                        COALESCE(si.color_id, COALESCE(so.color_id, sa.color_id)) as color_id,
                        COALESCE(si.total_in, 0) - COALESCE(so.total_out, 0) + COALESCE(sa.total_adjustment, 0) as current_stock
                    FROM StockInAggregated si
                    FULL OUTER JOIN StockOutAggregated so 
                        ON si.variant_id = so.variant_id 
                        AND (si.size_id = so.size_id OR (si.size_id IS NULL AND so.size_id IS NULL))
                        AND (si.color_id = so.color_id OR (si.color_id IS NULL AND so.color_id IS NULL))
                    FULL OUTER JOIN StockAdjustmentsAggregated sa
                        ON COALESCE(si.variant_id, so.variant_id) = sa.variant_id
                        AND (COALESCE(si.size_id, so.size_id) = sa.size_id OR (COALESCE(si.size_id, so.size_id) IS NULL AND sa.size_id IS NULL))
                        AND (COALESCE(si.color_id, so.color_id) = sa.color_id OR (COALESCE(si.color_id, so.color_id) IS NULL AND sa.color_id IS NULL))
                )
                SELECT COUNT(DISTINCT CONCAT(ic.variant_id, '-', COALESCE(CAST(ic.size_id AS NVARCHAR), ''), '-', COALESCE(CAST(ic.color_id AS NVARCHAR), '')))
                FROM InventoryCombined ic
                INNER JOIN dbo.tbl_variants v ON ic.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                LEFT JOIN dbo.tbl_sizes sz ON ic.size_id = sz.id
                LEFT JOIN dbo.tbl_colors c ON ic.color_id = c.id
                LEFT JOIN dbo.tbl_inventories i ON ic.variant_id = i.variant_id 
                    AND (ic.size_id = i.size_id OR (ic.size_id IS NULL AND i.size_id IS NULL))
                    AND (ic.color_id = i.color_id OR (ic.color_id IS NULL AND i.color_id IS NULL))
                    AND i.archives IS NULL
                WHERE v.user_id = @UserId AND v.archived_at IS NULL AND ic.current_stock > 0
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm OR sz.name LIKE @SearchTerm OR c.name LIKE @SearchTerm)");

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<InventoryModel?> GetInventoryDetailsAsync(int variantId, int? sizeId, int? colorId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                WITH StockInAggregated AS (
                    SELECT 
                        si.variant_id,
                        si.size_id,
                        si.color_id,
                        SUM(si.quantity_added) as total_in
                    FROM dbo.tbl_stock_in si
                    WHERE si.archives IS NULL 
                        AND si.variant_id = @VariantId
                        AND (@SizeId IS NULL OR si.size_id = @SizeId)
                        AND (@ColorId IS NULL OR si.color_id = @ColorId)
                    GROUP BY si.variant_id, si.size_id, si.color_id
                ),
                StockOutAggregated AS (
                    SELECT 
                        so.variant_id,
                        so.size_id,
                        so.color_id,
                        SUM(so.quantity_removed) as total_out
                    FROM dbo.tbl_stock_out so
                    WHERE so.archives IS NULL 
                        AND so.variant_id = @VariantId
                        AND (@SizeId IS NULL OR so.size_id = @SizeId)
                        AND (@ColorId IS NULL OR so.color_id = @ColorId)
                    GROUP BY so.variant_id, so.size_id, so.color_id
                ),
                StockAdjustmentsAggregated AS (
                    SELECT 
                        sa.variant_id,
                        sa.size_id,
                        sa.color_id,
                        SUM(CASE 
                            WHEN sa.adjustment_type = 'Increase' THEN sa.quantity_adjusted
                            WHEN sa.adjustment_type = 'Decrease' THEN -sa.quantity_adjusted
                            ELSE 0
                        END) as total_adjustment
                    FROM dbo.tbl_stock_adjustments sa
                    WHERE sa.archives IS NULL 
                        AND sa.variant_id = @VariantId
                        AND (@SizeId IS NULL OR sa.size_id = @SizeId)
                        AND (@ColorId IS NULL OR sa.color_id = @ColorId)
                    GROUP BY sa.variant_id, sa.size_id, sa.color_id
                )
                SELECT 
                    COALESCE(si.variant_id, COALESCE(so.variant_id, sa.variant_id)) as variant_id,
                    v.name as variant_name,
                    p.name as product_name,
                    COALESCE(si.size_id, COALESCE(so.size_id, sa.size_id)) as size_id,
                    sz.name as size_name,
                    COALESCE(si.color_id, COALESCE(so.color_id, sa.color_id)) as color_id,
                    c.name as color_name,
                    c.hex_value as color_hex,
                    COALESCE(si.total_in, 0) - COALESCE(so.total_out, 0) + COALESCE(sa.total_adjustment, 0) as current_stock,
                    COALESCE(i.reorder_level, 0) as reorder_level,
                    COALESCE(i.timestamps, GETDATE()) as last_updated,
                    i.user_id,
                    COALESCE(u.name, u.fname + ' ' + u.lname, 'N/A') as user_name,
                    v.price,
                    v.cost_price,
                    p.image,
                    p.image_content_type,
                    p.category_id,
                    cat.name as category_name
                FROM StockInAggregated si
                FULL OUTER JOIN StockOutAggregated so 
                    ON si.variant_id = so.variant_id 
                    AND si.size_id = so.size_id 
                    AND si.color_id = so.color_id
                FULL OUTER JOIN StockAdjustmentsAggregated sa
                    ON COALESCE(si.variant_id, so.variant_id) = sa.variant_id
                    AND COALESCE(si.size_id, so.size_id) = sa.size_id
                    AND COALESCE(si.color_id, so.color_id) = sa.color_id
                INNER JOIN dbo.tbl_variants v ON COALESCE(si.variant_id, COALESCE(so.variant_id, sa.variant_id)) = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                LEFT JOIN dbo.tbl_categories cat ON p.category_id = cat.id
                LEFT JOIN dbo.tbl_sizes sz ON COALESCE(si.size_id, so.size_id) = sz.id
                LEFT JOIN dbo.tbl_colors c ON COALESCE(si.color_id, so.color_id) = c.id
                LEFT JOIN dbo.tbl_inventories i ON COALESCE(si.variant_id, so.variant_id) = i.variant_id 
                    AND (COALESCE(si.size_id, so.size_id) = i.size_id OR (COALESCE(si.size_id, so.size_id) IS NULL AND i.size_id IS NULL))
                    AND (COALESCE(si.color_id, so.color_id) = i.color_id OR (COALESCE(si.color_id, so.color_id) IS NULL AND i.color_id IS NULL))
                    AND i.archives IS NULL
                LEFT JOIN dbo.tbl_users u ON i.user_id = u.id
                WHERE v.user_id = @UserId AND v.archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VariantId", variantId);
            cmd.Parameters.AddWithValue("@SizeId", (object?)sizeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ColorId", (object?)colorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new InventoryModel
                {
                    VariantId = reader.GetInt32(0),
                    VariantName = reader.GetString(1),
                    ProductName = reader.GetString(2),
                    SizeId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    SizeName = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ColorId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    ColorName = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ColorHexValue = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CurrentStock = reader.GetInt32(8),
                    ReorderLevel = reader.GetInt32(9),
                    LastUpdated = reader.GetDateTime(10),
                    UserId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    UserName = reader.IsDBNull(12) ? null : reader.GetString(12),
                    Price = reader.GetDecimal(13),
                    CostPrice = reader.IsDBNull(14) ? null : reader.GetDecimal(14),
                    ProductImageBase64 = reader.IsDBNull(15) ? null : Convert.ToBase64String((byte[])reader.GetValue(15)),
                    ImageContentType = reader.IsDBNull(16) ? "image/jpeg" : reader.GetString(16),
                    CategoryId = reader.GetInt32(17),
                    CategoryName = reader.IsDBNull(18) ? string.Empty : reader.GetString(18)
                };
            }

            return null;
        }

        public async Task<bool> UpdateReorderLevelAsync(int variantId, int? sizeId, int? colorId, int reorderLevel, int sellerUserId, int updatedByUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            try
            {
                // First verify the variant belongs to the seller
                var verifySql = "SELECT COUNT(*) FROM dbo.tbl_variants WHERE id = @VariantId AND user_id = @SellerUserId AND archived_at IS NULL";
                using var verifyCmd = new SqlCommand(verifySql, conn);
                verifyCmd.Parameters.AddWithValue("@VariantId", variantId);
                verifyCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                var count = await verifyCmd.ExecuteScalarAsync(ct);
                if (count == null || Convert.ToInt32(count) == 0)
                {
                    return false;
                }

                // Update or insert reorder level in tbl_inventories per variant-size-color combination
                var sql = @"
                    MERGE dbo.tbl_inventories AS target
                    USING (SELECT @VariantId as variant_id, @SizeId as size_id, @ColorId as color_id) AS source
                    ON target.variant_id = source.variant_id 
                        AND (target.size_id = source.size_id OR (target.size_id IS NULL AND source.size_id IS NULL))
                        AND (target.color_id = source.color_id OR (target.color_id IS NULL AND source.color_id IS NULL))
                        AND target.archives IS NULL
                    WHEN MATCHED THEN
                        UPDATE SET 
                            reorder_level = @ReorderLevel,
                            user_id = @UpdatedByUserId,
                            timestamps = SYSUTCDATETIME()
                    WHEN NOT MATCHED BY TARGET THEN
                        INSERT (variant_id, size_id, color_id, current_stock, reorder_level, user_id, timestamps)
                        VALUES (@VariantId, @SizeId, @ColorId, 0, @ReorderLevel, @UpdatedByUserId, SYSUTCDATETIME());";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@VariantId", variantId);
                cmd.Parameters.AddWithValue("@SizeId", (object?)sizeId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ColorId", (object?)colorId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ReorderLevel", reorderLevel);
                cmd.Parameters.AddWithValue("@UpdatedByUserId", updatedByUserId);

                await cmd.ExecuteNonQueryAsync(ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<InventoryDashboardStatsModel> GetDashboardStatsAsync(int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var today = DateTime.Today;
            var stats = new InventoryDashboardStatsModel();

            // Get total stock in count and quantity
            var totalStockInSql = @"
                SELECT COUNT(*), COALESCE(SUM(si.quantity_added), 0)
                FROM dbo.tbl_stock_in si
                INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                WHERE si.archives IS NULL AND v.user_id = @UserId";

            // Get today's stock in
            var todayStockInSql = @"
                SELECT COUNT(*), COALESCE(SUM(si.quantity_added), 0)
                FROM dbo.tbl_stock_in si
                INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                WHERE si.archives IS NULL AND v.user_id = @UserId
                AND CAST(si.timestamps AS DATE) = @Today";

            // Get total stock out count and quantity
            var totalStockOutSql = @"
                SELECT COUNT(*), COALESCE(SUM(so.quantity_removed), 0)
                FROM dbo.tbl_stock_out so
                INNER JOIN dbo.tbl_variants v ON so.variant_id = v.id
                WHERE so.archives IS NULL AND v.user_id = @UserId";

            // Get today's stock out
            var todayStockOutSql = @"
                SELECT COUNT(*), COALESCE(SUM(so.quantity_removed), 0)
                FROM dbo.tbl_stock_out so
                INNER JOIN dbo.tbl_variants v ON so.variant_id = v.id
                WHERE so.archives IS NULL AND v.user_id = @UserId
                AND CAST(so.timestamps AS DATE) = @Today";

            // Get total adjustments count
            var totalAdjustmentsSql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_stock_adjustments sa
                INNER JOIN dbo.tbl_variants v ON sa.variant_id = v.id
                WHERE sa.archives IS NULL AND v.user_id = @UserId";

            // Get today's adjustments
            var todayAdjustmentsSql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_stock_adjustments sa
                INNER JOIN dbo.tbl_variants v ON sa.variant_id = v.id
                WHERE sa.archives IS NULL AND v.user_id = @UserId
                AND CAST(sa.timestamps AS DATE) = @Today";

            // Get low stock items count
            var lowStockSql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_inventories i
                INNER JOIN dbo.tbl_variants v ON i.variant_id = v.id
                WHERE i.archives IS NULL 
                    AND v.user_id = @UserId 
                    AND v.archived_at IS NULL 
                    AND i.reorder_level IS NOT NULL
                    AND i.reorder_level > 0
                    AND i.current_stock <= i.reorder_level";

            // Get total products count
            var totalProductsSql = @"
                SELECT COUNT(DISTINCT v.id)
                FROM dbo.tbl_variants v
                WHERE v.user_id = @UserId AND v.archived_at IS NULL";

            // Execute queries
            using (var cmd = new SqlCommand(totalStockInSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    stats.TotalStockIn = reader.GetInt32(0);
                }
            }

            using (var cmd = new SqlCommand(todayStockInSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Today", today);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    stats.TodayStockIn = reader.GetInt32(0);
                }
            }

            using (var cmd = new SqlCommand(totalStockOutSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    stats.TotalStockOut = reader.GetInt32(0);
                }
            }

            using (var cmd = new SqlCommand(todayStockOutSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Today", today);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    stats.TodayStockOut = reader.GetInt32(0);
                }
            }

            using (var cmd = new SqlCommand(totalAdjustmentsSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                var result = await cmd.ExecuteScalarAsync(ct);
                stats.TotalAdjustments = result != null ? Convert.ToInt32(result) : 0;
            }

            using (var cmd = new SqlCommand(todayAdjustmentsSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Today", today);
                var result = await cmd.ExecuteScalarAsync(ct);
                stats.TodayAdjustments = result != null ? Convert.ToInt32(result) : 0;
            }

            using (var cmd = new SqlCommand(lowStockSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                var result = await cmd.ExecuteScalarAsync(ct);
                stats.LowStockItems = result != null ? Convert.ToInt32(result) : 0;
            }

            using (var cmd = new SqlCommand(totalProductsSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                var result = await cmd.ExecuteScalarAsync(ct);
                stats.TotalProducts = result != null ? Convert.ToInt32(result) : 0;
            }

            return stats;
        }

        public async Task<List<LowStockItemModel>> GetLowStockItemsAsync(int userId, int topCount = 10, CancellationToken ct = default)
        {
            var items = new List<LowStockItemModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT TOP (@TopCount) i.variant_id, p.name as product_name, v.name as variant_name,
                       sz.name as size_name, c.name as color_name,
                       i.current_stock, i.reorder_level
                FROM dbo.tbl_inventories i
                INNER JOIN dbo.tbl_variants v ON i.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                LEFT JOIN dbo.tbl_sizes sz ON i.size_id = sz.id
                LEFT JOIN dbo.tbl_colors c ON i.color_id = c.id
                WHERE i.archives IS NULL 
                    AND v.user_id = @UserId 
                    AND v.archived_at IS NULL 
                    AND i.reorder_level IS NOT NULL
                    AND i.reorder_level > 0
                    AND i.current_stock <= i.reorder_level
                ORDER BY i.current_stock ASC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@TopCount", topCount);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new LowStockItemModel
                {
                    VariantId = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    VariantName = reader.GetString(2),
                    SizeName = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ColorName = reader.IsDBNull(4) ? null : reader.GetString(4),
                    CurrentStock = reader.GetInt32(5),
                    ReorderLevel = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
                });
            }

            return items;
        }
    }
}

