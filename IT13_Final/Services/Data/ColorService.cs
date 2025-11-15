using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class ColorModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HexValue { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class ArchivedColorModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HexValue { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ArchivedDate { get; set; }
    }

    public class ColorDetailsModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HexValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }

    public interface IColorService
    {
        Task<List<ColorModel>> GetColorsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetColorsCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<ColorDetailsModel?> GetColorDetailsAsync(int colorId, CancellationToken ct = default);
        Task<int?> CreateColorAsync(string name, string hexValue, string? description, CancellationToken ct = default);
        Task<bool> UpdateColorAsync(int colorId, string name, string hexValue, string? description, CancellationToken ct = default);
        Task<bool> ArchiveColorAsync(int colorId, CancellationToken ct = default);
        Task<List<ArchivedColorModel>> GetArchivedColorsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedColorsCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreColorAsync(int colorId, CancellationToken ct = default);
        Task<ColorDetailsModel?> GetArchivedColorDetailsAsync(int colorId, CancellationToken ct = default);
        Task<List<ColorOption>> GetActiveColorsAsync(CancellationToken ct = default);
    }

    public class ColorOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HexValue { get; set; } = string.Empty;
    }

    public class ColorService : IColorService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<ColorModel>> GetColorsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var colors = new List<ColorModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, hex_value, description, created_at
                FROM dbo.tbl_colors
                WHERE archived_at IS NULL
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (name LIKE @SearchTerm OR hex_value LIKE @SearchTerm OR description LIKE @SearchTerm)") + @"
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
                colors.Add(new ColorModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    HexValue = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    DateCreated = reader.GetDateTime(4)
                });
            }

            return colors;
        }

        public async Task<int> GetColorsCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_colors
                WHERE archived_at IS NULL
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (name LIKE @SearchTerm OR hex_value LIKE @SearchTerm OR description LIKE @SearchTerm)");

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<ColorDetailsModel?> GetColorDetailsAsync(int colorId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, hex_value, description, created_at
                FROM dbo.tbl_colors
                WHERE id = @ColorId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ColorId", colorId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new ColorDetailsModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    HexValue = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Status = "Active",
                    DateCreated = reader.GetDateTime(4)
                };
            }

            return null;
        }

        public async Task<int?> CreateColorAsync(string name, string hexValue, string? description, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                INSERT INTO dbo.tbl_colors (name, hex_value, description, created_at)
                VALUES (@Name, @HexValue, @Description, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@HexValue", hexValue);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null ? Convert.ToInt32(result) : null;
        }

        public async Task<bool> UpdateColorAsync(int colorId, string name, string hexValue, string? description, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_colors
                SET name = @Name,
                    hex_value = @HexValue,
                    description = @Description,
                    updated_at = SYSUTCDATETIME()
                WHERE id = @ColorId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ColorId", colorId);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@HexValue", hexValue);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> ArchiveColorAsync(int colorId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_colors
                SET archived_at = SYSUTCDATETIME()
                WHERE id = @ColorId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ColorId", colorId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedColorModel>> GetArchivedColorsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var colors = new List<ArchivedColorModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, hex_value, description, archived_at
                FROM dbo.tbl_colors
                WHERE archived_at IS NOT NULL
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (name LIKE @SearchTerm OR hex_value LIKE @SearchTerm OR description LIKE @SearchTerm)") + @"
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
                colors.Add(new ArchivedColorModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    HexValue = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ArchivedDate = reader.GetDateTime(4)
                });
            }

            return colors;
        }

        public async Task<int> GetArchivedColorsCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_colors
                WHERE archived_at IS NOT NULL
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (name LIKE @SearchTerm OR hex_value LIKE @SearchTerm OR description LIKE @SearchTerm)");

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<bool> RestoreColorAsync(int colorId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_colors
                SET archived_at = NULL
                WHERE id = @ColorId AND archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ColorId", colorId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<ColorDetailsModel?> GetArchivedColorDetailsAsync(int colorId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, hex_value, description, created_at
                FROM dbo.tbl_colors
                WHERE id = @ColorId AND archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ColorId", colorId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new ColorDetailsModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    HexValue = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Status = "Archived",
                    DateCreated = reader.GetDateTime(4)
                };
            }

            return null;
        }

        public async Task<List<ColorOption>> GetActiveColorsAsync(CancellationToken ct = default)
        {
            var colors = new List<ColorOption>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, hex_value 
                FROM dbo.tbl_colors 
                WHERE archived_at IS NULL 
                ORDER BY name";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                colors.Add(new ColorOption
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    HexValue = reader.GetString(2)
                });
            }

            return colors;
        }
    }
}

