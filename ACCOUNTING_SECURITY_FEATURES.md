# Accounting Security Features Documentation

This document highlights security features and their functions that protect accounting operations and financial data in the SoftWear system, with explanations of how they work from the accounting personnel's perspective.

---

## 1. **Accounting Data Isolation Security Module**

### Most Important Function: Seller User ID Filtering in Financial Queries

**Location:** Throughout all accounting-related service files (DailySalesVerificationService.cs, ExpenseService.cs, IncomeBreakdownService.cs, etc.)

**How it works:**
- **Purpose:** Ensures accounting personnel can only access financial data from their assigned seller's business, maintaining complete data privacy.
- **Security Process:**
  1. **Daily Sales Data Isolation:**
     - Sales queries filter by cashier's seller: `WHERE u.user_id = @SellerUserId`
     - Accounting only sees sales from their seller's cashiers
     - Prevents access to other sellers' sales data
  2. **Expense Data Isolation:**
     - Expense queries: `WHERE e.seller_user_id = @SellerUserId AND e.archived_at IS NULL`
     - Accounting only sees expenses for their seller's business
     - Complete financial data privacy
  3. **Income Breakdown Isolation:**
     - Income queries: `WHERE u.user_id = @SellerUserId`
     - Only shows income from seller's cashiers
     - Financial reports scoped to seller's business
  4. **Purchase Order Isolation:**
     - PO queries join with suppliers: `WHERE s.user_id = @SellerUserId`
     - Only shows POs for seller's suppliers
     - Procurement data completely isolated

**Why it's important:** Financial data is highly sensitive and confidential. This ensures accounting personnel can only access financial information from their assigned seller's business. Even if an accounting user somehow gains access to another seller's account, they cannot see other sellers' financial data. This protects business confidentiality and prevents financial data leakage.

---

## 2. **Cashier Ownership Verification Security Module**

### Most Important Function: Cashier Verification Before Sales Operations

**Location:** `IT13_Final/Services/Data/DailySalesVerificationService.cs` (Line 368-389, 642-663, 703-724)

**How it works:**
- **Purpose:** Verifies that cashiers belong to the seller's business before allowing accounting to view or approve their sales.
- **Security Process:**
  1. **Verification Pattern:**
     ```sql
     SELECT COUNT(*) 
     FROM dbo.tbl_users 
     WHERE id = @CashierUserId 
     AND user_id = @SellerUserId 
     AND archived_at IS NULL
     ```
  2. **Before Viewing Sales:**
     - `GetDailySalesDetailsForAccountingAsync` verifies cashier belongs to seller
     - Returns null if verification fails
     - Prevents viewing other sellers' cashier sales
  3. **Before Approving Sales:**
     - `ApproveDailySalesAsync` verifies cashier ownership
     - Returns false if verification fails
     - Prevents approving other sellers' sales
  4. **Before Rejecting Sales:**
     - `RejectDailySalesAsync` verifies cashier ownership
     - Prevents rejecting other sellers' sales

**Why it's important:** This is a critical security control for financial approvals. It ensures accounting can only approve sales from cashiers that belong to their seller's business. This prevents unauthorized financial approvals and maintains proper organizational boundaries. Without this verification, accounting could accidentally approve sales from other sellers, causing financial discrepancies.

---

## 3. **Daily Sales Approval Security Module**

### Most Important Function: `ApproveDailySalesAsync`

**Location:** `IT13_Final/Services/Data/DailySalesVerificationService.cs` (Line 642-701)

**How it works:**
- **Purpose:** Securely approves cashier daily sales with ownership verification and audit trail.
- **Security Process:**
  1. **Ownership Verification:**
     - Verifies cashier belongs to seller before approval
     - Returns false if verification fails
  2. **Approval Process:**
     - Uses UPSERT (INSERT or UPDATE) for verification record
     - Sets status to 'Approved'
     - Records `verified_by` with accounting user ID
     - Links to seller's business
  3. **Financial Impact:**
     - Approved sales become part of official financial records
     - Included in income breakdowns and reports
     - Finalizes financial records for that day
  4. **Audit Trail:**
     - Records who approved (accounting user ID)
     - Records when approved (timestamp)
     - Complete accountability

