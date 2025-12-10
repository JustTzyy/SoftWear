# Cashier Security Features Documentation

This document highlights security features and their functions that protect cashier operations and transaction data in the SoftWear system, with explanations of how they work from the cashier's perspective.

---

## 1. **Cashier Data Isolation Security Module**

### Most Important Function: Cashier User ID Filtering in Sales Queries

**Location:** Throughout all sales-related service files (SalesService.cs, ReturnsService.cs, DailySalesVerificationService.cs)

**How it works:**
- **Purpose:** Ensures cashiers can only access their own sales and return transactions, maintaining complete transaction privacy.
- **Security Process:**
  1. **Sales Data Isolation:**
     - All sales queries filter by: `WHERE s.user_id = @CashierUserId`
     - Cashiers only see sales they processed
     - Prevents cashiers from viewing other cashiers' transactions
  2. **Returns Data Isolation:**
     - Return queries: `WHERE r.user_id = @CashierUserId`
     - Cashiers only see returns they processed
     - Complete return transaction privacy
  3. **Dashboard Statistics:**
     - Statistics filtered by cashier's user ID
     - Only shows cashier's own sales and returns
     - Performance metrics scoped to individual cashier
  4. **Daily Sales Verification:**
     - Daily sales submissions scoped to cashier
     - Only cashier can submit their own daily sales
     - Complete transaction accountability

**Why it's important:** Transaction data is highly sensitive and personal. This ensures cashiers can only access their own sales and returns. Even if a cashier somehow gains access to another cashier's account, they cannot see other cashiers' transactions. This protects transaction privacy and maintains proper accountability for each cashier's performance.

---

## 2. **Sales Creation Security Module**

### Most Important Function: `CreateSaleAsync`

**Location:** `IT13_Final/Services/Data/SalesService.cs` (Line 173-272)

**How it works:**
- **Purpose:** Securely processes sales transactions with transaction safety, automatic inventory updates, and complete audit trails.
- **Security Process:**
  1. **Transaction Wrapping:**
     - All sales operations wrapped in database transaction
     - Either all operations succeed or all roll back
     - Ensures data consistency
  2. **Sale Number Generation:**
     - Generates unique sale number within transaction
     - Format: `SALE-YYYYMMDD-####`
     - Prevents duplicate sale numbers
  3. **Sale Record Creation:**
     - Inserts sale record with cashier's user ID: `user_id = @CashierUserId`
     - Links sale to cashier for accountability
     - Records payment method and amount
  4. **Sale Items Creation:**
     - Inserts sale items for each product
     - Links to sale record
     - Records quantity, price, and subtotal
  5. **Automatic Inventory Reduction:**
     - Creates stock_out records for each item
     - Uses variant owner's user_id (seller)
     - Automatically reduces inventory
     - Prevents overselling
  6. **Payment Recording:**
     - Inserts payment record
     - Records amount paid, change given
     - Records reference number for GCash transactions
     - Complete payment audit trail

**Why it's important:** This is the core security function for point-of-sale operations. The transaction ensures atomicity - either the entire sale (sale record, items, inventory reduction, payment) succeeds or nothing is saved. The automatic inventory reduction ensures real-time stock accuracy. The cashier user ID linking provides complete accountability for all sales.

---

## 3. **Return Creation Security Module**

### Most Important Function: `CreateReturnAsync`

**Location:** `IT13_Final/Services/Data/ReturnsService.cs` (Line 350-416)

**How it works:**
- **Purpose:** Securely processes return requests with transaction safety and approval workflow.
- **Security Process:**
  1. **Transaction Wrapping:**
     - All return operations wrapped in database transaction
     - Either all operations succeed or all roll back
     - Ensures data consistency
  2. **Return Number Generation:**
     - Generates unique return number within transaction
     - Format: `RET-YYYYMMDD-####`
     - Prevents duplicate return numbers
  3. **Return Record Creation:**
     - Inserts return record with cashier's user ID: `user_id = @CashierUserId`
     - Links return to cashier for accountability
     - Links to original sale for audit trail
     - Sets status to 'Pending' (requires accounting approval)
  4. **Return Items Creation:**
     - Inserts return items for each returned product
     - Links to original sale item
     - Records condition (New/Used/Damaged)
     - Records quantity returned
  5. **Approval Workflow:**
     - Returns require accounting approval before refund
     - Prevents unauthorized refunds
     - Maintains financial control

