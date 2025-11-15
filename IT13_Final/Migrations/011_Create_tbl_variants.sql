SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variants') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_variants (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(200) NOT NULL,
        price DECIMAL(18,2) NOT NULL,
        cost_price DECIMAL(18,2) NULL,
        product_id INT NOT NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_variants PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_variants_product FOREIGN KEY (product_id) REFERENCES dbo.tbl_products(id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_variants_name ON dbo.tbl_variants(name);
    CREATE NONCLUSTERED INDEX IX_tbl_variants_product_id ON dbo.tbl_variants(product_id);
    CREATE NONCLUSTERED INDEX IX_tbl_variants_archived_at ON dbo.tbl_variants(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_variants created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_variants already exists.';
END
GO
