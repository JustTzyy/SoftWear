SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_variant_colors') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_variant_colors (
        id INT IDENTITY(1,1) NOT NULL,
        variant_id INT NOT NULL,
        color_id INT NOT NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_tbl_variant_colors PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT FK_tbl_variant_colors_variant FOREIGN KEY (variant_id) REFERENCES dbo.tbl_variants(id) ON DELETE CASCADE,
        CONSTRAINT FK_tbl_variant_colors_color FOREIGN KEY (color_id) REFERENCES dbo.tbl_colors(id),
        CONSTRAINT UQ_tbl_variant_colors_variant_color UNIQUE (variant_id, color_id)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_variant_colors_variant_id ON dbo.tbl_variant_colors(variant_id);
    CREATE NONCLUSTERED INDEX IX_tbl_variant_colors_color_id ON dbo.tbl_variant_colors(color_id);
    
    PRINT 'Table dbo.tbl_variant_colors created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_variant_colors already exists.';
END
GO