**Why it's important:** This provides secure return processing with complete audit trails. The transaction ensures atomicity - either the entire return (return record and items) succeeds or nothing is saved. The pending status ensures returns go through proper approval before refunds are processed. The cashier user ID linking provides complete accountability.

---

## 4. **Payment Processing Security Module**

### Most Important Function: Payment Recording in `CreateSaleAsync`

**Location:** `IT13_Final/Services/Data/SalesService.cs` (Line 246-258)

**How it works:**
- **Purpose:** Securely records payment details with complete audit trails and payment method tracking.
- **Security Process:**
  1. **Payment Record Creation:**
     - Inserts payment record linked to sale
     - Records amount paid by customer
     - Records payment method (Cash or GCash)
     - Records change given (for cash payments)
     - Records reference number (for GCash transactions)
  2. **Payment Method Validation:**
     - Validates payment method is valid
     - Ensures proper payment recording
  3. **Change Calculation:**
     - Calculates change for cash payments
     - Ensures correct change given
     - Helps with cash reconciliation
  4. **GCash Reference Tracking:**
     - Records GCash transaction reference number
     - Enables verification of digital payments
     - Complete payment audit trail

**Why it's important:** Payment data is critical for financial accuracy and cash reconciliation. This ensures all payments are properly recorded with complete details. The payment method tracking enables proper financial reporting. The reference number for GCash enables verification of digital payments. This is essential for daily sales verification and cash drawer reconciliation.

---

## 5. **Inventory Viewing Security Module**

### Most Important Function: `GetInventoriesAsync` (Read-Only Access)

**Location:** `IT13_Final/Services/Data/InventoryService.cs` (Line 68-204)

**How it works:**
- **Purpose:** Provides cashiers with read-only inventory visibility scoped to their seller's products.
- **Security Process:**
  1. **Inventory Calculation:**
     - Uses CTEs to aggregate stock-in, stock-out, and adjustments
     - All CTEs filter by variant owner: `WHERE v.user_id = @SellerUserId`
     - Only includes transactions for seller's variants
  2. **Current Stock Calculation:**
     - Calculates: `StockIn - StockOut + Adjustments`
     - Only for seller's variants
     - Real-time inventory levels
  3. **Read-Only Access:**
     - Cashiers can only view inventory
     - Cannot modify inventory levels
     - Prevents unauthorized inventory changes
  4. **Data Filtering:**
     - Filters to show only items with stock > 0 (available items)
     - Joins with products, variants, categories
     - Complete product information for POS

**Why it's important:** Cashiers need to see available inventory when processing sales. This function provides real-time stock visibility while ensuring complete data isolation. The read-only access ensures cashiers cannot modify inventory levels, which is restricted to stock clerks. The variant owner filtering ensures cashiers cannot see other sellers' inventory levels.

---

## 6. **Daily Sales Submission Security Module**

### Most Important Function: Daily Sales Verification Submission

**Location:** `IT13_Final/Services/Data/DailySalesVerificationService.cs`

**How it works:**
- **Purpose:** Securely submits daily sales for accounting approval with cashier ownership verification.
- **Security Process:**
  1. **Cashier Ownership:**
     - Daily sales linked to cashier's user ID
     - Only cashier can submit their own daily sales
     - Prevents submitting other cashiers' sales
  2. **Sales Aggregation:**
     - Aggregates all sales for the cashier and date
     - Calculates total sales, cash, GCash amounts
     - Includes return totals
  3. **Verification Status:**
     - Sets status to 'Pending' for accounting approval
     - Prevents sales from being included in reports until approved
     - Maintains financial control
  4. **Audit Trail:**
     - Records who submitted (cashier user ID)
     - Records when submitted (timestamp)
     - Complete accountability

**Why it's important:** This provides secure daily sales submission with proper approval workflow. The cashier ownership ensures cashiers can only submit their own sales. The pending status ensures sales go through proper accounting approval before being included in financial reports. This is essential for financial control and cash reconciliation.

---

## 7. **Sales Reporting Security Module**

### Most Important Function: `GetSalesForReportsAsync`

**Location:** `IT13_Final/Services/Data/SalesService.cs` (Line 274-356)

