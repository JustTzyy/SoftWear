# Stock Clerk Module Important Functions Documentation

This document highlights the most important function in each module accessible to stock clerks in the SoftWear system and provides a brief explanation of how it works.

---

## 1. **Purchase Order Management Module** (PurchaseOrderService.cs)

### Most Important Function: `CreatePurchaseOrderAsync`

**Location:** `IT13_Final/Services/Data/PurchaseOrderService.cs` (Line 527-616)

**How it works:**
- **Purpose:** Creates a purchase order with multiple line items for inventory procurement from suppliers, initiating the procurement workflow.
- **Process:**
  1. Begins a database transaction to ensure data consistency
  2. Verifies that the supplier belongs to the seller's business (security check)
  3. Generates a unique PO number (format: `PO-YYYYMM-####`) within the transaction:
     - Counts existing POs for the seller in the current year
     - Increments by 1 for the sequence number
  4. Calculates total amount by summing all line items (quantity × unit price)
  5. Inserts the main purchase order record with:
     - PO number, supplier ID, status ('Pending' - requires accounting approval)
     - Total amount, notes (optional), expected delivery date (optional)
     - Created by user ID (stock clerk)
  6. For each item in the PO:
     - Inserts PO item record with variant, size, color, quantity, unit price, and total price
  7. Commits the transaction if all operations succeed
  8. Rolls back all changes if any operation fails

**Why it's important:** This is the primary procurement function for stock clerks. It initiates the inventory replenishment process by creating purchase orders that go through accounting approval before funds are committed. The transaction ensures atomicity - either the entire PO with all items is created, or nothing is saved. This prevents partial POs and maintains data integrity. Once approved by accounting, the PO can be fulfilled and stock-in records are automatically created.

---

## 2. **Stock-In Operations Module** (StockInService.cs)

### Most Important Function: `CreateStockInAsync`

**Location:** `IT13_Final/Services/Data/StockInService.cs` (Line 243-270)

**How it works:**
- **Purpose:** Records products added to inventory, typically when receiving goods from suppliers or fulfilling purchase orders.
- **Process:**
  1. Inserts stock-in record into `tbl_stock_in` with:
     - User ID (stock clerk who performed the operation)
     - Variant ID, size ID, color ID (identifies specific product variant)
     - Quantity added (how many units were received)
     - Cost price (per unit cost for inventory valuation)
     - Supplier ID (optional, links to supplier if from PO or direct delivery)
     - Timestamp (when the stock-in occurred)
  2. The stock-in record becomes part of the inventory calculation in `GetInventoriesAsync`
  3. Returns the newly created stock-in ID if successful

**Why it's important:** This is the primary mechanism for adding inventory to the system. Stock clerks use this when receiving goods from suppliers, fulfilling purchase orders, or manually adding stock. The cost price tracking enables inventory valuation and profit margin calculations. Combined with stock-out (from sales) and adjustments, it maintains accurate inventory levels. The supplier link enables tracking of procurement sources and supports supplier payment processing.

---

## 3. **Stock-Out Operations Module** (StockOutService.cs)

### Most Important Function: `CreateStockOutAsync`

**Location:** `IT13_Final/Services/Data/StockOutService.cs` (Line 220-247)

**How it works:**
- **Purpose:** Records products removed from inventory for reasons other than sales (which are handled automatically by the POS system).
- **Process:**
  1. Inserts stock-out record into `tbl_stock_out` with:
     - User ID (stock clerk who performed the operation)
     - Variant ID, size ID, color ID (identifies specific product variant)
     - Quantity removed (how many units were taken out)
     - Reason (optional, e.g., "Damaged", "Returned to Supplier", "Sample", "Theft")
     - Timestamp (when the stock-out occurred)
  2. The stock-out record reduces inventory in the calculation used by `GetInventoriesAsync`
  3. Returns the newly created stock-out ID if successful

**Why it's important:** This function handles inventory reductions that aren't sales transactions. Stock clerks use this for damaged goods, items returned to suppliers, samples, theft, or other non-sale removals. It maintains a complete audit trail of all inventory movements and ensures accurate stock levels. The reason field provides context for why inventory was removed, which is important for inventory analysis and loss tracking.

---

## 4. **Stock Adjustment Module** (StockAdjustmentService.cs)

### Most Important Function: `CreateStockAdjustmentAsync`

**Location:** `IT13_Final/Services/Data/StockAdjustmentService.cs` (Line 261-314)

