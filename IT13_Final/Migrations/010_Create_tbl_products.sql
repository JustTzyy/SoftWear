SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_products') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_products (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(200) NOT NULL,
        description NVARCHAR(1000) NULL,
        category_id INT NOT NULL,
        image VARBINARY(MAX) NULL,
        image_content_type NVARCHAR(100) NULL,
        status NVARCHAR(20) NOT NULL DEFAULT 'Active',
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_products PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_products_category FOREIGN KEY (category_id) REFERENCES dbo.tbl_categories(id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_products_name ON dbo.tbl_products(name);
    CREATE NONCLUSTERED INDEX IX_tbl_products_category_id ON dbo.tbl_products(category_id);
    CREATE NONCLUSTERED INDEX IX_tbl_products_archived_at ON dbo.tbl_products(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_products created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_products already exists.';
END
GO

