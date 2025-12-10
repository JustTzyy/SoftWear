# SOFTWEAR - Retail Management System
## Defense Documentation

---

## 1. TOOLS USED

### IDE (Integrated Development Environment)
- **Visual Studio 2022** / **Visual Studio Code** - Primary development environment for .NET MAUI Blazor application
- **SQL Server Management Studio (SSMS)** - Database management and query execution tool

### Development Tools
- **Git** - Version control system
- **PowerShell** - Scripting for database migrations and Azure deployment
- **Azure Portal** - Cloud database management and deployment

---

## 2. TECHNOLOGY/FRAMEWORKS

### Frontend Framework
- **.NET MAUI (Multi-platform App UI)** - Cross-platform framework for building native applications
- **Blazor** - Web UI framework using C# and Razor syntax
- **Razor Components** - Component-based UI architecture
- **CSS3** - Styling and responsive design
- **JavaScript (JSInterop)** - Client-side interactions and PDF generation

### Backend Framework
- **ASP.NET Core** - Web application framework
- **C# (.NET 10.0)** - Primary programming language
- **Microsoft.Data.SqlClient** - SQL Server database connectivity

### Database
- **Microsoft SQL Server** - Local database (SQL Server Express)
- **Azure SQL Database** - Cloud-hosted database for production
- **SQL Server Management Studio (SSMS)** - Database administration

### Database Synchronization
- **DatabaseSyncService** - Custom service for syncing local and Azure databases
- **Background Service** - Automatic data synchronization between local and cloud databases

### Security
- **SHA-256** - Password hashing algorithm
- **Role-Based Access Control (RBAC)** - User permission management
- **Session Management** - User authentication state management

### Additional Technologies
- **Entity Framework Core** (implicit through data access patterns)
- **Dependency Injection** - Service registration and management
- **Async/Await Pattern** - Asynchronous programming for better performance

---

## 3. SYSTEM USERS

The system supports **5 distinct user roles**, each with specific responsibilities and access levels:

1. **System Administrator (Admin)**
   - Primary Role: Full system management and oversight
   - Responsibilities:
     - Manage all sellers and their accounts
     - Monitor system-wide activity logs
     - Configure system settings and fees
     - Approve seller permission requests
     - Database synchronization management
     - View comprehensive system reports

2. **Seller (Business Owner)**
   - Primary Role: Business management and oversight
   - Responsibilities:
     - Manage products, categories, colors, sizes, and variants
     - Manage supplier information
     - Manage staff (Cashiers, Stock Clerks, Accounting personnel)
     - View business reports (sales, expenses, profit/loss)
     - Monitor staff activity logs
     - Approve staff actions when required

3. **Accounting Personnel**
   - Primary Role: Financial management and reconciliation
   - Responsibilities:
     - Manage income and expense records
     - Process supplier payments
     - Approve purchase orders from stock clerks
     - Approve refund/return requests from cashiers
     - Verify daily sales transactions
     - Generate financial reports (profit/loss, tax reports, cash flow)
     - Manage petty cash transactions

4. **Stock Clerk**
   - Primary Role: Inventory management and procurement
   - Responsibilities:
     - Manage inventory levels
     - Create and manage purchase orders
     - Record stock-in transactions
     - Record stock-out transactions
     - Perform stock adjustments
     - Manage supplier records
     - Generate inventory reports

5. **Cashier**
   - Primary Role: Point-of-sale operations and customer service
   - Responsibilities:
     - Process sales transactions (POS system)
     - Handle product returns and refunds
     - Generate sales receipts
     - Submit daily sales for verification
     - View personal sales and return reports

---

## 4. SYSTEM MODULES

### Authentication & Security Module
- **Login Module** - User authentication with email and password
- **Password Management** - Secure password hashing (SHA-256), password change functionality
- **Session Management** - User session tracking and state management
- **Role-Based Access Control** - Permission-based navigation and feature access

### Dashboard Module
- **Role-Specific Dashboards** - Customized dashboards for each user role
- **KPI Display** - Key Performance Indicators (sales, expenses, inventory, etc.)
- **Quick Actions** - Fast access to common tasks
- **Data Visualization** - Charts and graphs for business metrics

### Product Management Module
- **Product Record** - Create, view, edit, and archive products
- **Category Management** - Organize products by categories
- **Color Management** - Manage product color options with hex codes
- **Size Management** - Manage product size options
- **Variant Management** - Create product variants (product + color + size combinations)
- **Product Images** - Upload and manage product images

### Inventory Management Module
- **Inventory Record** - View all inventory items with stock levels
- **Stock Level Tracking** - Real-time inventory quantity monitoring
- **Reorder Point Management** - Set minimum stock thresholds
- **Low Stock Alerts** - Automatic notifications for low inventory

### Purchase Order Module
- **Purchase Order Creation** - Create POs for supplier procurement
- **PO Management** - View, edit, and track purchase orders
- **PO Approval Workflow** - Accounting approval process
- **PO Status Tracking** - Pending, approved, completed, cancelled statuses
- **PO Items Management** - Add products and quantities to purchase orders

