                                                                                                                                                                                                                                                                                                                                                                        # Seller Security Features Documentation

This document highlights security features and their functions that protect seller data and operations in the SoftWear system, with explanations of how they work from the seller's perspective.

---

## 1. **Seller Data Isolation Security Module**

### Most Important Function: User ID Filtering in All Data Queries

**Location:** Throughout all data service files (ProductService.cs, VariantService.cs, SupplierService.cs, etc.)

**How it works:**
- **Purpose:** Ensures sellers can only access and manage their own business data, completely isolating data between different sellers.
- **Security Process:**
  1. **Product Data Isolation:**
     - All product queries include: `WHERE p.user_id = @UserId AND p.archived_at IS NULL`
     - Sellers only see products they created
     - Product creation automatically links to seller: `user_id = @UserId`
  2. **Variant Data Isolation:**
     - Variant queries: `WHERE v.user_id = @UserId AND v.archived_at IS NULL`
     - Sellers only see variants for their products
     - Variant creation links to seller automatically
  3. **Supplier Data Isolation:**
     - Supplier queries: `WHERE user_id = @UserId AND archived_at IS NULL`
     - Sellers only see their own suppliers
     - Prevents access to other sellers' supplier information
  4. **Inventory Data Isolation:**
     - Inventory queries filter by variant owner: `WHERE v.user_id = @UserId`
     - Sellers only see inventory for their products
     - Stock operations only affect seller's inventory
  5. **Sales Data Isolation:**
     - Sales queries filter by cashier's seller: `WHERE u.user_id = @SellerUserId`
     - Sellers only see sales from their cashiers
     - Complete business data privacy

**Why it's important:** This is the foundation of multi-tenant security. It ensures complete data privacy between businesses. Even if a seller somehow gains access to another seller's account, they cannot see or modify other sellers' data. This protects business confidentiality and prevents data leakage between competing businesses on the platform.

---

## 2. **Staff Ownership Verification Security Module**

### Most Important Function: Staff Creation with Ownership Linking

**Location:** `IT13_Final/Services/Data/UserService.cs` (CreateCashierAsync, CreateAccountingAsync, CreateStockClerkAsync)

**How it works:**
- **Purpose:** Ensures staff members are properly linked to the seller's business and can only access seller's data.
- **Security Process:**
  1. **Staff Creation:**
     - When seller creates staff (cashier, accounting, stock clerk):
       - Staff record includes: `user_id = @SellerUserId`
       - This links staff to seller's business
       - Staff inherits seller's data access scope
  2. **Staff Data Access:**
     - Cashiers can only see sales from their seller: `WHERE u.user_id = @SellerUserId`
     - Accounting can only see financial data from their seller
     - Stock clerks can only see inventory from their seller
  3. **Ownership Verification:**
     - Before operations, system verifies: `WHERE id = @StaffId AND user_id = @SellerUserId`
     - Prevents sellers from accessing other sellers' staff
     - Ensures staff operations only affect seller's business

**Why it's important:** This ensures that staff members created by a seller can only access that seller's data. It prevents staff from accidentally or maliciously accessing other sellers' information. The ownership link creates a security boundary that protects business data while allowing sellers to manage their team effectively.

---

## 3. **Product Ownership Verification Security Module**

### Most Important Function: Product/Variant Ownership Checks

**Location:** `IT13_Final/Services/Data/ProductService.cs`, `VariantService.cs`

**How it works:**
- **Purpose:** Verifies that products and variants belong to the seller before allowing operations.
- **Security Process:**
  1. **Product Queries:**
     - All product operations filter by: `WHERE p.user_id = @UserId`
     - Product creation: `INSERT ... user_id = @UserId`
     - Product updates: `WHERE id = @ProductId AND user_id = @UserId`
     - Product deletion: `WHERE id = @ProductId AND user_id = @UserId`
  2. **Variant Queries:**
     - All variant operations filter by: `WHERE v.user_id = @UserId`
     - Variant creation links to seller automatically
     - Variant updates verify ownership before allowing changes
  3. **Ownership Verification Pattern:**
     ```sql
     SELECT COUNT(*) 
     FROM tbl_variants 
     WHERE id = @VariantId 
     AND user_id = @SellerUserId 
     AND archived_at IS NULL
     ```
  4. **Operation Blocking:**
     - If ownership verification fails, operation returns false/null
     - Prevents unauthorized product modifications

**Why it's important:** This ensures sellers can only manage their own products and variants. Even if there's a bug or security vulnerability, the database-level ownership checks prevent sellers from modifying other sellers' products. This protects product catalogs and pricing information from unauthorized access.

---

## 4. **Supplier Ownership Verification Security Module**

### Most Important Function: Supplier Ownership Checks

