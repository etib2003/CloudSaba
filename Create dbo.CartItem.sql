USE [saba1db]
GO

/****** Object: Table [dbo].[CartItem] Script Date: 05/12/2023 12:51:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CartItem] (
    [ItemId]      NVARCHAR (450)  NOT NULL,
    [CartId]      NVARCHAR (MAX)  NULL,
    [Weight]      FLOAT (53)      NOT NULL,
    [Quantity]    INT             NOT NULL,
    [Price]       DECIMAL (18, 2) NOT NULL,
    [DateCreated] DATETIME2 (7)   NOT NULL,
    [FlavourId]   NVARCHAR (MAX)  NULL,
    [OrderId]     NVARCHAR (MAX)  NULL,
    [OrderId1]    INT             NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_CartItem_OrderId1]
    ON [dbo].[CartItem]([OrderId1] ASC);


GO
ALTER TABLE [dbo].[CartItem]
    ADD CONSTRAINT [PK_CartItem] PRIMARY KEY CLUSTERED ([ItemId] ASC);


GO
ALTER TABLE [dbo].[CartItem]
    ADD CONSTRAINT [FK_CartItem_Order_OrderId1] FOREIGN KEY ([OrderId1]) REFERENCES [dbo].[Order] ([Id]);