**Why it's important:** This is the primary financial control mechanism. It ensures only verified, legitimate sales are included in financial records. The ownership verification prevents accounting from approving sales from other sellers. The audit trail provides complete accountability for financial approvals, essential for financial audits and compliance.

---

## 4. **Daily Sales Rejection Security Module**

### Most Important Function: `RejectDailySalesAsync`

**Location:** `IT13_Final/Services/Data/DailySalesVerificationService.cs` (Line 703-737)

**How it works:**
- **Purpose:** Securely rejects cashier daily sales with ownership verification and audit trail.
- **Security Process:**
  1. **Ownership Verification:**
     - Verifies cashier belongs to seller before rejection
     - Returns false if verification fails
  2. **Rejection Process:**
     - Uses UPSERT for verification record
     - Sets status to 'Rejected'
     - Records `verified_by` with accounting user ID
     - Records rejection reason (optional)
  3. **Financial Impact:**
     - Rejected sales are NOT included in financial records
     - Cashier must correct issues and resubmit
     - Maintains financial accuracy
  4. **Audit Trail:**
     - Records who rejected (accounting user ID)
     - Records when rejected (timestamp)
     - Complete accountability

**Why it's important:** This provides accounting with the ability to reject suspicious or incorrect sales submissions. The ownership verification ensures accounting can only reject sales from their seller's cashiers. The audit trail maintains accountability for rejections, which is important for financial control and dispute resolution.

---

## 5. **Purchase Order Approval Security Module**

### Most Important Function: `UpdatePurchaseOrderStatusAsync` (Approval)

**Location:** `IT13_Final/Services/Data/PurchaseOrderService.cs` (Line 618-757)

**How it works:**
- **Purpose:** Securely approves purchase orders with ownership verification and automatic inventory updates.
- **Security Process:**
  1. **Ownership Verification:**
     - Verifies PO belongs to seller: `WHERE po.id = @POId AND s.user_id = @UserId`
     - Joins PO with suppliers to verify seller ownership
     - Returns false if PO doesn't belong to seller
  2. **Transaction Wrapping:**
     - All operations wrapped in database transaction
     - Either all succeed or all roll back
     - Ensures data consistency
  3. **Approval Process:**
     - Updates PO status to 'Approved'
     - Records who approved (accounting user ID)
     - Updates timestamp
  4. **Automatic Inventory Update:**
     - Creates stock_in records for each PO item
     - Uses variant owner's user_id (seller)
     - Records cost price from PO
     - Links to supplier
  5. **Financial Impact:**
     - Approved POs commit procurement spending
     - Inventory automatically updated
     - Financial records updated

**Why it's important:** This controls procurement spending and inventory additions. The ownership verification ensures accounting can only approve POs from their seller's suppliers. The transaction ensures atomicity - either the entire approval succeeds (PO status, inventory updates) or nothing is saved. This prevents partial approvals that could create financial discrepancies.

---

## 6. **Return & Refund Approval Security Module**

### Most Important Function: `UpdateReturnStatusAsync`

**Location:** `IT13_Final/Services/Data/ReturnsService.cs` (Line 600-737)

**How it works:**
- **Purpose:** Securely approves or rejects return/refund requests with ownership verification and inventory restoration.
- **Security Process:**
  1. **Ownership Verification:**
     - Verifies return belongs to seller's business
     - Checks return links to seller's cashier
     - Returns false if verification fails
  2. **Approval Process:**
     - Updates return status to 'Approved'
     - Records who approved (accounting user ID)
     - Updates timestamp
  3. **Inventory Restoration:**
     - Creates stock_in records for returned items
     - Uses variant owner's user_id (seller)
     - Records condition (New/Used/Damaged)
     - Restores inventory to system
  4. **Rejection Process:**
     - Updates return status to 'Rejected'
     - Records rejection reason
     - No inventory restoration
  5. **Financial Impact:**
     - Approved returns trigger refunds
     - Inventory restored to system
     - Financial records updated

**Why it's important:** This controls refund disbursements and inventory restoration. The ownership verification ensures accounting can only approve returns from their seller's cashiers. The automatic inventory restoration upon approval ensures inventory accuracy. This is a critical financial control point that prevents unauthorized refunds.