**Location:** `IT13_Final/Services/Data/SupplierService.cs`, `PurchaseOrderService.cs`

**How it works:**
- **Purpose:** Ensures suppliers belong to the seller before allowing purchase order creation or supplier management.
- **Security Process:**
  1. **Supplier Queries:**
     - All supplier operations: `WHERE user_id = @UserId AND archived_at IS NULL`
     - Sellers only see their own suppliers
  2. **Purchase Order Verification:**
     - Before creating PO: `SELECT COUNT(*) FROM tbl_suppliers WHERE id = @SupplierId AND user_id = @SellerUserId`
     - Verifies supplier belongs to seller
     - PO creation blocked if verification fails
  3. **Supplier Payment Verification:**
     - Before processing payment: Verifies PO/supplier belongs to seller
     - Prevents payments to other sellers' suppliers

**Why it's important:** This prevents sellers from creating purchase orders with other sellers' suppliers, which could cause procurement confusion and security issues. It also protects supplier contact information and payment details from unauthorized access.

---

## 5. **Inventory Ownership Security Module**

### Most Important Function: Inventory Filtering by Variant Owner

**Location:** `IT13_Final/Services/Data/InventoryService.cs`

**How it works:**
- **Purpose:** Ensures sellers only see inventory for their own products.
- **Security Process:**
  1. **Inventory Queries:**
     - Inventory calculation filters by: `WHERE v.user_id = @UserId`
     - Joins through variants to products to get seller ownership
     - Only shows inventory for seller's products
  2. **Reorder Level Updates:**
     - Before updating reorder level: Verifies variant belongs to seller
     - `SELECT COUNT(*) FROM tbl_variants WHERE id = @VariantId AND user_id = @SellerUserId`
     - Operation blocked if variant doesn't belong to seller
  3. **Stock Operations:**
     - Stock-in/out operations use variant owner's user_id
     - Ensures stock operations only affect seller's inventory

**Why it's important:** Inventory data is sensitive business information. This ensures sellers cannot see other sellers' stock levels, which could provide competitive advantages. It also prevents unauthorized inventory modifications that could affect other businesses.

---

## 6. **Sales Data Isolation Security Module**

### Most Important Function: Sales Filtering by Cashier's Seller

**Location:** `IT13_Final/Services/Data/SalesService.cs`, `IncomeBreakdownService.cs`

**How it works:**
- **Purpose:** Ensures sellers only see sales data from their own cashiers and business operations.
- **Security Process:**
  1. **Sales Queries:**
     - Sales filtered by cashier's seller: `WHERE u.user_id = @SellerUserId`
     - Joins sales with users table to get cashier's seller
     - Only shows sales from seller's cashiers
  2. **Income Breakdown:**
     - Income queries: `WHERE u.user_id = @SellerUserId`
     - Groups by seller's cashiers only
     - Shows revenue breakdown for seller's business only
  3. **Daily Sales Verification:**
     - Only shows daily sales from seller's cashiers
     - Verification operations verify cashier belongs to seller

**Why it's important:** Sales data is highly sensitive business information. This ensures complete privacy of sales performance, revenue, and transaction details. Sellers cannot see other sellers' sales data, protecting competitive information and business confidentiality.

---

## 7. **Staff Management Security Module**

### Most Important Function: Staff Access Control

**Location:** `IT13_Final/Services/Data/UserService.cs` (GetCashierUsersAsync, GetAccountingUsersAsync, etc.)

**How it works:**
- **Purpose:** Ensures sellers can only view and manage staff that belong to their business.
- **Security Process:**
  1. **Staff Queries:**
     - Cashier queries: `WHERE u.user_id = @SellerUserId AND LOWER(r.name) = 'cashier'`
     - Accounting queries: `WHERE u.user_id = @SellerUserId AND LOWER(r.name) = 'accounting'`
     - Stock clerk queries: `WHERE u.user_id = @SellerUserId AND LOWER(r.name) = 'stockclerk'`
  2. **Staff Creation:**
     - When seller creates staff, `user_id` field links to seller
     - Staff automatically scoped to seller's business
  3. **Staff Updates:**
     - Staff updates verify ownership before allowing changes
     - Prevents modification of other sellers' staff

**Why it's important:** This ensures sellers can only manage their own team members. It prevents sellers from viewing or modifying other sellers' staff accounts, protecting employee information and maintaining proper organizational boundaries.

---

## 8. **Expense Data Isolation Security Module**

### Most Important Function: Expense Filtering by Seller

**Location:** `IT13_Final/Services/Data/ExpenseService.cs`

