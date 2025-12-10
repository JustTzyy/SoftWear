# System Security Features Documentation

This document highlights all security features and their functions across the SoftWear system, with explanations of how they work to protect the system and data.

---

## 1. **Authentication Security Module** (AuthService.cs)

### Most Important Function: `AuthenticateAsync`

**Location:** `IT13_Final/Services/Auth/AuthService.cs` (Line 23-92)

**How it works:**
- **Purpose:** Validates user credentials and establishes authenticated sessions with proper security measures.
- **Security Process:**
  1. **Password Hashing:**
     - Hashes the provided password using SHA-256 algorithm
     - Uses UTF-16LE encoding (Encoding.Unicode) to match SQL Server's HASHBYTES function
     - Converts hash to hexadecimal string for comparison
     - **Never stores or transmits plain text passwords**
  2. **User Validation:**
     - Queries database for user with matching email
     - Checks `is_active = 1` (only active users can log in)
     - Checks `archived_at IS NULL` (archived users cannot log in)
     - Joins with roles table to get user's role
  3. **Password Verification:**
     - Compares computed password hash with stored hash (case-insensitive)
     - Returns null if password doesn't match (no indication of which failed - email or password)
  4. **Session Establishment:**
     - Returns authenticated user object with ID, email, full name, and role
     - Role is normalized to lowercase for consistent comparison
  5. **Error Handling:**
     - Handles SQL connection errors securely
     - Provides helpful error messages for connection issues
     - Prevents information leakage about database structure

**Why it's important:** This is the primary security gateway for the entire system. It ensures only valid, active users with correct credentials can access the system. The SHA-256 hashing ensures passwords are never stored in plain text, protecting user accounts even if the database is compromised. The active/archived checks prevent unauthorized access from disabled accounts.

---

## 2. **Password Management Security Module** (UserService.cs)

### Most Important Function: `UpdatePasswordAsync`

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 326-378)

**How it works:**
- **Purpose:** Securely changes user passwords with current password verification and proper hashing.
- **Security Process:**
  1. **Current Password Verification:**
     - Retrieves stored password hash from database
     - Hashes the provided current password using SHA-256
     - Compares hashes to verify current password is correct
     - Returns false if current password doesn't match (prevents unauthorized password changes)
  2. **New Password Hashing:**
     - Hashes the new password using SHA-256 with UTF-16LE encoding
     - Converts to hexadecimal string
  3. **Password Update:**
     - Updates password hash in database
     - Sets `must_change_pw = 0` (password has been changed)
     - Updates timestamp for audit trail
  4. **Account Status Check:**
     - Only updates if account is not archived (`archived_at IS NULL`)

**Why it's important:** This function ensures password changes are secure and authorized. Requiring the current password prevents unauthorized password changes even if someone gains temporary access to a user's session. The automatic clearing of `must_change_pw` flag ensures users who change their password don't get stuck in a password change loop.

---

## 3. **Password Hashing Function** (Used in Multiple Services)

### Function: SHA-256 Password Hashing

**Location:** Used in `AuthService.cs`, `UserService.cs` (CreateAdminAsync, CreateSellerAsync, etc.)

**How it works:**
- **Purpose:** Securely hashes passwords before storage, ensuring passwords are never stored in plain text.
- **Implementation:**
  ```csharp
  string passwordHash = Convert.ToHexString(
      SHA256.HashData(Encoding.Unicode.GetBytes(password))
  );
  ```
- **Process:**
  1. Converts password string to bytes using UTF-16LE encoding (matches SQL Server HASHBYTES)
  2. Applies SHA-256 cryptographic hash function
  3. Converts hash bytes to hexadecimal string
  4. Stores hexadecimal string in database

**Why it's important:** SHA-256 is a one-way hash function, meaning passwords cannot be reversed or decrypted. Even if the database is compromised, attackers cannot retrieve original passwords. The UTF-16LE encoding ensures compatibility with SQL Server's HASHBYTES function, allowing consistent hashing across different systems.

