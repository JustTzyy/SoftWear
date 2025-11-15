SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tbl_sizes') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.tbl_sizes (
        id INT IDENTITY(1,1) NOT NULL,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        archived_at DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2(0) NULL,
        CONSTRAINT PK_tbl_sizes PRIMARY KEY CLUSTERED (id ASC)
    );
    
    CREATE NONCLUSTERED INDEX IX_tbl_sizes_name ON dbo.tbl_sizes(name);
    CREATE NONCLUSTERED INDEX IX_tbl_sizes_archived_at ON dbo.tbl_sizes(archived_at) WHERE archived_at IS NULL;
    
    PRINT 'Table dbo.tbl_sizes created successfully.';
END
ELSE
BEGIN
    PRINT 'Table dbo.tbl_sizes already exists.';
END
GO



