using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class IncomeBySellerModel
    {
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public int TransactionCount { get; set; }
    }

    public class IncomeByCategoryModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public int ItemCount { get; set; }
    }

    public class IncomeByPaymentMethodModel
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public int TransactionCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class IncomeSummaryModel
    {
        public decimal TotalGrossSales { get; set; }
        public decimal TotalReturns { get; set; }
        public decimal NetIncome { get; set; }
        public int TotalTransactions { get; set; }
        public List<IncomeBySellerModel> BySeller { get; set; } = new();
        public List<IncomeByCategoryModel> ByCategory { get; set; } = new();
        public List<IncomeByPaymentMethodModel> ByPaymentMethod { get; set; } = new();
    }

    public interface IIncomeBreakdownService
    {
        Task<IncomeSummaryModel> GetIncomeBreakdownAsync(int sellerUserId, DateTime? startDate = null, DateTime? endDate = null, bool onlyApprovedDays = false, CancellationToken ct = default);
    }

    public class IncomeBreakdownService : IIncomeBreakdownService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<IncomeSummaryModel> GetIncomeBreakdownAsync(int sellerUserId, DateTime? startDate = null, DateTime? endDate = null, bool onlyApprovedDays = false, CancellationToken ct = default)
        {
            var result = new IncomeSummaryModel();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Build date filter
            var dateFilter = "";
            if (startDate.HasValue)
            {
                dateFilter += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }
            if (endDate.HasValue)
            {
                dateFilter += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            // Filter for approved days only if requested
            var approvedDaysFilter = "";
            if (onlyApprovedDays && sellerUserId > 0)
            {
                approvedDaysFilter = @"
                    AND EXISTS (
                        SELECT 1 FROM dbo.tbl_daily_sales_verifications dsv
                        WHERE dsv.cashier_user_id = s.user_id
                        AND CAST(dsv.sale_date AS DATE) = CAST(s.timestamps AS DATE)
                        AND dsv.status = 'Approved'
                        AND dsv.archived_at IS NULL
                        AND dsv.seller_user_id = @SellerUserId
                    )";
            }
            // If sellerUserId is 0, we can't filter by approved days (need seller context)

            // Get total gross sales and transaction count
            // If sellerUserId is 0, show all sales (for accounting users without specific seller link)
            var sellerFilter = sellerUserId > 0 ? "AND u.user_id = @SellerUserId" : "";
            
            var summarySql = $@"
                SELECT 
                    COALESCE(SUM(s.amount), 0) as total_gross_sales,
                    COUNT(DISTINCT s.id) as total_transactions
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                WHERE s.archives IS NULL 
                AND s.status = 'Completed'
                {sellerFilter}
                AND u.archived_at IS NULL
                {dateFilter}
                {approvedDaysFilter}";

            using (var cmd = new SqlCommand(summarySql, conn))
            {
                if (sellerUserId > 0)
                {
                    cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
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
                if (await reader.ReadAsync(ct))
                {
                    result.TotalGrossSales = reader.GetDecimal(0);
                    result.TotalTransactions = reader.GetInt32(1);
                }
            }

            // Get total returns (filter by original sale date)
            var returnsDateFilter = "";
            if (startDate.HasValue)
            {
                returnsDateFilter += " AND CAST(s.timestamps AS DATE) >= @StartDate";
            }
            if (endDate.HasValue)
            {
                returnsDateFilter += " AND CAST(s.timestamps AS DATE) <= @EndDate";
            }

            var returnsSellerFilter = sellerUserId > 0 ? "AND u.user_id = @SellerUserId" : "";

            var returnsSql = $@"
                SELECT COALESCE(SUM(si.price * ri.quantity), 0) as total_returns
                FROM dbo.tbl_returns r
                INNER JOIN dbo.tbl_return_items ri ON r.id = ri.return_id
                INNER JOIN dbo.tbl_sales_items si ON ri.sale_item_id = si.id
                INNER JOIN dbo.tbl_sales s ON r.sale_id = s.id
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                WHERE r.archives IS NULL 
                AND r.status IN ('Approved', 'Completed')
                AND si.archives IS NULL
                {returnsSellerFilter}
                AND u.archived_at IS NULL
                {returnsDateFilter}";

            using (var cmd = new SqlCommand(returnsSql, conn))
            {
                if (sellerUserId > 0)
                {
                    cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                }
                if (startDate.HasValue)
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
                }
                if (endDate.HasValue)
                {
                    cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
                }

                var returnsResult = await cmd.ExecuteScalarAsync(ct);
                if (returnsResult != null && returnsResult != DBNull.Value)
                {
                    result.TotalReturns = Convert.ToDecimal(returnsResult);
                }
            }

            result.NetIncome = result.TotalGrossSales - result.TotalReturns;

            // Get income by cashier (grouped by cashier who made the sale)
            // Ensure cashiers belong to the same seller as the accounting user
            var byCashierSellerFilter = sellerUserId > 0 ? "AND u.user_id = @SellerUserId" : "";
            var byCashierSql = $@"
                SELECT 
                    s.user_id as cashier_id,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as cashier_name,
                    COALESCE(SUM(s.amount), 0) as total_income,
                    COUNT(DISTINCT s.id) as transaction_count
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                WHERE s.archives IS NULL 
                AND s.status = 'Completed'
                {byCashierSellerFilter}
                AND u.archived_at IS NULL
                {dateFilter}
                {approvedDaysFilter}
                GROUP BY s.user_id, u.name, u.fname, u.lname
                ORDER BY total_income DESC";

            using (var cmd = new SqlCommand(byCashierSql, conn))
            {
                if (sellerUserId > 0)
                {
                    cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
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
                    result.BySeller.Add(new IncomeBySellerModel
                    {
                        SellerId = reader.GetInt32(0),
                        SellerName = reader.GetString(1),
                        TotalIncome = reader.GetDecimal(2),
                        TransactionCount = reader.GetInt32(3)
                    });
                }
            }

            // Get income by category
            var byCategorySellerFilter = sellerUserId > 0 ? "AND u.user_id = @SellerUserId" : "";
            var byCategorySql = $@"
                SELECT 
                    c.id as category_id,
                    c.name as category_name,
                    COALESCE(SUM(si.subtotal), 0) as total_income,
                    SUM(si.quantity) as item_count
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_sales_items si ON s.id = si.sale_id
                INNER JOIN dbo.tbl_variants v ON si.variant_id = v.id
                INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                INNER JOIN dbo.tbl_categories c ON p.category_id = c.id
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                WHERE s.archives IS NULL 
                AND s.status = 'Completed'
                AND si.archives IS NULL
                AND c.archived_at IS NULL
                {byCategorySellerFilter}
                AND u.archived_at IS NULL
                {dateFilter}
                {approvedDaysFilter}
                GROUP BY c.id, c.name
                ORDER BY total_income DESC";

            using (var cmd = new SqlCommand(byCategorySql, conn))
            {
                if (sellerUserId > 0)
                {
                    cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
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
                    var itemCountValue = reader.IsDBNull(3) ? 0 : reader.GetValue(3);
                    var itemCount = itemCountValue is int intVal ? intVal : 
                                   itemCountValue is long longVal ? (int)longVal : 
                                   itemCountValue is decimal decVal ? (int)decVal : 0;
                    
                    result.ByCategory.Add(new IncomeByCategoryModel
                    {
                        CategoryId = reader.GetInt32(0),
                        CategoryName = reader.GetString(1),
                        TotalIncome = reader.GetDecimal(2),
                        ItemCount = itemCount
                    });
                }
            }

            // Get income by payment method
            var byPaymentMethodSellerFilter = sellerUserId > 0 ? "AND u.user_id = @SellerUserId" : "";
            var byPaymentMethodSql = $@"
                SELECT 
                    COALESCE(p.payment_method, s.payment_type) as payment_method,
                    COALESCE(SUM(CASE 
                        WHEN p.payment_method = 'Cash' THEN p.amount_paid - p.change_given
                        WHEN p.payment_method IS NULL AND s.payment_type = 'Cash' THEN s.amount
                        WHEN p.payment_method IS NOT NULL THEN s.amount
                        ELSE s.amount
                    END), 0) as total_income,
                    COUNT(DISTINCT s.id) as transaction_count
                FROM dbo.tbl_sales s
                INNER JOIN dbo.tbl_users u ON s.user_id = u.id
                LEFT JOIN dbo.tbl_payments p ON s.id = p.sale_id AND p.archives IS NULL
                WHERE s.archives IS NULL 
                AND s.status = 'Completed'
                {byPaymentMethodSellerFilter}
                AND u.archived_at IS NULL
                {dateFilter}
                {approvedDaysFilter}
                GROUP BY COALESCE(p.payment_method, s.payment_type)
                ORDER BY total_income DESC";

            using (var cmd = new SqlCommand(byPaymentMethodSql, conn))
            {
                if (sellerUserId > 0)
                {
                    cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                }
                if (startDate.HasValue)
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
                }
                if (endDate.HasValue)
                {
                    cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
                }

                decimal totalPaymentAmount = 0;
                var paymentMethods = new List<IncomeByPaymentMethodModel>();

                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    var method = new IncomeByPaymentMethodModel
                    {
                        PaymentMethod = reader.GetString(0),
                        TotalIncome = reader.GetDecimal(1),
                        TransactionCount = reader.GetInt32(2)
                    };
                    paymentMethods.Add(method);
                    totalPaymentAmount += method.TotalIncome;
                }

                // Calculate percentages
                if (totalPaymentAmount > 0)
                {
                    foreach (var method in paymentMethods)
                    {
                        method.Percentage = (method.TotalIncome / totalPaymentAmount) * 100;
                    }
                }

                result.ByPaymentMethod = paymentMethods;
            }

            return result;
        }
    }
}

