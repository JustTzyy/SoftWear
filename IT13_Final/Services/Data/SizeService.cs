using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class SizeModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ArchivedSizeModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ArchivedDate { get; set; }
    }

    public class SizeDetailsModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }

    public interface ISizeService
    {
        Task<List<SizeModel>> GetSizesAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetSizesCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<SizeDetailsModel?> GetSizeDetailsAsync(int sizeId, CancellationToken ct = default);
        Task<int?> CreateSizeAsync(string name, string? description, CancellationToken ct = default);
        Task<bool> UpdateSizeAsync(int sizeId, string name, string? description, CancellationToken ct = default);
        Task<bool> ArchiveSizeAsync(int sizeId, CancellationToken ct = default);
        Task<List<ArchivedSizeModel>> GetArchivedSizesAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedSizesCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreSizeAsync(int sizeId, CancellationToken ct = default);
        Task<SizeDetailsModel?> GetArchivedSizeDetailsAsync(int sizeId, CancellationToken ct = default);
        Task<List<SizeOption>> GetActiveSizesAsync(CancellationToken ct = default);
    }

    public class SizeOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class SizeService : ISizeService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<SizeModel>> GetSizesAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var sizes = new List<SizeModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, description, created_at
                FROM dbo.tbl_sizes
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
                sizes.Add(new SizeModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    DateCreated = reader.GetDateTime(3),
                    Status = "Active"
                });
            }

            return sizes;
        }

        public async Task<int> GetSizesCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_sizes
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

        public async Task<SizeDetailsModel?> GetSizeDetailsAsync(int sizeId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, description, created_at
                FROM dbo.tbl_sizes
                WHERE id = @SizeId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SizeId", sizeId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new SizeDetailsModel
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

        public async Task<int?> CreateSizeAsync(string name, string? description, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                INSERT INTO dbo.tbl_sizes (name, description, created_at)
                VALUES (@Name, @Description, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null ? Convert.ToInt32(result) : null;
        }

        public async Task<bool> UpdateSizeAsync(int sizeId, string name, string? description, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_sizes
                SET name = @Name, 
                    description = @Description,
                    updated_at = SYSUTCDATETIME()
                WHERE id = @SizeId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SizeId", sizeId);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> ArchiveSizeAsync(int sizeId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_sizes
                SET archived_at = SYSUTCDATETIME()
                WHERE id = @SizeId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SizeId", sizeId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedSizeModel>> GetArchivedSizesAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var sizes = new List<ArchivedSizeModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, description, archived_at
                FROM dbo.tbl_sizes
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
                sizes.Add(new ArchivedSizeModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ArchivedDate = reader.GetDateTime(3)
                });
            }

            return sizes;
        }

        public async Task<int> GetArchivedSizesCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_sizes
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

        public async Task<bool> RestoreSizeAsync(int sizeId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_sizes
                SET archived_at = NULL
                WHERE id = @SizeId AND archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SizeId", sizeId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<SizeDetailsModel?> GetArchivedSizeDetailsAsync(int sizeId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, description, created_at
                FROM dbo.tbl_sizes
                WHERE id = @SizeId AND archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SizeId", sizeId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new SizeDetailsModel
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

        public async Task<List<SizeOption>> GetActiveSizesAsync(CancellationToken ct = default)
        {
            var sizes = new List<SizeOption>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name 
                FROM dbo.tbl_sizes 
                WHERE archived_at IS NULL 
                ORDER BY name";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                sizes.Add(new SizeOption
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            return sizes;
        }
    }
}