---

## 4. **Session Management Security Module** (AuthState.cs)

### Most Important Function: `SetUser` and `Logout`

**Location:** `IT13_Final/Services/Auth/AuthState.cs` (Line 17-27)

**How it works:**
- **Purpose:** Manages user session state securely throughout the application lifecycle.
- **Security Process:**
  1. **Session Establishment (`SetUser`):**
     - Stores authenticated user object in memory
     - Triggers change event to update UI
     - User object contains: ID, email, full name, and role
  2. **Session Persistence:**
     - Session persists across page navigation
     - Used for role-based UI rendering
     - Used for access control checks
  3. **Session Termination (`Logout`):**
     - Clears user object from memory
     - Triggers change event to update UI
     - Prevents further access to protected resources

**Why it's important:** This provides secure session management without storing sensitive data in cookies or local storage. The in-memory storage ensures sessions are cleared when the application closes. The change event ensures UI updates immediately when session state changes, preventing access to restricted areas after logout.

---

## 5. **Role-Based Access Control (RBAC) Module**

### Most Important Function: Role Validation in Database Queries

**Location:** Throughout all service files (e.g., `UserService.cs`, `ProductService.cs`, etc.)

**How it works:**
- **Purpose:** Enforces role-based access control at the database and application level.
- **Security Process:**
  1. **Role Assignment:**
     - Each user has a `role_id` linking to `tbl_roles`
     - Roles: admin, seller, accounting, cashier, stockclerk
     - Role is retrieved during authentication and stored in session
  2. **Route Protection:**
     - Routes are prefixed with role name: `/admin/...`, `/seller/...`, etc.
     - UI conditionally renders based on role: `@if (AuthState.CurrentUser?.Role == "admin")`
  3. **Service-Level Validation:**
     - Service methods check user role before executing operations
     - Admin-only functions verify admin role
     - Seller functions verify seller role or seller's staff
  4. **Database-Level Filtering:**
     - Queries filter by role when retrieving role-specific data
     - Example: `WHERE LOWER(r.name) = 'admin'` for admin user queries

**Why it's important:** RBAC ensures users can only access features appropriate for their role. This prevents privilege escalation and maintains separation of duties. The multi-layer approach (route, UI, service, database) provides defense in depth, ensuring security even if one layer is bypassed.

---

## 6. **Data Isolation Security Module**

### Most Important Function: User/Seller ID Filtering

**Location:** Throughout all data service files

**How it works:**
- **Purpose:** Ensures users can only access data belonging to their business or organization.
- **Security Process:**
  1. **Seller Data Isolation:**
     - All seller data queries include: `WHERE user_id = @SellerUserId`
     - Products, variants, suppliers, inventory all filtered by seller's user_id
     - Prevents sellers from accessing other sellers' data
  2. **Staff Data Isolation:**
     - Staff members (cashiers, accounting, stock clerks) linked to seller via `user_id` field
     - Queries filter by: `WHERE u.user_id = @SellerUserId`
     - Staff can only access their seller's data
  3. **Cashier Data Isolation:**
     - Cashiers can only see their own sales: `WHERE user_id = @CashierUserId`
     - Prevents cashiers from viewing other cashiers' transactions
  4. **Verification Functions:**
     - Many operations include verification: "Verify cashier belongs to seller"
     - Example: `SELECT COUNT(*) FROM tbl_users WHERE id = @CashierUserId AND user_id = @SellerUserId`
     - Returns false/error if ownership verification fails

**Why it's important:** This implements multi-tenant security, ensuring complete data isolation between different businesses. Even if a user gains unauthorized access, they cannot access data from other sellers. This is critical for a platform serving multiple businesses and protects business confidentiality.

---

## 7. **Account Status Security Module** (UserService.cs)

