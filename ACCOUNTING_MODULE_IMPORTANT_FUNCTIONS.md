# Accounting Module Important Functions Documentation

This document highlights the most important function in each module accessible to accounting personnel in the SoftWear system and provides a brief explanation of how it works.

---

## 1. **Daily Sales Verification Module** (DailySalesVerificationService.cs)

### Most Important Function: `ApproveDailySalesAsync`

**Location:** `IT13_Final/Services/Data/DailySalesVerificationService.cs` (Line 642-701)

**How it works:**
- **Purpose:** Approves a cashier's daily sales submission, finalizing the financial records for that day and making them available for accounting reports.
- **Process:**
  1. Verifies that the cashier belongs to the seller's business (security check)
  2. Uses an UPSERT operation (INSERT or UPDATE) to handle the verification record:
     - If a verification record already exists for this cashier and date:
       - Updates the status to 'Approved'
       - Sets the verified_by field to the accounting user ID
       - Updates the timestamp
     - If no verification record exists:
       - Creates a new verification record with status 'Approved'
       - Records the accounting user who approved it
       - Links it to the seller's business
  3. Once approved, these sales become part of the official financial records
  4. Approved daily sales are included in income breakdowns, profit & loss reports, and cash flow analysis

**Why it's important:** This is the primary financial control mechanism in the system. It ensures cashier accountability, validates daily transactions, and finalizes financial records. Only approved daily sales are included in financial reports, ensuring accuracy and preventing unverified transactions from affecting business metrics. This function establishes the audit trail for financial reconciliation and is essential for maintaining financial integrity.

---

## 2. **Purchase Order Approval Module** (PurchaseOrderService.cs)

### Most Important Function: `UpdatePurchaseOrderStatusAsync` (Approval)

**Location:** `IT13_Final/Services/Data/PurchaseOrderService.cs` (Line 650-757)

**How it works:**
- **Purpose:** Approves or rejects purchase orders submitted by stock clerks, enabling procurement and automatically creating stock-in records when approved.
- **Process:**
  1. Begins a database transaction to ensure atomicity
  2. Verifies the PO exists and belongs to the seller's business
  3. Updates the PO status to 'Approved' or 'Rejected'
  4. If approved:
     - Retrieves all PO items with their variant, size, color, quantity, and unit price
     - For each item:
       - Determines the variant owner's user_id (seller who owns the product)
       - Creates a stock_in record with:
         - Quantity added (from PO item)
         - Cost price (from PO item unit price)
         - Supplier ID (from PO)
         - Variant, size, and color information
         - Timestamp
     - This automatically adds inventory to the system
  5. Commits the transaction if all operations succeed
  6. Rolls back all changes if any operation fails

**Why it's important:** This function controls procurement spending and inventory additions. It ensures that purchase orders are reviewed before funds are committed and inventory is added. The automatic stock-in creation upon approval streamlines the procurement-to-inventory process, ensuring inventory levels are updated immediately when POs are approved. This is a critical financial control point that prevents unauthorized purchases and maintains inventory accuracy.

---

## 3. **Return & Refund Approval Module** (ReturnsService.cs)

### Most Important Function: `UpdateReturnStatusAsync`

**Location:** `IT13_Final/Services/Data/ReturnsService.cs` (Line 100)

**How it works:**
- **Purpose:** Approves or rejects return/refund requests from cashiers, controlling refund disbursements and inventory restoration.
- **Process:**
  1. Verifies the return exists and belongs to the seller's business
  2. Updates the return status to 'Approved' or 'Rejected'
  3. If approved:
     - For each returned item:
       - Creates a stock_in record to restore inventory
       - Links the stock-in to the return for audit trail
       - Records the item condition (New/Used/Damaged)
     - Sets return status to 'Approved' (enables refund processing)
  4. If rejected:
     - Sets return status to 'Rejected'
     - No inventory restoration occurs
     - No refund is processed

