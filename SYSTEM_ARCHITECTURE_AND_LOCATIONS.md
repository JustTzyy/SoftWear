# SOFTWEAR System Architecture & File Locations Guide
## Complete System Information for Defense

---

## 📁 PROJECT STRUCTURE OVERVIEW

```
IT13_Final/
├── Components/          # Frontend UI Components
├── Services/            # Backend Business Logic
├── Migrations/          # Database Scripts
├── Platforms/           # Platform-Specific Code
├── Resources/           # Images, Fonts, Icons
├── wwwroot/             # Static Web Assets
└── MauiProgram.cs       # Application Entry Point
```

---

## 🔐 SECURITY IMPLEMENTATION LOCATIONS

### 1. Authentication & Password Security

**Location**: `IT13_Final/Services/Auth/AuthService.cs`

**Key Security Features**:
- **Password Hashing**: SHA-256 algorithm
- **Encoding**: Unicode (UTF-16LE) to match SQL Server HASHBYTES
- **Implementation**:
  ```csharp
  string passwordHex = Convert.ToHexString(
      SHA256.HashData(Encoding.Unicode.GetBytes(password))
  );
  ```
- **Connection String**: Line 20-21
  ```csharp
  "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=30;"
  ```

**What It Does**:
- Validates user credentials
- Compares hashed passwords (never stores plain text)
- Returns authenticated user with role information
- Handles connection errors securely

---

### 2. Session Management

**Location**: `IT13_Final/Services/Auth/AuthState.cs`

**Purpose**: Manages user session state throughout the application

**Key Features**:
- Stores authenticated user information
- Provides role-based access control
- Persists across page navigation
- Used for conditional UI rendering

**Usage**: Injected into all pages via `@inject AuthState AuthState`

---

### 3. Role-Based Access Control (RBAC)

**Implementation Locations**:
- **Route Protection**: Each page has `@page "/role/pagename"` directive
- **UI Conditional Rendering**: `@if (AuthState.CurrentUser?.Role == "admin")`
- **Service Validation**: All service methods check user permissions

**Example Locations**:
- `Components/Pages/Admin/*.razor` - Admin-only pages
- `Components/Pages/Seller/*.razor` - Seller-only pages
- `Components/Pages/Accounting/*.razor` - Accounting-only pages
- `Components/Pages/StockClerk/*.razor` - Stock Clerk-only pages
- `Components/Pages/Cashier/*.razor` - Cashier-only pages

---

### 4. SQL Injection Prevention

**Location**: All service files in `IT13_Final/Services/Data/*.cs`

**Implementation**: Parameterized queries throughout

**Example**:
```csharp
cmd.Parameters.AddWithValue("@email", email.Trim());
cmd.Parameters.AddWithValue("@userId", userId);
```

**Files Using Parameterized Queries**:
- `AuthService.cs` - Line 45
- `UserService.cs` - All database operations
- `ProductService.cs` - All CRUD operations
- `SalesService.cs` - All transaction operations
- All other service files

---

### 5. Audit Logging (Security Tracking)

**Location**: `IT13_Final/Services/Audit/HistoryService.cs`

**Purpose**: Tracks all user actions for security and compliance

**Key Features**:
- Logs every significant user action
- Stores: user_id, status, module, description, timestamp
- Cannot be deleted or modified by users
- Used for compliance and forensic analysis

**Connection String**: Line 29-30
```csharp
"Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;"
```

**Usage**: Called automatically throughout the application:
```csharp
await HistoryService.LogAsync(userId, "create", "Products", "Created product: Product Name");
```

---

## ☁️ AZURE CONFIGURATION LOCATIONS

### 1. Azure Database Connection

**Location**: `IT13_Final/Services/Data/DatabaseSyncService.cs`

**Azure Connection String**: Line 58-59
```csharp
private readonly string _azureConnectionString =
    "Server=tcp:jusstzy.database.windows.net,1433;Initial Catalog=db_SoftWear;Persist Security Info=False;User ID=justin;Password=JussPogi27;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;";
```

**Azure Details**:
- **Server**: `jusstzy.database.windows.net`
- **Database**: `db_SoftWear`
- **Port**: `1433`
- **Username**: `justin`
- **Password**: `JussPogi27`
- **Encryption**: Enabled (SSL/TLS)

