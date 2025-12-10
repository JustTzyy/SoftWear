# Cashier Module Important Functions Documentation

This document highlights the most important function in each module accessible to cashiers in the SoftWear system and provides a brief explanation of how it works.

---

## 1. **Point of Sale (POS) / Sales Module** (SalesService.cs)

### Most Important Function: `CreateSaleAsync`

**Location:** `IT13_Final/Services/Data/SalesService.cs` (Line 173-272)

**How it works:**
- **Purpose:** Processes a complete sales transaction with inventory updates in a single atomic operation, handling the entire POS workflow.
- **Process:**
  1. Begins a database transaction to ensure data consistency
  2. Generates a unique sale number within the transaction (format: `SALE-YYYYMMDD-####`)
  3. Inserts the main sale record into `tbl_sales` with:
     - Sale number, total amount, payment method, status ('Completed')
     - Cashier user ID and timestamp
  4. For each item in the sale:
     - Inserts sale item record into `tbl_sales_items` with variant, size, color, quantity, price, and subtotal
     - **Automatically creates a stock_out record** to reduce inventory:
       - Links to the variant owner's user_id (seller)
       - Records quantity removed
       - Sets reason as "Sale: [SaleNumber]" for audit trail
  5. Inserts payment record into `tbl_payments` with:
     - Amount paid, payment method, change given
     - Reference number (for GCash transactions)
  6. Commits the transaction if all operations succeed
  7. Rolls back all changes if any operation fails

**Why it's important:** This is the core function that cashiers use for every customer transaction. It ensures atomicity - either the entire sale succeeds (sale record, items, inventory reduction, payment) or nothing is saved. This prevents data inconsistencies like sales without inventory updates or partial transactions. The automatic inventory reduction ensures real-time stock accuracy, so cashiers always see current availability. This function is the foundation of the POS system and handles the complete sales workflow in one operation.

---

## 2. **Returns & Refunds Module** (ReturnsService.cs)

### Most Important Function: `CreateReturnAsync`

**Location:** `IT13_Final/Services/Data/ReturnsService.cs` (Line 350-416)

**How it works:**
- **Purpose:** Processes product returns with approval workflow, enabling customers to return items and request refunds.
- **Process:**
  1. Begins a database transaction to ensure data consistency
  2. Generates a unique return number within the transaction (format: `RET-YYYYMMDD-####`)
  3. Inserts return record into `tbl_returns` with:
     - Return number, sale ID (links to original sale), reason (optional)
     - Status set to 'Pending' (requires accounting approval for refund)
     - Cashier user ID and timestamp
  4. For each returned item:
     - Inserts return item record with:
       - Sale item ID (links to original sale item)
       - Variant, size, color, quantity
       - Condition (New/Used/Damaged) for inventory quality tracking
  5. Note: Inventory restoration happens when accounting approves the return (not immediately)
  6. Commits the transaction if all operations succeed
  7. Rolls back all changes if any operation fails

**Why it's important:** This function handles customer returns and initiates the refund process. It creates a complete audit trail linking returns to original sales, enabling proper refund processing. The pending status ensures that refunds go through proper approval before money is disbursed. The condition tracking (New/Used/Damaged) helps manage inventory quality when items are restored to stock. This is essential for customer service and maintaining accurate financial records.

---

## 3. **Dashboard & Statistics Module** (SalesService.cs)

### Most Important Function: `GetDashboardStatsAsync`

**Location:** `IT13_Final/Services/Data/SalesService.cs` (Line 502-580)

**How it works:**
- **Purpose:** Provides cashiers with real-time performance metrics and transaction summaries for their daily operations.
- **Process:**
  1. Queries sales data filtered by cashier's user ID
  2. Calculates total sales:
     - Counts all completed sales transactions
     - Sums total revenue from all sales
  3. Calculates today's sales:
     - Counts completed sales for today
     - Sums revenue for today
  4. Calculates total returns:
     - Counts all return transactions processed by the cashier
  5. Calculates today's returns:
     - Counts return transactions for today
  6. Returns a dashboard stats model with:
     - Total sales count and revenue
     - Today's sales count and revenue
     - Total returns count
     - Today's returns count

**Why it's important:** This function provides cashiers with immediate visibility into their performance. It helps them track daily sales goals, monitor transaction volumes, and understand their productivity. The today vs. total comparison enables cashiers to see their daily progress and compare it to their overall performance. This is essential for self-monitoring and helps cashiers stay motivated and aware of their sales performance.

---

## 4. **Sales Reporting Module** (SalesService.cs)

### Most Important Function: `GetSalesForReportsAsync`

**Location:** `IT13_Final/Services/Data/SalesService.cs` (Line 274-356)