**How it works:**
- **Purpose:** Corrects inventory discrepancies by increasing or decreasing stock levels, typically used after physical inventory counts.
- **Process:**
  1. Verifies that the variant belongs to the seller's business (security check)
  2. Validates adjustment type is either "Increase" or "Decrease"
  3. Validates quantity is greater than zero
  4. Inserts stock adjustment record into `tbl_stock_adjustments` with:
     - Variant ID, size ID, color ID (identifies specific product variant)
     - Adjustment type ("Increase" or "Decrease")
     - Quantity adjusted (how many units to add or subtract)
     - Reason (optional, e.g., "Physical count discrepancy", "Found stock", "Lost stock")
     - User ID (stock clerk who made the adjustment)
     - Timestamp
  5. The adjustment affects inventory calculation:
     - "Increase" adds to stock
     - "Decrease" subtracts from stock
  6. Returns the newly created adjustment ID if successful

**Why it's important:** This function corrects inventory discrepancies discovered during physical counts or audits. It's essential for maintaining accurate inventory records when the system stock doesn't match physical stock. The adjustment type and reason provide a complete audit trail for why stock levels were changed. This is critical for inventory accuracy and helps identify issues like theft, damage, or recording errors.

---

## 5. **Inventory Management Module** (InventoryService.cs)

### Most Important Function: `GetInventoriesAsync`

**Location:** `IT13_Final/Services/Data/InventoryService.cs` (Line 68-204)

**How it works:**
- **Purpose:** Calculates and retrieves real-time inventory levels by aggregating all stock movements (stock-in, stock-out, and adjustments).
- **Process:**
  1. Uses complex CTEs (Common Table Expressions) to aggregate inventory data:
     - **StockInAggregated:** Sums all stock-in quantities grouped by variant, size, and color
     - **StockOutAggregated:** Sums all stock-out quantities grouped by variant, size, and color
     - **StockAdjustmentsAggregated:** Calculates net adjustments (increases minus decreases)
  2. Combines these CTEs using FULL OUTER JOINs to handle all combinations
  3. Calculates current stock: `StockIn - StockOut + Adjustments`
  4. Joins with product, variant, category, size, color, and user tables
  5. Filters by seller's user ID to show only their inventory
  6. Filters to show only items with stock > 0 (or all items based on context)
  7. Supports search and pagination
  8. Returns complete inventory information including prices, images, stock levels, and reorder points

**Why it's important:** This function provides stock clerks with real-time visibility into inventory levels. It dynamically calculates stock from transaction history rather than maintaining a separate stock table, ensuring accuracy and providing a complete audit trail. Stock clerks use this to monitor stock levels, identify low stock items, plan procurement, and make inventory decisions. The seller-scoped filtering ensures stock clerks only see their seller's inventory.

---

## 6. **Reorder Level Management Module** (InventoryService.cs)

### Most Important Function: `UpdateReorderLevelAsync`

**Location:** `IT13_Final/Services/Data/InventoryService.cs` (Line 415-464)

**How it works:**
- **Purpose:** Sets or updates reorder points for inventory items to trigger low stock alerts and procurement planning.
- **Process:**
  1. Verifies that the variant belongs to the seller's business (security check)
  2. Uses a MERGE statement (UPSERT operation) to update or insert reorder level:
     - If a record exists for the variant-size-color combination:
       - Updates the reorder_level field
       - Updates the user_id (who set the reorder level)
       - Updates the timestamp
     - If no record exists:
       - Creates a new inventory record with reorder_level
       - Sets current_stock to 0 (will be calculated from transactions)
       - Records who set the reorder level
  3. Returns true if successful, false otherwise

**Why it's important:** Reorder points help stock clerks identify when inventory needs to be replenished. When current stock falls at or below the reorder level, it triggers alerts and helps plan purchase orders. This function enables proactive inventory management, preventing stockouts and ensuring adequate inventory levels. It's a key tool for inventory planning and procurement scheduling.

---

## 7. **Supplier Management Module** (SupplierService.cs)

### Most Important Function: `GetSuppliersAsync`

**Location:** `IT13_Final/Services/Data/SupplierService.cs` (Line 61-121)

**How it works:**
- **Purpose:** Retrieves supplier information for purchase order creation and procurement management.
- **Process:**
  1. Queries `tbl_suppliers` filtered by seller's user ID
  2. Supports search by company name, contact person, email, or contact number
  3. Supports date range filtering (by creation date)
  4. Returns supplier records with:
     - Company name, contact person, email, contact number
     - Status (Active/Inactive)
     - Creation date
  5. Supports pagination for large supplier lists
  6. Orders results by creation date (newest first)

**Why it's important:** Stock clerks need supplier information to create purchase orders. This function provides access to supplier records, enabling stock clerks to select suppliers when creating POs. It's essential for the procurement workflow and helps maintain supplier relationships. The search and filtering capabilities make it easy to find specific suppliers quickly.