**How it works:**
- **Purpose:** Ensures sellers only see expenses for their own business.
- **Security Process:**
  1. **Expense Queries:**
     - All expense operations: `WHERE seller_user_id = @SellerUserId AND archived_at IS NULL`
     - Sellers only see their own expenses
     - Expense creation links to seller: `seller_user_id = @SellerUserId`
  2. **Expense Reports:**
     - Reports filtered by seller's user ID
     - Only shows seller's business expenses
     - Financial data completely isolated

**Why it's important:** Expense data reveals business spending patterns and financial information. This ensures complete privacy of expense records and prevents sellers from seeing other sellers' financial data.

---

## 9. **Purchase Order Ownership Security Module**

### Most Important Function: PO Ownership Verification

**Location:** `IT13_Final/Services/Data/PurchaseOrderService.cs`

**How it works:**
- **Purpose:** Ensures sellers only see and manage purchase orders for their own business.
- **Security Process:**
  1. **PO Queries:**
     - PO queries join with suppliers: `WHERE s.user_id = @UserId`
     - Only shows POs for seller's suppliers
     - PO creation verifies supplier belongs to seller
  2. **PO Verification:**
     - Before PO operations: `SELECT COUNT(*) FROM tbl_suppliers WHERE id = @SupplierId AND user_id = @SellerUserId`
     - Verifies supplier ownership before allowing PO creation
     - Prevents POs with other sellers' suppliers

**Why it's important:** Purchase orders contain procurement information and spending details. This ensures sellers cannot see other sellers' procurement activities and prevents unauthorized PO creation that could affect other businesses.

---

## 10. **Authentication Security Module** (Seller Access)

### Most Important Function: `AuthenticateAsync`

**Location:** `IT13_Final/Services/Auth/AuthService.cs` (Line 23-92)

**How it works:**
- **Purpose:** Securely authenticates seller accounts and establishes authenticated sessions.
- **Security Process:**
  1. **Password Hashing:**
     - Seller passwords hashed with SHA-256
     - Never stored in plain text
     - UTF-16LE encoding matches SQL Server format
  2. **Account Status Checks:**
     - Verifies `is_active = 1` (only active sellers can log in)
     - Verifies `archived_at IS NULL` (archived sellers cannot log in)
  3. **Role Verification:**
     - Retrieves seller role from database
     - Returns authenticated seller with role information
  4. **Session Establishment:**
     - Creates authenticated session with seller ID and role
     - Session used for all subsequent operations

**Why it's important:** This is the security gateway for seller access. It ensures only valid, active seller accounts can access the system. The password hashing protects seller accounts even if the database is compromised.

---

## 11. **Password Management Security Module** (Seller Self-Service)

### Most Important Function: `UpdatePasswordAsync`

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 326-378)

**How it works:**
- **Purpose:** Allows sellers to securely change their own passwords with proper verification.
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

**Why it's important:** Sellers can securely manage their own passwords without admin intervention. The current password requirement prevents unauthorized password changes even if someone gains temporary access to a seller's session.

---

## 12. **Audit Logging Security Module** (Seller Activity Tracking)

### Most Important Function: `LogAsync` and `GetLogsAsync`

**Location:** `IT13_Final/Services/Audit/HistoryService.cs`

**How it works:**
- **Purpose:** Tracks all seller actions for security monitoring and accountability.
- **Security Process:**
  1. **Activity Logging:**
     - Logs all seller actions (create product, create staff, etc.)
     - Records: seller ID, action type, module, description, timestamp
  2. **Seller Log Access:**
     - Sellers can view their own activity logs
     - Filtered by seller's user ID: `WHERE h.user_id = @UserId`
     - Sellers cannot see other sellers' logs
  3. **Audit Trail:**
     - Complete history of seller operations
     - Enables tracking of changes and actions
     - Supports security investigations

**Why it's important:** This provides sellers with visibility into their own account activity. It helps sellers monitor their operations and detect any unauthorized access. The seller-scoped filtering ensures privacy while maintaining accountability.

---

## 13. **Account Status Security Module** (Seller Account Management)

### Most Important Function: Account Status Checks

**Location:** `IT13_Final/Services/Auth/AuthService.cs`, `UserService.cs`

**How it works:**
- **Purpose:** Controls seller account access and enforces security policies.
- **Security Process:**
  1. **Account Activation (`is_active`):**
     - Authentication checks: `WHERE ISNULL(u.is_active,1)=1`
     - Only active seller accounts can log in
     - Admin can deactivate seller accounts
  2. **Account Archival (`archived_at`):**
     - Authentication checks: `WHERE u.archived_at IS NULL`
     - Archived sellers cannot log in
     - Soft delete preserves data for audit
  3. **Forced Password Change (`must_change_pw`):**
     - New sellers must change password on first login
     - `MustChangePasswordAsync` checks this flag
     - Application redirects to password change if needed