**How it works:**
- **Purpose:** Retrieves detailed sales transaction history for cashiers to review their sales, generate reports, and analyze performance.
- **Process:**
  1. Queries `tbl_sales` joined with users and payments tables
  2. Filters by cashier's user ID (shows only their sales)
  3. Supports search by sale number
  4. Supports date range filtering (start date and end date)
  5. Optionally filters to only include sales from approved daily sales verifications (for financial accuracy)
  6. Returns sales records with:
     - Sale ID, sale number, amount, payment method
     - Timestamp, cashier name
     - Amount paid, change given, reference number (for GCash)
  7. Supports pagination for large result sets
  8. Orders results by timestamp (newest first)

**Why it's important:** This function provides cashiers with a complete history of their sales transactions. They can review past sales, verify transaction details, and generate reports for their records. The date filtering enables cashiers to analyze performance over specific periods. The optional "approved days only" filter ensures financial accuracy when generating reports. This is essential for cashiers to track their sales history and identify trends or issues.

---

## 5. **Returns Reporting Module** (ReturnsService.cs)

### Most Important Function: `GetReturnsForReportsAsync`

**Location:** `IT13_Final/Services/Data/ReturnsService.cs` (Line 418-485)

**How it works:**
- **Purpose:** Retrieves detailed return transaction history for cashiers to review returns they've processed and track return patterns.
- **Process:**
  1. Queries `tbl_returns` joined with sales and users tables
  2. Filters by cashier's user ID (shows only their returns)
  3. Supports search by return number or sale number
  4. Supports date range filtering (start date and end date)
  5. Returns return records with:
     - Return ID, return number, sale ID, sale number
     - Reason, status (Pending/Approved/Rejected)
     - Timestamp, cashier name
  6. Supports pagination for large result sets
  7. Orders results by timestamp (newest first)

**Why it's important:** This function provides cashiers with a complete history of returns they've processed. They can review return requests, track approval status, and analyze return patterns. Understanding return reasons helps cashiers identify product issues or customer concerns. The status tracking shows which returns have been approved or rejected by accounting, enabling cashiers to follow up on pending refunds. This is essential for customer service and helps cashiers understand return trends.

---

## 6. **Inventory Viewing Module** (InventoryService.cs)

### Most Important Function: `GetInventoriesAsync`

**Location:** `IT13_Final/Services/Data/InventoryService.cs` (Line 68-204)

**How it works:**
- **Purpose:** Provides real-time inventory visibility for cashiers to see available products, stock levels, and prices when processing sales.
- **Process:**
  1. Uses complex CTEs (Common Table Expressions) to aggregate inventory data:
     - **StockInAggregated:** Sums all stock-in quantities grouped by variant, size, and color
     - **StockOutAggregated:** Sums all stock-out quantities grouped by variant, size, and color
     - **StockAdjustmentsAggregated:** Calculates net adjustments (increases minus decreases)
  2. Combines these CTEs using FULL OUTER JOINs
  3. Calculates current stock: `StockIn - StockOut + Adjustments`
  4. Joins with product, variant, category, size, color tables
  5. Filters by seller's user ID (shows only their seller's inventory)
  6. Filters to show only items with stock > 0 (available items)
  7. Returns inventory information including:
     - Product and variant names, sizes, colors
     - Current stock levels
     - Prices (selling price and cost price)
     - Product images (for visual identification)
     - Category information
  8. Supports search and pagination

**Why it's important:** Cashiers need to see available inventory when processing sales. This function provides real-time stock visibility, ensuring cashiers only sell items that are in stock. The dynamic calculation ensures accuracy, and the inclusion of prices and images helps cashiers quickly identify and select products. This is essential for the POS workflow and prevents overselling or selling out-of-stock items.

---

## 7. **Daily Sales Summary Module** (DailySalesVerificationService.cs)

### Most Important Function: `GetDailySalesDetailsForAccountingAsync` (Cashier View)

**Location:** `IT13_Final/Services/Data/DailySalesVerificationService.cs` (Line 368-640)

**How it works:**
- **Purpose:** Provides cashiers with a detailed breakdown of their daily sales for review and submission to accounting.
- **Process:**
  1. Verifies cashier belongs to the seller's business (security check)
  2. Aggregates all sales for the specified cashier and date:
     - Counts total transactions
     - Sums total sales amount
     - Calculates cash amount (cash sales minus change given)
     - Calculates GCash amount (GCash sales)
  3. Aggregates returns for the date:
     - Counts return transactions
     - Sums total return amounts
  4. Groups sales by payment method for breakdown
  5. Retrieves individual sales and returns for detailed view
  6. Retrieves verification status (Pending/Approved/Rejected)
  7. Returns comprehensive daily sales summary with:
     - Total sales, transactions, cash, GCash
     - Total returns and return count
     - Expected cash (cash sales - returns)
     - Verification status
     - List of all sales and returns for the day