---

### 2. Database Synchronization Service

**Location**: `IT13_Final/Services/Data/DatabaseSyncService.cs`

**Purpose**: Synchronizes data between local and Azure databases

**Key Methods**:
- `SyncAllTablesAsync()` - Push local → Azure
- `PullAllTablesAsync()` - Pull Azure → Local
- `TestLocalConnectionAsync()` - Test local DB connection
- `TestAzureConnectionAsync()` - Test Azure DB connection

**Tables Synced** (Line 62-91):
1. tbl_roles
2. tbl_users
3. tbl_histories
4. tbl_addresses
5. tbl_colors
6. tbl_sizes
7. tbl_categories
8. tbl_products
9. tbl_variants
10. tbl_variant_sizes
11. tbl_variant_colors
12. tbl_suppliers
13. tbl_inventories
14. tbl_stock_in
15. tbl_stock_out
16. tbl_stock_adjustments
17. tbl_purchase_orders
18. tbl_po_items
19. tbl_sales
20. tbl_sales_items
21. tbl_payments
22. tbl_returns
23. tbl_return_items
24. tbl_daily_sales_verifications
25. tbl_expenses
26. tbl_supplier_invoices
27. tbl_supplier_payments

---

### 3. Automatic Background Sync

**Location**: `IT13_Final/Services/Data/AutoSyncBackgroundService.cs`

**Registration**: `IT13_Final/MauiProgram.cs` (Line 47-54)

**Purpose**: Automatically syncs local database to Azure every hour

**How It Works**:
1. Service starts when application launches
2. Runs sync operation every 60 minutes
3. Syncs all tables from local to Azure
4. Logs sync results

---

### 4. Azure Database Setup Scripts

**Location**: `IT13_Final/Migrations/`

**Key Files**:
- `Azure_SQL_Setup.sql` - Complete Azure database setup script
- `Setup_Azure_SQL.ps1` - PowerShell automation script
- `Sync_Data_To_Azure.ps1` - Data migration script
- `AZURE_SETUP_README.md` - Setup instructions
- `FIX_AZURE_CONNECTION.md` - Troubleshooting guide

---

## 🗄️ DATABASE CONFIGURATION

### Local Database Connection

**Location**: All service files in `IT13_Final/Services/Data/*.cs`

**Connection String Format**:
```csharp
"Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=30;"
```

**Files Using Local Connection**:
- `AuthService.cs` - Line 20-21
- `HistoryService.cs` - Line 29-30
- `UserService.cs` - Line 170
- `ProductService.cs` - Line 65
- `InventoryService.cs` - Line 65
- `SalesService.cs` - Line 135
- `PurchaseOrderService.cs` - Line 96
- `StockInService.cs` - Line 56
- `StockOutService.cs` - Line 56
- `StockAdjustmentService.cs` - Line 63
- `SupplierService.cs` - Line 65
- `ExpenseService.cs` - Line 60
- `SupplierInvoiceService.cs` - Line 77
- `DailySalesVerificationService.cs` - Line 69
- `IncomeBreakdownService.cs` - Line 48
- `CashflowAuditService.cs` - Line 48
- All other data services

---

### Database Migration Scripts

**Location**: `IT13_Final/Migrations/`

**Key Migration Files**:
1. `001_Create_tbl_users.sql` - User accounts table
2. `002_Create_tbl_roles.sql` - User roles table
3. `003_Create_tbl_histories.sql` - Audit log table
4. `004_Create_tbl_addresses.sql` - Address table
5. `007_Create_tbl_colors.sql` - Color options
6. `008_Create_tbl_sizes.sql` - Size options
7. `009_Create_tbl_categories.sql` - Product categories
8. `010_Create_tbl_products.sql` - Products table
9. `011_Create_tbl_variants.sql` - Product variants
10. `016_Create_tbl_suppliers.sql` - Suppliers table
11. `018_Create_tbl_inventories.sql` - Inventory levels
12. `019_Create_tbl_stock_in.sql` - Stock-in transactions
13. `020_Create_tbl_stock_out.sql` - Stock-out transactions
14. `024_Create_tbl_purchase_orders.sql` - Purchase orders
15. `025_Create_tbl_po_items.sql` - PO line items
16. `033_Create_tbl_sales.sql` - Sales transactions
17. `034_Create_tbl_sales_items.sql` - Sales line items
18. `035_Create_tbl_payments.sql` - Payment records
19. `036_Create_tbl_returns.sql` - Return transactions
20. `037_Create_tbl_return_items.sql` - Return line items
21. `039_Create_tbl_daily_sales_verifications.sql` - Daily sales verification
22. `040_Create_tbl_expenses.sql` - Expense records
23. `041_Create_tbl_supplier_invoices_and_payments.sql` - Supplier invoices/payments