### Most Important Function: `MustChangePasswordAsync` and Account Status Checks

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 661-698)

**How it works:**
- **Purpose:** Enforces account security policies including forced password changes and account activation status.
- **Security Process:**
  1. **Forced Password Change (`MustChangePasswordAsync`):**
     - Checks `must_change_pw` flag in user record
     - New users have `must_change_pw = 1` (must change on first login)
     - After password change, flag is set to 0
     - Application can redirect to password change page if flag is true
  2. **Account Activation (`is_active`):**
     - Authentication checks: `WHERE ISNULL(u.is_active,1)=1`
     - Only active accounts can log in
     - Admins can deactivate accounts without archiving
  3. **Account Archival (`archived_at`):**
     - Authentication checks: `WHERE u.archived_at IS NULL`
     - Archived accounts cannot log in
     - Soft delete preserves data for audit while preventing access

**Why it's important:** These controls enable account management and security policies. Forced password changes ensure new users set their own passwords, preventing default password usage. Account activation allows temporary suspension without deletion. Archival provides permanent deactivation while maintaining audit trails.

---

## 8. **Email Uniqueness Security Module** (UserService.cs)

### Most Important Function: Email Validation in User Creation

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 717-726, 782-791, etc.)

**How it works:**
- **Purpose:** Ensures email addresses are unique across the system, preventing account conflicts and security issues.
- **Security Process:**
  1. **Email Uniqueness Check:**
     - Before creating/updating user: `SELECT COUNT(*) FROM tbl_users WHERE email = @email AND archived_at IS NULL`
     - Checks only non-archived users (archived emails can be reused)
     - Throws `EmailAlreadyExistsException` if email exists
  2. **Email Trimming:**
     - Trims whitespace: `email.Trim()`
     - Prevents issues with accidental spaces
  3. **Case-Insensitive Comparison:**
     - Email comparison is case-insensitive
     - Prevents duplicate accounts with different cases

**Why it's important:** Email uniqueness prevents account conflicts and ensures each user has a unique identity. This is essential for authentication (email is the username) and prevents security issues like account takeover. The archived user exclusion allows email reuse after account deletion.

---

## 9. **Audit Logging Security Module** (HistoryService.cs)

### Most Important Function: `LogAsync`

**Location:** `IT13_Final/Services/Audit/HistoryService.cs` (Line 32-47)

**How it works:**
- **Purpose:** Creates comprehensive audit trails of all user actions for security monitoring and compliance.
- **Security Process:**
  1. **Action Logging:**
     - Logs every significant user action (create, update, delete, approve, etc.)
     - Records: user ID, action status, module name, description, timestamp
     - Inserts into `tbl_histories` table
  2. **Audit Trail Fields:**
     - **User ID:** Who performed the action
     - **Status:** Action type (create, update, delete, approve, login, etc.)
     - **Module:** Which module/feature was used
     - **Description:** Detailed description of the action
     - **Timestamp:** When the action occurred
  3. **Query Capabilities:**
     - Filter by user, status, module, date range
     - Search by description
     - Pagination for large audit logs
     - Joins with users and roles for complete information

**Why it's important:** Audit logging provides complete accountability and enables security monitoring. Admins can track all system activities, identify suspicious behavior, investigate security incidents, and maintain compliance. The comprehensive logging ensures no action goes unrecorded, providing a complete security audit trail.

---

## 10. **Data Ownership Verification Module**

### Most Important Function: Ownership Verification Before Operations

**Location:** Throughout service files (e.g., `PurchaseOrderService.cs`, `DailySalesVerificationService.cs`, `StockAdjustmentService.cs`)