**Why it's important:** This function enables cashiers to review their daily sales before submitting to accounting. It provides a complete summary of the day's transactions, helping cashiers verify totals and prepare for daily sales verification. Cashiers can use this to reconcile their cash drawer and ensure all transactions are accounted for. This is essential for daily operations and helps cashiers maintain accuracy in their sales records.

---

## 8. **Product Search & Selection Module** (InventoryService.cs)

### Most Important Function: `GetInventoriesAsync` (with search)

**Location:** `IT13_Final/Services/Data/InventoryService.cs` (Line 68-204)

**How it works:**
- **Purpose:** Enables cashiers to search and filter inventory when processing sales, making it easy to find products quickly.
- **Process:**
  1. Uses the same inventory aggregation as the inventory viewing module
  2. Supports search by:
     - Variant name
     - Product name
     - Size name
     - Color name
  3. Filters results to show only available items (stock > 0)
  4. Returns matching inventory items with complete product information
  5. Supports pagination for large product catalogs

**Why it's important:** During busy sales periods, cashiers need to quickly find products. This search functionality enables fast product lookup, improving transaction speed and customer service. The filtering by availability ensures cashiers only see sellable items. This is essential for efficient POS operations and helps cashiers process sales quickly and accurately.

---

## 9. **Payment Processing Module** (SalesService.cs)

### Most Important Function: `CreateSaleAsync` (Payment Component)

**Location:** `IT13_Final/Services/Data/SalesService.cs` (Line 246-258)

**How it works:**
- **Purpose:** Records payment details as part of the sales transaction, handling cash and digital payments.
- **Process:**
  1. As part of `CreateSaleAsync`, inserts payment record into `tbl_payments` with:
     - Sale ID (links to sale)
     - Amount paid (customer payment)
     - Payment method (Cash or GCash)
     - Change given (for cash payments)
     - Reference number (for GCash transactions - transaction ID)
     - Timestamp
  2. The payment record is linked to the sale for complete transaction history
  3. Payment method determines how the sale is categorized in reports

**Why it's important:** This function records how customers paid, which is essential for cash reconciliation and financial reporting. The change calculation helps cashiers verify correct change was given. The reference number for GCash enables verification of digital payments. This is critical for daily sales verification and helps cashiers reconcile their cash drawer at the end of the day.

---

## 10. **Transaction History Module** (SalesService.cs & ReturnsService.cs)

### Most Important Function: `GetSalesForReportsAsync` and `GetReturnsForReportsAsync`

**Location:** `IT13_Final/Services/Data/SalesService.cs` and `ReturnsService.cs`

**How it works:**
- **Purpose:** Provides cashiers with access to their complete transaction history for both sales and returns.
- **Process:**
  1. **Sales History:** Retrieves all sales transactions with filtering and search
  2. **Returns History:** Retrieves all return transactions with filtering and search
  3. Both support:
     - Date range filtering
     - Search by transaction number
     - Pagination
     - Detailed transaction information
  4. Cashiers can view:
     - Transaction dates and times
     - Amounts and payment methods
     - Status (for returns: Pending/Approved/Rejected)
     - Complete transaction details

**Why it's important:** Cashiers need access to their transaction history to:
- Verify past transactions
- Handle customer inquiries about previous purchases
- Track their performance over time
- Resolve disputes or questions
- Generate personal reports

This is essential for customer service and helps cashiers provide accurate information about past transactions.

---

## Summary

Each cashier module's most important function serves as a critical transaction processing or reporting mechanism:

- **Point of Sale (Sales):** Processes customer transactions with automatic inventory updates
- **Returns & Refunds:** Handles product returns with approval workflow
- **Dashboard & Statistics:** Provides real-time performance metrics
- **Sales Reporting:** Complete sales transaction history
- **Returns Reporting:** Complete return transaction history
- **Inventory Viewing:** Real-time stock availability for POS
- **Daily Sales Summary:** Daily transaction breakdown for verification
- **Product Search:** Fast product lookup for sales processing
- **Payment Processing:** Records payment details and methods
- **Transaction History:** Complete access to past transactions

**Key Cashier-Specific Characteristics:**
- All functions are scoped to the cashier's user ID (cashiers only see their own transactions)
- Sales automatically reduce inventory in real-time
- Returns require accounting approval before refunds are processed
- Complete audit trails for all transactions
- Support for multiple payment methods (Cash and GCash)
- Real-time inventory visibility ensures accurate stock levels
- Daily sales summaries enable end-of-day reconciliation
- Transaction history supports customer service and dispute resolution

These functions work together to provide comprehensive point-of-sale operations, ensuring accurate transactions, proper inventory management, and complete financial records in the SoftWear system.