### Stock Operations Module
- **Stock-In Record** - Record products added to inventory
- **Stock-Out Record** - Record products removed from inventory
- **Stock Adjustment** - Correct inventory discrepancies
- **Stock Movement Tracking** - Complete audit trail of inventory changes

### Supplier Management Module
- **Supplier Record** - Manage supplier information and contact details
- **Supplier Payment Tracking** - Track payments to suppliers
- **Supplier Invoice Management** - Manage supplier invoices and balances

### Sales Module
- **Point of Sale (POS)** - Cashier interface for processing sales
- **Sales Transaction Processing** - Real-time sales recording
- **Receipt Generation** - Print/display sales receipts
- **Sales History** - View all completed sales transactions

### Returns & Refunds Module
- **Return Processing** - Handle product returns
- **Refund Management** - Process refund requests
- **Refund Approval Workflow** - Accounting approval for refunds
- **Return History** - Track all return transactions

### Accounting Module
- **Income Management** - Track and categorize business income
- **Expense Management** - Record and categorize business expenses
- **Supplier Payments** - Process and track supplier payment transactions
- **Petty Cash Management** - Track small cash expenditures
- **Financial Reconciliation** - Balance and verify financial records

### Approval Module
- **Purchase Order Approvals** - Accounting review and approval of POs
- **Refund/Return Approvals** - Accounting approval for refund requests
- **Daily Sales Verification** - Verify cashier-submitted daily sales

### Reports Module
- **Sales Reports** - Comprehensive sales transaction reports
- **Expense Reports** - Detailed expense analysis
- **Profit & Loss Reports** - Business profitability statements
- **Tax Reports** - VAT calculations and tax obligations
- **Inventory Reports** - Stock movement and inventory analysis
- **Purchase Order Reports** - PO history and analysis
- **Stock-In Reports** - Stock-in transaction history
- **Stock-Out Reports** - Stock-out transaction history
- **Cash Flow Audit** - Cash inflow and outflow tracking
- **Daily Sales Verification Reports** - Verified sales transaction reports

### Staff Management Module
- **User Management** - Create, view, edit, and archive user accounts
- **Role Assignment** - Assign roles to users
- **Staff Records** - Manage cashier, stock clerk, and accounting staff
- **Permission Management** - Handle permission requests

### Audit Log Module
- **Activity Logging** - Automatic logging of all user actions
- **Audit Trail** - Complete history of system changes
- **Activity Reports** - Filterable activity logs by user, module, date, and status
- **Security Monitoring** - Track login/logout and security events

### Settings Module
- **User Settings** - Personal account settings and password management
- **System Settings** - System-wide configuration (Admin only)
- **Database Sync Settings** - Local and Azure database synchronization

---

## 5. DATABASE

### Database Type
- **Relational Database Management System (RDBMS)**
- **Microsoft SQL Server** (Local Development)
- **Azure SQL Database** (Cloud Production)

### Database Name
- **db_SoftWear**

### Key Database Tables
1. **tbl_roles** - User roles (admin, seller, cashier, accounting, stockclerk)
2. **tbl_users** - User accounts and authentication
3. **tbl_histories** - Audit log and activity tracking
4. **tbl_products** - Product master data
5. **tbl_categories** - Product categories
6. **tbl_colors** - Color options
7. **tbl_sizes** - Size options
8. **tbl_variants** - Product variants
9. **tbl_inventories** - Inventory stock levels
10. **tbl_suppliers** - Supplier information
11. **tbl_purchase_orders** - Purchase order headers
12. **tbl_po_items** - Purchase order line items
13. **tbl_stock_in** - Stock-in transactions
14. **tbl_stock_out** - Stock-out transactions
15. **tbl_stock_adjustments** - Stock adjustment records
16. **tbl_sales** - Sales transaction headers
17. **tbl_sales_items** - Sales transaction line items
18. **tbl_payments** - Payment records
19. **tbl_returns** - Return transaction headers
20. **tbl_return_items** - Return transaction line items
21. **tbl_expenses** - Expense records
22. **tbl_supplier_invoices** - Supplier invoice records
23. **tbl_supplier_payments** - Supplier payment records
24. **tbl_daily_sales_verifications** - Daily sales verification records
25. **tbl_addresses** - Address information

### Database Features
- **Foreign Key Constraints** - Data integrity enforcement
- **Indexes** - Optimized query performance
- **Triggers** - Automatic inventory updates on stock transactions
- **Stored Procedures** (implicit through service layer)
- **Transaction Support** - ACID compliance for data consistency

---

## 6. HOSTING/LINK

### Local Development
- **Database Server**: `localhost\SQLEXPRESS`
- **Database Name**: `db_SoftWear`
- **Connection**: Integrated Security (Windows Authentication)

### Cloud Hosting (Production)
- **Platform**: Microsoft Azure
- **Database Server**: `jusstzy.database.windows.net`
- **Database Name**: `db_SoftWear`
- **Authentication**: SQL Server Authentication
- **Connection String**: Encrypted connection with SSL/TLS