**Trigger Files**:
- `021_Create_triggers_update_inventory.sql` - Auto-update inventory on stock operations

**Seed Data Files**:
- `014_Seed_Data_Roles_And_Users.sql` - Default roles and users
- `046_Seed_Dummy_Users.sql` - Test users
- `047_Reset_Database_And_Seed_Users.sql` - Reset and seed script

**Complete Setup**:
- `Azure_SQL_Setup.sql` - Complete Azure database setup
- `Run_All_Migrations_SSMS.sql` - Run all migrations in order

---

## 🎨 FRONTEND COMPONENTS LOCATIONS

### Layout Components

**Location**: `IT13_Final/Components/Layout/`

**Files**:
- `Admin/Admin.razor` - Admin layout with sidebar navigation
- `Seller/Seller.razor` - Seller layout with sidebar navigation
- `Accounting/Accounting.razor` - Accounting layout
- `StockClerk/StockClerk.razor` - Stock Clerk layout
- `Cashier/Cashier.razor` - Cashier layout
- `Auth/Auth.razor` - Authentication layout (login page)
- `MainLayout.razor` - Main application layout
- `NavMenu.razor` - Navigation menu component

---

### Page Components by Role

#### Admin Pages
**Location**: `IT13_Final/Components/Pages/Admin/`

**Files**:
- `Dashboard.razor` - Admin dashboard with system-wide KPIs
- `Admin_Record.razor` - Manage admin accounts
- `Seller_Record.razor` - Manage seller accounts
- `AuditLogs.razor` - System-wide audit logs
- `ActivityLog.razor` - Admin activity log
- `Database_Sync.razor` - Database synchronization interface
- `Permission_Requests.razor` - Seller permission requests
- `SellerFees.razor` - Platform fee management
- `FeeSummary.razor` - Fee summary reports
- `MonthlyCollection.razor` - Monthly collection reports
- `Settings.razor` - System settings
- `Archive_Admin_Record.razor` - Archived admin accounts
- `Archive_Seller_Record.razor` - Archived seller accounts

#### Seller Pages
**Location**: `IT13_Final/Components/Pages/Seller/`

**Key Files**:
- `Dashboard.razor` - Business dashboard
- **Product Management**:
  - `Product_Record.razor` - Product CRUD
  - `Category_Record.razor` - Category management
  - `Color_Record.razor` - Color management
  - `Size_Record.razor` - Size management
  - `Variant_Record.razor` - Variant management
- **Staff Management**:
  - `Cashier_Record.razor` - Cashier accounts
  - `Stock_Clerk_Record.razor` - Stock Clerk accounts
  - `Accounting_Record.razor` - Accounting accounts
- **Reports**:
  - `AccountingIncomeBreakdown.razor` - Income breakdown
  - `AccountingExpenseReports.razor` - Expense reports
  - `AccountingFinancialReports.razor` - Financial reports
  - `AccountingProfitLossReports.razor` - P&L reports
  - `AccountingTaxReports.razor` - Tax reports
  - `AccountingDailySalesVerificationReports.razor` - Daily sales verification
  - `AccountingCashInOutAudit.razor` - Cash flow audit
  - `CashierSalesReports.razor` - Cashier sales reports
  - `CashierReturnsReports.razor` - Cashier returns reports
  - `StockClerkPurchaseOrderReports.razor` - PO reports
  - `StockClerkStockInReports.razor` - Stock-in reports
  - `StockClerkStockOutReports.razor` - Stock-out reports
  - `StockClerkStockAdjustmentReports.razor` - Stock adjustment reports
