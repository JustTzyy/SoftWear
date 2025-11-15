using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ArchivedCategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ArchivedDate { get; set; }
    }

    public class CategoryDetailsModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }

    public interface ICategoryService
    {
        Task<List<CategoryModel>> GetCategoriesAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetCategoriesCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<CategoryDetailsModel?> GetCategoryDetailsAsync(int categoryId, CancellationToken ct = default);
        Task<int?> CreateCategoryAsync(string name, string? description, CancellationToken ct = default);
        Task<bool> UpdateCategoryAsync(int categoryId, string name, string? description, CancellationToken ct = default);
        Task<bool> ArchiveCategoryAsync(int categoryId, CancellationToken ct = default);
        Task<List<ArchivedCategoryModel>> GetArchivedCategoriesAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedCategoriesCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreCategoryAsync(int categoryId, CancellationToken ct = default);
        Task<CategoryDetailsModel?> GetArchivedCategoryDetailsAsync(int categoryId, CancellationToken ct = default);
    }

    public class CategoryService : ICategoryService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<CategoryModel>> GetCategoriesAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var categories = new List<CategoryModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, description, created_at
                FROM dbo.tbl_categories
                WHERE archived_at IS NULL
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (name LIKE @SearchTerm OR description LIKE @SearchTerm)") + @"
                ORDER BY created_at DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                categories.Add(new CategoryModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    DateCreated = reader.GetDateTime(3),
                    Status = "Active"
                });
            }

            return categories;
        }

        public async Task<int> GetCategoriesCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_categories
                WHERE archived_at IS NULL
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (name LIKE @SearchTerm OR description LIKE @SearchTerm)");

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<CategoryDetailsModel?> GetCategoryDetailsAsync(int categoryId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, description, created_at
                FROM dbo.tbl_categories
                WHERE id = @CategoryId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new CategoryDetailsModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Status = "Active",
                    DateCreated = reader.GetDateTime(3)
                };
            }

            return null;
        }

        public async Task<int?> CreateCategoryAsync(string name, string? description, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                INSERT INTO dbo.tbl_categories (name, description, created_at)
                VALUES (@Name, @Description, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null ? Convert.ToInt32(result) : null;
        }

        public async Task<bool> UpdateCategoryAsync(int categoryId, string name, string? description, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_categories
                SET name = @Name, 
                    description = @Description,
                    updated_at = SYSUTCDATETIME()
                WHERE id = @CategoryId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> ArchiveCategoryAsync(int categoryId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_categories
                SET archived_at = SYSUTCDATETIME()
                WHERE id = @CategoryId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedCategoryModel>> GetArchivedCategoriesAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var categories = new List<ArchivedCategoryModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, description, archived_at
                FROM dbo.tbl_categories
                WHERE archived_at IS NOT NULL
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (name LIKE @SearchTerm OR description LIKE @SearchTerm)") + @"
                ORDER BY archived_at DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                categories.Add(new ArchivedCategoryModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ArchivedDate = reader.GetDateTime(3)
                });
            }

            return categories;
        }

        public async Task<int> GetArchivedCategoriesCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_categories
                WHERE archived_at IS NOT NULL
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (name LIKE @SearchTerm OR description LIKE @SearchTerm)");

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<bool> RestoreCategoryAsync(int categoryId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_categories
                SET archived_at = NULL
                WHERE id = @CategoryId AND archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<CategoryDetailsModel?> GetArchivedCategoryDetailsAsync(int categoryId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, description, created_at
                FROM dbo.tbl_categories
                WHERE id = @CategoryId AND archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new CategoryDetailsModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Status = "Archived",
                    DateCreated = reader.GetDateTime(3)
                };
            }

            return null;
        }
    }
}

