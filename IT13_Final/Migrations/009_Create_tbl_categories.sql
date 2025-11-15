SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_categories') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_categories (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_categories PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_categories_name ON dbo.tbl_categories(name);
    CREATE NONCLUSTERED INDEX IX_tbl_categories_archived_at ON dbo.tbl_categories(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_categories created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_categories already exists.';
END
GO

