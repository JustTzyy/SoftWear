using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Audit
{
    public interface IHistoryService
    {
        Task LogAsync(int userId, string status, string module, string description, DateTime? ts = null, CancellationToken ct = default);
        Task<List<HistoryLog>> GetLogsAsync(int? userId = null, string? searchTerm = null, string? status = null, string? module = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetLogsCountAsync(int? userId = null, string? searchTerm = null, string? status = null, string? module = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
        Task<List<string>> GetDistinctStatusesAsync(int? userId = null, CancellationToken ct = default);
        Task<List<string>> GetDistinctModulesAsync(int? userId = null, CancellationToken ct = default);
    }

    public sealed class HistoryLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public sealed class HistoryService : IHistoryService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task LogAsync(int userId, string status, string module, string description, DateTime? ts = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            const string sql = @"INSERT INTO dbo.tbl_histories (user_id, status, module, description, ts)
                                  VALUES (@uid, @status, @module, @desc, @ts)";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@module", module);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.Parameters.AddWithValue("@ts", (object?)(ts ?? DateTime.UtcNow) ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<List<HistoryLog>> GetLogsAsync(int? userId = null, string? searchTerm = null, string? status = null, string? module = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT h.id, h.user_id, h.status, h.module, h.description, h.ts,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS FullName,
                               r.name AS RoleName
                        FROM dbo.tbl_histories h
                        JOIN dbo.tbl_users u ON u.id = h.user_id
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE 1=1";

            if (userId.HasValue)
            {
                sql += " AND h.user_id = @userId";
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (h.description LIKE @search OR h.status LIKE @search OR h.module LIKE @search)";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND h.status = @status";
            }

            if (!string.IsNullOrWhiteSpace(module))
            {
                sql += " AND h.module = @module";
            }

            if (fromDate.HasValue)
            {
                sql += " AND CAST(h.ts AS DATE) >= @fromDate";
            }

            if (toDate.HasValue)
            {
                sql += " AND CAST(h.ts AS DATE) <= @toDate";
            }

            sql += " ORDER BY h.ts DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

            if (userId.HasValue)
            {
                cmd.Parameters.AddWithValue("@userId", userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }

            if (!string.IsNullOrWhiteSpace(module))
            {
                cmd.Parameters.AddWithValue("@module", module);
            }

            if (fromDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@fromDate", fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@toDate", toDate.Value.Date);
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var logs = new List<HistoryLog>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                logs.Add(new HistoryLog
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Status = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Module = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Timestamp = reader.GetDateTime(5),
                    UserName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    UserRole = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                });
            }

            return logs;
        }

        public async Task<int> GetLogsCountAsync(int? userId = null, string? searchTerm = null, string? status = null, string? module = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*) 
                        FROM dbo.tbl_histories h
                        JOIN dbo.tbl_users u ON u.id = h.user_id
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE 1=1";

            if (userId.HasValue)
            {
                sql += " AND h.user_id = @userId";
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (h.description LIKE @search OR h.status LIKE @search OR h.module LIKE @search)";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND h.status = @status";
            }

            if (!string.IsNullOrWhiteSpace(module))
            {
                sql += " AND h.module = @module";
            }

            if (fromDate.HasValue)
            {
                sql += " AND CAST(h.ts AS DATE) >= @fromDate";
            }

            if (toDate.HasValue)
            {
                sql += " AND CAST(h.ts AS DATE) <= @toDate";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

            if (userId.HasValue)
            {
                cmd.Parameters.AddWithValue("@userId", userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@status", status);
            }

            if (!string.IsNullOrWhiteSpace(module))
            {
                cmd.Parameters.AddWithValue("@module", module);
            }

            if (fromDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@fromDate", fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@toDate", toDate.Value.Date);
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<List<string>> GetDistinctStatusesAsync(int? userId = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = "SELECT DISTINCT h.status FROM dbo.tbl_histories h WHERE h.status IS NOT NULL AND h.status != ''";
            
            if (userId.HasValue)
            {
                sql += " AND h.user_id = @userId";
            }

            sql += " ORDER BY h.status";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

            if (userId.HasValue)
            {
                cmd.Parameters.AddWithValue("@userId", userId.Value);
            }

            var statuses = new List<string>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (!reader.IsDBNull(0))
                {
                    statuses.Add(reader.GetString(0));
                }
            }

            return statuses;
        }

        public async Task<List<string>> GetDistinctModulesAsync(int? userId = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = "SELECT DISTINCT h.module FROM dbo.tbl_histories h WHERE h.module IS NOT NULL AND h.module != ''";
            
            if (userId.HasValue)
            {
                sql += " AND h.user_id = @userId";
            }

            sql += " ORDER BY h.module";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

            if (userId.HasValue)
            {
                cmd.Parameters.AddWithValue("@userId", userId.Value);
            }

            var modules = new List<string>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (!reader.IsDBNull(0))
                {
                    modules.Add(reader.GetString(0));
                }
            }

            return modules;
        }
    }
}