**How it works:**
- **Purpose:** Verifies data ownership before allowing operations, preventing unauthorized access and modifications.
- **Security Process:**
  1. **Verification Pattern:**
     ```sql
     SELECT COUNT(*) 
     FROM [table] 
     WHERE id = @Id 
     AND user_id = @UserId 
     AND archived_at IS NULL
     ```
  2. **Common Verifications:**
     - **Variant Ownership:** Verify variant belongs to seller before stock operations
     - **Cashier Ownership:** Verify cashier belongs to seller before sales verification
     - **Supplier Ownership:** Verify supplier belongs to seller before PO creation
     - **PO Ownership:** Verify PO belongs to seller before approval
  3. **Operation Blocking:**
     - If verification returns 0, operation is blocked
     - Returns false/null to indicate failure
     - Prevents cross-business data access

**Why it's important:** This provides defense-in-depth for data isolation. Even if a user somehow bypasses UI restrictions, database-level verification prevents unauthorized operations. This is critical for multi-tenant security and ensures data integrity across business boundaries.

---

## 11. **SQL Injection Prevention Module**

### Most Important Function: Parameterized Queries

**Location:** Throughout all service files

**How it works:**
- **Purpose:** Prevents SQL injection attacks by using parameterized queries instead of string concatenation.
- **Security Process:**
  1. **Parameterized Queries:**
     - All SQL queries use `@parameterName` placeholders
     - Parameters added via `cmd.Parameters.AddWithValue("@param", value)`
     - SQL Server treats parameters as data, not executable code
  2. **Example:**
     ```csharp
     var sql = "SELECT * FROM tbl_users WHERE email = @email";
     cmd.Parameters.AddWithValue("@email", email);
     ```
  3. **Search Term Handling:**
     - Search terms use LIKE with parameterized wildcards: `$"%{searchTerm}%"`
     - Prevents injection while allowing pattern matching
  4. **No String Concatenation:**
     - Never builds SQL by concatenating user input
     - All user input goes through parameters

**Why it's important:** SQL injection is one of the most common and dangerous security vulnerabilities. Parameterized queries completely prevent SQL injection by separating code (SQL) from data (parameters). This is a fundamental security practice that protects the database from malicious input.

---

## 12. **Account Creation Security Module** (UserService.cs)

### Most Important Function: `CreateAdminAsync`, `CreateSellerAsync`, etc.

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 700-774, 1022-1097, etc.)

**How it works:**
- **Purpose:** Creates user accounts with proper security settings and validation.
- **Security Process:**
  1. **Role Verification:**
     - Retrieves role ID from database
     - Validates role exists before creating user
     - Throws exception if role not found
  2. **Email Uniqueness:**
     - Checks email doesn't already exist
     - Throws exception if duplicate
  3. **Password Security:**
     - Hashes password using SHA-256 before storage
     - Never stores plain text password
  4. **Account Security Settings:**
     - `is_active = 1` (account is active)
     - `must_change_pw = 1` (user must change password on first login)
     - `archived_at = NULL` (account is not archived)
  5. **Data Validation:**
     - Trims and validates input
     - Handles optional fields properly
     - Converts data types securely

**Why it's important:** Secure account creation ensures all new accounts follow security best practices. The forced password change prevents default password usage. Proper role assignment ensures users get correct permissions. Email validation prevents account conflicts.

---

## 13. **Transaction Security Module**

### Most Important Function: Database Transactions for Atomicity

**Location:** Throughout service files (e.g., `SalesService.cs`, `PurchaseOrderService.cs`, `ReturnsService.cs`)

**How it works:**
- **Purpose:** Ensures data consistency and prevents partial operations that could create security vulnerabilities.
- **Security Process:**
  1. **Transaction Wrapping:**
     ```csharp
     using var transaction = conn.BeginTransaction();
     try {
         // Multiple operations
         transaction.Commit();
     } catch {
         transaction.Rollback();
     }
     ```
  2. **Atomic Operations:**
     - Either all operations succeed, or all are rolled back
     - Prevents partial data states
     - Ensures data integrity
  3. **Example (Sales):**
     - Sale record, sale items, stock-out records, payment record
     - All must succeed together
     - If any fails, entire transaction rolls back

