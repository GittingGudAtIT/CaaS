

-- Drop all tables
DROP TABLE IF EXISTS Customer;
DROP TABLE IF EXISTS OrderProduct;
DROP TABLE IF EXISTS OrderEntry;
DROP TABLE IF EXISTS [Order];
DROP TABLE IF EXISTS ProductDiscountProduct;
DROP TABLE IF EXISTS DiscountFreeProduct;
DROP TABLE IF EXISTS Discount;
DROP TABLE IF EXISTS CartEntry;
DROP TABLE IF EXISTS Cart;
DROP TABLE IF EXISTS Product;
DROP TABLE IF EXISTS Shop;


-- Create all tables
CREATE TABLE [dbo].[Shop] (
    [Id]     UNIQUEIDENTIFIER   DEFAULT newid(),
    [Name]   VARCHAR (50)       NOT NULL,
    [AppKey] VARCHAR (50)       NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[Cart] (
    [Id]        UNIQUEIDENTIFIER    DEFAULT newid(),
    [ShopId]    UNIQUEIDENTIFIER    NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [fk_shop_cart] FOREIGN KEY ([ShopId])
        REFERENCES [dbo].[Shop] (Id) ON DELETE CASCADE
);

CREATE TABLE [dbo].[Product] (
    [Id]           UNIQUEIDENTIFIER DEFAULT newid(),
    [ShopId]       UNIQUEIDENTIFIER NOT NULL,
    [Price]        DECIMAL(18,2)    NOT NULL,
    [Name]         VARCHAR (50)     NOT NULL,
    [Description]  VARCHAR (MAX)    NULL,
    [DownloadLink] VARCHAR (MAX)    NOT NULL,
    [ImageNr]      INT              DEFAULT 0,            
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [fk_shop_product] FOREIGN KEY ([ShopId]) 
        REFERENCES [dbo].[Shop] ([Id]) ON DELETE CASCADE
);


CREATE TABLE [dbo].[CartEntry] (
    [ProductId]  UNIQUEIDENTIFIER NOT NULL,
    [CartId]     UNIQUEIDENTIFIER NOT NULL,
    [Count]      INT              NOT NULL,
    CONSTRAINT [fk_product_cartentry] FOREIGN KEY ([ProductId]) 
    REFERENCES [dbo].[Product] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_cart_cartentry] FOREIGN KEY ([CartId]) 
    REFERENCES [dbo].[Cart] ([Id]) ON DELETE CASCADE,
    PRIMARY KEY CLUSTERED  ([CartId] ASC, [ProductId] ASC)
);

-- discount tables
CREATE TABLE [dbo].[Discount] (
    [Id]                    UNIQUEIDENTIFIER    DEFAULT newid(),
    [ShopId]                UNIQUEIDENTIFIER    NOT NULL,
    [Tag]                   VARCHAR (50)        NOT NULL,
    [Description]           VARCHAR (MAX)       NOT NULL,
    [OffType]               INT                 NOT NULL,
    [OffValue]              DECIMAL (18, 10)    NOT NULL,
    [MinType]               INT                 NOT NULL,
    [MinValue]              DECIMAL (10, 2)     NOT NULL,
    [Is4AllProducts]        BIT                 NOT NULL,
    [ValidFrom]             DATETIME            NOT NULL,
    [ValidTo]               DATETIME            NOT NULL,

    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [fk_shop_discount] FOREIGN KEY ([ShopId]) 
    REFERENCES [dbo].[Shop] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[ProductDiscountProduct] (
    [ProductId]  UNIQUEIDENTIFIER NOT NULL,
    [DiscountId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [fk_product_productdiscountproduct] FOREIGN KEY ([ProductId]) 
    REFERENCES [dbo].[Product] ([Id])  ON DELETE CASCADE,
    CONSTRAINT [fk_discount_productdiscountproduct] FOREIGN KEY ([DiscountId]) 
    REFERENCES [dbo].[Discount] ([Id]) ON DELETE NO ACTION,
    PRIMARY KEY CLUSTERED ([ProductId] ASC, [DiscountId] ASC)
);

CREATE TABLE [dbo].[DiscountFreeProduct] (
    [ProductId]  UNIQUEIDENTIFIER NOT NULL,
    [DiscountId] UNIQUEIDENTIFIER NOT NULL,
    [Count]      INT              NOT NULL,
    CONSTRAINT [fk_product_discountfreeproduc] FOREIGN KEY ([ProductId]) 
    REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [fk_discount_discountfreeproduct] FOREIGN KEY ([DiscountId]) 
    REFERENCES [dbo].[Discount] ([Id]) ON DELETE NO ACTION,
    PRIMARY KEY CLUSTERED ([ProductId] ASC, [DiscountId] ASC)
);

-- Order tables
CREATE TABLE [dbo].[Order] (
    [Id]            UNIQUEIDENTIFIER DEFAULT newid(),
    [ShopId]        UNIQUEIDENTIFIER NOT NULL,
	[DateTime]	    DATETIME		 NOT NULL,
	[OffSum]		DECIMAL(18,2)	 NOT NULL,
    [Total]         DECIMAL(18, 2)   NOT NULL,
    [DownloadLink]  VARCHAR(MAX)     NOT NULL,
    PRIMARY KEY CLUSTERED  ([Id] ASC),
	CONSTRAINT [fk_shop_order] FOREIGN KEY ([ShopId]) 
    REFERENCES [dbo].[Shop] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [dbo].[OrderEntry] (
    [RowNr]      INT              NOT NULL,
    [OrderId]    UNIQUEIDENTIFIER NOT NULL,
    [Count]      INT              NOT NULL,
    PRIMARY KEY CLUSTERED  ([RowNr] ASC, [OrderId] ASC),
    CONSTRAINT [fk_order_orderentry] FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE,
);

CREATE TABLE [dbo].[OrderProduct] (
	[RowNr]		    INT				    NOT NULL,
	[OrderId]	    UNIQUEIDENTIFIER	NOT NULL,
	[OriginalId]	UNIQUEIDENTIFIER	NOT NULL,
	[Price]		    DECIMAL(18,2)	    NOT NULL,
	[Name]		    VARCHAR(50)		    NOT NULL,
	PRIMARY KEY CLUSTERED ([RowNr] ASC, [OrderId] ASC),
	CONSTRAINT [fk_orderentry_orderproduct] FOREIGN KEY ([RowNr], [OrderId])
	REFERENCES [dbo].[OrderEntry] ([RowNr], [OrderId]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[Customer] (
    [OrderId]   UNIQUEIDENTIFIER    NOT NULL,
    [Firstname] VARCHAR (50)        NOT NULL,
    [Lastname]  VARCHAR (50)        NOT NULL,
    [Email]     VARCHAR (50)        NOT NULL,
    PRIMARY KEY CLUSTERED ([OrderId] ASC),
    CONSTRAINT [fk_order_customer] FOREIGN KEY ([OrderId]) 
        REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE
);