---

## 7. **Expense Data Access Security Module**

### Most Important Function: Expense Filtering by Seller

**Location:** `IT13_Final/Services/Data/ExpenseService.cs` (Line 63-88)

**How it works:**
- **Purpose:** Ensures accounting can only view and manage expenses for their seller's business.
- **Security Process:**
  1. **Expense Queries:**
     - All expense operations: `WHERE e.seller_user_id = @SellerUserId AND e.archived_at IS NULL`
     - Accounting only sees expenses for their seller
     - Complete expense data isolation
  2. **Expense Creation:**
     - Expense creation links to seller: `seller_user_id = @SellerUserId`
     - Accounting can create expenses for their seller
     - Cannot create expenses for other sellers
  3. **Expense Updates:**
     - Expense updates verify ownership before allowing changes
     - Prevents modification of other sellers' expenses
  4. **Expense Reports:**
     - Reports filtered by seller's user ID
     - Only shows seller's business expenses
     - Financial data completely isolated

**Why it's important:** Expense data reveals business spending patterns and financial information. This ensures accounting can only access expenses from their assigned seller's business. This protects financial confidentiality and prevents accounting from viewing other sellers' expense data.

---

## 8. **Income Data Access Security Module**

### Most Important Function: Income Filtering by Seller

**Location:** `IT13_Final/Services/Data/IncomeBreakdownService.cs` (Line 85-100)

**How it works:**
- **Purpose:** Ensures accounting can only view income data from their seller's business.
- **Security Process:**
  1. **Income Queries:**
     - Income queries filter by: `WHERE u.user_id = @SellerUserId`
     - Joins sales with users to get cashier's seller
     - Only shows income from seller's cashiers
  2. **Income Breakdown:**
     - Groups income by seller's cashiers only
     - Shows revenue breakdown for seller's business
     - Complete income data isolation
  3. **Approved Sales Filter:**
     - Optionally filters to only approved daily sales
     - Ensures financial accuracy
     - Only includes verified transactions

**Why it's important:** Income data is highly sensitive business information. This ensures accounting can only view income from their assigned seller's business. This protects revenue information and prevents accounting from accessing other sellers' income data.

---

## 9. **Supplier Payment Security Module**

### Most Important Function: `CreatePaymentAsync` with Ownership Verification

**Location:** `IT13_Final/Services/Data/SupplierInvoiceService.cs` (Line 720-746)

**How it works:**
- **Purpose:** Securely processes supplier payments with ownership verification.
- **Security Process:**
  1. **PO Ownership Verification:**
     - If paying a PO: `SELECT COUNT(*) FROM tbl_purchase_orders po INNER JOIN tbl_suppliers s ON po.supplier_id = s.id WHERE po.id = @POId AND s.user_id = @SellerUserId`
     - Verifies PO belongs to seller's supplier
     - Returns null if verification fails
  2. **Payment Processing:**
     - Records payment amount and date
     - Links to PO or invoice
     - Updates payment status
  3. **Financial Impact:**
     - Records payment in financial system
     - Updates supplier invoice status
     - Tracks accounts payable

**Why it's important:** This controls supplier payments and prevents unauthorized payments. The ownership verification ensures accounting can only process payments for their seller's suppliers. This prevents payments to other sellers' suppliers and maintains proper financial controls.

---

## 10. **Authentication Security Module** (Accounting Access)

### Most Important Function: `AuthenticateAsync`

**Location:** `IT13_Final/Services/Auth/AuthService.cs` (Line 23-92)

**How it works:**
- **Purpose:** Securely authenticates accounting accounts and establishes authenticated sessions.
- **Security Process:**
  1. **Password Hashing:**
     - Accounting passwords hashed with SHA-256
     - Never stored in plain text
     - UTF-16LE encoding matches SQL Server format
  2. **Account Status Checks:**
     - Verifies `is_active = 1` (only active accounting can log in)
     - Verifies `archived_at IS NULL` (archived accounting cannot log in)
  3. **Role Verification:**
     - Retrieves accounting role from database
     - Returns authenticated user with role information
  4. **Session Establishment:**
     - Creates authenticated session with accounting user ID and role
     - Session used for all subsequent operations

