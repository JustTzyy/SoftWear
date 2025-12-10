# Seller Module Important Functions Documentation

This document highlights the most important function in each module accessible to sellers in the SoftWear system and provides a brief explanation of how it works.

---

## 1. **Product Variant Management Module** (VariantService.cs)

### Most Important Function: `CreateVariantAsync`

**Location:** `IT13_Final/Services/Data/VariantService.cs` (Line 296-366)

**How it works:**
- **Purpose:** Creates a product variant that combines a product with size and color options, establishing sellable inventory items with pricing.
- **Process:**
  1. Begins a database transaction to ensure data consistency
  2. Inserts the main variant record into `tbl_variants` with:
     - User ID (associates variant with seller's business)
     - Variant name (e.g., "Cotton T-Shirt - Red")
     - Selling price (customer-facing price)
     - Cost price (optional, for profit margin calculations)
     - Product ID (links to base product)
     - Creation timestamp
  3. Retrieves the newly created variant ID using `SCOPE_IDENTITY()`
  4. For each size in the provided list:
     - Inserts a record into `tbl_variant_sizes` linking the variant to the size
  5. For each color in the provided list:
     - Inserts a record into `tbl_variant_colors` linking the variant to the color
  6. Commits the transaction if all operations succeed
  7. Rolls back all changes if any operation fails

**Why it's important:** Variants are the core sellable units in the system. This function creates the actual inventory items that appear in the POS system and can be sold to customers. It establishes the relationship between products, sizes, and colors, and sets the pricing structure. Without variants, products cannot be sold. The transaction ensures that variant creation is atomic - either the variant and all its size/color associations are created, or nothing is saved.

---

## 2. **Staff Management Module** (UserService.cs)

### Most Important Function: `CreateCashierAsync`

**Location:** `IT13_Final/Services/Data/UserService.cs` (Line 1674-1749)

**How it works:**
- **Purpose:** Creates a cashier staff account linked to the seller's business, enabling point-of-sale operations.
- **Process:**
  1. Retrieves the cashier role ID from the `tbl_roles` table
  2. Validates email uniqueness across all users
  3. Hashes the password using SHA-256 with UTF-16LE encoding (matching SQL Server format)
  4. Constructs the full name from first, middle (optional), and last name
  5. Converts sex string to integer (0=Male, 1=Female, or NULL)
  6. Inserts the new cashier user with:
     - All personal information (name, contact, birthday, age, sex)
     - `is_active = 1` (active by default)
     - `must_change_pw = 1` (forces password change on first login for security)
     - `role_id` set to cashier role
     - **`user_id` set to seller's ID** (critical: links cashier to seller's business)
  7. Returns the newly created cashier user ID

**Why it's important:** Cashiers are essential for processing sales transactions. This function establishes the staff hierarchy where cashiers belong to a specific seller's business. The `user_id` field creates the organizational link that ensures cashiers can only access their seller's data (products, inventory, sales). This is a core seller responsibility - managing their staff to operate their business. Similar functions exist for creating Accounting and Stock Clerk staff.

---

## 3. **Product Management Module** (ProductService.cs)

### Most Important Function: `CreateProductAsync`

**Location:** `IT13_Final/Services/Data/ProductService.cs` (Line 188-213)

