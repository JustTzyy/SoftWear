#!/usr/bin/env python3
"""
Script to generate SQL for seller ID 2 with:
- 15 active accounting users
- 15 active cashier users
- 15 active stock clerk users
- 15 archived accounting users
- 15 archived cashier users
- 15 archived stock clerk users
"""

# First names for staff
accounting_first_names = ['Liza', 'Ana', 'Maria', 'Carmen', 'Patricia', 'Rosa', 'Isabel', 'Elena', 'Sofia', 'Grace', 'Lisa', 'Karen', 'Angela', 'Christine', 'Melissa']
cashier_first_names = ['Carlos', 'Jose', 'Roberto', 'Miguel', 'Fernando', 'Ricardo', 'Manuel', 'Pedro', 'Andres', 'Ramon', 'Vincent', 'Anthony', 'Ronald', 'Edward', 'Brian']
stock_clerk_first_names = ['Maria', 'Rosa', 'Lucia', 'Carmen', 'Patricia', 'Fernando', 'Miguel', 'Roberto', 'Jose', 'Carlos', 'Ramon', 'Andres', 'Vincent', 'Anthony', 'Ronald']

# Last names for staff (using common Filipino surnames)
last_names = ['Santos', 'Cruz', 'Reyes', 'Garcia', 'Torres', 'Fernandez', 'Lopez', 'Mendoza', 'Villanueva', 'Ramos', 'Gonzales', 'Martinez', 'Rodriguez', 'Dela Cruz', 'Bautista']

# Middle names pool
middle_names = ['Cruz', 'Torres', 'Fernandez', 'Reyes', 'Garcia', 'Mendoza', 'Santos', 'Lopez', 'Villanueva', 'Ramos', 'Gonzales', 'Martinez', 'Rodriguez', 'Dela Cruz', 'Bautista']

def generate_staff_sql(seller_id_var, role_type, role_id_var, staff_count_var, first_names, base_contact_start, is_archived=False):
    sql_lines = []
    last_name_base = 'Claire'  # Based on JackieClaire
    
    for i in range(1, 16):
        staff_num = i
        first_name = first_names[i-1]
        middle_name = middle_names[(i-1) % len(middle_names)]
        last_name = last_names[(i-1) % len(last_names)]
        # Remove spaces from email
        email_first = first_name.replace(' ', '')
        email_last = last_name.replace(' ', '')
        email_role = role_type.replace(' ', '')
        email = f'{email_first}{email_last}{email_role}{staff_num}@SoftWear.com'
        name = f'{first_name} {last_name} {role_type} {staff_num}'
        contact = str(int(base_contact_start) + i)
        bday_year = 1988 + (i % 7)
        bday_month = (i % 12) + 1
        bday_day = (i % 28) + 1
        age = 2025 - bday_year
        sex = 1 if role_type == 'Accounting' else (0 if i % 2 == 0 else 1)
        
        # Set dates
        if is_archived:
            created_at = f'2023-{(i % 12) + 1:02d}-{(i % 28) + 1:02d} 10:00:00'
            archived_at = f'2024-{(i % 12) + 1:02d}-{(i % 28) + 1:02d} 14:30:00'
            archived_clause = f", '{archived_at}'"
        else:
            created_at = f'2024-{(i % 12) + 1:02d}-{(i % 28) + 1:02d} 10:00:00'
            archived_clause = ", NULL"
        
        street_num = 200 + i
        sql_lines.append(f"    INSERT INTO dbo.tbl_users (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, role_id, user_id, is_active, must_change_pw, created_at, archived_at) VALUES ('{email}', @PasswordHash, '{name}', '{first_name}', '{middle_name}', '{last_name}', '{contact}', '{bday_year}-{bday_month:02d}-{bday_day:02d}', {age}, {sex}, {role_id_var}, {seller_id_var}, 1, 0, '{created_at}'{archived_clause}); SET @StaffId = SCOPE_IDENTITY(); INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at) VALUES (@StaffId, '{street_num} Ayala Avenue', 'Makati', 'Metro Manila', '1226', '{created_at}'); SET @StaffCount = @StaffCount + 1;")
    
    return '\n'.join(sql_lines)

