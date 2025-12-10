# Stock Clerk Security Features Documentation

This document highlights security features and their functions that protect stock clerk operations and inventory data in the SoftWear system, with explanations of how they work from the stock clerk's perspective.

---

## 1. **Stock Clerk Data Isolation Security Module**

### Most Important Function: Seller User ID Filtering in Inventory Queries

**Location:** Throughout all inventory-related service files (InventoryService.cs, StockInService.cs, StockOutService.cs, StockAdjustmentService.cs)

**How it works:**
- **Purpose:** Ensures stock clerks can only access inventory data from their assigned seller's business, maintaining complete data privacy.
- **Security Process:**
  1. **Inventory Data Isolation:**
     - Inventory queries filter by variant owner: `WHERE v.user_id = @UserId`
     - Joins through variants to products to get seller ownership
     - Stock clerks only see inventory for their seller's products
  2. **Stock-In Data Isolation:**
     - Stock-in queries: `WHERE si.archives IS NULL AND v.user_id = @UserId`
     - Only shows stock-in records for seller's variants
     - Complete stock-in history isolation
  3. **Stock-Out Data Isolation:**
     - Stock-out queries: `WHERE so.archives IS NULL AND v.user_id = @UserId`
     - Only shows stock-out records for seller's variants
     - Complete stock-out history isolation
  4. **Stock Adjustment Isolation:**
     - Adjustment queries: `WHERE sa.archives IS NULL AND v.user_id = @UserId`
     - Only shows adjustments for seller's variants
     - Complete adjustment history isolation
  5. **Purchase Order Isolation:**
     - PO queries join with suppliers: `WHERE s.user_id = @SellerUserId`
     - Only shows POs for seller's suppliers
     - Procurement data completely isolated

**Why it's important:** Inventory data is sensitive business information. This ensures stock clerks can only access inventory from their assigned seller's business. Even if a stock clerk somehow gains access to another seller's account, they cannot see other sellers' inventory data. This protects business confidentiality and prevents inventory data leakage between competing businesses.

---

## 2. **Variant Ownership Verification Security Module**

### Most Important Function: Variant Ownership Checks Before Stock Operations

**Location:** `IT13_Final/Services/Data/StockAdjustmentService.cs` (Line 261-277), `InventoryService.cs` (Line 415-431)

**How it works:**
- **Purpose:** Verifies that variants belong to the seller before allowing stock operations.
- **Security Process:**
  1. **Verification Pattern:**
     ```sql
     SELECT COUNT(*) 
     FROM dbo.tbl_variants 
     WHERE id = @VariantId 
     AND user_id = @SellerUserId 
     AND archived_at IS NULL
     ```
  2. **Before Stock Adjustments:**
     - `CreateStockAdjustmentAsync` verifies variant belongs to seller
     - Returns null if verification fails
     - Prevents adjustments to other sellers' variants
  3. **Before Reorder Level Updates:**
     - `UpdateReorderLevelAsync` verifies variant ownership
     - Returns false if verification fails
     - Prevents updating reorder levels for other sellers' variants
  4. **Operation Blocking:**
     - If ownership verification fails, operation returns null/false
     - Prevents unauthorized inventory modifications

**Why it's important:** This is a critical security control for inventory operations. It ensures stock clerks can only perform operations on variants that belong to their seller's business. This prevents unauthorized inventory modifications that could affect other sellers' inventory levels or create data inconsistencies.

---

## 3. **Purchase Order Creation Security Module**

### Most Important Function: `CreatePurchaseOrderAsync` with Supplier Verification

**Location:** `IT13_Final/Services/Data/PurchaseOrderService.cs` (Line 527-616)

**How it works:**
- **Purpose:** Securely creates purchase orders with supplier ownership verification and transaction safety.
- **Security Process:**
  1. **Supplier Ownership Verification:**
     - Before creating PO: `SELECT COUNT(*) FROM tbl_suppliers WHERE id = @SupplierId AND user_id = @SellerUserId AND archived_at IS NULL`
     - Verifies supplier belongs to seller
     - Returns null if verification fails
  2. **Transaction Wrapping:**
     - All PO creation operations wrapped in database transaction
     - Either all operations succeed or all roll back
     - Ensures data consistency
  3. **PO Number Generation:**
     - Generates unique PO number within transaction
     - Format: `PO-YYYYMM-####`
     - Scoped to seller's business
  4. **PO Creation:**
     - Inserts PO record with status 'Pending'
     - Links to verified supplier
     - Records total amount
     - Links to stock clerk (created_by)
  5. **PO Items Creation:**
     - Inserts PO items for each product
     - Links to PO record
     - All within same transaction