**How it works:**
- **Purpose:** Provides cashiers with access to their own sales history with proper filtering and search capabilities.
- **Security Process:**
  1. **Sales Filtering:**
     - All sales queries: `WHERE s.user_id = @CashierUserId`
     - Only shows cashier's own sales
     - Prevents viewing other cashiers' sales
  2. **Search Capabilities:**
     - Search by sale number
     - Date range filtering
     - Payment method filtering
  3. **Approved Sales Filter:**
     - Optionally filters to only approved daily sales
     - Ensures financial accuracy
     - Only includes verified transactions
  4. **Pagination:**
     - Supports pagination for large result sets
     - Efficient data retrieval
  5. **Complete Sale Information:**
     - Includes sale details, items, payment information
     - Complete transaction history

**Why it's important:** Cashiers need access to their sales history for customer service and performance tracking. The cashier user ID filtering ensures cashiers can only see their own sales. The approved sales filter ensures financial accuracy when generating reports. This is essential for cashier accountability and customer service.

---

## 8. **Returns Reporting Security Module**

### Most Important Function: `GetReturnsForReportsAsync`

**Location:** `IT13_Final/Services/Data/ReturnsService.cs` (Line 418-485)

**How it works:**
- **Purpose:** Provides cashiers with access to their own return history with proper filtering and search capabilities.
- **Security Process:**
  1. **Returns Filtering:**
     - All return queries: `WHERE r.user_id = @CashierUserId`
     - Only shows cashier's own returns
     - Prevents viewing other cashiers' returns
  2. **Search Capabilities:**
     - Search by return number or sale number
     - Date range filtering
     - Status filtering (Pending/Approved/Rejected)
  3. **Return Status Tracking:**
     - Shows approval status for each return
     - Enables cashiers to track refund status
     - Helps with customer service
  4. **Pagination:**
     - Supports pagination for large result sets
     - Efficient data retrieval
  5. **Complete Return Information:**
     - Includes return details, items, original sale
     - Complete return transaction history

**Why it's important:** Cashiers need access to their return history for customer service and tracking. The cashier user ID filtering ensures cashiers can only see their own returns. The status tracking helps cashiers follow up on pending refunds. This is essential for customer service and return management.

---

## 9. **Dashboard Statistics Security Module**

### Most Important Function: `GetDashboardStatsAsync`

**Location:** `IT13_Final/Services/Data/SalesService.cs` (Line 502-580)

**How it works:**
- **Purpose:** Provides cashiers with real-time performance metrics scoped to their own transactions.
- **Security Process:**
  1. **Statistics Filtering:**
     - All statistics filtered by: `WHERE s.user_id = @CashierUserId`
     - Only shows cashier's own sales and returns
     - Prevents viewing other cashiers' performance
  2. **Sales Metrics:**
     - Total sales count and revenue
     - Today's sales count and revenue
     - Performance comparison
  3. **Returns Metrics:**
     - Total returns count
     - Today's returns count
     - Return tracking
  4. **Real-Time Updates:**
     - Statistics calculated in real-time
     - Always current performance data
     - Helps cashiers track their performance

**Why it's important:** Cashiers need visibility into their performance. The cashier user ID filtering ensures cashiers can only see their own statistics. This helps cashiers track their daily progress and compare it to their overall performance. This is essential for self-monitoring and motivation.

---

## 10. **Authentication Security Module** (Cashier Access)

### Most Important Function: `AuthenticateAsync`

**Location:** `IT13_Final/Services/Auth/AuthService.cs` (Line 23-92)

**How it works:**
- **Purpose:** Securely authenticates cashier accounts and establishes authenticated sessions.
- **Security Process:**
  1. **Password Hashing:**
     - Cashier passwords hashed with SHA-256
     - Never stored in plain text
     - UTF-16LE encoding matches SQL Server format
  2. **Account Status Checks:**
     - Verifies `is_active = 1` (only active cashiers can log in)
     - Verifies `archived_at IS NULL` (archived cashiers cannot log in)
  3. **Role Verification:**
     - Retrieves cashier role from database
     - Returns authenticated user with role information
  4. **Session Establishment:**
     - Creates authenticated session with cashier user ID and role
     - Session used for all subsequent operations

**Why it's important:** This is the security gateway for cashier access. It ensures only valid, active cashier accounts can access the system. The password hashing protects cashier accounts even if the database is compromised.

