using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class SupplierInvoiceModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty; // PO Number or Invoice Number
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Description { get; set; }
        public string SourceType { get; set; } = "PurchaseOrder"; // "PurchaseOrder", "StockIn", or "Manual"
        public int? StockInId { get; set; }
        public int? POId { get; set; } // Purchase Order ID
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int SellerUserId { get; set; }
        
        // Calculated fields
        public decimal TotalPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid"; // "Paid", "Partially Paid", "Unpaid"
    }

    public class SupplierPaymentModel
    {
        public int Id { get; set; }
        public int? InvoiceId { get; set; }
        public int? POId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Cash, GCash, Bank
        public DateTime PaymentDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string? ReceiptImage { get; set; }
        public string? ReceiptContentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int SellerUserId { get; set; }
    }

    public class CreateSupplierPaymentModel
    {
        public int? InvoiceId { get; set; } // Optional: for manually created invoices
        public int? POId { get; set; } // Optional: for Purchase Order payments
        public string? StockInGroupKey { get; set; } // Optional: for stock-in group payments (format: "STOCK-{date}-{supplier_id}")
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string? ReceiptImage { get; set; }
        public string? ReceiptContentType { get; set; }
    }

    public interface ISupplierInvoiceService
    {
        Task<List<SupplierInvoiceModel>> GetPayableItemsAsync(int sellerUserId, string? searchTerm = null, int? supplierId = null, string? paymentStatus = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetPayableItemsCountAsync(int sellerUserId, string? searchTerm = null, int? supplierId = null, string? paymentStatus = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
        Task<SupplierInvoiceModel?> GetPayableItemByIdAsync(string sourceType, int sourceId, int sellerUserId, CancellationToken ct = default);
        Task<SupplierInvoiceModel?> GetPayableItemByStockInGroupAsync(DateTime stockInDate, int supplierId, int sellerUserId, CancellationToken ct = default);
        Task<List<SupplierPaymentModel>> GetPaymentsByPOIdAsync(int poId, int sellerUserId, CancellationToken ct = default);
        Task<List<SupplierPaymentModel>> GetPaymentsByStockInGroupAsync(string stockInGroupKey, int sellerUserId, CancellationToken ct = default);
        Task<List<SupplierPaymentModel>> GetPaymentsByInvoiceIdAsync(int invoiceId, int sellerUserId, CancellationToken ct = default);
        Task<int?> CreatePaymentAsync(int sellerUserId, int createdByUserId, CreateSupplierPaymentModel model, CancellationToken ct = default);
        Task<bool> DeletePaymentAsync(int paymentId, int sellerUserId, CancellationToken ct = default);
    }

    public class SupplierInvoiceService : ISupplierInvoiceService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<SupplierInvoiceModel>> GetPayableItemsAsync(int sellerUserId, string? searchTerm = null, int? supplierId = null, string? paymentStatus = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var payableItems = new List<SupplierInvoiceModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Get Purchase Orders (Completed) as payable items
            var poSql = @"
                SELECT 
                    po.id as po_id,
                    po.po_number,
                    po.supplier_id,
                    s.company_name as supplier_name,
                    CAST(po.created_at AS DATE) as invoice_date,
                    po.total_amount,
                    po.notes as description,
                    po.created_at,
                    po.created_by,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name,
                    COALESCE(SUM(sp.amount_paid), 0) as total_paid
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                INNER JOIN dbo.tbl_users u ON po.created_by = u.id
                LEFT JOIN dbo.tbl_supplier_payments sp ON po.id = sp.po_id AND sp.archived_at IS NULL
                WHERE po.archived_at IS NULL
                AND po.status = 'Completed'
                AND s.user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                poSql += " AND (po.po_number LIKE @SearchTerm OR s.company_name LIKE @SearchTerm OR po.notes LIKE @SearchTerm)";
            }

            if (supplierId.HasValue)
            {
                poSql += " AND po.supplier_id = @SupplierId";
            }

            if (startDate.HasValue)
            {
                poSql += " AND CAST(po.created_at AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                poSql += " AND CAST(po.created_at AS DATE) <= @EndDate";
            }

            poSql += @"
                GROUP BY po.id, po.po_number, po.supplier_id, s.company_name, CAST(po.created_at AS DATE), 
                         po.total_amount, po.notes, po.created_at, po.created_by, u.name, u.fname, u.lname";

            // Filter by payment status
            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                if (paymentStatus == "Paid")
                {
                    poSql += " HAVING COALESCE(SUM(sp.amount_paid), 0) >= po.total_amount";
                }
                else if (paymentStatus == "Partially Paid")
                {
                    poSql += " HAVING COALESCE(SUM(sp.amount_paid), 0) > 0 AND COALESCE(SUM(sp.amount_paid), 0) < po.total_amount";
                }
                else if (paymentStatus == "Unpaid")
                {
                    poSql += " HAVING COALESCE(SUM(sp.amount_paid), 0) = 0";
                }
            }

            using var poCmd = new SqlCommand(poSql, conn);
            poCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                poCmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            if (supplierId.HasValue)
            {
                poCmd.Parameters.AddWithValue("@SupplierId", supplierId.Value);
            }

            if (startDate.HasValue)
            {
                poCmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                poCmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }

            using (var poReader = await poCmd.ExecuteReaderAsync(ct))
            {
            while (await poReader.ReadAsync(ct))
            {
                var totalAmount = poReader.GetDecimal(5);
                var totalPaid = poReader.GetDecimal(10);
                var remainingBalance = totalAmount - totalPaid;
                
                string status;
                if (totalPaid >= totalAmount)
                {
                    status = "Paid";
                }
                else if (totalPaid > 0)
                {
                    status = "Partially Paid";
                }
                else
                {
                    status = "Unpaid";
                }

                var poId = poReader.GetInt32(0);

                payableItems.Add(new SupplierInvoiceModel
                {
                    Id = poId,
                    InvoiceNumber = poReader.GetString(1),
                    SupplierId = poReader.GetInt32(2),
                    SupplierName = poReader.GetString(3),
                    InvoiceDate = poReader.GetDateTime(4),
                    TotalAmount = totalAmount,
                    Description = poReader.IsDBNull(6) ? null : poReader.GetString(6),
                    SourceType = "PurchaseOrder",
                    POId = poId,
                    StockInId = null,
                    CreatedAt = poReader.GetDateTime(7),
                    CreatedBy = poReader.GetInt32(8),
                    CreatedByName = poReader.GetString(9),
                    SellerUserId = sellerUserId,
                    TotalPaid = totalPaid,
                    RemainingBalance = remainingBalance,
                    PaymentStatus = status
                });
            }
            }

            // Get stock-in records grouped by supplier and date
            var stockInSql = @"
                SELECT 
                    CAST(si.timestamps AS DATE) as invoice_date,
                    si.supplier_id,
                    s.company_name as supplier_name,
                    SUM(si.quantity_added * si.cost_price) as total_amount,
                    COUNT(*) as stock_in_count,
                    MIN(si.timestamps) as created_at,
                    MIN(si.user_id) as created_by,
                    COALESCE(SUM(sp.amount_paid), 0) as total_paid
                FROM dbo.tbl_stock_in si
                INNER JOIN dbo.tbl_suppliers s ON si.supplier_id = s.id
                LEFT JOIN dbo.tbl_supplier_payments sp ON sp.stock_in_group_key = CONCAT('STOCK-', FORMAT(CAST(si.timestamps AS DATE), 'yyyyMMdd'), '-', si.supplier_id) AND sp.archived_at IS NULL
                WHERE si.archives IS NULL
                AND si.supplier_id IS NOT NULL
                AND s.user_id = @SellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                stockInSql += " AND (s.company_name LIKE @SearchTerm)";
            }

            if (supplierId.HasValue)
            {
                stockInSql += " AND si.supplier_id = @SupplierId";
            }

            if (startDate.HasValue)
            {
                stockInSql += " AND CAST(si.timestamps AS DATE) >= @StartDate";
            }

            if (endDate.HasValue)
            {
                stockInSql += " AND CAST(si.timestamps AS DATE) <= @EndDate";
            }

            stockInSql += @"
                GROUP BY CAST(si.timestamps AS DATE), si.supplier_id, s.company_name";

            // Filter by payment status for stock-in
            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                if (paymentStatus == "Paid")
                {
                    stockInSql += " HAVING COALESCE(SUM(sp.amount_paid), 0) >= SUM(si.quantity_added * si.cost_price)";
                }
                else if (paymentStatus == "Partially Paid")
                {
                    stockInSql += " HAVING COALESCE(SUM(sp.amount_paid), 0) > 0 AND COALESCE(SUM(sp.amount_paid), 0) < SUM(si.quantity_added * si.cost_price)";
                }
                else if (paymentStatus == "Unpaid")
                {
                    stockInSql += " HAVING COALESCE(SUM(sp.amount_paid), 0) = 0";
                }
            }

            using var stockInCmd = new SqlCommand(stockInSql, conn);
            stockInCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                stockInCmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            if (supplierId.HasValue)
            {
                stockInCmd.Parameters.AddWithValue("@SupplierId", supplierId.Value);
            }

            if (startDate.HasValue)
            {
                stockInCmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                stockInCmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }

            using (var stockInReader = await stockInCmd.ExecuteReaderAsync(ct))
            {
            while (await stockInReader.ReadAsync(ct))
            {
                var totalAmount = stockInReader.GetDecimal(3);
                var totalPaid = stockInReader.GetDecimal(7);
                var remainingBalance = totalAmount - totalPaid;
                
                string status;
                if (totalPaid >= totalAmount)
                {
                    status = "Paid";
                }
                else if (totalPaid > 0)
                {
                    status = "Partially Paid";
                }
                else
                {
                    status = "Unpaid";
                }

                var stockInDate = stockInReader.GetDateTime(0);
                var stockInSupplierId = stockInReader.GetInt32(1);
                var stockInGroupKey = $"STOCK-{stockInDate:yyyyMMdd}-{stockInSupplierId}";

                payableItems.Add(new SupplierInvoiceModel
                {
                    Id = 0, // No ID for stock-in groups
                    InvoiceNumber = stockInGroupKey,
                    SupplierId = stockInSupplierId,
                    SupplierName = stockInReader.GetString(2),
                    InvoiceDate = stockInDate,
                    TotalAmount = totalAmount,
                    Description = $"Stock-in from {stockInReader.GetInt32(4)} items",
                    SourceType = "StockIn",
                    POId = null,
                    StockInId = null,
                    CreatedAt = stockInReader.GetDateTime(5),
                    CreatedBy = stockInReader.GetInt32(6),
                    CreatedByName = string.Empty, // Will be filled if needed
                    SellerUserId = sellerUserId,
                    TotalPaid = totalPaid,
                    RemainingBalance = remainingBalance,
                    PaymentStatus = status
                });
            }
            }

            // Sort combined results and apply pagination
            var sortedItems = payableItems
                .OrderByDescending(x => x.InvoiceDate)
                .ThenByDescending(x => x.CreatedAt)
                .ToList();

            // Apply payment status filter if needed (for stock-in items)
            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                sortedItems = sortedItems.Where(x => x.PaymentStatus == paymentStatus).ToList();
            }

            // Apply pagination
            var offset = (page - 1) * pageSize;
            return sortedItems.Skip(offset).Take(pageSize).ToList();
        }

        public async Task<int> GetPayableItemsCountAsync(int sellerUserId, string? searchTerm = null, int? supplierId = null, string? paymentStatus = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            // Get all items and count them (simpler approach)
            // Pass paymentStatus to GetPayableItemsAsync so SQL filtering works correctly
            var allItems = await GetPayableItemsAsync(sellerUserId, searchTerm, supplierId, paymentStatus, startDate, endDate, 1, int.MaxValue, ct);
            
            // The payment status filter is already applied in GetPayableItemsAsync, so just return count
            return allItems.Count;
        }

        public async Task<SupplierInvoiceModel?> GetPayableItemByIdAsync(string sourceType, int sourceId, int sellerUserId, CancellationToken ct = default)
        {
            if (sourceType != "PurchaseOrder") return null;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    po.id as po_id,
                    po.po_number,
                    po.supplier_id,
                    s.company_name as supplier_name,
                    CAST(po.created_at AS DATE) as invoice_date,
                    po.total_amount,
                    po.notes as description,
                    po.created_at,
                    po.created_by,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name,
                    COALESCE(SUM(sp.amount_paid), 0) as total_paid
                FROM dbo.tbl_purchase_orders po
                INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                INNER JOIN dbo.tbl_users u ON po.created_by = u.id
                LEFT JOIN dbo.tbl_supplier_payments sp ON po.id = sp.po_id AND sp.archived_at IS NULL
                WHERE po.id = @SourceId
                AND po.archived_at IS NULL
                AND po.status = 'Completed'
                AND s.user_id = @SellerUserId
                GROUP BY po.id, po.po_number, po.supplier_id, s.company_name, CAST(po.created_at AS DATE), 
                         po.total_amount, po.notes, po.created_at, po.created_by, u.name, u.fname, u.lname";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SourceId", sourceId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var totalAmount = reader.GetDecimal(5);
                var totalPaid = reader.GetDecimal(10);
                var remainingBalance = totalAmount - totalPaid;
                
                string status;
                if (totalPaid >= totalAmount)
                {
                    status = "Paid";
                }
                else if (totalPaid > 0)
                {
                    status = "Partially Paid";
                }
                else
                {
                    status = "Unpaid";
                }

                return new SupplierInvoiceModel
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    SupplierId = reader.GetInt32(2),
                    SupplierName = reader.GetString(3),
                    InvoiceDate = reader.GetDateTime(4),
                    TotalAmount = totalAmount,
                    Description = reader.IsDBNull(6) ? null : reader.GetString(6),
                    SourceType = "PurchaseOrder",
                    POId = reader.GetInt32(0),
                    StockInId = null,
                    CreatedAt = reader.GetDateTime(7),
                    CreatedBy = reader.GetInt32(8),
                    CreatedByName = reader.GetString(9),
                    SellerUserId = sellerUserId,
                    TotalPaid = totalPaid,
                    RemainingBalance = remainingBalance,
                    PaymentStatus = status
                };
            }

            return null;
        }

        public async Task<SupplierInvoiceModel?> GetPayableItemByStockInGroupAsync(DateTime stockInDate, int supplierId, int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    CAST(si.timestamps AS DATE) as invoice_date,
                    si.supplier_id,
                    s.company_name as supplier_name,
                    SUM(si.quantity_added * si.cost_price) as total_amount,
                    COUNT(*) as stock_in_count,
                    MIN(si.timestamps) as created_at,
                    MIN(si.user_id) as created_by,
                    COALESCE(SUM(sp.amount_paid), 0) as total_paid
                FROM dbo.tbl_stock_in si
                INNER JOIN dbo.tbl_suppliers s ON si.supplier_id = s.id
                LEFT JOIN dbo.tbl_supplier_payments sp ON sp.stock_in_group_key = CONCAT('STOCK-', FORMAT(CAST(si.timestamps AS DATE), 'yyyyMMdd'), '-', si.supplier_id) AND sp.archived_at IS NULL
                WHERE si.archives IS NULL
                AND si.supplier_id = @SupplierId
                AND CAST(si.timestamps AS DATE) = @StockInDate
                AND s.user_id = @SellerUserId
                GROUP BY CAST(si.timestamps AS DATE), si.supplier_id, s.company_name";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SupplierId", supplierId);
            cmd.Parameters.AddWithValue("@StockInDate", stockInDate.Date);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var totalAmount = reader.GetDecimal(3);
                var totalPaid = reader.GetDecimal(7);
                var remainingBalance = totalAmount - totalPaid;
                
                string status;
                if (totalPaid >= totalAmount)
                {
                    status = "Paid";
                }
                else if (totalPaid > 0)
                {
                    status = "Partially Paid";
                }
                else
                {
                    status = "Unpaid";
                }

                var stockInGroupKey = $"STOCK-{stockInDate:yyyyMMdd}-{supplierId}";

                return new SupplierInvoiceModel
                {
                    Id = 0,
                    InvoiceNumber = stockInGroupKey,
                    SupplierId = supplierId,
                    SupplierName = reader.GetString(2),
                    InvoiceDate = stockInDate,
                    TotalAmount = totalAmount,
                    Description = $"Stock-in from {reader.GetInt32(4)} items",
                    SourceType = "StockIn",
                    POId = null,
                    StockInId = null,
                    CreatedAt = reader.GetDateTime(5),
                    CreatedBy = reader.GetInt32(6),
                    CreatedByName = string.Empty,
                    SellerUserId = sellerUserId,
                    TotalPaid = totalPaid,
                    RemainingBalance = remainingBalance,
                    PaymentStatus = status
                };
            }

            return null;
        }

        public async Task<List<SupplierPaymentModel>> GetPaymentsByPOIdAsync(int poId, int sellerUserId, CancellationToken ct = default)
        {
            var payments = new List<SupplierPaymentModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    sp.id,
                    sp.invoice_id,
                    sp.po_id,
                    po.po_number as invoice_number,
                    sp.amount_paid,
                    sp.payment_method,
                    sp.payment_date,
                    sp.reference_number,
                    sp.notes,
                    sp.receipt_image_base64,
                    sp.receipt_image_content_type,
                    sp.created_at,
                    sp.created_by,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name,
                    sp.seller_user_id
                FROM dbo.tbl_supplier_payments sp
                INNER JOIN dbo.tbl_purchase_orders po ON sp.po_id = po.id
                INNER JOIN dbo.tbl_users u ON sp.created_by = u.id
                WHERE sp.po_id = @POId
                AND sp.archived_at IS NULL
                AND sp.seller_user_id = @SellerUserId
                ORDER BY sp.payment_date DESC, sp.created_at DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@POId", poId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                payments.Add(new SupplierPaymentModel
                {
                    Id = reader.GetInt32(0),
                    InvoiceId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    POId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    InvoiceNumber = reader.GetString(3),
                    AmountPaid = reader.GetDecimal(4),
                    PaymentMethod = reader.GetString(5),
                    PaymentDate = reader.GetDateTime(6),
                    ReferenceNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ReceiptImage = reader.IsDBNull(9) ? null : reader.GetString(9),
                    ReceiptContentType = reader.IsDBNull(10) ? null : reader.GetString(10),
                    CreatedAt = reader.GetDateTime(11),
                    CreatedBy = reader.GetInt32(12),
                    CreatedByName = reader.GetString(13),
                    SellerUserId = reader.GetInt32(14)
                });
            }

            return payments;
        }

        public async Task<List<SupplierPaymentModel>> GetPaymentsByStockInGroupAsync(string stockInGroupKey, int sellerUserId, CancellationToken ct = default)
        {
            var payments = new List<SupplierPaymentModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    sp.id,
                    sp.invoice_id,
                    sp.po_id,
                    sp.stock_in_group_key as invoice_number,
                    sp.amount_paid,
                    sp.payment_method,
                    sp.payment_date,
                    sp.reference_number,
                    sp.notes,
                    sp.receipt_image_base64,
                    sp.receipt_image_content_type,
                    sp.created_at,
                    sp.created_by,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name,
                    sp.seller_user_id
                FROM dbo.tbl_supplier_payments sp
                INNER JOIN dbo.tbl_users u ON sp.created_by = u.id
                WHERE sp.stock_in_group_key = @StockInGroupKey
                AND sp.archived_at IS NULL
                AND sp.seller_user_id = @SellerUserId
                ORDER BY sp.payment_date DESC, sp.created_at DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StockInGroupKey", stockInGroupKey);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                payments.Add(new SupplierPaymentModel
                {
                    Id = reader.GetInt32(0),
                    InvoiceId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    POId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    InvoiceNumber = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    AmountPaid = reader.GetDecimal(4),
                    PaymentMethod = reader.GetString(5),
                    PaymentDate = reader.GetDateTime(6),
                    ReferenceNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ReceiptImage = reader.IsDBNull(9) ? null : reader.GetString(9),
                    ReceiptContentType = reader.IsDBNull(10) ? null : reader.GetString(10),
                    CreatedAt = reader.GetDateTime(11),
                    CreatedBy = reader.GetInt32(12),
                    CreatedByName = reader.GetString(13),
                    SellerUserId = reader.GetInt32(14)
                });
            }

            return payments;
        }

        public async Task<List<SupplierPaymentModel>> GetPaymentsByInvoiceIdAsync(int invoiceId, int sellerUserId, CancellationToken ct = default)
        {
            var payments = new List<SupplierPaymentModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT 
                    sp.id,
                    sp.invoice_id,
                    sp.po_id,
                    si.invoice_number,
                    sp.amount_paid,
                    sp.payment_method,
                    sp.payment_date,
                    sp.reference_number,
                    sp.notes,
                    sp.receipt_image_base64,
                    sp.receipt_image_content_type,
                    sp.created_at,
                    sp.created_by,
                    COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) as created_by_name,
                    sp.seller_user_id
                FROM dbo.tbl_supplier_payments sp
                INNER JOIN dbo.tbl_supplier_invoices si ON sp.invoice_id = si.id
                INNER JOIN dbo.tbl_users u ON sp.created_by = u.id
                WHERE sp.invoice_id = @InvoiceId
                AND sp.archived_at IS NULL
                AND sp.seller_user_id = @SellerUserId
                ORDER BY sp.payment_date DESC, sp.created_at DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                payments.Add(new SupplierPaymentModel
                {
                    Id = reader.GetInt32(0),
                    InvoiceId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    POId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    InvoiceNumber = reader.GetString(3),
                    AmountPaid = reader.GetDecimal(4),
                    PaymentMethod = reader.GetString(5),
                    PaymentDate = reader.GetDateTime(6),
                    ReferenceNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ReceiptImage = reader.IsDBNull(9) ? null : reader.GetString(9),
                    ReceiptContentType = reader.IsDBNull(10) ? null : reader.GetString(10),
                    CreatedAt = reader.GetDateTime(11),
                    CreatedBy = reader.GetInt32(12),
                    CreatedByName = reader.GetString(13),
                    SellerUserId = reader.GetInt32(14)
                });
            }

            return payments;
        }

        public async Task<int?> CreatePaymentAsync(int sellerUserId, int createdByUserId, CreateSupplierPaymentModel model, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            int? invoiceId = model.InvoiceId;

            // Verify PO belongs to seller if paying a PO
            if (model.POId.HasValue)
            {
                var verifySql = @"
                    SELECT COUNT(*) 
                    FROM dbo.tbl_purchase_orders po
                    INNER JOIN dbo.tbl_suppliers s ON po.supplier_id = s.id
                    WHERE po.id = @POId 
                    AND po.status = 'Completed'
                    AND po.archived_at IS NULL
                    AND s.user_id = @SellerUserId";
                using var verifyCmd = new SqlCommand(verifySql, conn);
                verifyCmd.Parameters.AddWithValue("@POId", model.POId.Value);
                verifyCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                var count = await verifyCmd.ExecuteScalarAsync(ct);
                if (count == null || Convert.ToInt32(count) == 0)
                {
                    return null;
                }
            }

            // Auto-create supplier invoice for stock-in groups if it doesn't exist
            if (!string.IsNullOrWhiteSpace(model.StockInGroupKey) && !invoiceId.HasValue)
            {
                // Parse stock-in group key: format is "STOCK-{yyyyMMdd}-{supplier_id}"
                // Example: "STOCK-20251208-21"
                var parts = model.StockInGroupKey.Split('-');
                if (parts.Length >= 3 && parts[0] == "STOCK")
                {
                    if (DateTime.TryParseExact(parts[1], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var stockInDate) &&
                        int.TryParse(parts[2], out var supplierId))
                    {
                        // Check if invoice already exists for this stock-in group
                        var checkInvoiceSql = @"
                            SELECT id 
                            FROM dbo.tbl_supplier_invoices 
                            WHERE invoice_number = @InvoiceNumber 
                            AND seller_user_id = @SellerUserId 
                            AND archived_at IS NULL";
                        using var checkCmd = new SqlCommand(checkInvoiceSql, conn);
                        checkCmd.Parameters.AddWithValue("@InvoiceNumber", model.StockInGroupKey);
                        checkCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                        var existingInvoiceId = await checkCmd.ExecuteScalarAsync(ct);
                        
                        if (existingInvoiceId != null && existingInvoiceId != DBNull.Value)
                        {
                            invoiceId = Convert.ToInt32(existingInvoiceId);
                        }
                        else
                        {
                            // Get stock-in details to create invoice
                            var stockInDetailsSql = @"
                                SELECT 
                                    SUM(si.quantity_added * si.cost_price) as total_amount,
                                    COUNT(*) as stock_in_count
                                FROM dbo.tbl_stock_in si
                                INNER JOIN dbo.tbl_suppliers s ON si.supplier_id = s.id
                                WHERE si.archives IS NULL
                                AND si.supplier_id = @SupplierId
                                AND CAST(si.timestamps AS DATE) = @StockInDate
                                AND s.user_id = @SellerUserId";
                            
                            using var detailsCmd = new SqlCommand(stockInDetailsSql, conn);
                            detailsCmd.Parameters.AddWithValue("@SupplierId", supplierId);
                            detailsCmd.Parameters.AddWithValue("@StockInDate", stockInDate.Date);
                            detailsCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                            
                            using var detailsReader = await detailsCmd.ExecuteReaderAsync(ct);
                            if (await detailsReader.ReadAsync(ct))
                            {
                                var totalAmount = detailsReader.GetDecimal(0);
                                var stockInCount = detailsReader.GetInt32(1);
                                
                                detailsReader.Close();
                                
                                // Create supplier invoice
                                var createInvoiceSql = @"
                                    INSERT INTO dbo.tbl_supplier_invoices 
                                        (invoice_number, supplier_id, invoice_date, total_amount, description, source_type, created_by, seller_user_id, created_at)
                                    VALUES 
                                        (@InvoiceNumber, @SupplierId, @InvoiceDate, @TotalAmount, @Description, 'StockIn', @CreatedBy, @SellerUserId, SYSUTCDATETIME());
                                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                                
                                using var createInvoiceCmd = new SqlCommand(createInvoiceSql, conn);
                                createInvoiceCmd.Parameters.AddWithValue("@InvoiceNumber", model.StockInGroupKey);
                                createInvoiceCmd.Parameters.AddWithValue("@SupplierId", supplierId);
                                createInvoiceCmd.Parameters.AddWithValue("@InvoiceDate", stockInDate.Date);
                                createInvoiceCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                                createInvoiceCmd.Parameters.AddWithValue("@Description", $"Stock-in from {stockInCount} items");
                                createInvoiceCmd.Parameters.AddWithValue("@CreatedBy", createdByUserId);
                                createInvoiceCmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);
                                
                                var newInvoiceId = await createInvoiceCmd.ExecuteScalarAsync(ct);
                                if (newInvoiceId != null && newInvoiceId != DBNull.Value)
                                {
                                    invoiceId = Convert.ToInt32(newInvoiceId);
                                }
                            }
                        }
                    }
                }
            }

            var sql = @"
                INSERT INTO dbo.tbl_supplier_payments 
                    (invoice_id, po_id, stock_in_group_key, amount_paid, payment_method, payment_date, reference_number, notes, receipt_image_base64, receipt_image_content_type, created_by, seller_user_id, created_at)
                VALUES 
                    (@InvoiceId, @POId, @StockInGroupKey, @AmountPaid, @PaymentMethod, @PaymentDate, @ReferenceNumber, @Notes, @ReceiptImage, @ReceiptContentType, @CreatedBy, @SellerUserId, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@InvoiceId", (object?)invoiceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@POId", (object?)model.POId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StockInGroupKey", (object?)model.StockInGroupKey ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AmountPaid", model.AmountPaid);
            cmd.Parameters.AddWithValue("@PaymentMethod", model.PaymentMethod);
            cmd.Parameters.AddWithValue("@PaymentDate", model.PaymentDate.Date);
            cmd.Parameters.AddWithValue("@ReferenceNumber", (object?)model.ReferenceNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)model.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReceiptImage", (object?)model.ReceiptImage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReceiptContentType", (object?)model.ReceiptContentType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", createdByUserId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : null;
        }

        public async Task<bool> DeletePaymentAsync(int paymentId, int sellerUserId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE dbo.tbl_supplier_payments
                SET archived_at = SYSUTCDATETIME()
                WHERE id = @PaymentId
                AND seller_user_id = @SellerUserId
                AND archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaymentId", paymentId);
            cmd.Parameters.AddWithValue("@SellerUserId", sellerUserId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }
    }
}