**Why it's important:** This controls procurement operations and prevents unauthorized purchase orders. The supplier verification ensures stock clerks can only create POs with their seller's suppliers. The transaction ensures atomicity - either the entire PO (header and items) is created or nothing is saved. This prevents partial POs that could create procurement discrepancies.

---

## 4. **Stock-In Operation Security Module**

### Most Important Function: `CreateStockInAsync`

**Location:** `IT13_Final/Services/Data/StockInService.cs` (Line 243-271)

**How it works:**
- **Purpose:** Securely records stock-in transactions with proper user tracking and data validation.
- **Security Process:**
  1. **User ID Tracking:**
     - Records `user_id` field with stock clerk's ID
     - Links stock-in to stock clerk who created it
     - Enables audit trail
  2. **Variant Linking:**
     - Links to variant, size, and color
     - Variant must belong to seller (enforced by inventory queries)
     - Prevents stock-in for other sellers' variants
  3. **Data Validation:**
     - Validates quantity is positive
     - Validates cost price is positive
     - Validates variant exists
  4. **Inventory Impact:**
     - Stock-in automatically increases inventory (via triggers)
     - Uses variant owner's user_id for inventory calculation
     - Ensures inventory only affects seller's products

**Why it's important:** This provides secure stock-in recording with complete audit trails. The user ID tracking ensures accountability for all stock-in operations. The variant linking ensures stock-in only affects the seller's inventory. This is essential for inventory accuracy and prevents unauthorized stock additions.

---

## 5. **Stock-Out Operation Security Module**

### Most Important Function: `CreateStockOutAsync`

**Location:** `IT13_Final/Services/Data/StockOutService.cs` (Line 220-247)

**How it works:**
- **Purpose:** Securely records stock-out transactions (non-sales) with proper user tracking and reason documentation.
- **Security Process:**
  1. **User ID Tracking:**
     - Records `user_id` field with stock clerk's ID
     - Links stock-out to stock clerk who created it
     - Enables audit trail
  2. **Variant Linking:**
     - Links to variant, size, and color
     - Variant must belong to seller (enforced by inventory queries)
     - Prevents stock-out for other sellers' variants
  3. **Reason Documentation:**
     - Records reason for stock-out (damage, transfer, etc.)
     - Required for audit and accountability
     - Helps track inventory losses
  4. **Data Validation:**
     - Validates quantity is positive
     - Validates variant exists
  5. **Inventory Impact:**
     - Stock-out automatically decreases inventory (via triggers)
     - Uses variant owner's user_id for inventory calculation
     - Ensures inventory only affects seller's products

**Why it's important:** This provides secure stock-out recording with complete audit trails. The reason documentation ensures accountability for inventory reductions. The variant linking ensures stock-out only affects the seller's inventory. This is essential for inventory accuracy and prevents unauthorized stock removals.

---

## 6. **Stock Adjustment Security Module**

### Most Important Function: `CreateStockAdjustmentAsync`

**Location:** `IT13_Final/Services/Data/StockAdjustmentService.cs` (Line 261-314)

**How it works:**
- **Purpose:** Securely records inventory adjustments with ownership verification and reason documentation.
- **Security Process:**
  1. **Variant Ownership Verification:**
     - Verifies variant belongs to seller before adjustment
     - `SELECT COUNT(*) FROM tbl_variants WHERE id = @VariantId AND user_id = @SellerUserId`
     - Returns null if verification fails
  2. **Adjustment Type Validation:**
     - Validates adjustment type is "Increase" or "Decrease"
     - Prevents invalid adjustment types
  3. **Quantity Validation:**
     - Validates quantity is positive
     - Prevents zero or negative adjustments
  4. **Reason Documentation:**
     - Records reason for adjustment (discrepancy, damage, etc.)
     - Required for audit and accountability
     - Helps track inventory corrections
  5. **User ID Tracking:**
     - Records `user_id` field with stock clerk's ID
     - Links adjustment to stock clerk who created it
     - Enables audit trail
  6. **Inventory Impact:**
     - Adjustment automatically updates inventory (via triggers)
     - Uses variant owner's user_id for inventory calculation
     - Ensures inventory only affects seller's products