- **Other**:
  - `AuditLogs.razor` - Staff activity logs
  - `ActivityLog.razor` - Seller activity log
  - `Settings.razor` - User settings

#### Accounting Pages
**Location**: `IT13_Final/Components/Pages/Accounting/`

**Files**:
- `Dashboard.razor` - Financial dashboard
- `Income.razor` - Income management
- `Expenses.razor` - Expense management
- `SupplierPayments.razor` - Supplier payment processing
- `PettyCash.razor` - Petty cash management
- `PurchaseOrderApprovals.razor` - PO approval workflow
- `RefundReturnApprovals.razor` - Refund approval workflow
- `DailySalesVerification.razor` - Daily sales verification
- `ProfitLoss.razor` - Profit & Loss reports
- `TaxReports.razor` - Tax reports
- `SalesReports.razor` - Sales reports
- `ExpenseReports.razor` - Expense reports
- `CashflowAudit.razor` - Cash flow audit
- `ActivityLog.razor` - Activity log
- `Settings.razor` - Settings

#### Stock Clerk Pages
**Location**: `IT13_Final/Components/Pages/StockClerk/`

**Files**:
- `Dashboard.razor` - Inventory dashboard
- `Inventory_Record.razor` - Inventory management
- `Purchase_Orders.razor` - Purchase order management
- `Stock_In_Record.razor` - Stock-in transactions
- `Stock_Out_Record.razor` - Stock-out transactions
- `Stock_Adjustment.razor` - Stock adjustments
- `Supplier_Record.razor` - Supplier management
- `Stock_In_Report.razor` - Stock-in reports
- `Stock_Out_Report.razor` - Stock-out reports
- `ActivityLog.razor` - Activity log
- `Settings.razor` - Settings

#### Cashier Pages
**Location**: `IT13_Final/Components/Pages/Cashier/`

**Files**:
- `Dashboard.razor` - Sales dashboard
- `Sales.razor` - Point of Sale (POS) system
- `Returns.razor` - Return processing
- `Sales_Report.razor` - Sales reports
- `Returns_Report.razor` - Returns reports
- `ActivityLog.razor` - Activity log
- `Settings.razor` - Settings

#### Authentication Pages
**Location**: `IT13_Final/Components/Pages/Auth/`

**Files**:
- `Login.razor` - User login page

---

## 🔧 BACKEND SERVICES LOCATIONS

### Service Layer Structure

**Location**: `IT13_Final/Services/`

**Organization**:
```
Services/
├── Auth/              # Authentication services
├── Audit/             # Audit logging services
└── Data/              # Business logic services
```

---

### Authentication Services

**Location**: `IT13_Final/Services/Auth/`

**Files**:
- `AuthService.cs` - User authentication and password validation
- `AuthState.cs` - Session state management

---

### Audit Services

**Location**: `IT13_Final/Services/Audit/`

**Files**:
- `HistoryService.cs` - Activity logging and audit trail

---

### Data Services

**Location**: `IT13_Final/Services/Data/`

**Files**:
- `UserService.cs` - User account management
- `ProductService.cs` - Product CRUD operations
- `CategoryService.cs` - Category management
- `ColorService.cs` - Color management
- `SizeService.cs` - Size management
- `VariantService.cs` - Variant management
- `InventoryService.cs` - Inventory management
- `SupplierService.cs` - Supplier management
- `PurchaseOrderService.cs` - Purchase order operations
- `StockInService.cs` - Stock-in transactions
- `StockOutService.cs` - Stock-out transactions
- `StockAdjustmentService.cs` - Stock adjustments
- `SalesService.cs` - Sales transactions
- `ReturnsService.cs` - Return processing
- `ExpenseService.cs` - Expense management
- `SupplierInvoiceService.cs` - Supplier invoice/payment management
- `DailySalesVerificationService.cs` - Daily sales verification
- `IncomeBreakdownService.cs` - Income calculations
- `CashflowAuditService.cs` - Cash flow tracking
- `DatabaseSyncService.cs` - Database synchronization
- `AutoSyncBackgroundService.cs` - Automatic background sync
- `AddressService.cs` - Address management
- `PhilippineAddressService.cs` - Philippine address data

---

## 📦 APPLICATION CONFIGURATION

