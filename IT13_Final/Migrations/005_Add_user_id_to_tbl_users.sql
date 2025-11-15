-- Migration: Add user_id to tbl_users table
-- Description: Adds a self-referential foreign key column to tbl_users for hierarchical relationships
-- Date: Add user_id column with foreign key constraint

-- Step 1: Add the user_id column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.tbl_users') 
    AND name = 'user_id'
)
BEGIN
    ALTER TABLE dbo.tbl_users
    ADD user_id INT NULL;
    
    PRINT 'Column user_id added to tbl_users table.';
END
ELSE
BEGIN
    PRINT 'Column user_id already exists in tbl_users table.';
END
GO

-- Step 2: Create index on user_id for better performance
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE object_id = OBJECT_ID(N'dbo.tbl_users') 
    AND name = 'IX_tbl_users_user_id'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_tbl_users_user_id 
    ON dbo.tbl_users(user_id);
    
    PRINT 'Index IX_tbl_users_user_id created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_tbl_users_user_id already exists.';
END
GO

-- Step 3: Add foreign key constraint (self-referential)
IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE object_id = OBJECT_ID(N'dbo.FK_tbl_users_user_id') 
    AND parent_object_id = OBJECT_ID(N'dbo.tbl_users')
)
BEGIN
    ALTER TABLE dbo.tbl_users
    ADD CONSTRAINT FK_tbl_users_user_id 
    FOREIGN KEY (user_id) 
    REFERENCES dbo.tbl_users(id);
    
    PRINT 'Foreign key constraint FK_tbl_users_user_id created successfully.';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_tbl_users_user_id already exists.';
END
GO

PRINT '';
PRINT 'Migration completed: user_id column added to tbl_users table with self-referential foreign key.';