**Why it's important:** Transactions prevent data inconsistencies that could be exploited. For example, a sale without inventory reduction could allow overselling. A PO approval without stock-in creation could create procurement discrepancies. Atomic operations ensure data always remains in a consistent, secure state.

---

## 14. **Input Validation Security Module**

### Most Important Function: Input Sanitization and Validation

**Location:** Throughout all service files

**How it works:**
- **Purpose:** Validates and sanitizes user input to prevent security vulnerabilities and data corruption.
- **Security Process:**
  1. **String Trimming:**
     - All string inputs are trimmed: `email.Trim()`, `name.Trim()`
     - Removes leading/trailing whitespace
  2. **Null Handling:**
     - Optional fields use `(object?)value ?? DBNull.Value`
     - Prevents null reference exceptions
  3. **Type Validation:**
     - Validates data types before database operations
     - Converts types safely (e.g., `Convert.ToInt32()`)
  4. **Range Validation:**
     - Validates quantities > 0
     - Validates dates are reasonable
     - Validates amounts are positive
  5. **Enum/Status Validation:**
     - Validates adjustment types ("Increase"/"Decrease")
     - Validates status values match expected values

**Why it's important:** Input validation prevents malformed data from entering the system, which could cause errors, data corruption, or security vulnerabilities. Proper null handling prevents exceptions that could expose system information. Type validation ensures data integrity.

---

## 15. **Connection String Security Module**

### Most Important Function: Secure Database Connection

**Location:** All service files

**How it works:**
- **Purpose:** Establishes secure database connections with proper authentication and encryption.
- **Security Process:**
  1. **Integrated Security:**
     - Uses Windows Authentication: `Integrated Security=SSPI`
     - No passwords in connection strings
     - Uses current Windows user credentials
  2. **Certificate Trust:**
     - `TrustServerCertificate=True` for development
     - Enables encrypted connections
  3. **Connection Timeout:**
     - `Connection Timeout=30` prevents hanging connections
     - Fails fast if database is unavailable
  4. **Server Specification:**
     - Explicit server name prevents connection hijacking
     - Local server for development security

**Why it's important:** Secure database connections prevent unauthorized access and data interception. Windows Authentication is more secure than SQL authentication as it doesn't require password storage. Encrypted connections protect data in transit. Connection timeouts prevent resource exhaustion attacks.

---

## Summary

The SoftWear system implements comprehensive security through multiple layers:

### Authentication & Access Control
- **SHA-256 Password Hashing:** Passwords never stored in plain text
- **Session Management:** Secure in-memory session state
- **Role-Based Access Control:** Multi-layer role enforcement
- **Account Status Control:** Active/archived account management
- **Forced Password Changes:** Security policy enforcement

### Data Protection
- **Data Isolation:** Complete multi-tenant data separation
- **Ownership Verification:** Database-level access control
- **SQL Injection Prevention:** Parameterized queries throughout
- **Transaction Security:** Atomic operations prevent inconsistencies

### Audit & Monitoring
- **Comprehensive Audit Logging:** Complete activity tracking
- **Email Uniqueness:** Prevents account conflicts
- **Input Validation:** Prevents malformed data and attacks

### Security Best Practices
- **Secure Connections:** Windows Authentication and encryption
- **Error Handling:** Prevents information leakage
- **Defense in Depth:** Multiple security layers
- **Least Privilege:** Users only access their own data

**Key Security Principles:**
1. **Never trust user input** - All input is validated and sanitized
2. **Defense in depth** - Multiple security layers (UI, service, database)
3. **Least privilege** - Users only access what they need
4. **Complete audit trails** - All actions are logged
5. **Data isolation** - Complete separation between businesses
6. **Secure by default** - New accounts require password changes

These security features work together to create a robust, secure system that protects user data, prevents unauthorized access, and maintains complete accountability.