**Why it's important:** This provides secure inventory adjustment recording with complete ownership verification. The ownership check ensures stock clerks can only adjust inventory for their seller's variants. The reason documentation ensures accountability for all adjustments. This is essential for inventory accuracy and prevents unauthorized inventory modifications.

---

## 7. **Inventory Viewing Security Module**

### Most Important Function: `GetInventoriesAsync`

**Location:** `IT13_Final/Services/Data/InventoryService.cs` (Line 68-204)

**How it works:**
- **Purpose:** Provides stock clerks with real-time inventory visibility scoped to their seller's products.
- **Security Process:**
  1. **Inventory Calculation:**
     - Uses CTEs to aggregate stock-in, stock-out, and adjustments
     - All CTEs filter by: `WHERE v.user_id = @UserId`
     - Only includes transactions for seller's variants
  2. **Current Stock Calculation:**
     - Calculates: `StockIn - StockOut + Adjustments`
     - Only for seller's variants
     - Real-time inventory levels
  3. **Data Filtering:**
     - Filters to show only items with stock > 0 (optional)
     - Joins with products, variants, categories
     - Complete product information
  4. **Search and Pagination:**
     - Supports search by product/variant name
     - Pagination for large inventories
     - All filtered by seller's variants

**Why it's important:** Stock clerks need to see available inventory for their seller's products. This function provides real-time stock visibility while ensuring complete data isolation. The variant owner filtering ensures stock clerks cannot see other sellers' inventory levels, protecting business confidentiality.

---

## 8. **Reorder Level Security Module**

### Most Important Function: `UpdateReorderLevelAsync`

**Location:** `IT13_Final/Services/Data/InventoryService.cs` (Line 415-464)

**How it works:**
- **Purpose:** Securely updates reorder levels with variant ownership verification.
- **Security Process:**
  1. **Variant Ownership Verification:**
     - Verifies variant belongs to seller before update
     - `SELECT COUNT(*) FROM tbl_variants WHERE id = @VariantId AND user_id = @SellerUserId`
     - Returns false if verification fails
  2. **Reorder Level Update:**
     - Updates or inserts reorder level in `tbl_inventories`
     - Scoped to variant-size-color combination
     - Links to seller's variant
  3. **Low Stock Alerts:**
     - Reorder levels used for low stock notifications
     - Helps manage inventory replenishment
     - Scoped to seller's products only

**Why it's important:** This allows stock clerks to set reorder levels for their seller's products. The ownership verification ensures stock clerks can only update reorder levels for their seller's variants. This is essential for inventory management and prevents unauthorized reorder level modifications.

---

## 9. **Purchase Order Viewing Security Module**

### Most Important Function: Purchase Order Queries with Supplier Filtering

**Location:** `IT13_Final/Services/Data/PurchaseOrderService.cs`

**How it works:**
- **Purpose:** Ensures stock clerks can only view purchase orders for their seller's suppliers.
- **Security Process:**
  1. **PO Queries:**
     - All PO queries join with suppliers: `WHERE s.user_id = @UserId`
     - Only shows POs for seller's suppliers
     - Complete PO data isolation
  2. **PO Status Filtering:**
     - Can filter by status (Pending, Approved, Completed, Cancelled)
     - All filtered by seller's suppliers
     - Prevents viewing other sellers' POs
  3. **PO Details:**
     - PO details include supplier information
     - Only accessible if supplier belongs to seller
     - Complete procurement data privacy

**Why it's important:** Purchase orders contain procurement information and spending details. This ensures stock clerks can only view POs from their seller's suppliers. This protects procurement confidentiality and prevents stock clerks from accessing other sellers' procurement data.

---

## 10. **Authentication Security Module** (Stock Clerk Access)

### Most Important Function: `AuthenticateAsync`

**Location:** `IT13_Final/Services/Auth/AuthService.cs` (Line 23-92)