**How it works:**
- **Purpose:** Creates a base product record that serves as the foundation for variants in the seller's catalog.
- **Process:**
  1. Opens a database connection
  2. Inserts a new product record into `tbl_products` with:
     - User ID (associates product with seller's business)
     - Product name and description
     - Category ID (organizes products by category)
     - Image data as VARBINARY (if provided) for product display
     - Image content type (MIME type) for proper rendering
     - Status set to 'Active'
     - Timestamp set to current UTC time
  3. Uses `SCOPE_IDENTITY()` to retrieve the newly created product ID
  4. Returns the product ID if successful, or null if creation fails

**Why it's important:** Products are the foundation of the seller's catalog. This function creates the base product that variants are built upon. It establishes the product's identity, category, and visual representation. Without products, there are no variants, and without variants, there's nothing to sell. The user_id association ensures products are scoped to the seller's business, maintaining data isolation between different sellers.

---

## 4. **Inventory Management Module** (InventoryService.cs)

### Most Important Function: `GetInventoriesAsync`

**Location:** `IT13_Final/Services/Data/InventoryService.cs` (Line 68-204)

**How it works:**
- **Purpose:** Calculates and retrieves real-time inventory levels for the seller's products by aggregating all stock movements.
- **Process:**
  1. Uses complex CTEs (Common Table Expressions) to aggregate inventory data:
     - **StockInAggregated:** Sums all stock-in quantities grouped by variant, size, and color
     - **StockOutAggregated:** Sums all stock-out quantities (from sales) grouped by variant, size, and color
     - **StockAdjustmentsAggregated:** Calculates net adjustments (increases minus decreases)
  2. Combines these CTEs using FULL OUTER JOINs to handle all combinations
  3. Calculates current stock: `StockIn - StockOut + Adjustments`
  4. Joins with product, variant, category, size, color, and user tables
  5. Filters by seller's user ID to show only their inventory
  6. Filters to show only items with stock > 0
  7. Supports search and pagination
  8. Returns complete inventory information including prices, images, and stock levels

**Why it's important:** This function provides sellers with real-time visibility into their inventory levels. It dynamically calculates stock from transaction history rather than maintaining a separate stock table, ensuring accuracy and providing a complete audit trail. Sellers use this to monitor stock levels, identify low stock items, and make inventory decisions. The seller-scoped filtering ensures sellers only see their own inventory.

---

## 5. **Supplier Management Module** (SupplierService.cs)

### Most Important Function: `CreateSupplierAsync`

**Location:** `IT13_Final/Services/Data/SupplierService.cs` (Line 47)

**How it works:**
- **Purpose:** Creates a supplier record with complete contact and address information for procurement operations.
- **Process:**
  1. Inserts supplier record into `tbl_suppliers` with:
     - Company name, contact person, email, contact number
     - Status (Active/Inactive)
     - User ID (associates supplier with seller's business)
     - Creation timestamp
  2. If address information is provided:
     - Inserts address record into `tbl_addresses` linked to the supplier
     - Stores street, city, province, and zip code
  3. Returns the newly created supplier ID

**Why it's important:** Suppliers are essential for procurement and inventory replenishment. This function establishes the supplier database that enables purchase orders, stock-in operations, and supplier invoice management. Proper supplier records with addresses are needed for delivery coordination and payment processing. The user_id association ensures suppliers are scoped to the seller's business.

---

## 6. **Income & Business Reports Module** (IncomeBreakdownService.cs)

### Most Important Function: `GetIncomeBreakdownAsync`

**Location:** `IT13_Final/Services/Data/IncomeBreakdownService.cs` (Line 51-354)

**How it works:**
- **Purpose:** Provides comprehensive income analysis for the seller's business, including breakdowns by cashier, category, and payment method.
- **Process:**
  1. Builds date filters for optional date range filtering
  2. Optionally filters to only include sales from approved daily sales verifications (for financial accuracy)
  3. Calculates total gross sales and transaction count:
     - Sums all completed sales amounts
     - Counts distinct sales transactions
     - Filters by seller's user_id to show only their business
  4. Calculates total returns:
     - Sums return amounts from approved/completed returns
     - Links returns to original sales to get sale dates
  5. Calculates net income: `Gross Sales - Returns`
  6. Groups income by cashier:
     - Shows which cashiers generated how much revenue
     - Counts transactions per cashier
  7. Groups income by product category:
     - Shows revenue per category
     - Counts items sold per category
  8. Groups income by payment method:
     - Shows cash vs GCash breakdown
     - Calculates percentages for each payment method
  9. Returns comprehensive income summary with all breakdowns

**Why it's important:** This function provides sellers with critical business intelligence. It shows where revenue is coming from (which cashiers, which categories, which payment methods), enabling data-driven business decisions. The optional "approved days only" filter ensures financial accuracy by only including verified sales. This is essential for understanding business performance, identifying trends, and making strategic decisions.

---

## 7. **Purchase Order Management Module** (PurchaseOrderService.cs)

### Most Important Function: `CreatePurchaseOrderAsync`

**Location:** `IT13_Final/Services/Data/PurchaseOrderService.cs` (Line 84)

**How it works:**
- **Purpose:** Creates a purchase order with multiple line items for supplier procurement, initiating the inventory replenishment process.
- **Process:**
  1. Generates a unique PO number
  2. Calculates total amount from all line items (quantity × unit price for each item)
  3. Inserts the main PO record with:
     - Supplier ID (links to supplier)
     - Status set to 'Pending' (requires accounting approval)
     - Expected delivery date (optional)
     - Notes (optional)
     - Seller user ID (associates PO with seller's business)
  4. For each item in the PO:
     - Inserts PO item record with variant, size, color, quantity, unit price, and total price
  5. Returns the newly created PO ID

**Why it's important:** Purchase orders are the foundation of inventory replenishment. This function initiates the procurement workflow, creates the approval chain (accounting must approve), and establishes the relationship between suppliers, products, and expected deliveries. It's the starting point for stock-in operations when POs are fulfilled. The seller-scoped association ensures POs are properly linked to the seller's business.

---

## 8. **Stock Operations Module** (StockInService.cs)

### Most Important Function: `CreateStockInAsync`

**Location:** `IT13_Final/Services/Data/StockInService.cs` (Line 54)

**How it works:**
- **Purpose:** Records products added to inventory, typically from purchase orders or supplier deliveries.
- **Process:**
  1. Inserts stock-in record into `tbl_stock_in` with:
     - Variant ID, size ID, color ID (identifies specific product variant)
     - Quantity added
     - Cost price (for inventory valuation and profit calculations)
     - Supplier ID (optional, links to supplier if from PO)
     - User ID (who performed the stock-in, typically stock clerk)
     - Timestamp
  2. The stock-in record becomes part of the inventory calculation in `GetInventoriesAsync`

**Why it's important:** This function is the primary mechanism for adding inventory to the seller's system. It's used when receiving goods from suppliers, fulfilling purchase orders, or manually adding stock. The cost price tracking enables inventory valuation and profit margin calculations. Combined with stock-out (from sales) and adjustments, it maintains accurate inventory levels. The supplier link enables tracking of procurement sources.

---

## 9. **Category Management Module** (CategoryService.cs)

### Most Important Function: `CreateCategoryAsync`

**Location:** `IT13_Final/Services/Data/CategoryService.cs`

**How it works:**
- **Purpose:** Creates product categories to organize the seller's product catalog.
- **Process:**
  1. Inserts category record into `tbl_categories` with:
     - Category name
     - User ID (associates category with seller's business)
     - Status set to 'Active'
     - Creation timestamp
  2. Returns the newly created category ID

**Why it's important:** Categories provide organization and structure to the product catalog. They enable sellers to group related products, making catalog management easier and improving customer browsing experience. Categories are also used in reporting and filtering. The user_id association ensures categories are scoped to the seller's business.

---

## 10. **Color & Size Management Modules** (ColorService.cs, SizeService.cs)

### Most Important Function: `CreateColorAsync` / `CreateSizeAsync`

**Location:** `IT13_Final/Services/Data/ColorService.cs` and `SizeService.cs`

**How it works:**
- **Purpose:** Creates color and size options that can be associated with product variants.
- **Process:**
  1. Inserts color/size record with:
     - Name (e.g., "Red", "Large")
     - For colors: Hex value (for visual display)
     - User ID (associates with seller's business)
     - Status set to 'Active'
     - Creation timestamp
  2. Returns the newly created color/size ID

**Why it's important:** Colors and sizes are building blocks for product variants. They enable sellers to create multiple variants from a single product (e.g., T-Shirt in Red/Small, Red/Medium, Blue/Small, etc.). The hex value for colors enables visual representation in the UI. These options are reusable across multiple products and variants, making catalog management efficient.

---

## Summary

Each seller module's most important function serves as the foundation or critical operation point for that module:

- **Product Variant Management:** Creates sellable inventory items (variants)
- **Staff Management:** Creates business staff (cashiers, accounting, stock clerks)
- **Product Management:** Creates base products
- **Inventory Management:** Provides real-time stock visibility
- **Supplier Management:** Establishes vendor relationships
- **Income & Reports:** Provides business intelligence
- **Purchase Orders:** Initiates procurement
- **Stock Operations:** Records inventory additions
- **Category Management:** Organizes product catalog
- **Color/Size Management:** Creates variant options

**Key Seller-Specific Characteristics:**
- All functions are scoped to the seller's business via `user_id` filtering
- Sellers manage their own isolated data (products, staff, suppliers, inventory)
- Functions support multi-tenant architecture where each seller's data is separate
- Staff creation functions link employees to the seller via `user_id` field
- Business reports aggregate data only from the seller's business operations

These functions work together to enable sellers to manage their complete retail business operations within the SoftWear platform.


