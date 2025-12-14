using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    #region Models

    /// <summary>
    /// Represents a subscription plan available in the system
    /// </summary>
    public class SubscriptionPlanModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal AdminFeePercentage { get; set; }
        public bool HasStockClerkAccess { get; set; }
        public bool HasCashierAccess { get; set; }
        public bool HasAccountingAccess { get; set; }
        public bool HasFullReportsAccess { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Represents a seller's active subscription
    /// </summary>
    public class SellerSubscriptionModel
    {
        public int Id { get; set; }
        public int SellerUserId { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string PlanCode { get; set; } = string.Empty;
        public decimal AdminFeePercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public int? PreviousPlanId { get; set; }
        public string? PreviousPlanName { get; set; }
        public DateTime? PlanChangedAt { get; set; }
        
        // Module Access (from plan)
        public bool HasStockClerkAccess { get; set; }
        public bool HasCashierAccess { get; set; }
        public bool HasAccountingAccess { get; set; }
        public bool HasFullReportsAccess { get; set; }
    }

    /// <summary>
    /// Represents a subscription transaction (admin fee or payment)
    /// </summary>
    public class SubscriptionTransactionModel
    {
        public int Id { get; set; }
        public int SellerUserId { get; set; }
        public int SubscriptionId { get; set; }
        public int? SaleId { get; set; }
        public string? SaleNumber { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal? SaleAmount { get; set; }
        public decimal AdminFeePercentage { get; set; }
        public decimal AdminFeeAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? CollectedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Summary of admin fees for a seller
    /// </summary>
    public class AdminFeeSummaryModel
    {
        public int SellerUserId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal AdminFeePercentage { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalAdminFees { get; set; }
        public decimal PendingAdminFees { get; set; }
        public decimal CollectedAdminFees { get; set; }
        public int TotalTransactions { get; set; }
    }

    #endregion

    #region Interface

    public interface ISubscriptionService
    {
        // Plan Management
        Task<List<SubscriptionPlanModel>> GetAllPlansAsync(bool activeOnly = true, CancellationToken ct = default);
        Task<SubscriptionPlanModel?> GetPlanByIdAsync(int planId, CancellationToken ct = default);
        Task<SubscriptionPlanModel?> GetPlanByCodeAsync(string code, CancellationToken ct = default);
        
        // Seller Subscription Management
        Task<SellerSubscriptionModel?> GetSellerSubscriptionAsync(int sellerUserId, CancellationToken ct = default);
        Task<int?> CreateSubscriptionAsync(int sellerUserId, int planId, CancellationToken ct = default);
        Task<bool> UpdateSubscriptionPlanAsync(int sellerUserId, int newPlanId, CancellationToken ct = default);
        Task<bool> CancelSubscriptionAsync(int sellerUserId, CancellationToken ct = default);
        Task<bool> HasActiveSubscriptionAsync(int sellerUserId, CancellationToken ct = default);
        
        // Module Access Validation
        Task<bool> CanAccessModuleAsync(int sellerUserId, string moduleName, CancellationToken ct = default);
        Task<bool> ValidateSubscriptionOnLoginAsync(int userId, string role, CancellationToken ct = default);
        
        // Admin Fee Calculation
        Task<decimal> GetAdminFeePercentageAsync(int sellerUserId, CancellationToken ct = default);
        Task<decimal> CalculateAdminFeeAsync(int sellerUserId, decimal saleAmount, CancellationToken ct = default);
        Task<int?> RecordAdminFeeTransactionAsync(int sellerUserId, int saleId, decimal saleAmount, CancellationToken ct = default);
        
        // Admin Fee Reporting
        Task<List<SubscriptionTransactionModel>> GetAdminFeeTransactionsAsync(int? sellerUserId = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
        Task<int> GetAdminFeeTransactionsCountAsync(int? sellerUserId = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null, CancellationToken ct = default);
        Task<AdminFeeSummaryModel?> GetAdminFeeSummaryAsync(int sellerUserId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<List<AdminFeeSummaryModel>> GetAllSellerAdminFeeSummariesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<bool> MarkAdminFeeAsCollectedAsync(int transactionId, CancellationToken ct = default);
        
        // Subscription History
        Task<List<SellerSubscriptionModel>> GetSubscriptionHistoryAsync(int sellerUserId, CancellationToken ct = default);
    }

    #endregion

    #region Service Implementation

    public class SubscriptionService : ISubscriptionService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        #region Plan Management

        public async Task<List<SubscriptionPlanModel>> GetAllPlansAsync(bool activeOnly = true, CancellationToken ct = default)
        {
            var plans = new List<SubscriptionPlanModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, code, description, price, admin_fee_percentage,
                       has_stock_clerk_access, has_cashier_access, has_accounting_access, 
                       has_full_reports_access, display_order, is_active, created_at
                FROM dbo.tbl_subscription_plans
                WHERE archived_at IS NULL";

            if (activeOnly)
            {
                sql += " AND is_active = 1";
            }

            sql += " ORDER BY display_order ASC";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                plans.Add(MapPlanFromReader(reader));
            }

            return plans;
        }

        public async Task<SubscriptionPlanModel?> GetPlanByIdAsync(int planId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, code, description, price, admin_fee_percentage,
                       has_stock_clerk_access, has_cashier_access, has_accounting_access, 
                       has_full_reports_access, display_order, is_active, created_at
                FROM dbo.tbl_subscription_plans
                WHERE id = @PlanId AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PlanId", planId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return MapPlanFromReader(reader);
            }

            return null;
        }

        public async Task<SubscriptionPlanModel?> GetPlanByCodeAsync(string code, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, name, code, description, price, admin_fee_percentage,
                       has_stock_clerk_access, has_cashier_access, has_accounting_access, 
                       has_full_reports_access, display_order, is_active, created_at
                FROM dbo.tbl_subscription_plans
                WHERE code = @Code AND archived_at IS NULL AND is_active = 1";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Code", code.ToUpperInvariant());

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return MapPlanFromReader(reader);
            }

            return null;
        }

        private static SubscriptionPlanModel MapPlanFromReader(SqlDataReader reader)
        {
            return new SubscriptionPlanModel
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Code = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                Price = reader.GetDecimal(4),
                AdminFeePercentage = reader.GetDecimal(5),
                HasStockClerkAccess = reader.GetBoolean(6),
                HasCashierAccess = reader.GetBoolean(7),
                HasAccountingAccess = reader.GetBoolean(8),
                HasFullReportsAccess = reader.GetBoolean(9),
                DisplayOrder = reader.GetInt32(10),
                IsActive = reader.GetBoolean(11),
                CreatedAt = reader.GetDateTime(12)
            };
        }

        #endregion

        #region Seller Subscription Management

        public async Task<SellerSubscriptionModel?> GetSellerSubscriptionAsync(int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT ss.id, ss.seller_user_id, ss.plan_id, sp.name as plan_name, sp.code as plan_code,
                       sp.admin_fee_percentage, ss.start_date, ss.end_date, ss.status,
                       ss.last_payment_date, ss.next_billing_date, ss.previous_plan_id,
                       pp.name as previous_plan_name, ss.plan_changed_at,
                       sp.has_stock_clerk_access, sp.has_cashier_access, sp.has_accounting_access, sp.has_full_reports_access
                FROM dbo.tbl_seller_subscriptions ss
                INNER JOIN dbo.tbl_subscription_plans sp ON ss.plan_id = sp.id
                LEFT JOIN dbo.tbl_subscription_plans pp ON ss.previous_plan_id = pp.id
                WHERE ss.seller_user_id = @SellerUserId 
                AND ss.status = 'Active'
                AND ss.archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return MapSubscriptionFromReader(reader);
            }

            return null;
        }

        public async Task<int?> CreateSubscriptionAsync(int sellerUserId, int planId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Check if seller already has an active subscription
            var checkSql = @"
                SELECT COUNT(*) FROM dbo.tbl_seller_subscriptions 
                WHERE seller_user_id = @SellerUserId AND status = 'Active' AND archived_at IS NULL";

            using var checkCmd = new SqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            var existingCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct));

            if (existingCount > 0)
            {
                return null; // Already has active subscription
            }

            var sql = @"
                INSERT INTO dbo.tbl_seller_subscriptions 
                    (seller_user_id, plan_id, start_date, status, created_at)
                VALUES 
                    (@SellerUserId, @PlanId, SYSUTCDATETIME(), 'Active', SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            cmd.Parameters.AddWithValue("@PlanId", planId);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : null;
        }

        public async Task<bool> UpdateSubscriptionPlanAsync(int sellerUserId, int newPlanId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Get current subscription
            var getCurrentSql = @"
                SELECT id, plan_id FROM dbo.tbl_seller_subscriptions 
                WHERE seller_user_id = @SellerUserId AND status = 'Active' AND archived_at IS NULL";

            using var getCurrentCmd = new SqlCommand(getCurrentSql, conn);
            getCurrentCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            int? currentSubscriptionId = null;
            int? currentPlanId = null;

            using (var reader = await getCurrentCmd.ExecuteReaderAsync(ct))
            {
                if (await reader.ReadAsync(ct))
                {
                    currentSubscriptionId = reader.GetInt32(0);
                    currentPlanId = reader.GetInt32(1);
                }
            }

            if (!currentSubscriptionId.HasValue)
            {
                // No active subscription, create new one
                return await CreateSubscriptionAsync(sellerUserId, newPlanId, ct) != null;
            }

            if (currentPlanId == newPlanId)
            {
                return true; // Already on this plan
            }

            // Update to new plan
            var updateSql = @"
                UPDATE dbo.tbl_seller_subscriptions
                SET plan_id = @NewPlanId,
                    previous_plan_id = @CurrentPlanId,
                    plan_changed_at = SYSUTCDATETIME(),
                    updated_at = SYSUTCDATETIME()
                WHERE id = @SubscriptionId";

            using var updateCmd = new SqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@NewPlanId", newPlanId);
            updateCmd.Parameters.AddWithValue("@CurrentPlanId", currentPlanId);
            updateCmd.Parameters.AddWithValue("@SubscriptionId", currentSubscriptionId);

            var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> CancelSubscriptionAsync(int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_seller_subscriptions
                SET status = 'Cancelled',
                    end_date = SYSUTCDATETIME(),
                    updated_at = SYSUTCDATETIME()
                WHERE seller_user_id = @SellerUserId 
                AND status = 'Active' 
                AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> HasActiveSubscriptionAsync(int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*) FROM dbo.tbl_seller_subscriptions 
                WHERE seller_user_id = @SellerUserId 
                AND status = 'Active' 
                AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            return count > 0;
        }

        private static SellerSubscriptionModel MapSubscriptionFromReader(SqlDataReader reader)
        {
            return new SellerSubscriptionModel
            {
                Id = reader.GetInt32(0),
                SellerUserId = reader.GetInt32(1),
                PlanId = reader.GetInt32(2),
                PlanName = reader.GetString(3),
                PlanCode = reader.GetString(4),
                AdminFeePercentage = reader.GetDecimal(5),
                StartDate = reader.GetDateTime(6),
                EndDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                Status = reader.GetString(8),
                LastPaymentDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                NextBillingDate = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                PreviousPlanId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                PreviousPlanName = reader.IsDBNull(12) ? null : reader.GetString(12),
                PlanChangedAt = reader.IsDBNull(13) ? null : reader.GetDateTime(13),
                HasStockClerkAccess = reader.GetBoolean(14),
                HasCashierAccess = reader.GetBoolean(15),
                HasAccountingAccess = reader.GetBoolean(16),
                HasFullReportsAccess = reader.GetBoolean(17)
            };
        }

        #endregion

        #region Module Access Validation

        public async Task<bool> CanAccessModuleAsync(int sellerUserId, string moduleName, CancellationToken ct = default)
        {
            var subscription = await GetSellerSubscriptionAsync(sellerUserId, ct);
            
            if (subscription == null)
            {
                return false; // No active subscription
            }

            return moduleName.ToLowerInvariant() switch
            {
                "stockclerk" or "stock_clerk" or "inventory" => subscription.HasStockClerkAccess,
                "cashier" or "pos" or "sales" => subscription.HasCashierAccess,
                "accounting" or "finance" => subscription.HasAccountingAccess,
                "reports" or "analytics" => subscription.HasFullReportsAccess,
                _ => false
            };
        }

        public async Task<bool> ValidateSubscriptionOnLoginAsync(int userId, string role, CancellationToken ct = default)
        {
            // Only validate for sellers
            if (!role.Equals("seller", StringComparison.OrdinalIgnoreCase))
            {
                return true; // Non-sellers don't need subscription validation
            }

            return await HasActiveSubscriptionAsync(userId, ct);
        }

        #endregion

        #region Admin Fee Calculation

        public async Task<decimal> GetAdminFeePercentageAsync(int sellerUserId, CancellationToken ct = default)
        {
            var subscription = await GetSellerSubscriptionAsync(sellerUserId, ct);
            return subscription?.AdminFeePercentage ?? 0m;
        }

        public async Task<decimal> CalculateAdminFeeAsync(int sellerUserId, decimal saleAmount, CancellationToken ct = default)
        {
            var feePercentage = await GetAdminFeePercentageAsync(sellerUserId, ct);
            return Math.Round(saleAmount * (feePercentage / 100m), 2);
        }

        public async Task<int?> RecordAdminFeeTransactionAsync(int sellerUserId, int saleId, decimal saleAmount, CancellationToken ct = default)
        {
            var subscription = await GetSellerSubscriptionAsync(sellerUserId, ct);
            if (subscription == null)
            {
                return null;
            }

            var adminFeeAmount = Math.Round(saleAmount * (subscription.AdminFeePercentage / 100m), 2);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                INSERT INTO dbo.tbl_subscription_transactions 
                    (seller_user_id, subscription_id, sale_id, transaction_type, sale_amount, 
                     admin_fee_percentage, admin_fee_amount, status, created_at)
                VALUES 
                    (@SellerUserId, @SubscriptionId, @SaleId, 'AdminFee', @SaleAmount,
                     @AdminFeePercentage, @AdminFeeAmount, 'Pending', SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            cmd.Parameters.AddWithValue("@SubscriptionId", subscription.Id);
            cmd.Parameters.AddWithValue("@SaleId", saleId);
            cmd.Parameters.AddWithValue("@SaleAmount", saleAmount);
            cmd.Parameters.AddWithValue("@AdminFeePercentage", subscription.AdminFeePercentage);
            cmd.Parameters.AddWithValue("@AdminFeeAmount", adminFeeAmount);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : null;
        }

        #endregion

        #region Admin Fee Reporting

        public async Task<List<SubscriptionTransactionModel>> GetAdminFeeTransactionsAsync(
            int? sellerUserId = null, DateTime? startDate = null, DateTime? endDate = null, 
            string? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var transactions = new List<SubscriptionTransactionModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT st.id, st.seller_user_id, st.subscription_id, st.sale_id, s.sale_number,
                       st.transaction_type, st.sale_amount, st.admin_fee_percentage, 
                       st.admin_fee_amount, st.status, st.collected_at, st.created_at
                FROM dbo.tbl_subscription_transactions st
                LEFT JOIN dbo.tbl_sales s ON st.sale_id = s.id
                WHERE 1=1";

            if (sellerUserId.HasValue)
            {
                sql += " AND st.seller_user_id = @SellerUserId";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(st.created_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(st.created_at AS DATE) <= @EndDate";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND st.status = @Status";
            }

            sql += " ORDER BY st.created_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);

            if (sellerUserId.HasValue)
                cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId.Value);
            if (startDate.HasValue)
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            if (endDate.HasValue)
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@Status", status);

            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                transactions.Add(new SubscriptionTransactionModel
                {
                    Id = reader.GetInt32(0),
                    SellerUserId = reader.GetInt32(1),
                    SubscriptionId = reader.GetInt32(2),
                    SaleId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    SaleNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    TransactionType = reader.GetString(5),
                    SaleAmount = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                    AdminFeePercentage = reader.GetDecimal(7),
                    AdminFeeAmount = reader.GetDecimal(8),
                    Status = reader.GetString(9),
                    CollectedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                    CreatedAt = reader.GetDateTime(11)
                });
            }

            return transactions;
        }

        public async Task<int> GetAdminFeeTransactionsCountAsync(
            int? sellerUserId = null, DateTime? startDate = null, DateTime? endDate = null, 
            string? status = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*) FROM dbo.tbl_subscription_transactions WHERE 1=1";

            if (sellerUserId.HasValue)
            {
                sql += " AND seller_user_id = @SellerUserId";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(created_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(created_at AS DATE) <= @EndDate";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @Status";
            }

            using var cmd = new SqlCommand(sql, conn);

            if (sellerUserId.HasValue)
                cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId.Value);
            if (startDate.HasValue)
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            if (endDate.HasValue)
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@Status", status);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<AdminFeeSummaryModel?> GetAdminFeeSummaryAsync(int sellerUserId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    st.seller_user_id,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as seller_name,
                    sp.name as plan_name,
                    sp.admin_fee_percentage,
                    COALESCE(SUM(st.sale_amount), 0) as total_sales_amount,
                    COALESCE(SUM(st.admin_fee_amount), 0) as total_admin_fees,
                    COALESCE(SUM(CASE WHEN st.status = 'Pending' THEN st.admin_fee_amount ELSE 0 END), 0) as pending_admin_fees,
                    COALESCE(SUM(CASE WHEN st.status = 'Collected' THEN st.admin_fee_amount ELSE 0 END), 0) as collected_admin_fees,
                    COUNT(*) as total_transactions
                FROM dbo.tbl_subscription_transactions st
                INNER JOIN dbo.tbl_seller_subscriptions ss ON st.subscription_id = ss.id
                INNER JOIN dbo.tbl_subscription_plans sp ON ss.plan_id = sp.id
                INNER JOIN dbo.tbl_users u ON st.seller_user_id = u.id
                WHERE st.seller_user_id = @SellerUserId
                AND st.transaction_type = 'AdminFee'";

            if (startDate.HasValue)
            {
                sql += " AND CAST(st.created_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(st.created_at AS DATE) <= @EndDate";
            }

            sql += " GROUP BY st.seller_user_id, u.name, u.fname, u.lname, sp.name, sp.admin_fee_percentage";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            if (startDate.HasValue)
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            if (endDate.HasValue)
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new AdminFeeSummaryModel
                {
                    SellerUserId = reader.GetInt32(0),
                    SellerName = reader.GetString(1),
                    PlanName = reader.GetString(2),
                    AdminFeePercentage = reader.GetDecimal(3),
                    TotalSalesAmount = reader.GetDecimal(4),
                    TotalAdminFees = reader.GetDecimal(5),
                    PendingAdminFees = reader.GetDecimal(6),
                    CollectedAdminFees = reader.GetDecimal(7),
                    TotalTransactions = reader.GetInt32(8)
                };
            }

            return null;
        }

        public async Task<List<AdminFeeSummaryModel>> GetAllSellerAdminFeeSummariesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            var summaries = new List<AdminFeeSummaryModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    st.seller_user_id,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as seller_name,
                    sp.name as plan_name,
                    sp.admin_fee_percentage,
                    COALESCE(SUM(st.sale_amount), 0) as total_sales_amount,
                    COALESCE(SUM(st.admin_fee_amount), 0) as total_admin_fees,
                    COALESCE(SUM(CASE WHEN st.status = 'Pending' THEN st.admin_fee_amount ELSE 0 END), 0) as pending_admin_fees,
                    COALESCE(SUM(CASE WHEN st.status = 'Collected' THEN st.admin_fee_amount ELSE 0 END), 0) as collected_admin_fees,
                    COUNT(*) as total_transactions
                FROM dbo.tbl_subscription_transactions st
                INNER JOIN dbo.tbl_seller_subscriptions ss ON st.subscription_id = ss.id
                INNER JOIN dbo.tbl_subscription_plans sp ON ss.plan_id = sp.id
                INNER JOIN dbo.tbl_users u ON st.seller_user_id = u.id
                WHERE st.transaction_type = 'AdminFee'";

            if (startDate.HasValue)
            {
                sql += " AND CAST(st.created_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(st.created_at AS DATE) <= @EndDate";
            }

            sql += " GROUP BY st.seller_user_id, u.name, u.fname, u.lname, sp.name, sp.admin_fee_percentage";
            sql += " ORDER BY total_admin_fees DESC";

            using var cmd = new SqlCommand(sql, conn);

            if (startDate.HasValue)
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            if (endDate.HasValue)
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                summaries.Add(new AdminFeeSummaryModel
                {
                    SellerUserId = reader.GetInt32(0),
                    SellerName = reader.GetString(1),
                    PlanName = reader.GetString(2),
                    AdminFeePercentage = reader.GetDecimal(3),
                    TotalSalesAmount = reader.GetDecimal(4),
                    TotalAdminFees = reader.GetDecimal(5),
                    PendingAdminFees = reader.GetDecimal(6),
                    CollectedAdminFees = reader.GetDecimal(7),
                    TotalTransactions = reader.GetInt32(8)
                });
            }

            return summaries;
        }

        public async Task<bool> MarkAdminFeeAsCollectedAsync(int transactionId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_subscription_transactions
                SET status = 'Collected',
                    collected_at = SYSUTCDATETIME(),
                    updated_at = SYSUTCDATETIME()
                WHERE id = @TransactionId AND status = 'Pending'";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TransactionId", transactionId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        #endregion

        #region Subscription History

        public async Task<List<SellerSubscriptionModel>> GetSubscriptionHistoryAsync(int sellerUserId, CancellationToken ct = default)
        {
            var subscriptions = new List<SellerSubscriptionModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT ss.id, ss.seller_user_id, ss.plan_id, sp.name as plan_name, sp.code as plan_code,
                       sp.admin_fee_percentage, ss.start_date, ss.end_date, ss.status,
                       ss.last_payment_date, ss.next_billing_date, ss.previous_plan_id,
                       pp.name as previous_plan_name, ss.plan_changed_at,
                       sp.has_stock_clerk_access, sp.has_cashier_access, sp.has_accounting_access, sp.has_full_reports_access
                FROM dbo.tbl_seller_subscriptions ss
                INNER JOIN dbo.tbl_subscription_plans sp ON ss.plan_id = sp.id
                LEFT JOIN dbo.tbl_subscription_plans pp ON ss.previous_plan_id = pp.id
                WHERE ss.seller_user_id = @SellerUserId
                AND ss.archived_at IS NULL
                ORDER BY ss.created_at DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                subscriptions.Add(MapSubscriptionFromReader(reader));
            }

            return subscriptions;
        }

        #endregion
    }

    #endregion
}