### Application Entry Point

**Location**: `IT13_Final/MauiProgram.cs`

**Purpose**: Configures dependency injection and service registration

**Key Services Registered** (Line 21-45):
- Authentication services
- Audit services
- All data services
- Auto-sync background service

---

### Project Configuration

**Location**: `IT13_Final/IT13_Final.csproj`

**Key Information**:
- **Target Framework**: .NET 10.0
- **Platforms**: Android, iOS, MacCatalyst, Windows
- **Package References**:
  - Microsoft.Maui.Controls
  - Microsoft.AspNetCore.Components.WebView.Maui
  - Microsoft.Data.SqlClient (Version 5.2.2)

---

### Static Assets

**Location**: `IT13_Final/wwwroot/`

**Contents**:
- `index.html` - Main HTML file
- `app.css` - Global styles
- `images/` - Image assets
  - `bg-image-login.jpg` - Login background
  - `LOGO.png` - Application logo
- `lib/bootstrap/` - Bootstrap CSS framework

---

## 🗂️ DATABASE TRIGGERS LOCATIONS

### Inventory Update Triggers

**Location**: `IT13_Final/Migrations/021_Create_triggers_update_inventory.sql`

**Triggers**:
1. **TR_tbl_stock_in_UpdateInventory** - Updates inventory on stock-in
2. **TR_tbl_stock_out_UpdateInventory** - Updates inventory on stock-out
3. **TR_tbl_stock_adjustments_UpdateInventory** - Updates inventory on adjustment

**How They Work**:
- Fire automatically after INSERT on respective tables
- Calculate new inventory quantity
- Update `tbl_inventories` table
- Handle new inventory items if they don't exist

---

## 📊 DATA MODELS

**Location**: Defined inline in service files (no separate Models folder)

**Model Definitions**: Each service file contains its own model classes

**Example Locations**:
- `UserService.cs` - UserModel, CreateUserModel, etc.
- `ProductService.cs` - ProductModel, CreateProductModel, etc.
- `SalesService.cs` - SalesModel, CreateSalesModel, etc.

---

## 🔄 ROUTING CONFIGURATION

**Location**: `IT13_Final/Components/Routes.razor`

**Purpose**: Defines application routing

**Route Patterns**:
- `/` - Home page
- `/login` - Login page
- `/{role}/dashboard` - Role-specific dashboards
- `/{role}/{module}` - Role-specific modules

---

## 📝 INTERFACE GUIDES

**Location**: `IT13_Final/Components/Pages/{Role}/INTERFACE_GUIDE.md`

**Files**:
- `Seller/INTERFACE_GUIDE.md` - Seller interface documentation
- `StockClerk/INTERFACE_GUIDE.md` - Stock Clerk interface documentation
- `Cashier/INTERFACE_GUIDE.md` - Cashier interface documentation
- `Accounting/INTERFACE_GUIDE.md` - Accounting interface documentation

---

## 🎯 KEY FEATURES BY LOCATION

### 1. Password Security
- **Hashing**: `Services/Auth/AuthService.cs` (Line 30)
- **Password Update**: `Services/Data/UserService.cs` (Line 326-372)

### 2. Role-Based Access
- **Route Protection**: Each page file has `@page` directive
- **UI Conditional**: `@if (AuthState.CurrentUser?.Role == "role")`
- **Service Validation**: All service methods

### 3. Audit Logging
- **Service**: `Services/Audit/HistoryService.cs`
- **Database Table**: `tbl_histories` (created by `003_Create_tbl_histories.sql`)
- **Usage**: Called throughout application

### 4. Database Sync
- **Service**: `Services/Data/DatabaseSyncService.cs`
- **UI**: `Components/Pages/Admin/Database_Sync.razor`
- **Background**: `Services/Data/AutoSyncBackgroundService.cs`

### 5. Inventory Management
- **Service**: `Services/Data/InventoryService.cs`
- **Triggers**: `Migrations/021_Create_triggers_update_inventory.sql`
- **UI**: `Components/Pages/StockClerk/Inventory_Record.razor`

### 6. Sales Processing
- **Service**: `Services/Data/SalesService.cs`
- **UI**: `Components/Pages/Cashier/Sales.razor`
- **Database**: `tbl_sales`, `tbl_sales_items` tables