### Database Synchronization
- **Bidirectional Sync** - Data synchronization between local and Azure databases
- **Automatic Sync** - Background service for continuous synchronization
- **Manual Sync** - On-demand synchronization through admin interface

---

## 7. TYPE OF USERS/ROLE-BASED ACCESS

### 1. System Administrator (Admin)
**Primary Role**: Full system management and oversight

**Access Level**: System-wide access

**Key Permissions**:
- ✅ Full access to all system modules
- ✅ Manage all sellers and their accounts
- ✅ View system-wide audit logs
- ✅ Configure system settings and fees
- ✅ Approve seller permission requests
- ✅ Database synchronization management
- ✅ View all business reports across all sellers
- ✅ Manage admin accounts

**Restricted Actions**: None (full administrative privileges)

---

### 2. Seller (Business Owner)
**Primary Role**: Business management and oversight

**Access Level**: Business-specific access (own business only)

**Key Permissions**:
- ✅ Manage own products, categories, colors, sizes, variants
- ✅ Manage own suppliers
- ✅ Manage own staff (Cashiers, Stock Clerks, Accounting)
- ✅ View own business reports (sales, expenses, profit/loss)
- ✅ View staff activity logs (own staff only)
- ✅ Approve staff actions when required
- ✅ View inventory levels
- ✅ Request additional permissions from Admin

**Restricted Actions**:
- ❌ Cannot access other sellers' data
- ❌ Cannot modify system settings
- ❌ Cannot manage other sellers

---

### 3. Accounting Personnel
**Primary Role**: Financial management and reconciliation

**Access Level**: Financial operations for assigned seller

**Key Permissions**:
- ✅ Manage income and expense records
- ✅ Process supplier payments
- ✅ Approve purchase orders from stock clerks
- ✅ Approve refund/return requests from cashiers
- ✅ Verify daily sales transactions
- ✅ Generate financial reports (profit/loss, tax, cash flow)
- ✅ Manage petty cash transactions
- ✅ View financial audit logs

**Restricted Actions**:
- ❌ Cannot manage products or inventory
- ❌ Cannot process sales transactions
- ❌ Cannot create purchase orders
- ❌ Cannot manage staff accounts

---

### 4. Stock Clerk
**Primary Role**: Inventory management and procurement

**Access Level**: Inventory operations for assigned seller

**Key Permissions**:
- ✅ View and manage inventory levels
- ✅ Create and manage purchase orders
- ✅ Record stock-in transactions
- ✅ Record stock-out transactions
- ✅ Perform stock adjustments
- ✅ View supplier records
- ✅ Generate inventory reports
- ✅ View own activity logs

**Restricted Actions**:
- ❌ Cannot approve purchase orders (requires Accounting approval)
- ❌ Cannot process sales transactions
- ❌ Cannot manage financial records
- ❌ Cannot manage products or variants
- ❌ Cannot manage staff accounts

---

### 5. Cashier
**Primary Role**: Point-of-sale operations and customer service

**Access Level**: Sales operations for assigned seller

**Key Permissions**:
- ✅ Process sales transactions (POS system)
- ✅ Handle product returns
- ✅ Process refunds (pending approval)
- ✅ Generate sales receipts
- ✅ Submit daily sales for verification
- ✅ View own sales and return reports
- ✅ View own activity logs

**Restricted Actions**:
- ❌ Cannot approve refunds (requires Accounting approval)
- ❌ Cannot manage inventory
- ❌ Cannot create purchase orders
- ❌ Cannot view financial reports
- ❌ Cannot manage products or staff

---

## 8. PROJECT OBJECTIVES

### Primary Objectives

1. **Streamline Retail Operations**
   - Automate inventory management processes
   - Integrate point-of-sale system with inventory tracking
   - Reduce manual data entry and human errors
   - Improve operational efficiency through digitalization

2. **Enhance Financial Management**
   - Provide real-time financial tracking and reporting
   - Automate accounting processes (income, expenses, supplier payments)
   - Generate comprehensive financial reports (profit/loss, tax, cash flow)
   - Ensure accurate financial reconciliation

3. **Improve Inventory Control**
   - Real-time inventory tracking and stock level monitoring
   - Automated stock movement recording (stock-in, stock-out, adjustments)
   - Low stock alerts and reorder point management
   - Complete audit trail of inventory changes

4. **Ensure Data Security and Compliance**
   - Implement role-based access control for data security
   - Maintain comprehensive audit logs for compliance
   - Secure password hashing and authentication
   - Protect sensitive business and financial data

5. **Support Multi-User Collaboration**
   - Enable multiple users to work simultaneously
   - Implement approval workflows for critical operations
   - Provide role-specific dashboards and interfaces
   - Support staff management and activity monitoring

---

## 9. PROJECT DESCRIPTION

### Project Overview
**SOFTWEAR** is a comprehensive retail management system designed to help clothing and apparel businesses manage their entire operations from inventory to sales to accounting. The system provides a unified platform for managing products, suppliers, staff, sales transactions, and financial records.

