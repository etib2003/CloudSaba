USE [saba1db]
GO

/****** Object: Table [dbo].[IceCream] Script Date: 05/12/2023 12:14:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[IceCream] (
    [Id]       NVARCHAR (450)  NOT NULL,
    [Name]     NVARCHAR (MAX)  NULL,
    [Price]    DECIMAL (18, 2) NOT NULL,
    [ImageUrl] NVARCHAR (MAX)  NULL,
    [Details]  NVARCHAR (MAX)  NULL
);