### 7. Financial Management
- **Income**: `Services/Data/IncomeBreakdownService.cs`
- **Expenses**: `Services/Data/ExpenseService.cs`
- **Supplier Payments**: `Services/Data/SupplierInvoiceService.cs`
- **UI**: `Components/Pages/Accounting/`

---

## 🔍 QUICK REFERENCE: WHERE TO FIND THINGS

### Security
- **Password Hashing**: `Services/Auth/AuthService.cs:30`
- **Session Management**: `Services/Auth/AuthState.cs`
- **Audit Logging**: `Services/Audit/HistoryService.cs`
- **SQL Injection Prevention**: All service files (parameterized queries)

### Azure
- **Connection String**: `Services/Data/DatabaseSyncService.cs:58-59`
- **Sync Service**: `Services/Data/DatabaseSyncService.cs`
- **Background Sync**: `Services/Data/AutoSyncBackgroundService.cs`
- **Setup Scripts**: `Migrations/Azure_SQL_Setup.sql`

### Database
- **Local Connection**: All service files (line ~65-170)
- **Migrations**: `Migrations/*.sql`
- **Triggers**: `Migrations/021_Create_triggers_update_inventory.sql`

### Frontend
- **Pages**: `Components/Pages/{Role}/`
- **Layouts**: `Components/Layout/{Role}/`
- **Styles**: `Components/Pages/{Role}/*.razor.css`

### Backend
- **Services**: `Services/Data/*.cs`
- **Authentication**: `Services/Auth/*.cs`
- **Audit**: `Services/Audit/*.cs`

### Configuration
- **Dependency Injection**: `MauiProgram.cs`
- **Project Settings**: `IT13_Final.csproj`
- **Static Assets**: `wwwroot/`

---

## 📋 IMPORTANT CONNECTION STRINGS

### Local Database
```
Server=localhost\SQLEXPRESS;Database=db_SoftWear;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=30;
```

### Azure Database
```
Server=tcp:jusstzy.database.windows.net,1433;Initial Catalog=db_SoftWear;Persist Security Info=False;User ID=justin;Password=JussPogi27;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;
```

---

## 🎓 DEFENSE TALKING POINTS

### When Asked About Security:
1. **Password Security**: "We use SHA-256 hashing with Unicode encoding, implemented in `Services/Auth/AuthService.cs`"
2. **SQL Injection**: "All queries use parameterized statements, preventing SQL injection attacks"
3. **Access Control**: "Role-based access control is implemented at both route and service levels"
4. **Audit Trail**: "Every significant action is logged in `tbl_histories` via `HistoryService.cs`"

### When Asked About Azure:
1. **Connection**: "Azure connection is configured in `DatabaseSyncService.cs` with encrypted SSL/TLS"
2. **Sync**: "Automatic background sync runs every hour via `AutoSyncBackgroundService.cs`"
3. **Setup**: "Database setup scripts are in `Migrations/Azure_SQL_Setup.sql`"

### When Asked About Architecture:
1. **Frontend**: "Blazor components in `Components/Pages/` organized by user role"
2. **Backend**: "Service-oriented architecture in `Services/Data/` with dependency injection"
3. **Database**: "SQL Server with triggers for automatic inventory updates"

### When Asked About Features:
1. **Inventory**: "Automatic updates via triggers in `Migrations/021_Create_triggers_update_inventory.sql`"
2. **Sales**: "POS system in `Components/Pages/Cashier/Sales.razor` with service in `SalesService.cs`"
3. **Reports**: "Comprehensive reporting in `Components/Pages/{Role}/` with filtering and export"

---

## ✅ CHECKLIST FOR DEFENSE

- [ ] Know location of security implementation
- [ ] Know Azure connection string location
- [ ] Understand database structure (check Migrations folder)
- [ ] Know where each role's pages are located
- [ ] Understand service layer architecture
- [ ] Know how audit logging works
- [ ] Understand database synchronization
- [ ] Know where triggers are defined
- [ ] Understand dependency injection setup
- [ ] Know project structure and organization

---

**Last Updated**: [Current Date]
**System Version**: 1.0
**Prepared for**: Defense Presentation



