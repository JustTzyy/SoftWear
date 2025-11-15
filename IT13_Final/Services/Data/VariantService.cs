using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class VariantModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public List<string> Sizes { get; set; } = new();
        public List<string> Colors { get; set; } = new();
    }

    public class ArchivedVariantModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public DateTime ArchivedDate { get; set; }
    }

    public class VariantDetailsModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageBase64 { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public List<int> SizeIds { get; set; } = new();
        public List<int> ColorIds { get; set; } = new();
        public List<string> SizeNames { get; set; } = new();
        public List<string> ColorNames { get; set; } = new();
        public List<string> ColorHexValues { get; set; } = new();
    }

    public class ProductOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageBase64 { get; set; }
    }

    public interface IVariantService
    {
        Task<List<VariantModel>> GetVariantsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetVariantsCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<VariantDetailsModel?> GetVariantDetailsAsync(int variantId, CancellationToken ct = default);
        Task<int?> CreateVariantAsync(string name, decimal price, decimal? costPrice, int productId, List<int> sizeIds, List<int> colorIds, CancellationToken ct = default);
        Task<bool> UpdateVariantAsync(int variantId, string name, decimal price, decimal? costPrice, int productId, List<int> sizeIds, List<int> colorIds, CancellationToken ct = default);
        Task<bool> ArchiveVariantAsync(int variantId, CancellationToken ct = default);
        Task<List<ArchivedVariantModel>> GetArchivedVariantsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedVariantsCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreVariantAsync(int variantId, CancellationToken ct = default);
        Task<VariantDetailsModel?> GetArchivedVariantDetailsAsync(int variantId, CancellationToken ct = default);
        Task<List<ProductOption>> GetActiveProductsAsync(CancellationToken ct = default);
    }

    public class VariantService : IVariantService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<VariantModel>> GetVariantsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var variants = new List<VariantModel>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT v.id, v.name, v.price, v.cost_price, v.product_id, p.name as product_name, 
                       v.created_at
                FROM dbo.tbl_variants v
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                WHERE v.archived_at IS NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm)";
            }

            sql += " ORDER BY v.created_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            var variantData = new List<(int Id, string Name, decimal Price, decimal? CostPrice, int ProductId, string ProductName, DateTime DateCreated)>();
            
            while (await reader.ReadAsync(ct))
            {
                variantData.Add((
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDecimal(2),
                    reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    reader.GetInt32(4),
                    reader.GetString(5),
                    reader.GetDateTime(6)
                ));
            }
            reader.Close();

            // Now load sizes and colors for each variant
            foreach (var data in variantData)
            {
                var variant = new VariantModel
                {
                    Id = data.Id,
                    Name = data.Name,
                    Price = data.Price,
                    CostPrice = data.CostPrice,
                    ProductId = data.ProductId,
                    ProductName = data.ProductName,
                    Status = "Active",
                    DateCreated = data.DateCreated
                };

                // Load sizes and colors
                variant.Sizes = await GetVariantSizesAsync(data.Id, conn, ct);
                variant.Colors = await GetVariantColorsAsync(data.Id, conn, ct);

                variants.Add(variant);
            }

            return variants;
        }

        private async Task<List<string>> GetVariantSizesAsync(int variantId, SqlConnection conn, CancellationToken ct)
        {
            var sizes = new List<string>();
            var sql = @"
                SELECT s.name
                FROM dbo.tbl_variant_sizes vs
                INNER JOIN dbo.tbl_sizes s ON vs.size_id = s.id
                WHERE vs.variant_id = @VariantId
                ORDER BY s.name";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VariantId", variantId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                sizes.Add(reader.GetString(0));
            }

            return sizes;
        }

        private async Task<List<string>> GetVariantColorsAsync(int variantId, SqlConnection conn, CancellationToken ct)
        {
            var colors = new List<string>();
            var sql = @"
                SELECT c.name
                FROM dbo.tbl_variant_colors vc
                INNER JOIN dbo.tbl_colors c ON vc.color_id = c.id
                WHERE vc.variant_id = @VariantId
                ORDER BY c.name";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VariantId", variantId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                colors.Add(reader.GetString(0));
            }

            return colors;
        }

        public async Task<int> GetVariantsCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*) 
                FROM dbo.tbl_variants v
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                WHERE v.archived_at IS NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm)";
            }

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            return (int)await cmd.ExecuteScalarAsync(ct);
        }

        public async Task<VariantDetailsModel?> GetVariantDetailsAsync(int variantId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT v.id, v.name, v.price, v.cost_price, v.product_id, p.name as product_name, 
                       p.image, p.image_content_type, v.created_at
                FROM dbo.tbl_variants v
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                WHERE v.id = @VariantId AND v.archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VariantId", variantId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var variant = new VariantDetailsModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    CostPrice = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    ProductId = reader.GetInt32(4),
                    ProductName = reader.GetString(5),
                    Status = "Active",
                    DateCreated = reader.GetDateTime(8)
                };

                // Load product image
                if (!reader.IsDBNull(6))
                {
                    var imageBytes = (byte[])reader.GetValue(6);
                    variant.ProductImageBase64 = Convert.ToBase64String(imageBytes);
                }

                reader.Close();

                // Load size IDs and names
                var sizeSql = @"
                    SELECT vs.size_id, s.name
                    FROM dbo.tbl_variant_sizes vs
                    INNER JOIN dbo.tbl_sizes s ON vs.size_id = s.id
                    WHERE vs.variant_id = @VariantId
                    ORDER BY s.name";

                using var sizeCmd = new SqlCommand(sizeSql, conn);
                sizeCmd.Parameters.AddWithValue("@VariantId", variantId);
                using var sizeReader = await sizeCmd.ExecuteReaderAsync(ct);
                while (await sizeReader.ReadAsync(ct))
                {
                    variant.SizeIds.Add(sizeReader.GetInt32(0));
                    variant.SizeNames.Add(sizeReader.GetString(1));
                }
                sizeReader.Close();

                // Load color IDs, names, and hex values
                var colorSql = @"
                    SELECT vc.color_id, c.name, c.hex_value
                    FROM dbo.tbl_variant_colors vc
                    INNER JOIN dbo.tbl_colors c ON vc.color_id = c.id
                    WHERE vc.variant_id = @VariantId
                    ORDER BY c.name";

                using var colorCmd = new SqlCommand(colorSql, conn);
                colorCmd.Parameters.AddWithValue("@VariantId", variantId);
                using var colorReader = await colorCmd.ExecuteReaderAsync(ct);
                while (await colorReader.ReadAsync(ct))
                {
                    variant.ColorIds.Add(colorReader.GetInt32(0));
                    variant.ColorNames.Add(colorReader.GetString(1));
                    variant.ColorHexValues.Add(colorReader.GetString(2));
                }

                return variant;
            }
            return null;
        }

        public async Task<int?> CreateVariantAsync(string name, decimal price, decimal? costPrice, int productId, List<int> sizeIds, List<int> colorIds, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            using var transaction = conn.BeginTransaction();

            try
            {
                // Insert variant
                var sql = @"
                    INSERT INTO dbo.tbl_variants (name, price, cost_price, product_id, created_at)
                    VALUES (@Name, @Price, @CostPrice, @ProductId, SYSUTCDATETIME());
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var cmd = new SqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@CostPrice", (object?)costPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductId", productId);

                var result = await cmd.ExecuteScalarAsync(ct);
                if (result == null)
                {
                    transaction.Rollback();
                    return null;
                }

                var variantId = Convert.ToInt32(result);

                // Insert sizes
                if (sizeIds.Count > 0)
                {
                    var sizeSql = @"
                        INSERT INTO dbo.tbl_variant_sizes (variant_id, size_id, created_at)
                        VALUES (@VariantId, @SizeId, SYSUTCDATETIME());";

                    foreach (var sizeId in sizeIds)
                    {
                        using var sizeCmd = new SqlCommand(sizeSql, conn, transaction);
                        sizeCmd.Parameters.AddWithValue("@VariantId", variantId);
                        sizeCmd.Parameters.AddWithValue("@SizeId", sizeId);
                        await sizeCmd.ExecuteNonQueryAsync(ct);
                    }
                }

                // Insert colors
                if (colorIds.Count > 0)
                {
                    var colorSql = @"
                        INSERT INTO dbo.tbl_variant_colors (variant_id, color_id, created_at)
                        VALUES (@VariantId, @ColorId, SYSUTCDATETIME());";

                    foreach (var colorId in colorIds)
                    {
                        using var colorCmd = new SqlCommand(colorSql, conn, transaction);
                        colorCmd.Parameters.AddWithValue("@VariantId", variantId);
                        colorCmd.Parameters.AddWithValue("@ColorId", colorId);
                        await colorCmd.ExecuteNonQueryAsync(ct);
                    }
                }

                transaction.Commit();
                return variantId;
            }
            catch
            {
                transaction.Rollback();
                return null;
            }
        }

        public async Task<bool> UpdateVariantAsync(int variantId, string name, decimal price, decimal? costPrice, int productId, List<int> sizeIds, List<int> colorIds, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            using var transaction = conn.BeginTransaction();

            try
            {
                // Update variant
                var sql = @"
                    UPDATE dbo.tbl_variants 
                    SET name = @Name, 
                        price = @Price, 
                        cost_price = @CostPrice,
                        product_id = @ProductId,
                        updated_at = SYSUTCDATETIME()
                    WHERE id = @VariantId AND archived_at IS NULL";

                using var cmd = new SqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@VariantId", variantId);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@CostPrice", (object?)costPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductId", productId);

                var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
                if (rowsAffected == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // Delete existing sizes
                var deleteSizesSql = "DELETE FROM dbo.tbl_variant_sizes WHERE variant_id = @VariantId";
                using var deleteSizesCmd = new SqlCommand(deleteSizesSql, conn, transaction);
                deleteSizesCmd.Parameters.AddWithValue("@VariantId", variantId);
                await deleteSizesCmd.ExecuteNonQueryAsync(ct);

                // Insert new sizes
                if (sizeIds.Count > 0)
                {
                    var sizeSql = @"
                        INSERT INTO dbo.tbl_variant_sizes (variant_id, size_id, created_at)
                        VALUES (@VariantId, @SizeId, SYSUTCDATETIME());";

                    foreach (var sizeId in sizeIds)
                    {
                        using var sizeCmd = new SqlCommand(sizeSql, conn, transaction);
                        sizeCmd.Parameters.AddWithValue("@VariantId", variantId);
                        sizeCmd.Parameters.AddWithValue("@SizeId", sizeId);
                        await sizeCmd.ExecuteNonQueryAsync(ct);
                    }
                }

                // Delete existing colors
                var deleteColorsSql = "DELETE FROM dbo.tbl_variant_colors WHERE variant_id = @VariantId";
                using var deleteColorsCmd = new SqlCommand(deleteColorsSql, conn, transaction);
                deleteColorsCmd.Parameters.AddWithValue("@VariantId", variantId);
                await deleteColorsCmd.ExecuteNonQueryAsync(ct);

                // Insert new colors
                if (colorIds.Count > 0)
                {
                    var colorSql = @"
                        INSERT INTO dbo.tbl_variant_colors (variant_id, color_id, created_at)
                        VALUES (@VariantId, @ColorId, SYSUTCDATETIME());";

                    foreach (var colorId in colorIds)
                    {
                        using var colorCmd = new SqlCommand(colorSql, conn, transaction);
                        colorCmd.Parameters.AddWithValue("@VariantId", variantId);
                        colorCmd.Parameters.AddWithValue("@ColorId", colorId);
                        await colorCmd.ExecuteNonQueryAsync(ct);
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public async Task<bool> ArchiveVariantAsync(int variantId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_variants 
                SET archived_at = SYSUTCDATETIME(), 
                    updated_at = SYSUTCDATETIME()
                WHERE id = @VariantId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VariantId", variantId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedVariantModel>> GetArchivedVariantsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var variants = new List<ArchivedVariantModel>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT v.id, v.name, v.price, p.name as product_name, v.archived_at
                FROM dbo.tbl_variants v
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                WHERE v.archived_at IS NOT NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm)";
            }

            sql += " ORDER BY v.archived_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                variants.Add(new ArchivedVariantModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    ProductName = reader.GetString(3),
                    ArchivedDate = reader.GetDateTime(4)
                });
            }

            return variants;
        }

        public async Task<int> GetArchivedVariantsCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*) 
                FROM dbo.tbl_variants v
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                WHERE v.archived_at IS NOT NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (v.name LIKE @SearchTerm OR p.name LIKE @SearchTerm)";
            }

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            return (int)await cmd.ExecuteScalarAsync(ct);
        }

        public async Task<bool> RestoreVariantAsync(int variantId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_variants 
                SET archived_at = NULL, 
                    updated_at = SYSUTCDATETIME()
                WHERE id = @VariantId AND archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VariantId", variantId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<VariantDetailsModel?> GetArchivedVariantDetailsAsync(int variantId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT v.id, v.name, v.price, v.cost_price, v.product_id, p.name as product_name, 
                       p.image, p.image_content_type, v.created_at
                FROM dbo.tbl_variants v
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                WHERE v.id = @VariantId AND v.archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VariantId", variantId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var variant = new VariantDetailsModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    CostPrice = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    ProductId = reader.GetInt32(4),
                    ProductName = reader.GetString(5),
                    Status = "Archived",
                    DateCreated = reader.GetDateTime(8)
                };

                // Load product image
                if (!reader.IsDBNull(6))
                {
                    var imageBytes = (byte[])reader.GetValue(6);
                    variant.ProductImageBase64 = Convert.ToBase64String(imageBytes);
                }

                reader.Close();

                // Load size IDs and names
                var sizeSql = @"
                    SELECT vs.size_id, s.name
                    FROM dbo.tbl_variant_sizes vs
                    INNER JOIN dbo.tbl_sizes s ON vs.size_id = s.id
                    WHERE vs.variant_id = @VariantId
                    ORDER BY s.name";

                using var sizeCmd = new SqlCommand(sizeSql, conn);
                sizeCmd.Parameters.AddWithValue("@VariantId", variantId);
                using var sizeReader = await sizeCmd.ExecuteReaderAsync(ct);
                while (await sizeReader.ReadAsync(ct))
                {
                    variant.SizeIds.Add(sizeReader.GetInt32(0));
                    variant.SizeNames.Add(sizeReader.GetString(1));
                }
                sizeReader.Close();

                // Load color IDs, names, and hex values
                var colorSql = @"
                    SELECT vc.color_id, c.name, c.hex_value
                    FROM dbo.tbl_variant_colors vc
                    INNER JOIN dbo.tbl_colors c ON vc.color_id = c.id
                    WHERE vc.variant_id = @VariantId
                    ORDER BY c.name";

                using var colorCmd = new SqlCommand(colorSql, conn);
                colorCmd.Parameters.AddWithValue("@VariantId", variantId);
                using var colorReader = await colorCmd.ExecuteReaderAsync(ct);
                while (await colorReader.ReadAsync(ct))
                {
                    variant.ColorIds.Add(colorReader.GetInt32(0));
                    variant.ColorNames.Add(colorReader.GetString(1));
                    variant.ColorHexValues.Add(colorReader.GetString(2));
                }

                return variant;
            }
            return null;
        }

        public async Task<List<ProductOption>> GetActiveProductsAsync(CancellationToken ct = default)
        {
            var products = new List<ProductOption>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, image, image_content_type
                FROM dbo.tbl_products 
                WHERE archived_at IS NULL 
                ORDER BY name";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                var product = new ProductOption
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };

                if (!reader.IsDBNull(2))
                {
                    var imageBytes = (byte[])reader.GetValue(2);
                    var contentType = reader.IsDBNull(3) ? "image/jpeg" : reader.GetString(3);
                    product.ImageBase64 = Convert.ToBase64String(imageBytes);
                }

                products.Add(product);
            }

            return products;
        }
    }
}