# Generate SQL
sql_output = []
sql_output.append("-- Migration: Seed Staff for Seller ID 2")
sql_output.append("-- Description: Adds 15 active and 15 archived staff members (accounting, cashier, stock clerk) for seller ID 2")
sql_output.append("-- Date: 2025-12-08")
sql_output.append("-- All users have password: password123")
sql_output.append("")
sql_output.append("SET ANSI_NULLS ON")
sql_output.append("GO")
sql_output.append("SET QUOTED_IDENTIFIER ON")
sql_output.append("GO")
sql_output.append("")
sql_output.append("PRINT '';")
sql_output.append("PRINT '========================================';")
sql_output.append("PRINT 'Seeding Staff for Seller ID 2';")
sql_output.append("PRINT '========================================';")
sql_output.append("PRINT '';")
sql_output.append("")
sql_output.append("-- ========================================")
sql_output.append("-- Step 1: Verify Seller ID 2 Exists")
sql_output.append("-- ========================================")
sql_output.append("PRINT 'Step 1: Verifying seller ID 2 exists...';")
sql_output.append("")
sql_output.append("DECLARE @SellerId INT = 2;")
sql_output.append("DECLARE @SellerExists INT;")
sql_output.append("")
sql_output.append("SELECT @SellerExists = COUNT(*) FROM dbo.tbl_users WHERE id = @SellerId AND archived_at IS NULL;")
sql_output.append("")
sql_output.append("IF @SellerExists = 0")
sql_output.append("BEGIN")
sql_output.append("    PRINT 'ERROR: Seller ID 2 not found or is archived!';")
sql_output.append("    RETURN;")
sql_output.append("END")
sql_output.append("")
sql_output.append("PRINT '  - Seller ID 2 verified';")
sql_output.append("PRINT '';")
sql_output.append("")
sql_output.append("-- ========================================")
sql_output.append("-- Step 2: Verify Roles Exist")
sql_output.append("-- ========================================")
sql_output.append("PRINT 'Step 2: Verifying roles exist...';")
sql_output.append("")
sql_output.append("DECLARE @AccountingRoleId INT;")
sql_output.append("DECLARE @CashierRoleId INT;")
sql_output.append("DECLARE @StockClerkRoleId INT;")
sql_output.append("")
sql_output.append("SELECT @AccountingRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting';")
sql_output.append("SELECT @CashierRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier';")
sql_output.append("SELECT @StockClerkRoleId = id FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk';")
sql_output.append("")
sql_output.append("IF @AccountingRoleId IS NULL OR @CashierRoleId IS NULL OR @StockClerkRoleId IS NULL")
sql_output.append("BEGIN")
sql_output.append("    PRINT 'ERROR: One or more roles not found!';")
sql_output.append("    RETURN;")
sql_output.append("END")
sql_output.append("")
sql_output.append("PRINT '  - All roles verified';")
sql_output.append("PRINT '';")
sql_output.append("")
sql_output.append("-- ========================================")
sql_output.append("-- Step 3: Insert Staff Members")
sql_output.append("-- ========================================")
sql_output.append("PRINT 'Step 3: Inserting staff members...';")
sql_output.append("PRINT '';")
sql_output.append("")
sql_output.append("DECLARE @PasswordHash NVARCHAR(256) = UPPER(CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', N'password123'), 2));")
sql_output.append("DECLARE @StaffId INT;")
sql_output.append("DECLARE @StaffCount INT = 0;")
sql_output.append("")
sql_output.append("-- 15 Active Accounting users for Seller ID 2")
sql_output.append(generate_staff_sql("@SellerId", "Accounting", "@AccountingRoleId", "@StaffCount", accounting_first_names, "09301234500", False))
sql_output.append("")
sql_output.append("-- 15 Active Cashier users for Seller ID 2")
sql_output.append(generate_staff_sql("@SellerId", "Cashier", "@CashierRoleId", "@StaffCount", cashier_first_names, "09302345600", False))
sql_output.append("")
sql_output.append("-- 15 Active Stock Clerk users for Seller ID 2")
sql_output.append(generate_staff_sql("@SellerId", "StockClerk", "@StockClerkRoleId", "@StaffCount", stock_clerk_first_names, "09303456700", False))
sql_output.append("")
sql_output.append("-- 15 Archived Accounting users for Seller ID 2")
sql_output.append(generate_staff_sql("@SellerId", "AccountingArchived", "@AccountingRoleId", "@StaffCount", accounting_first_names, "09304567800", True))
sql_output.append("")
sql_output.append("-- 15 Archived Cashier users for Seller ID 2")
sql_output.append(generate_staff_sql("@SellerId", "CashierArchived", "@CashierRoleId", "@StaffCount", cashier_first_names, "09305678900", True))
sql_output.append("")
sql_output.append("-- 15 Archived Stock Clerk users for Seller ID 2")
sql_output.append(generate_staff_sql("@SellerId", "StockClerkArchived", "@StockClerkRoleId", "@StaffCount", stock_clerk_first_names, "09306789000", True))
sql_output.append("")
sql_output.append("PRINT '';")
sql_output.append("PRINT '========================================';")
sql_output.append("PRINT 'Summary: ' + CAST(@StaffCount AS NVARCHAR(10)) + ' staff members inserted';")
sql_output.append("PRINT '  - 15 active accounting users';")
sql_output.append("PRINT '  - 15 active cashier users';")
sql_output.append("PRINT '  - 15 active stock clerk users';")
sql_output.append("PRINT '  - 15 archived accounting users';")
sql_output.append("PRINT '  - 15 archived cashier users';")
sql_output.append("PRINT '  - 15 archived stock clerk users';")
sql_output.append("PRINT '========================================';")
sql_output.append("PRINT '';")
sql_output.append("PRINT 'All users have:'")
sql_output.append("PRINT '  - Password: password123';")
sql_output.append("PRINT '  - Status: Active';")
sql_output.append("PRINT '  - Complete address information';")
sql_output.append("PRINT '';")
sql_output.append("PRINT 'Migration completed successfully!';")
sql_output.append("GO")

# Write to file
with open('054_Seed_Staff_For_Seller_ID_2.sql', 'w', encoding='utf-8') as f:
    f.write('\n'.join(sql_output))

print("Generated SQL file for seller ID 2 with:")
print("  - 15 active accounting users")
print("  - 15 active cashier users")
print("  - 15 active stock clerk users")
print("  - 15 archived accounting users")
print("  - 15 archived cashier users")
print("  - 15 archived stock clerk users")
print("Total: 90 staff members")