**Why it's important:** These controls enable account management and security policies. Sellers can be temporarily suspended (deactivated) or permanently disabled (archived) while maintaining data for audit purposes. Forced password changes ensure new sellers set secure passwords.

---

## 14. **Email Uniqueness Security Module**

### Most Important Function: Email Validation in Account Operations

**Location:** `IT13_Final/Services/Data/UserService.cs` (UpdateSellerAsync, etc.)

**How it works:**
- **Purpose:** Ensures seller email addresses are unique, preventing account conflicts.
- **Security Process:**
  1. **Email Uniqueness Check:**
     - Before updating seller email: `SELECT COUNT(*) FROM tbl_users WHERE email = @email AND id != @userId AND archived_at IS NULL`
     - Checks email doesn't exist for other users
     - Throws exception if duplicate found
  2. **Email Trimming:**
     - Trims whitespace: `email.Trim()`
     - Prevents issues with accidental spaces
  3. **Case-Insensitive:**
     - Email comparison is case-insensitive
     - Prevents duplicate accounts

**Why it's important:** Email is the username for authentication. Uniqueness ensures each seller has a unique identity and prevents account conflicts. This is essential for authentication and prevents security issues.

---

## 15. **Transaction Security Module** (Seller Data Integrity)

### Most Important Function: Database Transactions for Seller Operations

**Location:** Throughout seller-related service files

**How it works:**
- **Purpose:** Ensures seller operations are atomic and maintain data consistency.
- **Security Process:**
  1. **Transaction Wrapping:**
     - Seller operations wrapped in transactions
     - Example: Creating variant with sizes and colors
     - Either all operations succeed or all roll back
  2. **Data Consistency:**
     - Prevents partial data states
     - Ensures seller data always consistent
     - Example: Product creation with category - both must succeed
  3. **Error Handling:**
     - Transactions roll back on errors
     - Prevents corrupted seller data
     - Maintains data integrity

**Why it's important:** Transactions prevent data inconsistencies that could affect seller operations. For example, a variant creation without size/color links would create incomplete data. Atomic operations ensure seller data always remains consistent and secure.

---

## 16. **Input Validation Security Module** (Seller Data Protection)

### Most Important Function: Input Sanitization in Seller Operations

**Location:** Throughout all seller service files

**How it works:**
- **Purpose:** Validates and sanitizes seller input to prevent security vulnerabilities.
- **Security Process:**
  1. **String Trimming:**
     - All string inputs trimmed: `name.Trim()`, `email.Trim()`
     - Removes leading/trailing whitespace
  2. **Null Handling:**
     - Optional fields: `(object?)value ?? DBNull.Value`
     - Prevents null reference exceptions
  3. **Type Validation:**
     - Validates data types before database operations
     - Converts types safely
  4. **Business Logic Validation:**
     - Validates quantities are positive
     - Validates prices are reasonable
     - Validates dates are valid

**Why it's important:** Input validation prevents malformed data from entering seller's business data. This protects seller data integrity and prevents errors that could affect business operations.

---

## Summary

The SoftWear system implements comprehensive security features that protect seller data and operations:

### Data Protection & Isolation
- **Complete Data Isolation:** Sellers can only access their own business data
- **Product/Variant Ownership:** Sellers can only manage their own products
- **Supplier Ownership:** Sellers can only manage their own suppliers
- **Inventory Isolation:** Sellers only see their own inventory
- **Sales Data Privacy:** Sellers only see sales from their cashiers
- **Expense Data Privacy:** Sellers only see their own expenses

### Access Control
- **Staff Ownership:** Sellers can only manage their own staff
- **Ownership Verification:** Database-level checks prevent unauthorized access
- **Role-Based Access:** Sellers have appropriate permissions for their role
- **Account Status Control:** Active/archived account management

### Authentication & Account Security
- **Secure Authentication:** SHA-256 password hashing
- **Password Management:** Sellers can securely change passwords
- **Email Uniqueness:** Prevents account conflicts
- **Forced Password Changes:** Security policy enforcement

### Audit & Monitoring
- **Activity Logging:** Complete audit trail of seller actions
- **Seller Log Access:** Sellers can view their own activity
- **Transaction Security:** Atomic operations maintain data integrity

**Key Seller Security Principles:**
1. **Complete Data Isolation** - Sellers cannot access other sellers' data
2. **Ownership Verification** - All operations verify data ownership
3. **Multi-Layer Security** - UI, service, and database-level protection
4. **Audit Trails** - Complete accountability for seller actions
5. **Secure Authentication** - Passwords never stored in plain text
6. **Input Validation** - Prevents malformed data and attacks

These security features work together to create a secure, multi-tenant platform where sellers can operate their businesses with complete data privacy and protection.