### Target Audience
The system is designed for **retail clothing businesses** that need to:
- Manage inventory of clothing items with multiple variants (sizes, colors)
- Process sales transactions at point of sale
- Track financial operations (income, expenses, supplier payments)
- Manage staff with different roles and responsibilities
- Generate comprehensive business reports

### User Requirements and Expectations

**Business Owners (Sellers)** expect:
- Complete control over their business operations
- Real-time visibility into sales, expenses, and inventory
- Ability to manage products and staff efficiently
- Comprehensive reporting for business decision-making

**Accounting Personnel** expect:
- Accurate financial record-keeping
- Streamlined approval processes
- Detailed financial reports and analysis
- Easy supplier payment processing

**Stock Clerks** expect:
- Easy inventory management tools
- Simple purchase order creation and tracking
- Clear stock movement recording interfaces
- Inventory reports for planning

**Cashiers** expect:
- Fast and intuitive point-of-sale system
- Easy return and refund processing
- Clear transaction history
- Simple daily sales submission

**System Administrators** expect:
- Complete system oversight
- User and business management tools
- System-wide monitoring and reporting
- Database management capabilities

### Potential Impact

**On Users:**
- **Increased Efficiency**: Automation reduces manual work and errors
- **Better Decision-Making**: Real-time data and reports enable informed decisions
- **Improved Accuracy**: Automated calculations and validations reduce errors
- **Time Savings**: Streamlined processes save time for all users
- **Better Organization**: Centralized data management improves organization

**On Organizations:**
- **Cost Reduction**: Reduced manual labor and errors lower operational costs
- **Revenue Growth**: Better inventory management prevents stockouts and overstocking
- **Compliance**: Audit logs and financial tracking ensure regulatory compliance
- **Scalability**: System supports business growth and additional users
- **Data-Driven Insights**: Comprehensive reports enable strategic planning

**On Broader Community:**
- **Digital Transformation**: Promotes adoption of digital tools in retail sector
- **Job Creation**: System development and support create technical jobs
- **Knowledge Transfer**: Demonstrates modern software development practices
- **Innovation**: Encourages further innovation in retail technology

### Technology Stack Overview

**Frontend:**
- **.NET MAUI Blazor** - Modern cross-platform UI framework
- **Razor Components** - Component-based architecture for reusable UI elements
- **CSS3** - Responsive and modern styling
- **JavaScript** - Client-side interactions

**Backend:**
- **C# (.NET 10.0)** - Type-safe, object-oriented programming language
- **ASP.NET Core** - Robust web application framework
- **Microsoft.Data.SqlClient** - High-performance database connectivity

**Database:**
- **SQL Server** - Enterprise-grade relational database
- **Azure SQL Database** - Scalable cloud database solution

**Architecture:**
- **Service-Oriented Architecture** - Modular service layer for business logic
- **Dependency Injection** - Loose coupling and testability
- **Repository Pattern** - Data access abstraction
- **Async/Await** - Non-blocking I/O operations

---

## 10. PROTOTYPE (FRONTEND) - SCREENSHOTS AND DESCRIPTION

### Authentication Module

#### Login Screen
**Description**: The login screen provides secure access to the system. Users enter their email and password to authenticate. The system validates credentials against the database using SHA-256 hashing. Upon successful login, users are redirected to their role-specific dashboard.

**Key Features**:
- Email and password input fields
- Password visibility toggle
- Error message display for invalid credentials
- Responsive design for different screen sizes

**User Interaction**: Users enter their registered email and password, then click the "Sign In" button. The system authenticates and redirects based on user role.

---

### Dashboard Modules

#### Admin Dashboard
**Description**: The Admin Dashboard provides a comprehensive overview of the entire system. It displays key metrics including total sellers, active users, system-wide sales, and platform fees. The dashboard includes interactive charts and quick access to major system functions.

**Key Features**:
- System-wide KPI cards (Total Sellers, Active Users, Total Sales, Platform Fees)
- Interactive charts for revenue and user trends
- Quick action buttons for common tasks
- Search and filter capabilities
- Activity log summary

**User Interaction**: Admins can view system statistics, navigate to different modules via sidebar, and access quick actions from the dashboard.

#### Seller Dashboard
**Description**: The Seller Dashboard shows business-specific metrics including total sales, net income, expenses, returns, supplier payments, and inventory statistics. It provides a real-time view of business performance.

**Key Features**:
- Business KPI cards (Total Sales, Net Income, Expenses, Returns)
- Inventory statistics (Total Products, Low Stock Items)
- Financial summary charts
- Quick access to reports and management modules

**User Interaction**: Sellers can monitor their business performance, view key metrics, and quickly navigate to product management, staff management, or reports.

#### Accounting Dashboard
**Description**: The Accounting Dashboard focuses on financial metrics including total income, expenses, profit/loss, cash flow, and pending approvals. It helps accounting personnel monitor financial health.

**Key Features**:
- Financial KPI cards (Total Income, Total Expenses, Profit/Loss, Cash Flow)
- Pending approvals counter
- Financial trend charts
- Quick access to financial modules