**Why it's important:** This is the security gateway for accounting access. It ensures only valid, active accounting accounts can access the system. The password hashing protects accounting accounts even if the database is compromised.

---

## 11. **Password Management Security Module** (Accounting Self-Service)

### Most Important Function: `UpdatePasswordAsync`

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 326-378)

**How it works:**
- **Purpose:** Allows accounting personnel to securely change their own passwords with proper verification.
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

**Why it's important:** Accounting personnel can securely manage their own passwords without admin intervention. The current password requirement prevents unauthorized password changes even if someone gains temporary access to an accounting user's session.

---

## 12. **Audit Logging Security Module** (Accounting Activity Tracking)

### Most Important Function: `LogAsync` and `GetLogsAsync`

**Location:** `IT13_Final/Services/Audit/HistoryService.cs`

**How it works:**
- **Purpose:** Tracks all accounting actions for security monitoring and financial accountability.
- **Security Process:**
  1. **Activity Logging:**
     - Logs all accounting actions (approve sales, approve PO, etc.)
     - Records: accounting user ID, action type, module, description, timestamp
  2. **Accounting Log Access:**
     - Accounting can view their own activity logs
     - Filtered by accounting user ID: `WHERE h.user_id = @UserId`
     - Accounting cannot see other accounting users' logs
  3. **Financial Audit Trail:**
     - Complete history of financial approvals
     - Tracks who approved what and when
     - Essential for financial audits
  4. **Approval Logging:**
     - Every approval/rejection is logged
     - Records approval details
     - Maintains financial accountability

**Why it's important:** This provides complete accountability for financial operations. Accounting actions are logged for security monitoring and financial audits. The user-scoped filtering ensures privacy while maintaining accountability. This is essential for financial compliance and security investigations.

---

## 13. **Transaction Security Module** (Financial Data Integrity)

### Most Important Function: Database Transactions for Financial Operations

**Location:** Throughout accounting-related service files

**How it works:**
- **Purpose:** Ensures financial operations are atomic and maintain data consistency.
- **Security Process:**
  1. **Transaction Wrapping:**
     - Financial operations wrapped in transactions
     - Example: PO approval with stock-in creation
     - Either all operations succeed or all roll back
  2. **Financial Data Consistency:**
     - Prevents partial financial states
     - Ensures financial data always consistent
     - Example: Approval must include all related records
  3. **Error Handling:**
     - Transactions roll back on errors
     - Prevents corrupted financial data
     - Maintains data integrity

**Why it's important:** Transactions prevent financial data inconsistencies. For example, a PO approval without stock-in creation would create procurement discrepancies. Atomic operations ensure financial data always remains consistent and accurate.

---

## 14. **Input Validation Security Module** (Financial Data Protection)

### Most Important Function: Input Sanitization in Financial Operations

**Location:** Throughout all accounting service files

**How it works:**
- **Purpose:** Validates and sanitizes accounting input to prevent security vulnerabilities and financial errors.
- **Security Process:**
  1. **String Trimming:**
     - All string inputs trimmed: `description.Trim()`
     - Removes leading/trailing whitespace
  2. **Null Handling:**
     - Optional fields: `(object?)value ?? DBNull.Value`
     - Prevents null reference exceptions
  3. **Type Validation:**
     - Validates data types before database operations
     - Converts types safely
  4. **Financial Validation:**
     - Validates amounts are positive
     - Validates dates are reasonable
     - Validates status values match expected values

**Why it's important:** Input validation prevents malformed financial data from entering the system. This protects financial data integrity and prevents errors that could affect financial reports and business decisions.

---

## 15. **Account Status Security Module** (Accounting Account Management)

### Most Important Function: Account Status Checks

**Location:** `IT13_Final/Services/Auth/AuthService.cs`, `UserService.cs`

**How it works:**
- **Purpose:** Controls accounting account access and enforces security policies.
- **Security Process:**
  1. **Account Activation (`is_active`):**
     - Authentication checks: `WHERE ISNULL(u.is_active,1)=1`
     - Only active accounting accounts can log in
     - Seller/admin can deactivate accounting accounts
  2. **Account Archival (`archived_at`):**
     - Authentication checks: `WHERE u.archived_at IS NULL`
     - Archived accounting cannot log in
     - Soft delete preserves data for audit
  3. **Forced Password Change (`must_change_pw`):**
     - New accounting must change password on first login
     - `MustChangePasswordAsync` checks this flag
     - Application redirects to password change if needed

