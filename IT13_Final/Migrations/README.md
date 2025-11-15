# Database Migrations

This folder contains SQL migration scripts for the database schema.

## Migration Files

### 002_Create_tbl_roles.sql
Creates the `dbo.tbl_roles` table with all required columns, constraints, and indexes.

**Table Schema:**
- **id**: Primary Key (INT, IDENTITY)
- **name**: NVARCHAR(100), NOT NULL
- **desc**: NVARCHAR(500), NULL
- **created_at**: DATETIME2(0), NOT NULL, DEFAULT SYSUTCDATETIME()
- **updated_at**: DATETIME2(0), NULL

**Constraints:**
- Primary Key on `id`
- Unique constraint on `name` (prevents duplicate role names)

**Indexes:**
- Non-clustered index on `name`

**Note:** This table should be created before `tbl_users` since `tbl_users` has a foreign key reference to this table.

### 001_Create_tbl_users.sql
Creates the `dbo.tbl_users` table with all required columns, constraints, and indexes.

**Table Schema:**
- **id**: Primary Key (INT, IDENTITY)
- **email**: NVARCHAR(256), NOT NULL
- **pwd_hash**: NVARCHAR(256), NOT NULL
- **name**: NVARCHAR(150), NULL
- **fname**: NVARCHAR(100), NULL
- **mname**: NVARCHAR(100), NULL
- **lname**: NVARCHAR(100), NULL
- **contact_no**: NVARCHAR(30), NULL
- **bday**: DATE, NULL
- **age**: INT, NULL
- **sex**: TINYINT, NULL
- **is_active**: BIT, NOT NULL, DEFAULT 1
- **must_change_pw**: BIT, NOT NULL, DEFAULT 0
- **archived_at**: DATETIME2(0), NULL
- **created_at**: DATETIME2(0), NOT NULL, DEFAULT SYSUTCDATETIME()
- **updated_at**: DATETIME2(0), NULL
- **role_id**: INT, NULL (Foreign Key to tbl_roles)

**Constraints:**
- Primary Key on `id`
- Foreign Key on `role_id` referencing `dbo.tbl_roles(id)`

**Indexes:**
- Non-clustered index on `email`
- Non-clustered index on `role_id`
- Non-clustered filtered index on `archived_at` (WHERE archived_at IS NULL)

### 003_Create_tbl_histories.sql
Creates the `dbo.tbl_histories` table (audit log table) with all required columns, constraints, and indexes.

**Table Schema:**
- **id**: Primary Key (INT, IDENTITY)
- **user_id**: INT, NOT NULL (Foreign Key to tbl_users)
- **status**: NVARCHAR(32), NOT NULL
- **module**: NVARCHAR(64), NOT NULL
- **description**: NVARCHAR(256), NULL
- **ts**: DATETIME2(0), NOT NULL, DEFAULT SYSUTCDATETIME()

**Constraints:**
- Primary Key on `id`
- Foreign Key on `user_id` referencing `dbo.tbl_users(id)`

**Indexes:**
- Non-clustered index on `user_id`
- Non-clustered index on `ts` (DESC) for chronological queries
- Non-clustered index on `status` for filtering
- Non-clustered index on `module` for filtering

**Note:** This table should be created after `tbl_users` since it has a foreign key reference to that table.

### 004_Create_tbl_addresses.sql
Creates the `dbo.tbl_addresses` table with all required columns, constraints, and indexes.

**Table Schema:**
- **id**: Primary Key (INT, IDENTITY)
- **street**: NVARCHAR(200), NULL
- **city**: NVARCHAR(100), NULL
- **province**: NVARCHAR(100), NULL
- **zip**: NVARCHAR(20), NULL
- **archived_at**: DATETIME2(0), NULL
- **created_at**: DATETIME2(0), NOT NULL, DEFAULT SYSUTCDATETIME()
- **updated_at**: DATETIME2(0), NULL
- **user_id**: INT, NOT NULL (Foreign Key to tbl_users)

**Constraints:**
- Primary Key on `id`
- Foreign Key on `user_id` referencing `dbo.tbl_users(id)`

**Indexes:**
- Non-clustered index on `user_id`
- Non-clustered filtered index on `archived_at` (WHERE archived_at IS NULL)

**Note:** This table should be created after `tbl_users` since it has a foreign key reference to that table.

## Usage

### Running Migrations

Since the table already exists, this migration script is idempotent (safe to run multiple times). It will:
1. Check if the table exists
2. Only create the table if it doesn't exist
3. Create indexes and foreign key constraints

### To Apply Migrations:

**Important:** Run migrations in the correct order:
1. First run `002_Create_tbl_roles.sql` (roles table must exist before users table)
2. Then run `001_Create_tbl_users.sql` (users table references roles table)
3. Then run `003_Create_tbl_histories.sql` (histories table references users table)
4. Finally run `004_Create_tbl_addresses.sql` (addresses table references users table)

## Methods to Run Migrations:

### Method 1: Using the Batch Script (Easiest - Recommended)
Simply double-click `Run_Migrations.bat` in the Migrations folder, or run it from command prompt:

```batch
cd Migrations
Run_Migrations.bat
```

This will run all migrations automatically in the correct order.

### Method 2: Using SQL Server Management Studio (SSMS) - All-in-One Script
1. Open SQL Server Management Studio
2. Connect to your server: `localhost\SQLEXPRESS`
3. Open the database: `db_SoftWear`
4. Open `Run_All_Migrations_SSMS.sql` and click "Execute" (F5)

This single file contains all migrations and will run them in order.

### Method 3: Using SQL Server Management Studio (SSMS) - Individual Files
1. Open SQL Server Management Studio
2. Connect to your server: `localhost\SQLEXPRESS`
3. Open the database: `db_SoftWear`
4. Open each migration file (`.sql`) in order and execute:
   - Open `002_Create_tbl_roles.sql` → Click "Execute" (F5)
   - Open `001_Create_tbl_users.sql` → Click "Execute" (F5)
   - Open `003_Create_tbl_histories.sql` → Click "Execute" (F5)
   - Open `004_Create_tbl_addresses.sql` → Click "Execute" (F5)

### Method 4: Using Command Line (sqlcmd)
Open Command Prompt or PowerShell and run:

```batch
cd Migrations

REM Step 1: Create roles table
sqlcmd -S localhost\SQLEXPRESS -d db_SoftWear -i "002_Create_tbl_roles.sql"

REM Step 2: Create users table
sqlcmd -S localhost\SQLEXPRESS -d db_SoftWear -i "001_Create_tbl_users.sql"

REM Step 3: Create histories table
sqlcmd -S localhost\SQLEXPRESS -d db_SoftWear -i "003_Create_tbl_histories.sql"

REM Step 4: Create addresses table
sqlcmd -S localhost\SQLEXPRESS -d db_SoftWear -i "004_Create_tbl_addresses.sql"
```

### Method 5: Using Azure Data Studio
1. Open Azure Data Studio
2. Connect to your SQL Server: `localhost\SQLEXPRESS`
3. Navigate to `db_SoftWear` database
4. Open each `.sql` file and run them in order (same as SSMS)

### Method 6: Using Visual Studio
1. Open Visual Studio
2. Go to View → SQL Server Object Explorer
3. Connect to `localhost\SQLEXPRESS`
4. Right-click on `db_SoftWear` → New Query
5. Copy and paste the contents of each migration file in order and execute

### Note:
The migration script checks if the table exists before creating it, so it's safe to run even if the table already exists. This is useful for:
- Documenting the schema
- Version control
- Recreating the table structure in other environments
- Ensuring consistency across environments

