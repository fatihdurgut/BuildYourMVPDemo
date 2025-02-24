CREATE TABLE [dbo].[Products]
(
    [ProductId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(2000) NULL,
    [Price] DECIMAL(18,2) NOT NULL,
    [ImageUrl] NVARCHAR(500) NULL,
    [CategoryId] INT NOT NULL,
    [StockQuantity] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Products_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([CategoryId])
)