**User Interaction**: Accounting personnel can view financial overview, access income/expense management, and review pending approvals.

#### Stock Clerk Dashboard
**Description**: The Stock Clerk Dashboard displays inventory-related metrics including total products, low stock alerts, purchase orders status, and stock movement summaries.

**Key Features**:
- Inventory KPI cards (Total Products, Low Stock Items, Pending POs)
- Stock movement summaries
- Quick access to inventory and PO modules

**User Interaction**: Stock clerks can monitor inventory levels, view alerts, and quickly access stock operations.

#### Cashier Dashboard
**Description**: The Cashier Dashboard shows sales-related metrics including today's sales, total transactions, returns processed, and daily sales verification status.

**Key Features**:
- Sales KPI cards (Today's Sales, Total Transactions, Returns)
- Daily sales summary
- Quick access to POS and returns modules

**User Interaction**: Cashiers can view their sales performance and quickly access the POS system or returns module.

---

### Product Management Module

#### Product Record Screen
**Description**: This screen allows sellers to create, view, edit, and archive products. Products are the base items in the inventory (e.g., "Slim Fit Jeans", "Classic Cotton T-Shirt").

**Key Features**:
- Product list with search and filter
- Add/Edit product form (name, category, description, images)
- Archive/restore functionality
- Product image upload and preview

**User Interaction**: Sellers can add new products, edit existing ones, upload product images, and archive products that are no longer available.

#### Variant Record Screen
**Description**: This screen allows creating product variants by combining products with colors and sizes. Variants represent specific combinations (e.g., "Slim Fit Jeans - Blue - Size 32").

**Key Features**:
- Variant list with product, color, and size information
- Add/Edit variant form
- Cost price and selling price management
- Variant-specific inventory tracking

**User Interaction**: Sellers select a product, color, and size combination, then set pricing information to create a variant.

---

### Inventory Management Module

#### Inventory Record Screen
**Description**: This screen displays all inventory items with their current stock levels, reorder points, and variant information. It helps track inventory across all products.

**Key Features**:
- Complete inventory list with search and filter
- Stock level indicators (In Stock, Low Stock, Out of Stock)
- Reorder point management
- Quick access to stock operations

**User Interaction**: Users can view inventory levels, identify low stock items, and navigate to stock-in/out operations.

---

### Purchase Order Module

#### Purchase Orders Screen
**Description**: This screen allows stock clerks to create, view, and manage purchase orders for procuring inventory from suppliers.

**Key Features**:
- PO list with status indicators (Pending, Approved, Completed, Cancelled)
- Create PO form with supplier selection
- Add items to PO with quantities
- Submit for approval functionality
- PO details view

**User Interaction**: Stock clerks create POs by selecting a supplier, adding products and quantities, then submitting for accounting approval.

---

### Stock Operations Module

#### Stock-In Record Screen
**Description**: This screen allows stock clerks to record products added to inventory, typically when receiving goods from suppliers or purchase orders.

**Key Features**:
- Stock-in transaction form
- Product variant selection
- Quantity and cost price input
- Supplier and date tracking
- Automatic inventory update

**User Interaction**: Stock clerks select a product variant, enter quantity and cost price, select supplier, and record the stock-in transaction.

#### Stock-Out Record Screen
**Description**: This screen allows recording products removed from inventory, such as items sold, damaged, or returned to suppliers.

**Key Features**:
- Stock-out transaction form
- Product variant selection
- Quantity input
- Reason tracking
- Automatic inventory deduction

**User Interaction**: Stock clerks select a product variant, enter quantity and reason, then record the stock-out transaction.

---

### Sales Module

#### Point of Sale (POS) Screen
**Description**: This is the cashier's primary interface for processing sales transactions. It features a product selection area and a transaction cart.

**Key Features**:
- Product grid with search and category filters
- Shopping cart for selected items
- Quantity adjustment
- Total calculation
- Receipt generation
- Payment processing

**User Interaction**: Cashiers search/select products, add to cart, adjust quantities, and process payment to complete a sale.

---

### Returns Module

#### Returns Screen
**Description**: This screen allows cashiers to process product returns and initiate refund requests that require accounting approval.

**Key Features**:
- Return transaction form
- Sales transaction lookup
- Item selection for return
- Refund amount calculation
- Submit for approval functionality

**User Interaction**: Cashiers look up the original sale, select items to return, and submit refund request for accounting approval.

---

### Accounting Module

#### Income Screen
**Description**: This screen displays income breakdowns including gross sales, returns, and net income calculations.

**Key Features**:
- Income summary cards
- Income breakdown by category
- Date range filtering
- Export functionality

**User Interaction**: Accounting personnel can view income summaries, filter by date range, and analyze income trends.

#### Expenses Screen
**Description**: This screen allows creating, viewing, and managing business expenses including operating costs.

**Key Features**:
- Expense list with search and filter
- Add/Edit expense form
- Expense categories
- Amount and date tracking
- Archive functionality

**User Interaction**: Accounting personnel can add expenses, categorize them, and track spending over time.

#### Supplier Payments Screen
**Description**: This screen allows processing and tracking supplier payment transactions for purchase orders and stock-in records.

**Key Features**:
- Supplier invoice list
- Payment recording form
- Payment method selection (Cash, GCash, Bank)
- Payment history tracking
- Balance tracking

**User Interaction**: Accounting personnel view supplier invoices, record payments with method and reference number, and track payment history.

---

### Reports Module

#### Sales Reports Screen
**Description**: This screen displays comprehensive sales transaction reports with filtering options.

**Key Features**:
- Sales transaction list
- Date range filtering
- Search functionality
- Export to PDF
- Sales summary statistics

**User Interaction**: Users can filter sales by date, search for specific transactions, and export reports.

#### Profit & Loss Reports Screen
**Description**: This screen displays profit and loss statements showing business profitability over selected time periods.

**Key Features**:
- P&L statement generation
- Date range selection
- Income and expense breakdown
- Net profit/loss calculation
- Visual charts

**User Interaction**: Users select a date range and view detailed profit and loss analysis with visual representations.

---

### Staff Management Module

#### Staff Record Screens
**Description**: These screens allow sellers to view and manage their staff accounts (Cashiers, Stock Clerks, Accounting personnel).

**Key Features**:
- Staff list with role filtering
- Staff details view
- Archive/restore functionality
- Activity log viewing

**User Interaction**: Sellers can view staff information, archive inactive staff, and monitor staff activity.

---

### Audit Log Module

#### Activity Log Screen
**Description**: This screen displays comprehensive activity logs tracking all user actions in the system.

**Key Features**:
- Activity log list with filtering
- Search by user, module, status, or description
- Date range filtering
- User and role information
- Export functionality

**User Interaction**: Users can filter activity logs by various criteria to track system usage and user actions.

---

## 11. PROTOTYPE (BACKEND) - API/ALGO USAGE

### Service Layer Architecture

The system uses a **Service-Oriented Architecture** with the following key services:

#### Authentication Service (AuthService)
**Purpose**: Handles user authentication and password verification

**Key Methods**:
- `AuthenticateAsync(email, password)` - Validates user credentials
- Uses SHA-256 hashing to compare passwords
- Returns authenticated user object with role information

**How It Works**:
1. Receives email and password from login form
2. Hashes the password using SHA-256 with Unicode encoding
3. Queries database for user with matching email
4. Compares hashed password with stored hash
5. Returns user object if authentication succeeds

#### User Service (UserService)
**Purpose**: Manages user account operations

**Key Methods**:
- `GetUsersAsync()` - Retrieves user list with filtering
- `CreateUserAsync()` - Creates new user account
- `UpdateUserAsync()` - Updates user information
- `UpdatePasswordAsync()` - Changes user password
- `ArchiveUserAsync()` - Archives user account

**Database Operations**: Direct SQL queries using Microsoft.Data.SqlClient

#### Product Service (ProductService)
**Purpose**: Manages product data operations

**Key Methods**:
- `GetProductsAsync()` - Retrieves product list
- `CreateProductAsync()` - Creates new product
- `UpdateProductAsync()` - Updates product information
- `ArchiveProductAsync()` - Archives product

**Database Operations**: CRUD operations on `tbl_products` table

#### Inventory Service (InventoryService)
**Purpose**: Manages inventory stock levels and operations

**Key Methods**:
- `GetInventoriesAsync()` - Retrieves inventory list
- `UpdateInventoryAsync()` - Updates stock levels
- `GetLowStockItemsAsync()` - Identifies low stock items

**Database Operations**: Queries `tbl_inventories` with automatic updates via triggers

#### Sales Service (SalesService)
**Purpose**: Handles sales transaction processing

**Key Methods**:
- `CreateSaleAsync()` - Creates new sales transaction
- `GetSalesAsync()` - Retrieves sales history
- `ProcessPaymentAsync()` - Processes payment for sale

**Database Operations**: 
- Inserts into `tbl_sales` and `tbl_sales_items`
- Updates inventory quantities automatically
- Records payment in `tbl_payments`

#### Stock-In Service (StockInService)
**Purpose**: Manages stock-in transactions

**Key Methods**:
- `CreateStockInAsync()` - Records stock-in transaction
- `GetStockInsAsync()` - Retrieves stock-in history

**Database Operations**:
- Inserts into `tbl_stock_in`
- Triggers automatically update `tbl_inventories`

#### Purchase Order Service (PurchaseOrderService)
**Purpose**: Manages purchase order operations

**Key Methods**:
- `CreatePurchaseOrderAsync()` - Creates new PO
- `GetPurchaseOrdersAsync()` - Retrieves PO list
- `ApprovePurchaseOrderAsync()` - Approves PO (Accounting)
- `CompletePurchaseOrderAsync()` - Marks PO as completed

**Database Operations**: 
- Inserts into `tbl_purchase_orders` and `tbl_po_items`
- Updates PO status based on workflow

#### History Service (HistoryService)
**Purpose**: Manages audit logging

**Key Methods**:
- `LogAsync()` - Records user action in audit log
- `GetLogsAsync()` - Retrieves activity logs with filtering

**Database Operations**: Inserts into `tbl_histories` for every significant user action

**Usage**: Automatically called throughout the application to track:
- User logins/logouts
- Data creation, updates, deletions
- Approval actions
- Report generation

#### Database Sync Service (DatabaseSyncService)
**Purpose**: Synchronizes data between local and Azure databases

**Key Methods**:
- `SyncToAzureAsync()` - Copies data from local to Azure
- `SyncFromAzureAsync()` - Copies data from Azure to local
- `TestLocalConnectionAsync()` - Tests local database connection
- `TestAzureConnectionAsync()` - Tests Azure database connection

**How It Works**:
1. Connects to both local and Azure databases
2. Compares table data
3. Transfers missing or updated records
4. Maintains data consistency across environments

**Background Service**: Runs automatically to keep databases synchronized

---

### Database Triggers

The system uses **SQL Server Triggers** for automatic inventory management:

#### Stock-In Trigger
**Purpose**: Automatically updates inventory when stock-in is recorded

**How It Works**:
1. Trigger fires after INSERT on `tbl_stock_in`
2. Calculates new inventory quantity
3. Updates `tbl_inventories` table
4. Handles new inventory items if they don't exist

#### Stock-Out Trigger
**Purpose**: Automatically updates inventory when stock-out is recorded

**How It Works**:
1. Trigger fires after INSERT on `tbl_stock_out`
2. Deducts quantity from inventory
3. Updates `tbl_inventories` table
4. Prevents negative inventory (if configured)

#### Stock Adjustment Trigger
**Purpose**: Automatically updates inventory when adjustment is made

**How It Works**:
1. Trigger fires after INSERT on `tbl_stock_adjustments`
2. Applies adjustment to inventory quantity
3. Updates `tbl_inventories` table

---

## 12. SECURITY FEATURES

### Authentication Security

#### Password Hashing
**Process**: 
1. User enters password during login or password change
2. System uses **SHA-256** algorithm to hash the password
3. Password is encoded using **Unicode (UTF-16LE)** to match SQL Server's HASHBYTES function
4. Hashed password is converted to hexadecimal string
5. Stored hash in database is compared with computed hash

**Implementation**:
```csharp
string passwordHex = Convert.ToHexString(
    SHA256.HashData(Encoding.Unicode.GetBytes(password))
);
```

**Security Benefits**:
- Passwords are never stored in plain text
- Even database administrators cannot see actual passwords
- SHA-256 is a one-way hash function (cannot be reversed)

#### Session Management
**Process**:
1. Upon successful authentication, user object is stored in `AuthState`
2. User session persists across page navigation
3. Session includes user ID, email, full name, and role
4. Role determines which pages and features are accessible

**Security Benefits**:
- Prevents unauthorized access to role-specific features
- User identity is maintained throughout the session
- Role-based navigation prevents access to restricted pages

---

### Authorization Security

#### Role-Based Access Control (RBAC)
**Process**:
1. Each user is assigned a role (admin, seller, cashier, accounting, stockclerk)
2. Application routes are protected by role requirements
3. UI components conditionally render based on user role
4. Service methods validate user permissions before executing

**Implementation**:
- Route protection: `@page "/admin/dashboard"` - only accessible to admin role
- Conditional rendering: `@if (AuthState.CurrentUser?.Role == "admin")`
- Service validation: Methods check user role before performing operations

**Security Benefits**:
- Users can only access features appropriate for their role
- Prevents privilege escalation
- Clear separation of duties

#### Data Isolation
**Process**:
1. Sellers can only access their own business data
2. Staff members are linked to specific sellers
3. Database queries filter by `user_id` or `seller_id`
4. Cross-business data access is prevented

**Implementation**:
- All queries include seller/user filtering
- Foreign key relationships enforce data ownership
- Service methods require seller/user context

**Security Benefits**:
- Prevents data leakage between businesses
- Ensures data privacy
- Maintains business confidentiality

---

### Audit Logging Security

#### Comprehensive Activity Tracking
**Process**:
1. Every significant user action triggers an audit log entry
2. Log includes: user ID, action type (create, update, delete, login, etc.), module name, description, timestamp
3. Logs are stored in `tbl_histories` table
4. Logs cannot be deleted or modified by users

**Tracked Actions**:
- User authentication (login, logout)
- Data creation, updates, deletions
- Approval actions
- Report generation
- System configuration changes

**Security Benefits**:
- Complete audit trail for compliance
- Ability to track suspicious activities
- Accountability for all system actions
- Forensic analysis capability

---

### Database Security

#### Connection Security
**Process**:
1. Database connection strings are stored in service classes
2. Local database uses Windows Authentication (Integrated Security)
3. Azure database uses SQL Server Authentication with encrypted connection
4. Connection strings are not exposed in client-side code

**Security Features**:
- Encrypted connections to Azure (SSL/TLS)
- Parameterized queries prevent SQL injection
- Connection timeout handling
- Error handling prevents information leakage

#### SQL Injection Prevention
**Process**:
1. All database queries use parameterized statements
2. User input is never directly concatenated into SQL queries
3. Parameters are properly typed and validated

**Example**:
```csharp
cmd.Parameters.AddWithValue("@email", email.Trim());
```

**Security Benefits**:
- Prevents SQL injection attacks
- Ensures data integrity
- Protects database from malicious input

---

### Data Validation

#### Input Validation
**Process**:
1. Client-side validation using data annotations
2. Server-side validation in service methods
3. Database constraints enforce data integrity
4. Type checking prevents invalid data types

**Validation Examples**:
- Email format validation
- Required field validation
- Numeric range validation
- Date validation

**Security Benefits**:
- Prevents invalid data entry
- Reduces errors and inconsistencies
- Protects against malicious input

---

### Additional Security Measures

#### User Account Security
- **Account Archiving**: Users can be archived instead of deleted, preserving audit trail
- **Active Status**: Users can be deactivated without deletion
- **Password Change Requirement**: System can require password change on first login

#### Error Handling
- Generic error messages prevent information leakage
- Detailed errors logged server-side only
- User-friendly error messages displayed to users

#### Data Backup and Recovery
- Database synchronization provides backup capability
- Azure database serves as cloud backup
- Transaction support ensures data consistency

---

## 13. ADDITIONAL IMPORTANT INFORMATION FOR DEFENSE

### System Architecture Highlights

#### Multi-Tenant Architecture
- System supports multiple sellers (businesses)
- Each seller's data is completely isolated
- Sellers can manage their own staff and operations
- Admin has oversight of all sellers

#### Scalability Features
- Cloud database (Azure SQL) supports business growth
- Service-oriented architecture allows horizontal scaling
- Async/await pattern improves performance under load
- Indexed database tables optimize query performance

#### Data Consistency
- Database triggers ensure inventory accuracy
- Transaction support prevents partial updates
- Foreign key constraints maintain referential integrity
- Audit logs provide data change history

### Business Logic Highlights

#### Approval Workflows
- **Purchase Order Approval**: Stock Clerk creates PO → Accounting approves → PO can be completed
- **Refund Approval**: Cashier processes return → Accounting approves → Refund is processed
- **Daily Sales Verification**: Cashier submits daily sales → Accounting verifies → Sales are finalized

#### Inventory Management
- **Automatic Updates**: Stock-in, stock-out, and adjustments automatically update inventory
- **Low Stock Alerts**: System identifies items below reorder point
- **Stock Movement Tracking**: Complete history of all inventory changes

#### Financial Calculations
- **Net Income**: Gross Sales - Returns - Expenses
- **Profit/Loss**: Income - Expenses
- **Supplier Balance**: Total Amount - Total Paid
- **Tax Calculations**: VAT and admin fees computed automatically

### User Experience Features

#### Responsive Design
- System works on desktop, tablet, and mobile devices
- Adaptive layouts for different screen sizes
- Touch-friendly interfaces for mobile use

#### Search and Filtering
- All list screens support search functionality
- Date range filtering for reports
- Multi-criteria filtering (status, category, supplier, etc.)
- Real-time search with debouncing

#### Data Visualization
- Interactive charts and graphs
- KPI cards with trend indicators
- Color-coded status indicators
- Visual inventory level indicators

### Performance Optimizations

#### Database Optimization
- Indexed columns for fast queries
- Pagination for large datasets
- Filtered indexes for common queries
- Efficient JOIN operations

#### Application Performance
- Async/await for non-blocking operations
- Lazy loading of data
- Client-side caching where appropriate
- Optimized component rendering

### Integration Capabilities

#### Database Synchronization
- Bidirectional sync between local and cloud
- Automatic background synchronization
- Manual sync on demand
- Conflict resolution strategies

#### Reporting Capabilities
- PDF generation for reports
- Export functionality
- Print-friendly layouts
- Customizable date ranges

### Future Enhancement Possibilities

#### Potential Additions
- Mobile app version
- Barcode scanning integration
- Email notifications
- SMS alerts for low stock
- Advanced analytics and forecasting
- Multi-currency support
- E-commerce integration
- Customer management module
- Loyalty program integration

---

## CONCLUSION

The SOFTWEAR Retail Management System is a comprehensive solution that addresses the complete operational needs of retail clothing businesses. Through its modular architecture, role-based access control, and comprehensive feature set, it provides a robust platform for managing products, inventory, sales, and financial operations.

The system's security features, audit logging, and data isolation ensure that businesses can operate with confidence, knowing their data is protected and their operations are tracked. The cloud-based database architecture provides scalability for business growth, while the intuitive user interfaces ensure that staff can effectively use the system with minimal training.

By automating manual processes, providing real-time visibility into business operations, and generating comprehensive reports, the system enables businesses to make data-driven decisions, improve efficiency, and ultimately grow their operations.

---

**Prepared for Defense Presentation**
**Date**: [Current Date]
**System**: SOFTWEAR Retail Management System
**Version**: 1.0



