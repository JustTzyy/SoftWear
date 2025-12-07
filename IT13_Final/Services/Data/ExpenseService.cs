using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class ExpenseModel
    {
        public int Id { get; set; }
        public string ExpenseType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? ReceiptImage { get; set; }
        public string? ReceiptContentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int SellerUserId { get; set; }
    }

    public class CreateExpenseModel
    {
        public string ExpenseType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? ReceiptImage { get; set; }
        public string? ReceiptContentType { get; set; }
    }

    public class ArchivedExpenseModel
    {
        public int Id { get; set; }
        public string ExpenseType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; }
        public DateTime ArchivedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    public interface IExpenseService
    {
        Task<List<ExpenseModel>> GetExpensesAsync(int sellerUserId, string? searchTerm = null, string? expenseType = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetExpensesCountAsync(int sellerUserId, string? searchTerm = null, string? expenseType = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<ExpenseModel?> GetExpenseByIdAsync(int expenseId, int sellerUserId, CancellationToken ct = default);
        Task<int?> CreateExpenseAsync(int sellerUserId, int createdByUserId, CreateExpenseModel model, CancellationToken ct = default);
        Task<bool> UpdateExpenseAsync(int expenseId, int sellerUserId, CreateExpenseModel model, CancellationToken ct = default);
        Task<bool> DeleteExpenseAsync(int expenseId, int sellerUserId, CancellationToken ct = default);
        Task<List<string>> GetExpenseTypesAsync(CancellationToken ct = default);
        Task<decimal> GetTotalExpensesAsync(int sellerUserId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<List<ArchivedExpenseModel>> GetArchivedExpensesAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedExpensesCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreExpenseAsync(int expenseId, int sellerUserId, CancellationToken ct = default);
    }

    public class ExpenseService : IExpenseService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<ExpenseModel>> GetExpensesAsync(int sellerUserId, string? searchTerm = null, string? expenseType = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var expenses = new List<ExpenseModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    e.id,
                    e.expense_type,
                    e.amount,
                    e.description,
                    e.expense_date,
                    e.receipt_image,
                    e.receipt_content_type,
                    e.created_at,
                    e.updated_at,
                    e.created_by,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name,
                    e.seller_user_id
                FROM dbo.tbl_expenses e
                INNER JOIN dbo.tbl_users u ON e.created_by = u.id
                WHERE e.archived_at IS NULL
                AND e.seller_user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (e.description LIKE @SearchTerm OR e.expense_type LIKE @SearchTerm)";
            }

            if (!string.IsNullOrWhiteSpace(expenseType))
            {
                sql += " AND e.expense_type = @ExpenseType";
            }

            if (startDate.HasValue)
            {
                sql += " AND e.expense_date >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND e.expense_date <= @EndDate";
            }

            sql += @"
                ORDER BY e.expense_date DESC, e.created_at DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            if (!string.IsNullOrWhiteSpace(expenseType))
            {
                cmd.Parameters.AddWithValue("@ExpenseType", expenseType);
            }

            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                expenses.Add(new ExpenseModel
                {
                    Id = reader.GetInt32(0),
                    ExpenseType = reader.GetString(1),
                    Amount = reader.GetDecimal(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ExpenseDate = reader.GetDateTime(4),
                    ReceiptImage = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ReceiptContentType = reader.IsDBNull(6) ? null : reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7),
                    UpdatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    CreatedBy = reader.GetInt32(9),
                    CreatedByName = reader.GetString(10),
                    SellerUserId = reader.GetInt32(11)
                });
            }

            return expenses;
        }

        public async Task<int> GetExpensesCountAsync(int sellerUserId, string? searchTerm = null, string? expenseType = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_expenses e
                WHERE e.archived_at IS NULL
                AND e.seller_user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (e.description LIKE @SearchTerm OR e.expense_type LIKE @SearchTerm)";
            }

            if (!string.IsNullOrWhiteSpace(expenseType))
            {
                sql += " AND e.expense_type = @ExpenseType";
            }

            if (startDate.HasValue)
            {
                sql += " AND e.expense_date >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND e.expense_date <= @EndDate";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            if (!string.IsNullOrWhiteSpace(expenseType))
            {
                cmd.Parameters.AddWithValue("@ExpenseType", expenseType);
            }

            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<ExpenseModel?> GetExpenseByIdAsync(int expenseId, int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    e.id,
                    e.expense_type,
                    e.amount,
                    e.description,
                    e.expense_date,
                    e.receipt_image,
                    e.receipt_content_type,
                    e.created_at,
                    e.updated_at,
                    e.created_by,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name,
                    e.seller_user_id
                FROM dbo.tbl_expenses e
                INNER JOIN dbo.tbl_users u ON e.created_by = u.id
                WHERE e.id = @ExpenseId
                AND e.archived_at IS NULL
                AND e.seller_user_id = @SellerUserId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new ExpenseModel
                {
                    Id = reader.GetInt32(0),
                    ExpenseType = reader.GetString(1),
                    Amount = reader.GetDecimal(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ExpenseDate = reader.GetDateTime(4),
                    ReceiptImage = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ReceiptContentType = reader.IsDBNull(6) ? null : reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7),
                    UpdatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    CreatedBy = reader.GetInt32(9),
                    CreatedByName = reader.GetString(10),
                    SellerUserId = reader.GetInt32(11)
                };
            }

            return null;
        }

        public async Task<int?> CreateExpenseAsync(int sellerUserId, int createdByUserId, CreateExpenseModel model, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                INSERT INTO dbo.tbl_expenses 
                    (expense_type, amount, description, expense_date, receipt_image, receipt_content_type, created_by, seller_user_id, created_at)
                VALUES 
                    (@ExpenseType, @Amount, @Description, @ExpenseDate, @ReceiptImage, @ReceiptContentType, @CreatedBy, @SellerUserId, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ExpenseType", model.ExpenseType);
            cmd.Parameters.AddWithValue("@Amount", model.Amount);
            cmd.Parameters.AddWithValue("@Description", (object?)model.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate.Date);
            cmd.Parameters.AddWithValue("@ReceiptImage", (object?)model.ReceiptImage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReceiptContentType", (object?)model.ReceiptContentType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", createdByUserId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : null;
        }

        public async Task<bool> UpdateExpenseAsync(int expenseId, int sellerUserId, CreateExpenseModel model, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_expenses
                SET expense_type = @ExpenseType,
                    amount = @Amount,
                    description = @Description,
                    expense_date = @ExpenseDate,
                    receipt_image = @ReceiptImage,
                    receipt_content_type = @ReceiptContentType,
                    updated_at = SYSUTCDATETIME()
                WHERE id = @ExpenseId
                AND seller_user_id = @SellerUserId
                AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            cmd.Parameters.AddWithValue("@ExpenseType", model.ExpenseType);
            cmd.Parameters.AddWithValue("@Amount", model.Amount);
            cmd.Parameters.AddWithValue("@Description", (object?)model.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate.Date);
            cmd.Parameters.AddWithValue("@ReceiptImage", (object?)model.ReceiptImage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReceiptContentType", (object?)model.ReceiptContentType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteExpenseAsync(int expenseId, int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_expenses
                SET archived_at = SYSUTCDATETIME()
                WHERE id = @ExpenseId
                AND seller_user_id = @SellerUserId
                AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<string>> GetExpenseTypesAsync(CancellationToken ct = default)
        {
            var types = new List<string>
            {
                // Operating Expenses
                "Operating Expenses - Rent",
                "Operating Expenses - Office Supplies",
                "Operating Expenses - Printing materials",
                "Operating Expenses - Others",
                // Utilities Expenses
                "Utilities Expenses - Wifi/internet",
                "Utilities Expenses - Water",
                "Utilities Expenses - Electricity",
                "Utilities Expenses - Others",
                // Salaries and Wages
                "Salaries and Wages - Incentives",
                "Salaries and Wages - Staff Salaries",
                "Salaries and Wages - Overtime Pay",
                "Salaries and Wages - Others",
                // Maintenance and Repairs
                "Maintenance and Repairs - Miscellaneous",
                "Maintenance and Repairs - General Supplies",
                "Maintenance and Repairs - Fixing Equipment",
                "Maintenance and Repairs - Replacing Damaged Machine Repairs",
                "Maintenance and Repairs - Others"
            };

            return await Task.FromResult(types);
        }

        public async Task<decimal> GetTotalExpensesAsync(int sellerUserId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COALESCE(SUM(amount), 0)
                FROM dbo.tbl_expenses
                WHERE archived_at IS NULL
                AND seller_user_id = @SellerUserId";

            if (startDate.HasValue)
            {
                sql += " AND expense_date >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND expense_date <= @EndDate";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public async Task<List<ArchivedExpenseModel>> GetArchivedExpensesAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var expenses = new List<ArchivedExpenseModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    e.id,
                    e.expense_type,
                    e.amount,
                    e.description,
                    e.expense_date,
                    e.archived_at,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name
                FROM dbo.tbl_expenses e
                INNER JOIN dbo.tbl_users u ON e.created_by = u.id
                WHERE e.archived_at IS NOT NULL
                AND e.seller_user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (e.description LIKE @SearchTerm OR e.expense_type LIKE @SearchTerm)";
            }

            sql += @"
                ORDER BY e.archived_at DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                expenses.Add(new ArchivedExpenseModel
                {
                    Id = reader.GetInt32(0),
                    ExpenseType = reader.GetString(1),
                    Amount = reader.GetDecimal(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ExpenseDate = reader.GetDateTime(4),
                    ArchivedAt = reader.GetDateTime(5),
                    CreatedByName = reader.GetString(6)
                });
            }

            return expenses;
        }

        public async Task<int> GetArchivedExpensesCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_expenses e
                WHERE e.archived_at IS NOT NULL
                AND e.seller_user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (e.description LIKE @SearchTerm OR e.expense_type LIKE @SearchTerm)";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> RestoreExpenseAsync(int expenseId, int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_expenses
                SET archived_at = NULL
                WHERE id = @ExpenseId
                AND seller_user_id = @SellerUserId
                AND archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }
    }
}

