USE [saba1db]
GO

/****** Object: Table [dbo].[Order] Script Date: 05/12/2023 12:48:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Order] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]   NVARCHAR (MAX) NOT NULL,
    [LastName]    NVARCHAR (MAX) NOT NULL,
    [PhoneNumber] NVARCHAR (MAX) NOT NULL,
    [Email]       NVARCHAR (MAX) NOT NULL,
    [Street]      NVARCHAR (MAX) NOT NULL,
    [City]        NVARCHAR (MAX) NOT NULL,
    [HouseNumber] INT            NOT NULL,
    [Total]       FLOAT (53)     NOT NULL,
    [Date]        DATETIME2 (7)  NOT NULL,
    [FeelsLike]   FLOAT (53)     NOT NULL,
    [Humidity]    FLOAT (53)     NOT NULL,
    [IsItHoliday] BIT            NOT NULL,
    [Day]         INT            NOT NULL
);