---

## 11. **Password Management Security Module** (Cashier Self-Service)

### Most Important Function: `UpdatePasswordAsync`

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 326-378)

**How it works:**
- **Purpose:** Allows cashiers to securely change their own passwords with proper verification.
- **Security Process:**
  1. **Current Password Verification:**
     - Retrieves stored password hash
     - Hashes provided current password
     - Compares hashes to verify current password
     - Returns false if current password incorrect
  2. **New Password Hashing:**
     - Hashes new password with SHA-256
     - Stores hash in database
  3. **Account Update:**
     - Clears `must_change_pw` flag
     - Updates timestamp for audit

**Why it's important:** Cashiers can securely manage their own passwords without admin intervention. The current password requirement prevents unauthorized password changes even if someone gains temporary access to a cashier's session.

---

## 12. **Audit Logging Security Module** (Cashier Activity Tracking)

### Most Important Function: `LogAsync` and `GetLogsAsync`

**Location:** `IT13_Final/Services/Audit/HistoryService.cs`

**How it works:**
- **Purpose:** Tracks all cashier actions for security monitoring and transaction accountability.
- **Security Process:**
  1. **Activity Logging:**
     - Logs all cashier actions (create sale, create return, etc.)
     - Records: cashier user ID, action type, module, description, timestamp
  2. **Cashier Log Access:**
     - Cashiers can view their own activity logs
     - Filtered by cashier user ID: `WHERE h.user_id = @UserId`
     - Cashiers cannot see other cashiers' logs
  3. **Transaction Audit Trail:**
     - Complete history of sales and return operations
     - Tracks who performed what transaction and when
     - Essential for transaction audits
  4. **Operation Logging:**
     - Every sale and return is logged
     - Records transaction details
     - Maintains transaction accountability

**Why it's important:** This provides complete accountability for cashier operations. Cashier actions are logged for security monitoring and transaction audits. The user-scoped filtering ensures privacy while maintaining accountability. This is essential for transaction compliance and security investigations.

---

## 13. **Transaction Security Module** (Sales Data Integrity)

### Most Important Function: Database Transactions for Sales Operations

**Location:** Throughout sales-related service files

**How it works:**
- **Purpose:** Ensures sales operations are atomic and maintain data consistency.
- **Security Process:**
  1. **Transaction Wrapping:**
     - Sales operations wrapped in transactions
     - Example: Sale creation with items, stock-out, payment
     - Either all operations succeed or all roll back
  2. **Sales Data Consistency:**
     - Prevents partial sales states
     - Ensures sales data always consistent
     - Example: Sale must include all items and payment
  3. **Error Handling:**
     - Transactions roll back on errors
     - Prevents corrupted sales data
     - Maintains data integrity

**Why it's important:** Transactions prevent sales data inconsistencies. For example, a sale without inventory reduction could allow overselling. A sale without payment record could create financial discrepancies. Atomic operations ensure sales data always remains consistent and accurate.

---

## 14. **Input Validation Security Module** (Transaction Data Protection)

### Most Important Function: Input Sanitization in Sales Operations

**Location:** Throughout all sales service files

**How it works:**
- **Purpose:** Validates and sanitizes cashier input to prevent security vulnerabilities and transaction errors.
- **Security Process:**
  1. **String Trimming:**
     - All string inputs trimmed: `reason.Trim()`
     - Removes leading/trailing whitespace
  2. **Null Handling:**
     - Optional fields: `(object?)value ?? DBNull.Value`
     - Prevents null reference exceptions
  3. **Type Validation:**
     - Validates data types before database operations
     - Converts types safely
  4. **Transaction Validation:**
     - Validates quantities are positive
     - Validates prices are positive
     - Validates payment amounts are reasonable
     - Validates sale items exist

**Why it's important:** Input validation prevents malformed transaction data from entering the system. This protects transaction data integrity and prevents errors that could affect sales accuracy and financial records.

---

## 15. **Account Status Security Module** (Cashier Account Management)

### Most Important Function: Account Status Checks

**Location:** `IT13_Final/Services/Auth/AuthService.cs`, `UserService.cs`

