CREATE TABLE [rta].[Events] (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Event] nvarchar(max) NULL,
	CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
)
GO


--/*    ==Scripting Parameters==

--    Source Server Version : SQL Server 2016 (13.0.1742)
--    Source Database Engine Edition : Microsoft SQL Server Standard Edition
--    Source Database Engine Type : Standalone SQL Server

--    Target Server Version : SQL Server 2016
--    Target Database Engine Edition : Microsoft SQL Server Standard Edition
--    Target Database Engine Type : Standalone SQL Server
--*/

--USE [Infratest_Analytics]
--GO

--/****** Object:  Table [RtaTracer].[Logs]    Script Date: 2018-03-07 12:50:10 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE TABLE [RtaTracer].[Logs](
--	[Id] [int] IDENTITY(1,1) NOT NULL,
--	[Time] [datetime] NULL,
--	[Tenant] [nvarchar](255) NULL,
--	[MessageType] [nvarchar](500) NULL,
--	[Message] [nvarchar](max) NULL,
-- CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED 
--(
--	[Id] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
--GO

