SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variant_sizes') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_variant_sizes (
        id INT IDENTITY(1,1) NOT NULL,
        variant_id INT NOT NULL,
        size_id INT NOT NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_tbl_variant_sizes PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_variant_sizes_variant FOREIGN KEY (variant_id) REFERENCES dbo.tbl_variants(id) ON DELETE CASCADE,
        CONSTRAINT FK_tbl_variant_sizes_size FOREIGN KEY (size_id) REFERENCES dbo.tbl_sizes(id),
        CONSTRAINT UQ_tbl_variant_sizes_variant_size UNIQUE (variant_id, size_id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_variant_sizes_variant_id ON dbo.tbl_variant_sizes(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_variant_sizes_size_id ON dbo.tbl_variant_sizes(size_id);
    
    PRINT 'Table dbo.tbl_variant_sizes created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_variant_sizes already exists.';
END
GO