**How it works:**
- **Purpose:** Securely authenticates stock clerk accounts and establishes authenticated sessions.
- **Security Process:**
  1. **Password Hashing:**
     - Stock clerk passwords hashed with SHA-256
     - Never stored in plain text
     - UTF-16LE encoding matches SQL Server format
  2. **Account Status Checks:**
     - Verifies `is_active = 1` (only active stock clerks can log in)
     - Verifies `archived_at IS NULL` (archived stock clerks cannot log in)
  3. **Role Verification:**
     - Retrieves stock clerk role from database
     - Returns authenticated user with role information
  4. **Session Establishment:**
     - Creates authenticated session with stock clerk user ID and role
     - Session used for all subsequent operations

**Why it's important:** This is the security gateway for stock clerk access. It ensures only valid, active stock clerk accounts can access the system. The password hashing protects stock clerk accounts even if the database is compromised.

---

## 11. **Password Management Security Module** (Stock Clerk Self-Service)

### Most Important Function: `UpdatePasswordAsync`

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 326-378)

**How it works:**
- **Purpose:** Allows stock clerks to securely change their own passwords with proper verification.
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

**Why it's important:** Stock clerks can securely manage their own passwords without admin intervention. The current password requirement prevents unauthorized password changes even if someone gains temporary access to a stock clerk's session.

---

## 12. **Audit Logging Security Module** (Stock Clerk Activity Tracking)

### Most Important Function: `LogAsync` and `GetLogsAsync`

**Location:** `IT13_Final/Services/Audit/HistoryService.cs`

**How it works:**
- **Purpose:** Tracks all stock clerk actions for security monitoring and inventory accountability.
- **Security Process:**
  1. **Activity Logging:**
     - Logs all stock clerk actions (create stock-in, create PO, etc.)
     - Records: stock clerk user ID, action type, module, description, timestamp
  2. **Stock Clerk Log Access:**
     - Stock clerks can view their own activity logs
     - Filtered by stock clerk user ID: `WHERE h.user_id = @UserId`
     - Stock clerks cannot see other stock clerks' logs
  3. **Inventory Audit Trail:**
     - Complete history of inventory operations
     - Tracks who performed what operation and when
     - Essential for inventory audits
  4. **Operation Logging:**
     - Every stock operation is logged
     - Records operation details
     - Maintains inventory accountability

**Why it's important:** This provides complete accountability for inventory operations. Stock clerk actions are logged for security monitoring and inventory audits. The user-scoped filtering ensures privacy while maintaining accountability. This is essential for inventory compliance and security investigations.

---

## 13. **Transaction Security Module** (Inventory Data Integrity)

### Most Important Function: Database Transactions for Inventory Operations

**Location:** Throughout inventory-related service files

**How it works:**
- **Purpose:** Ensures inventory operations are atomic and maintain data consistency.
- **Security Process:**
  1. **Transaction Wrapping:**
     - Inventory operations wrapped in transactions
     - Example: PO creation with PO items
     - Either all operations succeed or all roll back
  2. **Inventory Data Consistency:**
     - Prevents partial inventory states
     - Ensures inventory data always consistent
     - Example: PO creation must include all items
  3. **Error Handling:**
     - Transactions roll back on errors
     - Prevents corrupted inventory data
     - Maintains data integrity

**Why it's important:** Transactions prevent inventory data inconsistencies. For example, a PO creation without PO items would create procurement discrepancies. Atomic operations ensure inventory data always remains consistent and accurate.

---

## 14. **Input Validation Security Module** (Inventory Data Protection)

### Most Important Function: Input Sanitization in Inventory Operations

**Location:** Throughout all inventory service files

**How it works:**
- **Purpose:** Validates and sanitizes stock clerk input to prevent security vulnerabilities and inventory errors.
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
  4. **Inventory Validation:**
     - Validates quantities are positive
     - Validates cost prices are positive
     - Validates adjustment types match expected values
     - Validates variants exist

**Why it's important:** Input validation prevents malformed inventory data from entering the system. This protects inventory data integrity and prevents errors that could affect inventory levels and business operations.

---

## 15. **Account Status Security Module** (Stock Clerk Account Management)

### Most Important Function: Account Status Checks

**Location:** `IT13_Final/Services/Auth/AuthService.cs`, `UserService.cs`

**How it works:**
- **Purpose:** Controls stock clerk account access and enforces security policies.
- **Security Process:**
  1. **Account Activation (`is_active`):**
     - Authentication checks: `WHERE ISNULL(u.is_active,1)=1`
     - Only active stock clerk accounts can log in
     - Seller/admin can deactivate stock clerk accounts
  2. **Account Archival (`archived_at`):**
     - Authentication checks: `WHERE u.archived_at IS NULL`
     - Archived stock clerks cannot log in
     - Soft delete preserves data for audit
  3. **Forced Password Change (`must_change_pw`):**
     - New stock clerks must change password on first login
     - `MustChangePasswordAsync` checks this flag
     - Application redirects to password change if needed

