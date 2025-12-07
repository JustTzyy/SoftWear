using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class CashMovementModel
    {
        public DateTime Timestamp { get; set; }
        public int CashierUserId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public decimal Amount { get; set; }
        public bool IsCashIn { get; set; }
    }

    public interface ICashflowAuditService
    {
        Task<List<CashMovementModel>> GetMovementsAsync(int sellerUserId, int? cashierUserId, DateTime? startDate, DateTime? endDate, CancellationToken ct = default);
    }

    public class CashflowAuditService : ICashflowAuditService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<CashMovementModel>> GetMovementsAsync(int sellerUserId, int? cashierUserId, DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
        {
            var movements = new List<CashMovementModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await LoadCashSalesAsync(movements, conn, sellerUserId, cashierUserId, startDate, endDate, ct);
            await LoadCashRefundsAsync(movements, conn, sellerUserId, cashierUserId, startDate, endDate, ct);
            await LoadCashExpensesAsync(movements, conn, sellerUserId, cashierUserId, startDate, endDate, ct);
            await LoadSupplierCashPaymentsAsync(movements, conn, sellerUserId, cashierUserId, startDate, endDate, ct);

            return movements
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        private async Task LoadCashSalesAsync(List<CashMovementModel> movements, SqlConnection conn, int sellerUserId, int? cashierUserId, DateTime? startDate, DateTime? endDate, CancellationToken ct)
        {
            var sql = @"
                SELECT 
                    s.timestamps,
                    s.user_id,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name,
                    s.sale_number,
                    COALESCE(p.amount_paid - p.change_given, s.amount) as cash_amount
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                LEFT JOIN dbo.tbl_payments p ON s.id = p.sale_id AND p.archives IS NULL
                WHERE s.archives IS NULL
                AND s.status = 'Completed'
                AND (p.payment_method = 'Cash' OR (p.payment_method IS NULL AND s.payment_type = 'Cash'))
                AND u.user_id = @SellerUserId";

            if (cashierUserId.HasValue)
            {
                sql += " AND s.user_id = @CashierUserId";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            if (cashierUserId.HasValue)
            {
                cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId.Value);
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
                var amount = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4);
                if (amount <= 0) continue;

                movements.Add(new CashMovementModel
                {
                    Timestamp = reader.GetDateTime(0),
                    CashierUserId = reader.GetInt32(1),
                    CashierName = reader.GetString(2),
                    Source = "Cash Sale",
                    Module = "Sales",
                    Reference = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Amount = amount,
                    IsCashIn = true
                });
            }
        }

        private async Task LoadCashRefundsAsync(List<CashMovementModel> movements, SqlConnection conn, int sellerUserId, int? cashierUserId, DateTime? startDate, DateTime? endDate, CancellationToken ct)
        {
            var sql = @"
                SELECT 
                    r.timestamps,
                    r.user_id,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name,
                    r.return_number,
                    COALESCE(SUM(si.price * ri.quantity), 0) as refund_amount
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_users u ON r.user_id = u.id
                INNER JOIN dbo.tbl_return_items ri ON r.id = ri.return_id AND ri.archives IS NULL
                INNER JOIN dbo.tbl_sales_items si ON ri.sale_item_id = si.id AND si.archives IS NULL
                WHERE r.archives IS NULL
                AND r.status IN ('Approved', 'Completed')
                AND u.user_id = @SellerUserId";

            if (cashierUserId.HasValue)
            {
                sql += " AND r.user_id = @CashierUserId";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(r.timestamps AS DATE) <= @EndDate";
            }

            sql += @"
                GROUP BY r.timestamps, r.user_id, u.name, u.fname, u.lname, r.return_number";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            if (cashierUserId.HasValue)
            {
                cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId.Value);
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
                var amount = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4);
                if (amount <= 0) continue;

                movements.Add(new CashMovementModel
                {
                    Timestamp = reader.GetDateTime(0),
                    CashierUserId = reader.GetInt32(1),
                    CashierName = reader.GetString(2),
                    Source = "Refund",
                    Module = "Returns",
                    Reference = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Amount = amount,
                    IsCashIn = false
                });
            }
        }

        private async Task LoadCashExpensesAsync(List<CashMovementModel> movements, SqlConnection conn, int sellerUserId, int? cashierUserId, DateTime? startDate, DateTime? endDate, CancellationToken ct)
        {
            var sql = @"
                SELECT 
                    e.expense_date,
                    e.created_by,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name,
                    e.expense_type,
                    e.amount,
                    e.id
                FROM dbo.tbl_expenses e
                INNER JOIN dbo.tbl_users u ON e.created_by = u.id
                WHERE e.archived_at IS NULL
                AND e.seller_user_id = @SellerUserId";

            if (cashierUserId.HasValue)
            {
                sql += " AND e.created_by = @CashierUserId";
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
            if (cashierUserId.HasValue)
            {
                cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId.Value);
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
                var amount = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4);
                if (amount <= 0) continue;

                movements.Add(new CashMovementModel
                {
                    Timestamp = reader.GetDateTime(0),
                    CashierUserId = reader.GetInt32(1),
                    CashierName = reader.GetString(2),
                    Source = reader.IsDBNull(3) ? "Cash Expense" : reader.GetString(3),
                    Module = "Expenses",
                    Reference = $"EXP-{reader.GetInt32(5)}",
                    Amount = amount,
                    IsCashIn = false
                });
            }
        }

        private async Task LoadSupplierCashPaymentsAsync(List<CashMovementModel> movements, SqlConnection conn, int sellerUserId, int? cashierUserId, DateTime? startDate, DateTime? endDate, CancellationToken ct)
        {
            var sql = @"
                SELECT 
                    sp.payment_date,
                    sp.created_by,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name,
                    po.po_number,
                    sp.amount_paid,
                    sp.id
                FROM dbo.tbl_supplier_payments sp
                INNER JOIN dbo.tbl_users u ON sp.created_by = u.id
                LEFT JOIN dbo.tbl_purchase_orders po ON sp.po_id = po.id
                WHERE sp.archived_at IS NULL
                AND sp.seller_user_id = @SellerUserId
                AND sp.payment_method = 'Cash'";

            if (cashierUserId.HasValue)
            {
                sql += " AND sp.created_by = @CashierUserId";
            }

            if (startDate.HasValue)
            {
                sql += " AND sp.payment_date >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND sp.payment_date <= @EndDate";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            if (cashierUserId.HasValue)
            {
                cmd.Parameters.AddWithValue("@CashierUserId", cashierUserId.Value);
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
                var amount = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4);
                if (amount <= 0) continue;

                var reference = reader.IsDBNull(3) ? null : reader.GetString(3);
                if (string.IsNullOrWhiteSpace(reference))
                {
                    reference = $"SUPPAY-{reader.GetInt32(5)}";
                }

                movements.Add(new CashMovementModel
                {
                    Timestamp = reader.GetDateTime(0),
                    CashierUserId = reader.GetInt32(1),
                    CashierName = reader.GetString(2),
                    Source = "Supplier Payment",
                    Module = "Supplier Payments",
                    Reference = reference,
                    Amount = amount,
                    IsCashIn = false
                });
            }
        }
    }
}