---

## 8. **Stock-In Reporting Module** (StockInService.cs)

### Most Important Function: `GetStockInsAsync`

**Location:** `IT13_Final/Services/Data/StockInService.cs` (Line 63-137)

**How it works:**
- **Purpose:** Retrieves stock-in transaction history for reporting and audit purposes.
- **Process:**
  1. Queries `tbl_stock_in` joined with variants, products, suppliers, and users
  2. Filters by stock clerk's user ID (shows only their transactions)
  3. Supports search by variant name, product name, or supplier name
  4. Supports date range filtering
  5. Returns stock-in records with:
     - Quantity added, cost price, timestamp
     - Variant and product information
     - Supplier information (if linked)
     - User who performed the stock-in
  6. Supports pagination
  7. Orders results by timestamp (newest first)

**Why it's important:** This function provides a complete audit trail of all stock-in transactions. Stock clerks use this to review their stock-in history, verify receipts, and generate reports. It's essential for inventory reconciliation and helps track procurement activities. The supplier link enables analysis of which suppliers are being used most frequently.

---

## 9. **Stock-Out Reporting Module** (StockOutService.cs)

### Most Important Function: `GetStockOutsAsync`

**Location:** `IT13_Final/Services/Data/StockOutService.cs` (Line 56-125)

**How it works:**
- **Purpose:** Retrieves stock-out transaction history for reporting and audit purposes.
- **Process:**
  1. Queries `tbl_stock_out` joined with variants, products, and users
  2. Filters by stock clerk's user ID (shows only their transactions)
  3. Supports search by variant name, product name, or reason
  4. Supports date range filtering
  5. Returns stock-out records with:
     - Quantity removed, reason, timestamp
     - Variant and product information
     - User who performed the stock-out
  6. Supports pagination
  7. Orders results by timestamp (newest first)

**Why it's important:** This function provides a complete audit trail of all stock-out transactions (excluding sales, which are handled separately). Stock clerks use this to review inventory removals, analyze loss patterns, and generate reports. It's essential for understanding why inventory was removed and helps identify issues like damage, theft, or operational inefficiencies.

---

## 10. **Stock Adjustment Reporting Module** (StockAdjustmentService.cs)

### Most Important Function: `GetStockAdjustmentsAsync`

**Location:** `IT13_Final/Services/Data/StockAdjustmentService.cs` (Line 66-155)

**How it works:**
- **Purpose:** Retrieves stock adjustment history for reporting and audit purposes.
- **Process:**
  1. Queries `tbl_stock_adjustments` joined with variants, products, sizes, colors, and users
  2. Filters by stock clerk's user ID (shows only their adjustments)
  3. Supports search by variant name, product name, reason, or adjustment type
  4. Supports filtering by adjustment type (Increase/Decrease)
  5. Supports date range filtering
  6. Returns adjustment records with:
     - Adjustment type, quantity adjusted, reason, timestamp
     - Variant, product, size, and color information
     - User who made the adjustment
  7. Supports pagination
  8. Orders results by timestamp (newest first)

**Why it's important:** This function provides a complete audit trail of all inventory adjustments. Stock clerks use this to review corrections made to inventory, analyze discrepancy patterns, and generate reports. It's essential for understanding inventory accuracy issues and helps identify systemic problems that need to be addressed. The adjustment type and reason provide context for why corrections were necessary.

---

## Summary

Each stock clerk module's most important function serves as a critical inventory operation or reporting mechanism:

- **Purchase Order Management:** Initiates procurement workflow
- **Stock-In Operations:** Records inventory additions
- **Stock-Out Operations:** Records inventory removals (non-sales)
- **Stock Adjustment:** Corrects inventory discrepancies
- **Inventory Management:** Provides real-time stock visibility
- **Reorder Level Management:** Enables proactive inventory planning
- **Supplier Management:** Provides supplier information for procurement
- **Stock-In Reporting:** Audit trail of inventory additions
- **Stock-Out Reporting:** Audit trail of inventory removals
- **Stock Adjustment Reporting:** Audit trail of inventory corrections

**Key Stock Clerk-Specific Characteristics:**
- All functions are scoped to the seller's business via `seller_user_id` or `user_id` filtering
- Functions maintain complete audit trails for all inventory movements
- Transaction-based operations ensure data consistency
- Integration with purchase orders enables end-to-end procurement workflow
- Real-time inventory calculation ensures accurate stock levels
- Support for size and color variants enables detailed inventory tracking
- Cost price tracking enables inventory valuation

These functions work together to provide comprehensive inventory management, ensuring accurate stock levels, complete audit trails, and efficient procurement operations in the SoftWear system.


