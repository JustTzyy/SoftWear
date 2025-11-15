using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class ProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string? ImageBase64 { get; set; }
    }

    public class ArchivedProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime ArchivedDate { get; set; }
        public string? ImageBase64 { get; set; }
    }

    public class ProductDetailsModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string? ImageBase64 { get; set; }
    }

    public class CategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public interface IProductService
    {
        Task<List<ProductModel>> GetProductsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetProductsCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<ProductDetailsModel?> GetProductDetailsAsync(int productId, CancellationToken ct = default);
        Task<int?> CreateProductAsync(string name, string? description, int categoryId, byte[]? imageData, string? imageContentType, CancellationToken ct = default);
        Task<bool> UpdateProductAsync(int productId, string name, string? description, int categoryId, byte[]? imageData, string? imageContentType, CancellationToken ct = default);
        Task<bool> ArchiveProductAsync(int productId, CancellationToken ct = default);
        Task<List<ArchivedProductModel>> GetArchivedProductsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedProductsCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreProductAsync(int productId, CancellationToken ct = default);
        Task<ProductDetailsModel?> GetArchivedProductDetailsAsync(int productId, CancellationToken ct = default);
        Task<List<CategoryOption>> GetActiveCategoriesAsync(CancellationToken ct = default);
    }

    public class ProductService : IProductService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<ProductModel>> GetProductsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var products = new List<ProductModel>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT p.id, p.name, p.description, p.category_id, c.name as category_name, 
                       p.status, p.created_at, p.image
                FROM dbo.tbl_products p
                INNER JOIN dbo.tbl_categories c ON p.category_id = c.id
                WHERE p.archived_at IS NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (p.name LIKE @SearchTerm OR p.description LIKE @SearchTerm OR c.name LIKE @SearchTerm)";
            }

            sql += " ORDER BY p.created_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
                var product = new ProductModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CategoryId = reader.GetInt32(3),
                    CategoryName = reader.GetString(4),
                    Status = reader.GetString(5),
                    DateCreated = reader.GetDateTime(6)
                };

                if (!reader.IsDBNull(7))
                {
                    var imageBytes = (byte[])reader[7];
                    product.ImageBase64 = Convert.ToBase64String(imageBytes);
                }

                products.Add(product);
            }

            return products;
        }

        public async Task<int> GetProductsCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*) 
                FROM dbo.tbl_products p
                INNER JOIN dbo.tbl_categories c ON p.category_id = c.id
                WHERE p.archived_at IS NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (p.name LIKE @SearchTerm OR p.description LIKE @SearchTerm OR c.name LIKE @SearchTerm)";
            }

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            return (int)await cmd.ExecuteScalarAsync(ct);
        }

        public async Task<ProductDetailsModel?> GetProductDetailsAsync(int productId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT p.id, p.name, p.description, p.category_id, c.name as category_name, 
                       p.status, p.created_at, p.image
                FROM dbo.tbl_products p
                INNER JOIN dbo.tbl_categories c ON p.category_id = c.id
                WHERE p.id = @ProductId AND p.archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ProductId", productId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var product = new ProductDetailsModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    CategoryId = reader.GetInt32(3),
                    CategoryName = reader.GetString(4),
                    Status = "Active",
                    DateCreated = reader.GetDateTime(6)
                };

                if (!reader.IsDBNull(7))
                {
                    var imageBytes = (byte[])reader[7];
                    product.ImageBase64 = Convert.ToBase64String(imageBytes);
                }

                return product;
            }
            return null;
        }

        public async Task<int?> CreateProductAsync(string name, string? description, int categoryId, byte[]? imageData, string? imageContentType, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                INSERT INTO dbo.tbl_products (name, description, category_id, image, image_content_type, status, created_at)
                VALUES (@Name, @Description, @CategoryId, @Image, @ImageContentType, 'Active', SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            
            // Properly handle VARBINARY parameter
            var imageParam = new SqlParameter("@Image", SqlDbType.VarBinary, -1);
            imageParam.Value = (object?)imageData ?? DBNull.Value;
            cmd.Parameters.Add(imageParam);
            
            cmd.Parameters.AddWithValue("@ImageContentType", (object?)imageContentType ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null ? Convert.ToInt32(result) : null;
        }

        public async Task<bool> UpdateProductAsync(int productId, string name, string? description, int categoryId, byte[]? imageData, string? imageContentType, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            string sql;
            if (imageData != null)
            {
                sql = @"
                    UPDATE dbo.tbl_products 
                    SET name = @Name, 
                        description = @Description, 
                        category_id = @CategoryId,
                        image = @Image,
                        image_content_type = @ImageContentType,
                        updated_at = SYSUTCDATETIME()
                    WHERE id = @ProductId AND archived_at IS NULL";
            }
            else
            {
                sql = @"
                    UPDATE dbo.tbl_products 
                    SET name = @Name, 
                        description = @Description, 
                        category_id = @CategoryId,
                        updated_at = SYSUTCDATETIME()
                    WHERE id = @ProductId AND archived_at IS NULL";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            if (imageData != null)
            {
                // Properly handle VARBINARY parameter
                var imageParam = new SqlParameter("@Image", SqlDbType.VarBinary, -1);
                imageParam.Value = imageData;
                cmd.Parameters.Add(imageParam);
                
                cmd.Parameters.AddWithValue("@ImageContentType", (object?)imageContentType ?? DBNull.Value);
            }

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> ArchiveProductAsync(int productId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_products 
                SET archived_at = SYSUTCDATETIME(), 
                    status = 'Archived',
                    updated_at = SYSUTCDATETIME()
                WHERE id = @ProductId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ProductId", productId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedProductModel>> GetArchivedProductsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var products = new List<ArchivedProductModel>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT p.id, p.name, p.description, c.name as category_name, p.archived_at, p.image
                FROM dbo.tbl_products p
                INNER JOIN dbo.tbl_categories c ON p.category_id = c.id
                WHERE p.archived_at IS NOT NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (p.name LIKE @SearchTerm OR p.description LIKE @SearchTerm OR c.name LIKE @SearchTerm)";
            }

            sql += " ORDER BY p.archived_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
                var product = new ArchivedProductModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CategoryName = reader.GetString(3),
                    ArchivedDate = reader.GetDateTime(4)
                };

                if (!reader.IsDBNull(5))
                {
                    var imageBytes = (byte[])reader[5];
                    product.ImageBase64 = Convert.ToBase64String(imageBytes);
                }

                products.Add(product);
            }

            return products;
        }

        public async Task<int> GetArchivedProductsCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*) 
                FROM dbo.tbl_products p
                INNER JOIN dbo.tbl_categories c ON p.category_id = c.id
                WHERE p.archived_at IS NOT NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (p.name LIKE @SearchTerm OR p.description LIKE @SearchTerm OR c.name LIKE @SearchTerm)";
            }

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            return (int)await cmd.ExecuteScalarAsync(ct);
        }

        public async Task<bool> RestoreProductAsync(int productId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_products 
                SET archived_at = NULL, 
                    status = 'Active',
                    updated_at = SYSUTCDATETIME()
                WHERE id = @ProductId AND archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ProductId", productId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<ProductDetailsModel?> GetArchivedProductDetailsAsync(int productId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT p.id, p.name, p.description, p.category_id, c.name as category_name, 
                       p.created_at, p.image
                FROM dbo.tbl_products p
                INNER JOIN dbo.tbl_categories c ON p.category_id = c.id
                WHERE p.id = @ProductId AND p.archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ProductId", productId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var product = new ProductDetailsModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    CategoryId = reader.GetInt32(3),
                    CategoryName = reader.GetString(4),
                    Status = "Archived",
                    DateCreated = reader.GetDateTime(5)
                };

                if (!reader.IsDBNull(6))
                {
                    var imageBytes = (byte[])reader[6];
                    product.ImageBase64 = Convert.ToBase64String(imageBytes);
                }

                return product;
            }
            return null;
        }

        public async Task<List<CategoryOption>> GetActiveCategoriesAsync(CancellationToken ct = default)
        {
            var categories = new List<CategoryOption>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name 
                FROM dbo.tbl_categories 
                WHERE archived_at IS NULL 
                ORDER BY name";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                categories.Add(new CategoryOption
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            return categories;
        }
    }
}