**Why it's important:** These controls enable account management and security policies. Stock clerk accounts can be temporarily suspended (deactivated) or permanently disabled (archived) while maintaining data for audit purposes. Forced password changes ensure new stock clerks set secure passwords.

---

## 16. **Email Uniqueness Security Module**

### Most Important Function: Email Validation in Account Operations

**Location:** `IT13_Final/Services/Data/UserService.cs`

**How it works:**
- **Purpose:** Ensures stock clerk email addresses are unique, preventing account conflicts.
- **Security Process:**
  1. **Email Uniqueness Check:**
     - Before updating stock clerk email: `SELECT COUNT(*) FROM tbl_users WHERE email = @email AND id != @userId AND archived_at IS NULL`
     - Checks email doesn't exist for other users
     - Throws exception if duplicate found
  2. **Email Trimming:**
     - Trims whitespace: `email.Trim()`
     - Prevents issues with accidental spaces
  3. **Case-Insensitive:**
     - Email comparison is case-insensitive
     - Prevents duplicate accounts

**Why it's important:** Email is the username for authentication. Uniqueness ensures each stock clerk has a unique identity and prevents account conflicts. This is essential for authentication and prevents security issues.

---

## 17. **SQL Injection Prevention Module**

### Most Important Function: Parameterized Queries

**Location:** Throughout all inventory service files

**How it works:**
- **Purpose:** Prevents SQL injection attacks in inventory operations.
- **Security Process:**
  1. **Parameterized Queries:**
     - All SQL queries use `@parameterName` placeholders
     - Parameters added via `cmd.Parameters.AddWithValue("@param", value)`
     - SQL Server treats parameters as data, not executable code
  2. **Inventory Data Protection:**
     - All inventory data goes through parameters
     - Prevents injection in inventory queries
     - Protects inventory database
  3. **No String Concatenation:**
     - Never builds SQL by concatenating user input
     - All user input goes through parameters

**Why it's important:** SQL injection is a critical security vulnerability, especially for inventory systems. Parameterized queries completely prevent SQL injection by separating code (SQL) from data (parameters). This protects the inventory database from malicious input.

---

## Summary

The SoftWear system implements comprehensive security features that protect stock clerk operations and inventory data:

### Inventory Data Protection & Isolation
- **Complete Data Isolation:** Stock clerks can only access their seller's inventory data
- **Variant Ownership Verification:** Stock clerks can only operate on their seller's variants
- **Supplier Ownership Verification:** Stock clerks can only create POs with their seller's suppliers
- **Inventory Viewing Privacy:** Stock clerks only see inventory for their seller's products
- **Stock Operation Privacy:** All stock operations scoped to seller's variants

### Operation Security
- **Ownership Verification:** All operations verify variant/supplier ownership before processing
- **Transaction Security:** Atomic operations ensure inventory data consistency
- **Audit Trails:** Complete accountability for all inventory operations
- **User Tracking:** All operations linked to stock clerk user ID

### Access Control
- **Role-Based Access:** Stock clerks have appropriate permissions for inventory operations
- **Account Status Control:** Active/archived account management
- **Secure Authentication:** SHA-256 password hashing
- **Password Management:** Stock clerks can securely change passwords

### Audit & Monitoring
- **Activity Logging:** Complete audit trail of stock clerk actions
- **Inventory Audit Trail:** All stock operations logged
- **Stock Clerk Log Access:** Stock clerks can view their own activity

**Key Stock Clerk Security Principles:**
1. **Complete Inventory Data Isolation** - Stock clerks cannot access other sellers' inventory
2. **Ownership Verification** - All operations verify variant/supplier ownership
3. **Multi-Layer Security** - UI, service, and database-level protection
4. **Complete Audit Trails** - All inventory operations logged
5. **Transaction Security** - Atomic operations maintain inventory data integrity
6. **Secure Authentication** - Passwords never stored in plain text

These security features work together to create a secure inventory management system where stock clerks can perform their duties with complete data privacy, proper access controls, and full accountability for all inventory operations.