**Why it's important:** This function controls refund disbursements and prevents unauthorized refunds. It ensures that returns are properly reviewed before money is refunded to customers. The automatic inventory restoration upon approval maintains accurate stock levels and provides a complete audit trail. This is essential for financial control, preventing refund fraud, and maintaining inventory accuracy.

---

## 4. **Expense Management Module** (ExpenseService.cs)

### Most Important Function: `CreateExpenseAsync`

**Location:** `IT13_Final/Services/Data/ExpenseService.cs` (Line 273-297)

**How it works:**
- **Purpose:** Records business expenses with receipt tracking for financial management and tax compliance.
- **Process:**
  1. Inserts expense record into `tbl_expenses` with:
     - Expense type (categorized expense for reporting)
     - Amount (expense value)
     - Description (optional details)
     - Expense date (when the expense occurred)
     - Receipt image (base64 encoded, optional)
     - Receipt content type (MIME type for image rendering)
     - Created by user ID (accounting user who recorded it)
     - Seller user ID (associates expense with seller's business)
     - Creation timestamp
  2. Returns the newly created expense ID

**Why it's important:** This function tracks all business expenditures, which is essential for profit & loss calculations, financial reporting, and tax compliance. The receipt image storage provides audit trails and supports expense verification during audits. Expenses are subtracted from income to calculate net profit, making this a critical component of financial management. Proper expense tracking enables accurate financial reporting and helps identify spending patterns.

---

## 5. **Supplier Payment Module** (SupplierInvoiceService.cs)

### Most Important Function: `CreatePaymentAsync`

**Location:** `IT13_Final/Services/Data/SupplierInvoiceService.cs` (Line 71)

**How it works:**
- **Purpose:** Records supplier payments for purchase orders or invoices, tracking accounts payable and payment history.
- **Process:**
  1. Validates the payment request (invoice ID, PO ID, or stock-in group)
  2. Inserts payment record into `tbl_supplier_payments` with:
     - Invoice ID or PO ID (links to payable item)
     - Amount paid
     - Payment method (Cash, GCash, Bank Transfer)
     - Payment date
     - Reference number (transaction reference for digital payments)
     - Notes (optional payment details)
     - Receipt image (optional, for payment proof)
     - Receipt content type (MIME type)
     - Created by user ID (accounting user)
     - Seller user ID (associates payment with seller's business)
     - Creation timestamp
  3. The payment is linked to the payable item (PO or invoice)
  4. Payment status is automatically calculated (Paid, Partially Paid, Unpaid) based on total payments vs. invoice amount
  5. Returns the newly created payment ID

**Why it's important:** This function manages accounts payable and tracks supplier payment obligations. It maintains a complete payment history, enables partial payments, and provides payment proof through receipt images. This is essential for cash flow management, supplier relationship management, and financial reconciliation. It ensures all supplier payments are properly recorded and tracked.

---

## 6. **Income Reporting Module** (IncomeBreakdownService.cs)

### Most Important Function: `GetIncomeBreakdownAsync`

**Location:** `IT13_Final/Services/Data/IncomeBreakdownService.cs` (Line 51-354)

**How it works:**
- **Purpose:** Provides comprehensive income analysis for financial reporting, including breakdowns by cashier, category, and payment method.
- **Process:**
  1. Builds date filters for optional date range filtering
  2. Optionally filters to only include sales from approved daily sales verifications (ensures financial accuracy)
  3. Calculates total gross sales:
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

**Why it's important:** This function provides critical financial intelligence for accounting personnel. It shows revenue sources (which cashiers, which categories, which payment methods), enabling data-driven financial decisions. The "approved days only" filter ensures financial accuracy by only including verified sales. This is essential for financial reporting, profit & loss statements, and identifying revenue trends. It's the foundation for financial analysis and business performance evaluation.

---

## 7. **Profit & Loss Reporting Module**

### Most Important Function: Income and Expense Aggregation

**How it works:**
- **Purpose:** Calculates profit and loss by combining income and expense data for financial statements.
- **Process:**
  1. Retrieves total income from approved daily sales (using `GetIncomeBreakdownAsync`)
  2. Retrieves total expenses from expense records (using `GetTotalExpensesAsync`)
  3. Calculates gross profit: `Total Income - Cost of Goods Sold`
  4. Calculates net profit: `Gross Profit - Total Expenses`
  5. Breaks down expenses by category
  6. Shows profit margins and percentages
  7. Provides period-over-period comparisons

**Why it's important:** This provides the fundamental financial statement that shows business profitability. It combines revenue and expenses to show whether the business is profitable, enabling financial decision-making, tax preparation, and business performance evaluation.

---

## 8. **Cash Flow Audit Module** (CashflowAuditService.cs)

### Most Important Function: Cash Flow Tracking

**How it works:**
- **Purpose:** Tracks cash inflows and outflows for cash flow management and reconciliation.
- **Process:**
  1. Aggregates cash inflows:
     - Cash sales (from approved daily sales)
     - GCash sales (from approved daily sales)
  2. Aggregates cash outflows:
     - Supplier payments (cash and digital)
     - Expenses (cash payments)
     - Petty cash disbursements
  3. Calculates net cash flow: `Inflows - Outflows`
  4. Tracks cash balance over time
  5. Identifies cash flow trends and patterns

**Why it's important:** This function provides visibility into cash movement, which is critical for cash flow management. It helps identify cash shortages, plan for expenses, and ensure sufficient cash reserves. This is essential for business liquidity management and financial planning.

---

## 9. **Tax Reporting Module**

### Most Important Function: Tax Calculation and Reporting

**How it works:**
- **Purpose:** Calculates tax obligations and generates tax reports for compliance.
- **Process:**
  1. Aggregates taxable income from approved sales
  2. Calculates tax deductions from expenses
  3. Applies tax rates and exemptions
  4. Generates tax reports with:
     - Total taxable income
     - Total deductions
     - Taxable amount
     - Tax due
  5. Provides period-based tax summaries

**Why it's important:** This function ensures tax compliance by accurately calculating tax obligations. It provides the data needed for tax filing and helps avoid penalties from incorrect tax reporting. This is essential for legal compliance and financial planning.

---

## 10. **Petty Cash Management Module**

### Most Important Function: Petty Cash Transaction Recording

**How it works:**
- **Purpose:** Tracks small cash expenditures for daily operations and maintains petty cash accountability.
- **Process:**
  1. Records petty cash transactions with:
     - Amount
     - Purpose/description
     - Recipient
     - Transaction date
     - Receipt (optional)
  2. Tracks petty cash balance
  3. Records petty cash replenishments
  4. Provides petty cash audit trail

**Why it's important:** This function maintains accountability for small cash transactions that don't go through formal expense processes. It prevents petty cash misuse and provides a complete audit trail for all cash movements. This is essential for internal controls and cash management.

---

## Summary

Each accounting module's most important function serves as a critical financial control or reporting mechanism:

- **Daily Sales Verification:** Financial control and transaction validation
- **Purchase Order Approval:** Procurement spending control and inventory management
- **Return & Refund Approval:** Refund control and inventory restoration
- **Expense Management:** Cost tracking and financial reporting
- **Supplier Payments:** Accounts payable management
- **Income Reporting:** Revenue analysis and financial intelligence
- **Profit & Loss:** Business profitability assessment
- **Cash Flow Audit:** Liquidity management
- **Tax Reporting:** Tax compliance
- **Petty Cash Management:** Small cash transaction control

**Key Accounting-Specific Characteristics:**
- All functions are scoped to the seller's business via `seller_user_id` filtering
- Approval workflows ensure financial controls and prevent unauthorized transactions
- Only approved/verified transactions are included in financial reports
- Complete audit trails are maintained for all financial operations
- Functions support financial reconciliation and compliance requirements
- Integration between modules ensures data consistency (e.g., approved sales → income reports)

These functions work together to provide comprehensive financial management, ensuring accuracy, control, and compliance in the SoftWear accounting system.


