using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class PurchaseOrderModel
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    public class PurchaseOrderDetailsModel
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierEmail { get; set; }
        public string? SupplierContactNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
        public List<POItemModel> Items { get; set; } = new();
    }

    public class POItemModel
    {
        public int Id { get; set; }
        public int POId { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHexValue { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int ReceivedQuantity { get; set; }
    }

    public class CreatePOModel
    {
        public int SupplierId { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public List<CreatePOItemModel> Items { get; set; } = new();
    }

    public class CreatePOItemModel
    {
        public int VariantId { get; set; }
        public int? SizeId { get; set; }
        public int? ColorId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrderModel>> GetPurchaseOrdersAsync(int userId, string? searchTerm = null, string? status = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetPurchaseOrdersCountAsync(int userId, string? searchTerm = null, string? status = null, CancellationToken ct = default);
        Task<List<PurchaseOrderModel>> GetCancelledPurchaseOrdersAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetCancelledPurchaseOrdersCountAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<List<PurchaseOrderModel>> GetCompletedPurchaseOrdersAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetCompletedPurchaseOrdersCountAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<PurchaseOrderDetailsModel?> GetPurchaseOrderDetailsAsync(int poId, int userId, CancellationToken ct = default);
        Task<int?> CreatePurchaseOrderAsync(int sellerUserId, int createdByUserId, CreatePOModel model, CancellationToken ct = default);
        Task<bool> UpdatePurchaseOrderStatusAsync(int poId, string status, int userId, CancellationToken ct = default);
        Task<bool> UpdatePurchaseOrderExpectedDateAsync(int poId, DateTime? expectedDeliveryDate, int userId, CancellationToken ct = default);
        Task<bool> DeletePurchaseOrderAsync(int poId, int userId, CancellationToken ct = default);
        Task<string> GeneratePONumberAsync(int userId, CancellationToken ct = default);
        Task<List<PurchaseOrderModel>> GetPendingPurchaseOrdersForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetPendingPurchaseOrdersCountForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<PurchaseOrderDetailsModel?> GetPurchaseOrderDetailsForAccountingAsync(int poId, int sellerUserId, CancellationToken ct = default);
    }

    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<PurchaseOrderModel>> GetPurchaseOrdersAsync(int userId, string? searchTerm = null, string? status = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var purchaseOrders = new List<PurchaseOrderModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT po.id, po.po_number, po.supplier_id, s.company_name, po.status, 
                       po.total_amount, po.created_at, po.expected_delivery_date,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                INNER JOIN dbo.tbl_users u ON po.created_by = u.id
                WHERE po.archived_at IS NULL AND po.status NOT IN ('Cancelled', 'Completed') AND s.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (po.po_number LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND po.status = @Status";
            }

            sql += " ORDER BY po.created_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@Status", status);
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                purchaseOrders.Add(new PurchaseOrderModel
                {
                    Id = reader.GetInt32(0),
                    PONumber = reader.GetString(1),
                    SupplierId = reader.GetInt32(2),
                    SupplierName = reader.GetString(3),
                    Status = reader.GetString(4),
                    TotalAmount = reader.GetDecimal(5),
                    CreatedAt = reader.GetDateTime(6),
                    ExpectedDeliveryDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    CreatedByName = reader.IsDBNull(8) ? "N/A" : reader.GetString(8)
                });
            }

            return purchaseOrders;
        }

        public async Task<int> GetPurchaseOrdersCountAsync(int userId, string? searchTerm = null, string? status = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                WHERE po.archived_at IS NULL AND po.status NOT IN ('Cancelled', 'Completed') AND s.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (po.po_number LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND po.status = @Status";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@Status", status);
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            var result = count != null ? Convert.ToInt32(count) : 0;
            
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"GetPurchaseOrdersCountAsync: userId={userId}, searchTerm='{searchTerm}', status='{status}', result={result}");
            
            return result;
        }

        public async Task<List<PurchaseOrderModel>> GetCancelledPurchaseOrdersAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var purchaseOrders = new List<PurchaseOrderModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT po.id, po.po_number, po.supplier_id, s.company_name, po.status, 
                       po.total_amount, po.created_at, po.expected_delivery_date,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                INNER JOIN dbo.tbl_users u ON po.created_by = u.id
                WHERE po.archived_at IS NOT NULL AND po.status = 'Cancelled' AND s.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (po.po_number LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(po.archived_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(po.archived_at AS DATE) <= @EndDate";
            }

            sql += " ORDER BY po.archived_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
            }
            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                purchaseOrders.Add(new PurchaseOrderModel
                {
                    Id = reader.GetInt32(0),
                    PONumber = reader.GetString(1),
                    SupplierId = reader.GetInt32(2),
                    SupplierName = reader.GetString(3),
                    Status = reader.GetString(4),
                    TotalAmount = reader.GetDecimal(5),
                    CreatedAt = reader.GetDateTime(6),
                    ExpectedDeliveryDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    CreatedByName = reader.IsDBNull(8) ? "N/A" : reader.GetString(8)
                });
            }

            return purchaseOrders;
        }

        public async Task<int> GetCancelledPurchaseOrdersCountAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                WHERE po.archived_at IS NOT NULL AND po.status = 'Cancelled' AND s.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (po.po_number LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(po.archived_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(po.archived_at AS DATE) <= @EndDate";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
            }
            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<List<PurchaseOrderModel>> GetCompletedPurchaseOrdersAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var purchaseOrders = new List<PurchaseOrderModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT po.id, po.po_number, po.supplier_id, s.company_name, po.status, 
                       po.total_amount, po.created_at, po.expected_delivery_date,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                INNER JOIN dbo.tbl_users u ON po.created_by = u.id
                WHERE po.archived_at IS NULL AND po.status = 'Completed' AND s.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (po.po_number LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(po.updated_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(po.updated_at AS DATE) <= @EndDate";
            }

            sql += " ORDER BY po.updated_at DESC, po.created_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
            }
            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                purchaseOrders.Add(new PurchaseOrderModel
                {
                    Id = reader.GetInt32(0),
                    PONumber = reader.GetString(1),
                    SupplierId = reader.GetInt32(2),
                    SupplierName = reader.GetString(3),
                    Status = reader.GetString(4),
                    TotalAmount = reader.GetDecimal(5),
                    CreatedAt = reader.GetDateTime(6),
                    ExpectedDeliveryDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    CreatedByName = reader.IsDBNull(8) ? "N/A" : reader.GetString(8)
                });
            }

            return purchaseOrders;
        }

        public async Task<int> GetCompletedPurchaseOrdersCountAsync(int userId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                WHERE po.archived_at IS NULL AND po.status = 'Completed' AND s.user_id = @UserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (po.po_number LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(po.updated_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(po.updated_at AS DATE) <= @EndDate";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
            }
            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<PurchaseOrderDetailsModel?> GetPurchaseOrderDetailsAsync(int poId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT po.id, po.po_number, po.supplier_id, s.company_name, s.email, s.contact_number,
                       po.status, po.total_amount, po.notes, po.expected_delivery_date,
                       po.created_at, po.updated_at, po.created_by, 
                       COALESCE(u1.name, (LTRIM(RTRIM(ISNULL(u1.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u1.lname,''))))) as created_by_name,
                       po.updated_by, 
                       COALESCE(u2.name, (LTRIM(RTRIM(ISNULL(u2.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u2.lname,''))))) as updated_by_name
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                INNER JOIN dbo.tbl_users u1 ON po.created_by = u1.id
                LEFT JOIN dbo.tbl_users u2 ON po.updated_by = u2.id
                WHERE po.id = @POId AND (@UserId = 0 OR s.user_id = @UserId)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@POId", poId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var po = new PurchaseOrderDetailsModel
                {
                    Id = reader.GetInt32(0),
                    PONumber = reader.GetString(1),
                    SupplierId = reader.GetInt32(2),
                    SupplierName = reader.GetString(3),
                    SupplierEmail = reader.IsDBNull(4) ? null : reader.GetString(4),
                    SupplierContactNumber = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Status = reader.GetString(6),
                    TotalAmount = reader.GetDecimal(7),
                    Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ExpectedDeliveryDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                    CreatedAt = reader.GetDateTime(10),
                    UpdatedAt = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                    CreatedBy = reader.GetInt32(12),
                    CreatedByName = reader.IsDBNull(13) ? "N/A" : reader.GetString(13),
                    UpdatedBy = reader.IsDBNull(14) ? null : reader.GetInt32(14),
                    UpdatedByName = reader.IsDBNull(15) ? null : reader.GetString(15),
                    Items = new List<POItemModel>()
                };

                reader.Close();

                // Load PO items
                var itemsSql = @"
                    SELECT poi.id, poi.po_id, poi.variant_id, v.name as variant_name, p.name as product_name,
                           poi.size_id, sz.name as size_name,
                           poi.color_id, c.name as color_name, c.hex_value as color_hex,
                           poi.quantity, poi.unit_price, poi.total_price, poi.received_quantity
                    FROM dbo.tbl_po_items poi
                    INNER JOIN dbo.tbl_variants v ON poi.variant_id = v.id
                    INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                    LEFT JOIN dbo.tbl_sizes sz ON poi.size_id = sz.id
                    LEFT JOIN dbo.tbl_colors c ON poi.color_id = c.id
                    WHERE poi.po_id = @POId
                    ORDER BY poi.id";

                using var itemsCmd = new SqlCommand(itemsSql, conn);
                itemsCmd.Parameters.AddWithValue("@POId", poId);

                using var itemsReader = await itemsCmd.ExecuteReaderAsync(ct);
                while (await itemsReader.ReadAsync(ct))
                {
                    po.Items.Add(new POItemModel
                    {
                        Id = itemsReader.GetInt32(0),
                        POId = itemsReader.GetInt32(1),
                        VariantId = itemsReader.GetInt32(2),
                        VariantName = itemsReader.GetString(3),
                        ProductName = itemsReader.GetString(4),
                        SizeId = itemsReader.IsDBNull(5) ? null : itemsReader.GetInt32(5),
                        SizeName = itemsReader.IsDBNull(6) ? null : itemsReader.GetString(6),
                        ColorId = itemsReader.IsDBNull(7) ? null : itemsReader.GetInt32(7),
                        ColorName = itemsReader.IsDBNull(8) ? null : itemsReader.GetString(8),
                        ColorHexValue = itemsReader.IsDBNull(9) ? null : itemsReader.GetString(9),
                        Quantity = itemsReader.GetInt32(10),
                        UnitPrice = itemsReader.GetDecimal(11),
                        TotalPrice = itemsReader.GetDecimal(12),
                        ReceivedQuantity = itemsReader.GetInt32(13)
                    });
                }

                return po;
            }

            return null;
        }

        public async Task<int?> CreatePurchaseOrderAsync(int sellerUserId, int createdByUserId, CreatePOModel model, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            using var transaction = conn.BeginTransaction();

            try
            {
                // Verify supplier belongs to seller
                var verifySql = "SELECT COUNT(*) FROM dbo.tbl_suppliers WHERE id = @SupplierId AND user_id = @SellerUserId AND archived_at IS NULL";
                using var verifyCmd = new SqlCommand(verifySql, conn, transaction);
                verifyCmd.Parameters.AddWithValue("@SupplierId", model.SupplierId);
                verifyCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                var count = await verifyCmd.ExecuteScalarAsync(ct);
                if (count == null || Convert.ToInt32(count) == 0)
                {
                    transaction.Rollback();
                    return null;
                }

                // Generate PO number (within transaction)
                var poNumberSql = @"
                    SELECT COUNT(*) + 1
                    FROM dbo.tbl_purchase_orders po
                    INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                    WHERE s.user_id = @SellerUserId AND po.archived_at IS NULL
                    AND YEAR(po.created_at) = YEAR(GETDATE())";
                using var poNumberCmd = new SqlCommand(poNumberSql, conn, transaction);
                poNumberCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                var poNumberResult = await poNumberCmd.ExecuteScalarAsync(ct);
                int sequence = poNumberResult != null ? Convert.ToInt32(poNumberResult) : 1;
                var poNumber = $"PO-{DateTime.Now:yyyyMM}-{sequence:D4}";

                // Calculate total amount
                decimal totalAmount = model.Items.Sum(item => item.Quantity * item.UnitPrice);

                // Insert purchase order
                var insertSql = @"
                    INSERT INTO dbo.tbl_purchase_orders (po_number, supplier_id, status, total_amount, notes, expected_delivery_date, created_by)
                    VALUES (@PONumber, @SupplierId, 'Pending', @TotalAmount, @Notes, @ExpectedDeliveryDate, @CreatedBy);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using var insertCmd = new SqlCommand(insertSql, conn, transaction);
                insertCmd.Parameters.AddWithValue("@PONumber", poNumber);
                insertCmd.Parameters.AddWithValue("@SupplierId", model.SupplierId);
                insertCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                insertCmd.Parameters.AddWithValue("@Notes", (object?)model.Notes ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@ExpectedDeliveryDate", (object?)model.ExpectedDeliveryDate ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@CreatedBy", createdByUserId);

                var poId = await insertCmd.ExecuteScalarAsync(ct);
                if (poId == null)
                {
                    transaction.Rollback();
                    return null;
                }

                int purchaseOrderId = Convert.ToInt32(poId);

                // Insert PO items
                foreach (var item in model.Items)
                {
                    var itemSql = @"
                        INSERT INTO dbo.tbl_po_items (po_id, variant_id, size_id, color_id, quantity, unit_price, total_price)
                        VALUES (@POId, @VariantId, @SizeId, @ColorId, @Quantity, @UnitPrice, @TotalPrice)";

                    using var itemCmd = new SqlCommand(itemSql, conn, transaction);
                    itemCmd.Parameters.AddWithValue("@POId", purchaseOrderId);
                    itemCmd.Parameters.AddWithValue("@VariantId", item.VariantId);
                    itemCmd.Parameters.AddWithValue("@SizeId", (object?)item.SizeId ?? DBNull.Value);
                    itemCmd.Parameters.AddWithValue("@ColorId", (object?)item.ColorId ?? DBNull.Value);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@TotalPrice", item.Quantity * item.UnitPrice);

                    await itemCmd.ExecuteNonQueryAsync(ct);
                }

                transaction.Commit();
                return purchaseOrderId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                // Log error for debugging
                System.Diagnostics.Debug.WriteLine($"Error creating purchase order: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> UpdatePurchaseOrderStatusAsync(int poId, string status, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            using var transaction = conn.BeginTransaction();

            try
            {
                // Set required options for filtered indexes within transaction
                using var setOptionsCmd = new SqlCommand("SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET ARITHABORT ON;", conn, transaction);
                await setOptionsCmd.ExecuteNonQueryAsync(ct);

                // Verify PO belongs to user's supplier and get supplier_id
                var verifySql = @"
                    SELECT po.supplier_id, po.status
                    FROM dbo.tbl_purchase_orders po
                    INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                    WHERE po.id = @POId AND s.user_id = @UserId AND po.archived_at IS NULL";

                using var verifyCmd = new SqlCommand(verifySql, conn, transaction);
                verifyCmd.Parameters.AddWithValue("@POId", poId);
                verifyCmd.Parameters.AddWithValue("@UserId", userId);
                
                using var reader = await verifyCmd.ExecuteReaderAsync(ct);
                if (!await reader.ReadAsync(ct))
                {
                    System.Diagnostics.Debug.WriteLine($"PO {poId} not found or doesn't belong to user {userId}");
                    transaction.Rollback();
                    return false;
                }

                int supplierId = reader.GetInt32(0);
                string currentStatus = reader.GetString(1);
                reader.Close();

                System.Diagnostics.Debug.WriteLine($"Updating PO {poId} from status '{currentStatus}' to '{status}', supplierId: {supplierId}");

                // If status is Cancelled, automatically archive it
                var sql = status == "Cancelled"
                    ? @"
                        UPDATE dbo.tbl_purchase_orders 
                        SET status = @Status, updated_at = SYSUTCDATETIME(), updated_by = @UserId, archived_at = SYSUTCDATETIME()
                        WHERE id = @POId"
                    : @"
                        UPDATE dbo.tbl_purchase_orders 
                        SET status = @Status, updated_at = SYSUTCDATETIME(), updated_by = @UserId
                        WHERE id = @POId";

                using var cmd = new SqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@POId", poId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                await cmd.ExecuteNonQueryAsync(ct);

                // If status is being changed to "Completed", add items to inventory via stock_in
                if (status == "Completed" && currentStatus != "Completed")
                {
                    System.Diagnostics.Debug.WriteLine($"Creating stock_in records for PO {poId}");
                    
                    // Get all PO items with variant owner's user_id - read into list first
                    var itemsSql = @"
                        SELECT poi.variant_id, poi.size_id, poi.color_id, poi.quantity, poi.unit_price, v.user_id
                        FROM dbo.tbl_po_items poi
                        INNER JOIN dbo.tbl_variants v ON poi.variant_id = v.id
                        WHERE poi.po_id = @POId";

                    var poItems = new List<(int variantId, int? sizeId, int? colorId, int quantity, decimal unitPrice, int variantOwnerUserId)>();

                    using (var itemsCmd = new SqlCommand(itemsSql, conn, transaction))
                    {
                        itemsCmd.Parameters.AddWithValue("@POId", poId);
                        using var itemsReader = await itemsCmd.ExecuteReaderAsync(ct);
                        while (await itemsReader.ReadAsync(ct))
                        {
                            int variantId = itemsReader.GetInt32(0);
                            int? sizeId = itemsReader.IsDBNull(1) ? null : itemsReader.GetInt32(1);
                            int? colorId = itemsReader.IsDBNull(2) ? null : itemsReader.GetInt32(2);
                            int quantity = itemsReader.GetInt32(3);
                            decimal unitPrice = itemsReader.GetDecimal(4);
                            int variantOwnerUserId = itemsReader.GetInt32(5);
                            
                            poItems.Add((variantId, sizeId, colorId, quantity, unitPrice, variantOwnerUserId));
                        }
                        // Reader is automatically closed when using block exits
                    }

                    // Now insert stock_in records (reader is closed)
                    int itemCount = 0;
                    foreach (var item in poItems)
                    {
                        itemCount++;
                        System.Diagnostics.Debug.WriteLine($"Creating stock_in for item {itemCount}: variantId={item.variantId}, sizeId={item.sizeId}, colorId={item.colorId}, quantity={item.quantity}, variantOwnerUserId={item.variantOwnerUserId}");

                        // Create stock_in record for each item using variant owner's user_id
                        var stockInSql = @"
                            INSERT INTO dbo.tbl_stock_in (user_id, variant_id, size_id, color_id, quantity_added, cost_price, supplier_id, timestamps)
                            VALUES (@UserId, @VariantId, @SizeId, @ColorId, @QuantityAdded, @CostPrice, @SupplierId, SYSUTCDATETIME())";

                        using var stockInCmd = new SqlCommand(stockInSql, conn, transaction);
                        stockInCmd.Parameters.AddWithValue("@UserId", item.variantOwnerUserId); // Use variant owner's user_id
                        stockInCmd.Parameters.AddWithValue("@VariantId", item.variantId);
                        stockInCmd.Parameters.AddWithValue("@SizeId", (object?)item.sizeId ?? DBNull.Value);
                        stockInCmd.Parameters.AddWithValue("@ColorId", (object?)item.colorId ?? DBNull.Value);
                        stockInCmd.Parameters.AddWithValue("@QuantityAdded", item.quantity);
                        stockInCmd.Parameters.AddWithValue("@CostPrice", item.unitPrice);
                        stockInCmd.Parameters.AddWithValue("@SupplierId", supplierId);

                        await stockInCmd.ExecuteNonQueryAsync(ct);
                        System.Diagnostics.Debug.WriteLine($"Successfully created stock_in record for item {itemCount}");
                    }
                    System.Diagnostics.Debug.WriteLine($"Created {itemCount} stock_in records for PO {poId}");
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                var errorMessage = $"Error updating purchase order status: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(errorMessage);
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    errorMessage += $" Inner: {ex.InnerException.Message}";
                }
                // Log SQL errors specifically
                if (ex is Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    System.Diagnostics.Debug.WriteLine($"SQL Error Number: {sqlEx.Number}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error State: {sqlEx.State}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error Class: {sqlEx.Class}");
                    errorMessage += $" SQL Error: {sqlEx.Number} - {sqlEx.Message}";
                }
                System.Diagnostics.Debug.WriteLine($"Full error: {errorMessage}");
                return false;
            }
        }

        public async Task<bool> UpdatePurchaseOrderExpectedDateAsync(int poId, DateTime? expectedDeliveryDate, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            try
            {
                // Verify PO belongs to user's supplier
                var verifySql = @"
                    SELECT COUNT(*) 
                    FROM dbo.tbl_purchase_orders po
                    INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                    WHERE po.id = @POId AND s.user_id = @UserId AND po.archived_at IS NULL";

                using var verifyCmd = new SqlCommand(verifySql, conn);
                verifyCmd.Parameters.AddWithValue("@POId", poId);
                verifyCmd.Parameters.AddWithValue("@UserId", userId);
                var count = await verifyCmd.ExecuteScalarAsync(ct);
                if (count == null || Convert.ToInt32(count) == 0)
                {
                    return false;
                }

                var sql = @"
                    UPDATE dbo.tbl_purchase_orders 
                    SET expected_delivery_date = @ExpectedDeliveryDate, updated_at = SYSUTCDATETIME(), updated_by = @UserId
                    WHERE id = @POId";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ExpectedDeliveryDate", (object?)expectedDeliveryDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@POId", poId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                await cmd.ExecuteNonQueryAsync(ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePurchaseOrderAsync(int poId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            try
            {
                // Verify PO belongs to user's supplier
                var verifySql = @"
                    SELECT COUNT(*) 
                    FROM dbo.tbl_purchase_orders po
                    INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                    WHERE po.id = @POId AND s.user_id = @UserId AND po.archived_at IS NULL";

                using var verifyCmd = new SqlCommand(verifySql, conn);
                verifyCmd.Parameters.AddWithValue("@POId", poId);
                verifyCmd.Parameters.AddWithValue("@UserId", userId);
                var count = await verifyCmd.ExecuteScalarAsync(ct);
                if (count == null || Convert.ToInt32(count) == 0)
                {
                    return false;
                }

                var sql = @"
                    UPDATE dbo.tbl_purchase_orders 
                    SET archived_at = SYSUTCDATETIME()
                    WHERE id = @POId";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@POId", poId);

                await cmd.ExecuteNonQueryAsync(ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GeneratePONumberAsync(int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*) + 1
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                WHERE s.user_id = @UserId AND po.archived_at IS NULL
                AND YEAR(po.created_at) = YEAR(GETDATE())";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var result = await cmd.ExecuteScalarAsync(ct);
            int sequence = result != null ? Convert.ToInt32(result) : 1;

            return $"PO-{DateTime.Now:yyyyMM}-{sequence:D4}";
        }

        public async Task<List<PurchaseOrderModel>> GetPendingPurchaseOrdersForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var purchaseOrders = new List<PurchaseOrderModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Get pending purchase orders created by stock clerks that belong to this seller
            var sql = @"
                SELECT po.id, po.po_number, po.supplier_id, s.company_name, po.status, 
                       po.total_amount, po.created_at, po.expected_delivery_date,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                INNER JOIN dbo.tbl_users u ON po.created_by = u.id
                WHERE po.archived_at IS NULL 
                AND po.status = 'Pending'
                AND s.user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (po.po_number LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(po.created_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(po.created_at AS DATE) <= @EndDate";
            }

            sql += " ORDER BY po.created_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                purchaseOrders.Add(new PurchaseOrderModel
                {
                    Id = reader.GetInt32(0),
                    PONumber = reader.GetString(1),
                    SupplierId = reader.GetInt32(2),
                    SupplierName = reader.GetString(3),
                    Status = reader.GetString(4),
                    TotalAmount = reader.GetDecimal(5),
                    CreatedAt = reader.GetDateTime(6),
                    ExpectedDeliveryDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    CreatedByName = reader.IsDBNull(8) ? "N/A" : reader.GetString(8)
                });
            }

            return purchaseOrders;
        }

        public async Task<int> GetPendingPurchaseOrdersCountForAccountingAsync(int sellerUserId, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                WHERE po.archived_at IS NULL 
                AND po.status = 'Pending'
                AND s.user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (po.po_number LIKE @SearchTerm OR s.company_name LIKE @SearchTerm)";
            }

            if (startDate.HasValue)
            {
                sql += " AND CAST(po.created_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                sql += " AND CAST(po.created_at AS DATE) <= @EndDate";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            if (startDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<PurchaseOrderDetailsModel?> GetPurchaseOrderDetailsForAccountingAsync(int poId, int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT po.id, po.po_number, po.supplier_id, s.company_name, po.status, 
                       po.total_amount, po.created_at, po.expected_delivery_date,
                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                INNER JOIN dbo.tbl_users u ON po.created_by = u.id
                WHERE po.id = @POId 
                AND po.archived_at IS NULL 
                AND s.user_id = @SellerUserId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@POId", poId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var poModel = new PurchaseOrderModel
                {
                    Id = reader.GetInt32(0),
                    PONumber = reader.GetString(1),
                    SupplierId = reader.GetInt32(2),
                    SupplierName = reader.GetString(3),
                    Status = reader.GetString(4),
                    TotalAmount = reader.GetDecimal(5),
                    CreatedAt = reader.GetDateTime(6),
                    ExpectedDeliveryDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    CreatedByName = reader.IsDBNull(8) ? "N/A" : reader.GetString(8)
                };

                reader.Close();

                // Get PO items
                var itemsSql = @"
                    SELECT poi.id, poi.po_id, poi.variant_id, v.name as variant_name, p.name as product_name,
                           poi.size_id, sz.name as size_name,
                           poi.color_id, c.name as color_name, c.hex_value as color_hex,
                           poi.quantity, poi.unit_price, poi.total_price, poi.received_quantity
                    FROM dbo.tbl_po_items poi
                    INNER JOIN dbo.tbl_variants v ON poi.variant_id = v.id
                    INNER JOIN dbo.tbl_products p ON v.product_id = p.id
                    LEFT JOIN dbo.tbl_sizes sz ON poi.size_id = sz.id
                    LEFT JOIN dbo.tbl_colors c ON poi.color_id = c.id
                    WHERE poi.po_id = @POId
                    ORDER BY poi.id";

                using var itemsCmd = new SqlCommand(itemsSql, conn);
                itemsCmd.Parameters.AddWithValue("@POId", poModel.Id);

                var items = new List<POItemModel>();
                using var itemsReader = await itemsCmd.ExecuteReaderAsync(ct);
                while (await itemsReader.ReadAsync(ct))
                {
                    items.Add(new POItemModel
                    {
                        Id = itemsReader.GetInt32(0),
                        POId = itemsReader.GetInt32(1),
                        VariantId = itemsReader.GetInt32(2),
                        VariantName = itemsReader.GetString(3),
                        ProductName = itemsReader.GetString(4),
                        SizeId = itemsReader.IsDBNull(5) ? null : itemsReader.GetInt32(5),
                        SizeName = itemsReader.IsDBNull(6) ? null : itemsReader.GetString(6),
                        ColorId = itemsReader.IsDBNull(7) ? null : itemsReader.GetInt32(7),
                        ColorName = itemsReader.IsDBNull(8) ? null : itemsReader.GetString(8),
                        ColorHexValue = itemsReader.IsDBNull(9) ? null : itemsReader.GetString(9),
                        Quantity = itemsReader.GetInt32(10),
                        UnitPrice = itemsReader.GetDecimal(11),
                        TotalPrice = itemsReader.GetDecimal(12),
                        ReceivedQuantity = itemsReader.GetInt32(13)
                    });
                }

                return new PurchaseOrderDetailsModel
                {
                    Id = poModel.Id,
                    PONumber = poModel.PONumber,
                    SupplierId = poModel.SupplierId,
                    SupplierName = poModel.SupplierName,
                    Status = poModel.Status,
                    TotalAmount = poModel.TotalAmount,
                    CreatedAt = poModel.CreatedAt,
                    ExpectedDeliveryDate = poModel.ExpectedDeliveryDate,
                    CreatedBy = 0, // Not available in this query
                    CreatedByName = poModel.CreatedByName,
                    Items = items
                };
            }

            return null;
        }
    }
}

