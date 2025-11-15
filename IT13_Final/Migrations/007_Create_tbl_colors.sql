-- Migration: Create tbl_colors table
-- Description: Creates the colors table with all required columns, constraints
-- Date: Generated migration for colors table

-- Drop table if exists (optional - uncomment if you want to recreate)
-- DROP TABLE IF EXISTS dbo.tbl_colors;

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create tbl_colors table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_colors') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_colors (
        -- Primary Key
        id INT IDENTITY(1,1) NOT NULL,
        
        -- Color Fields
        name NVARCHAR(100) NOT NULL,
        hex_value NVARCHAR(7) NOT NULL,
        description NVARCHAR(500) NULL,
        
        -- Audit Fields
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        
        -- Primary Key Constraint
        CONSTRAINT PK_tbl_colors PRIMARY KEY CLUSTERED (id ASC)
    );
    
    -- Create Indexes for better performance
    CREATE NONCLUSTERED INDEX IX_tbl_colors_name ON dbo.tbl_colors(name);
    CREATE NONCLUSTERED INDEX IX_tbl_colors_archived_at ON dbo.tbl_colors(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_colors created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_colors already exists.';
END
GO

