# Module Important Functions Documentation

This document highlights the most important function in each module of the SoftWear system and provides a brief explanation of how it works.

---

## 1. **Admin Module** (UserService.cs)

### Most Important Function: `CreateAdminAsync`

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 700-774)

**How it works:**
- **Purpose:** Creates a new admin user account in the system with proper role assignment and security measures.
- **Process:**
  1. Retrieves the admin role ID from the `tbl_roles` table
  2. Validates email uniqueness by checking if the email already exists in the database
  3. Hashes the password using SHA-256 with UTF-16LE encoding (matching SQL Server's HASHBYTES format)
  4. Constructs the full name from first, middle (optional), and last name
  5. Converts sex string to integer (0=Male, 1=Female, or NULL)
  6. Inserts the new admin user with:
     - `is_active = 1` (active by default)
     - `must_change_pw = 1` (forces password change on first login for security)
     - All personal information (name, contact, birthday, age, sex)
  7. Returns the newly created user ID

**Why it's important:** This function is the foundation of user management in the admin module. It ensures proper role assignment, password security, and account initialization. All other admin operations depend on properly created admin accounts.

---

## 2. **Authentication Module** (AuthService.cs)

### Most Important Function: `AuthenticateAsync`

**Location:** `IT13_Final/Services/Auth/AuthService.cs` (Line 23-92)

**How it works:**
- **Purpose:** Validates user credentials and returns authenticated user information.
- **Process:**
  1. Hashes the provided password using SHA-256 with UTF-16LE encoding to match database format
  2. Queries the database for a user with the provided email that is active and not archived
  3. Retrieves the stored password hash, user ID, role name, and full name
  4. Compares the computed password hash with the stored hash (case-insensitive)
  5. Returns an `AuthUser` object containing user ID, email, full name, and role if authentication succeeds
  6. Returns `null` if user not found or password doesn't match
  7. Handles SQL connection exceptions with helpful error messages

**Why it's important:** This is the security gateway for the entire system. Every user must authenticate through this function to access any module. It ensures only valid, active users can log in and determines their role-based access permissions.

---

## 3. **Product Management Module** (ProductService.cs)

### Most Important Function: `CreateProductAsync`

**Location:** `IT13_Final/Services/Data/ProductService.cs` (Line 188-213)

**How it works:**
- **Purpose:** Creates a new product record with optional image upload.
- **Process:**
  1. Opens a database connection
  2. Inserts a new product record into `tbl_products` with:
     - User ID (associates product with seller/admin)
     - Product name and description
     - Category ID (links to product category)
     - Image data as VARBINARY (if provided)
     - Image content type (MIME type)
     - Status set to 'Active'
     - Timestamp set to current UTC time
  3. Uses `SCOPE_IDENTITY()` to retrieve the newly created product ID
  4. Returns the product ID if successful, or null if creation fails

**Why it's important:** Products are the core entities in the inventory system. This function establishes the product catalog and enables all subsequent operations (variants, inventory tracking, sales). It handles binary image data properly and maintains data integrity through foreign key relationships.

---

## 4. **Inventory Management Module** (InventoryService.cs)

### Most Important Function: `GetInventoriesAsync`

**Location:** `IT13_Final/Services/Data/InventoryService.cs` (Line 68-204)

**How it works:**
- **Purpose:** Calculates and retrieves real-time inventory levels by aggregating stock movements.
- **Process:**
  1. Uses complex CTEs (Common Table Expressions) to aggregate inventory data:
     - **StockInAggregated:** Sums all stock-in quantities grouped by variant, size, and color
     - **StockOutAggregated:** Sums all stock-out quantities grouped by variant, size, and color
     - **StockAdjustmentsAggregated:** Calculates net adjustments (increases minus decreases)
  2. Combines these CTEs using FULL OUTER JOINs to handle cases where items exist in one table but not others
  3. Calculates current stock: `StockIn - StockOut + Adjustments`
  4. Joins with product, variant, category, size, color, and user tables to get complete information
  5. Filters by user ID, search terms, and only shows items with stock > 0
  6. Supports pagination with OFFSET/FETCH
  7. Returns inventory models with product details, variant info, stock levels, prices, and images

**Why it's important:** This function provides the real-time inventory view that drives all inventory-related decisions. It dynamically calculates stock levels from transaction history rather than maintaining a separate stock table, ensuring accuracy and providing a complete audit trail. It's used by POS systems, stock management, and reporting modules.

---

## 5. **Sales Module** (SalesService.cs)

### Most Important Function: `CreateSaleAsync`

**Location:** `IT13_Final/Services/Data/SalesService.cs` (Line 173-272)

**How it works:**
- **Purpose:** Processes a complete sales transaction with inventory updates in a single atomic operation.
- **Process:**
  1. Begins a database transaction to ensure data consistency
  2. Generates a unique sale number (format: `SALE-YYYYMMDD-####`)
  3. Inserts the main sale record into `tbl_sales` with:
     - Sale number, total amount, payment method, status ('Completed')
     - Cashier user ID and timestamp
  4. For each item in the sale:
     - Inserts sale item record into `tbl_sales_items` with variant, size, color, quantity, price, and subtotal
     - Creates a corresponding `stock_out` record to reduce inventory automatically
     - Links stock-out to the sale via sale number in the reason field
  5. Inserts payment record into `tbl_payments` with:
     - Amount paid, payment method, change given, and reference number (for GCash)
  6. Commits the transaction if all operations succeed
  7. Rolls back all changes if any operation fails

**Why it's important:** This is the core POS function that processes every sale. It ensures atomicity - either the entire sale succeeds (sale record, items, inventory reduction, payment) or nothing is saved. This prevents data inconsistencies like sales without inventory updates or partial transactions. The automatic inventory reduction ensures real-time stock accuracy.

---

## 6. **Purchase Order Module** (PurchaseOrderService.cs)

### Most Important Function: `CreatePurchaseOrderAsync`

**Location:** `IT13_Final/Services/Data/PurchaseOrderService.cs` (Line 84)

**How it works:**
- **Purpose:** Creates a purchase order with multiple line items for supplier procurement.
- **Process:**
  1. Generates a unique PO number
  2. Calculates total amount from all line items (quantity × unit price)
  3. Inserts the main PO record with supplier ID, status ('Pending'), expected delivery date, and notes
  4. For each item in the PO:
     - Inserts PO item record with variant, size, color, quantity, unit price, and total price
  5. Sets initial status to 'Pending' (requires accounting approval)
  6. Returns the newly created PO ID

**Why it's important:** Purchase orders are the foundation of procurement and inventory replenishment. This function initiates the procurement workflow, creates the approval chain, and establishes the relationship between suppliers, products, and expected deliveries. It's the starting point for stock-in operations when POs are fulfilled.

---

## 7. **Returns & Refunds Module** (ReturnsService.cs)

### Most Important Function: `CreateReturnAsync`

**Location:** `IT13_Final/Services/Data/ReturnsService.cs` (Line 98)

**How it works:**
- **Purpose:** Processes product returns with inventory restoration and approval workflow.
- **Process:**
  1. Validates that the sale exists and belongs to the cashier
  2. Validates return quantities don't exceed original sale quantities
  3. Generates a unique return number (format: `RET-YYYYMMDD-####`)
  4. Inserts return record with sale ID, reason, status ('Pending'), and cashier ID
  5. For each returned item:
     - Inserts return item record with sale item ID, variant, size, color, quantity, and condition (New/Used/Damaged)
  6. Creates stock-in records to restore inventory (quantity returned added back to stock)
  7. Sets status to 'Pending' (requires accounting approval for refund)
  8. Returns return ID and number

**Why it's important:** This function handles the critical reverse transaction flow. It ensures returned items are properly tracked, inventory is restored, and refunds go through proper approval. The condition tracking (New/Used/Damaged) helps manage inventory quality, and the approval workflow prevents unauthorized refunds.

---

## 8. **Expense Management Module** (ExpenseService.cs)

### Most Important Function: `CreateExpenseAsync`

**Location:** `IT13_Final/Services/Data/ExpenseService.cs` (Line 48)

**How it works:**
- **Purpose:** Records business expenses with receipt tracking for financial management.
- **Process:**
  1. Inserts expense record into `tbl_expenses` with:
     - Expense type (categorized expense)
     - Amount and description
     - Expense date
     - Receipt image (base64 encoded) and content type (optional)
     - Seller user ID (associates expense with seller)
     - Created by user ID and timestamp
  2. Supports receipt image storage for audit purposes
  3. Returns the newly created expense ID

**Why it's important:** This function tracks all business expenditures, which is essential for profit & loss calculations, financial reporting, and tax compliance. The receipt image storage provides audit trails and supports expense verification. It's a key component of the accounting module's expense tracking.

---

## 9. **Supplier Management Module** (SupplierService.cs)

### Most Important Function: `CreateSupplierAsync`

**Location:** `IT13_Final/Services/Data/SupplierService.cs` (Line 47)

**How it works:**
- **Purpose:** Creates a new supplier record with complete contact and address information.
- **Process:**
  1. Inserts supplier record into `tbl_suppliers` with:
     - Company name, contact person, email, contact number
     - Status (Active/Inactive)
     - User ID (associates supplier with seller/admin)
     - Creation timestamp
  2. If address information is provided:
     - Inserts address record into `tbl_addresses` linked to the supplier
     - Stores street, city, province, and zip code
  3. Returns the newly created supplier ID

**Why it's important:** Suppliers are essential for procurement operations. This function establishes the supplier database that enables purchase orders, stock-in operations, and supplier invoice management. Proper supplier records with addresses are needed for delivery coordination and payment processing.

---

## 10. **Stock Operations Module** (StockInService.cs)

### Most Important Function: `CreateStockInAsync`

**Location:** `IT13_Final/Services/Data/StockInService.cs` (Line 54)

**How it works:**
- **Purpose:** Records products added to inventory, typically from purchase orders or supplier deliveries.
- **Process:**
  1. Inserts stock-in record into `tbl_stock_in` with:
     - Variant ID, size ID, color ID (identifies specific product variant)
     - Quantity added
     - Cost price (for inventory valuation)
     - Supplier ID (optional, links to supplier if from PO)
     - User ID (who performed the stock-in)
     - Timestamp
  2. The stock-in record becomes part of the inventory calculation in `GetInventoriesAsync`

**Why it's important:** This function is the primary mechanism for adding inventory to the system. It's used when receiving goods from suppliers, fulfilling purchase orders, or manually adding stock. The cost price tracking enables inventory valuation and profit margin calculations. Combined with stock-out and adjustments, it maintains accurate inventory levels.

---

## 11. **Daily Sales Verification Module** (DailySalesVerificationService.cs)

### Most Important Function: `ApproveDailySalesAsync`

**Location:** `IT13_Final/Services/Data/DailySalesVerificationService.cs` (Line 61)

**How it works:**
- **Purpose:** Approves a cashier's daily sales submission, finalizing the financial records for that day.
- **Process:**
  1. Aggregates all sales and returns for the specified cashier and date
  2. Calculates totals: sales amount, transaction count, cash vs GCash breakdown, returns, discounts
  3. Compares expected cash (cash sales - returns) with actual cash submitted
  4. Calculates cash discrepancy
  5. Creates or updates `tbl_daily_sales_verifications` record with:
     - Status set to 'Approved'
     - Verified by user ID (accounting user)
     - Verification timestamp
     - All calculated totals and discrepancies
  6. Once approved, these sales are included in financial reports and accounting reconciliation

**Why it's important:** This function provides the financial control mechanism for daily operations. It ensures cashier accountability, detects discrepancies, and finalizes daily financial records. Approved daily sales become the source of truth for accounting reports, income tracking, and cash flow management. It's a critical internal control function.

---

## Summary

Each module's most important function serves as the foundation or critical operation point for that module:

- **Admin Module:** User creation and management
- **Authentication:** Security gateway
- **Product Management:** Catalog establishment
- **Inventory:** Real-time stock calculation
- **Sales:** Transaction processing
- **Purchase Orders:** Procurement initiation
- **Returns:** Reverse transaction handling
- **Expenses:** Financial tracking
- **Suppliers:** Vendor management
- **Stock Operations:** Inventory additions
- **Daily Sales Verification:** Financial control

These functions work together to create a complete retail management system with proper data integrity, security, and business process controls.