**Why it's important:** These controls enable account management and security policies. Accounting accounts can be temporarily suspended (deactivated) or permanently disabled (archived) while maintaining data for audit purposes. Forced password changes ensure new accounting users set secure passwords.

---

## 16. **Email Uniqueness Security Module**

### Most Important Function: Email Validation in Account Operations

**Location:** `IT13_Final/Services/Data/UserService.cs`

**How it works:**
- **Purpose:** Ensures accounting email addresses are unique, preventing account conflicts.
- **Security Process:**
  1. **Email Uniqueness Check:**
     - Before updating accounting email: `SELECT COUNT(*) FROM tbl_users WHERE email = @email AND id != @userId AND archived_at IS NULL`
     - Checks email doesn't exist for other users
     - Throws exception if duplicate found
  2. **Email Trimming:**
     - Trims whitespace: `email.Trim()`
     - Prevents issues with accidental spaces
  3. **Case-Insensitive:**
     - Email comparison is case-insensitive
     - Prevents duplicate accounts

**Why it's important:** Email is the username for authentication. Uniqueness ensures each accounting user has a unique identity and prevents account conflicts. This is essential for authentication and prevents security issues.

---

## 17. **SQL Injection Prevention Module**

### Most Important Function: Parameterized Queries

**Location:** Throughout all accounting service files

**How it works:**
- **Purpose:** Prevents SQL injection attacks in financial operations.
- **Security Process:**
  1. **Parameterized Queries:**
     - All SQL queries use `@parameterName` placeholders
     - Parameters added via `cmd.Parameters.AddWithValue("@param", value)`
     - SQL Server treats parameters as data, not executable code
  2. **Financial Data Protection:**
     - All financial data goes through parameters
     - Prevents injection in financial queries
     - Protects financial database
  3. **No String Concatenation:**
     - Never builds SQL by concatenating user input
     - All user input goes through parameters

**Why it's important:** SQL injection is a critical security vulnerability, especially for financial systems. Parameterized queries completely prevent SQL injection by separating code (SQL) from data (parameters). This protects the financial database from malicious input.

---

## Summary

The SoftWear system implements comprehensive security features that protect accounting operations and financial data:

### Financial Data Protection & Isolation
- **Complete Data Isolation:** Accounting can only access their seller's financial data
- **Cashier Ownership Verification:** Accounting can only approve sales from their seller's cashiers
- **PO Ownership Verification:** Accounting can only approve POs from their seller's suppliers
- **Return Ownership Verification:** Accounting can only approve returns from their seller's cashiers
- **Expense Data Privacy:** Accounting only sees their seller's expenses
- **Income Data Privacy:** Accounting only sees income from their seller's business

### Approval Workflow Security
- **Ownership Verification:** All approvals verify data ownership before processing
- **Transaction Security:** Atomic operations ensure financial data consistency
- **Audit Trails:** Complete accountability for all financial approvals
- **Status Management:** Approval/rejection status tracked and logged

### Access Control
- **Role-Based Access:** Accounting has appropriate permissions for financial operations
- **Account Status Control:** Active/archived account management
- **Secure Authentication:** SHA-256 password hashing
- **Password Management:** Accounting can securely change passwords

### Audit & Monitoring
- **Activity Logging:** Complete audit trail of accounting actions
- **Financial Audit Trail:** All approvals/rejections logged
- **Accounting Log Access:** Accounting can view their own activity

**Key Accounting Security Principles:**
1. **Complete Financial Data Isolation** - Accounting cannot access other sellers' financial data
2. **Ownership Verification** - All approvals verify data ownership
3. **Multi-Layer Security** - UI, service, and database-level protection
4. **Complete Audit Trails** - All financial operations logged
5. **Transaction Security** - Atomic operations maintain financial data integrity
6. **Secure Authentication** - Passwords never stored in plain text

These security features work together to create a secure financial management system where accounting personnel can perform their duties with complete data privacy, proper access controls, and full accountability for all financial operations.