**How it works:**
- **Purpose:** Controls cashier account access and enforces security policies.
- **Security Process:**
  1. **Account Activation (`is_active`):**
     - Authentication checks: `WHERE ISNULL(u.is_active,1)=1`
     - Only active cashier accounts can log in
     - Seller/admin can deactivate cashier accounts
  2. **Account Archival (`archived_at`):**
     - Authentication checks: `WHERE u.archived_at IS NULL`
     - Archived cashiers cannot log in
     - Soft delete preserves data for audit
  3. **Forced Password Change (`must_change_pw`):**
     - New cashiers must change password on first login
     - `MustChangePasswordAsync` checks this flag
     - Application redirects to password change if needed

**Why it's important:** These controls enable account management and security policies. Cashier accounts can be temporarily suspended (deactivated) or permanently disabled (archived) while maintaining data for audit purposes. Forced password changes ensure new cashiers set secure passwords.

---

## 16. **Email Uniqueness Security Module**

### Most Important Function: Email Validation in Account Operations

**Location:** `IT13_Final/Services/Data/UserService.cs`

**How it works:**
- **Purpose:** Ensures cashier email addresses are unique, preventing account conflicts.
- **Security Process:**
  1. **Email Uniqueness Check:**
     - Before updating cashier email: `SELECT COUNT(*) FROM tbl_users WHERE email = @email AND id != @userId AND archived_at IS NULL`
     - Checks email doesn't exist for other users
     - Throws exception if duplicate found
  2. **Email Trimming:**
     - Trims whitespace: `email.Trim()`
     - Prevents issues with accidental spaces
  3. **Case-Insensitive:**
     - Email comparison is case-insensitive
     - Prevents duplicate accounts

**Why it's important:** Email is the username for authentication. Uniqueness ensures each cashier has a unique identity and prevents account conflicts. This is essential for authentication and prevents security issues.

---

## 17. **SQL Injection Prevention Module**

### Most Important Function: Parameterized Queries

**Location:** Throughout all sales service files

**How it works:**
- **Purpose:** Prevents SQL injection attacks in sales operations.
- **Security Process:**
  1. **Parameterized Queries:**
     - All SQL queries use `@parameterName` placeholders
     - Parameters added via `cmd.Parameters.AddWithValue("@param", value)`
     - SQL Server treats parameters as data, not executable code
  2. **Transaction Data Protection:**
     - All transaction data goes through parameters
     - Prevents injection in sales queries
     - Protects sales database
  3. **No String Concatenation:**
     - Never builds SQL by concatenating user input
     - All user input goes through parameters

**Why it's important:** SQL injection is a critical security vulnerability, especially for transaction systems. Parameterized queries completely prevent SQL injection by separating code (SQL) from data (parameters). This protects the sales database from malicious input.

---

## Summary

The SoftWear system implements comprehensive security features that protect cashier operations and transaction data:

### Transaction Data Protection & Isolation
- **Complete Data Isolation:** Cashiers can only access their own sales and returns
- **Sales Creation Security:** Atomic operations with automatic inventory updates
- **Return Creation Security:** Secure return processing with approval workflow
- **Payment Processing Security:** Complete payment audit trails
- **Inventory Viewing Privacy:** Read-only access to seller's inventory

### Operation Security
- **Transaction Security:** Atomic operations ensure transaction data consistency
- **User Tracking:** All operations linked to cashier user ID
- **Audit Trails:** Complete accountability for all transactions
- **Approval Workflow:** Returns require accounting approval

### Access Control
- **Role-Based Access:** Cashiers have appropriate permissions for POS operations
- **Account Status Control:** Active/archived account management
- **Secure Authentication:** SHA-256 password hashing
- **Password Management:** Cashiers can securely change passwords

### Audit & Monitoring
- **Activity Logging:** Complete audit trail of cashier actions
- **Transaction Audit Trail:** All sales and returns logged
- **Cashier Log Access:** Cashiers can view their own activity

**Key Cashier Security Principles:**
1. **Complete Transaction Data Isolation** - Cashiers cannot access other cashiers' transactions
2. **Transaction Security** - Atomic operations maintain transaction data integrity
3. **Multi-Layer Security** - UI, service, and database-level protection
4. **Complete Audit Trails** - All transactions logged
5. **Approval Workflow** - Returns require accounting approval
6. **Secure Authentication** - Passwords never stored in plain text

These security features work together to create a secure point-of-sale system where cashiers can perform their duties with complete transaction privacy, proper access controls, and full accountability for all sales and return operations.


