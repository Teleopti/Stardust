USE [$(DBNAME)]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE SCHEMA [Stardust]
GO 

CREATE TABLE [Stardust].[JobDefinitions](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Serialized] [nvarchar](max) NULL,
	[Type] [nvarchar](max) NULL,
	[UserName] nvarchar(500) NOT NULL,
	[AssignedNode] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NULL
 CONSTRAINT [PK_JobDefinitions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))
GO

CREATE TABLE [Stardust].[WorkerNodes](
	[Id] [uniqueidentifier] NOT NULL,
	[Url] [nvarchar](450) NOT NULL,
	[Heartbeat] DateTime NULL,
	[Alive] Bit NULL
 CONSTRAINT [PK_WorkerNodes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))

GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_WorkerNodes_Url] ON [Stardust].[WorkerNodes]
(
	[Url] ASC
)
GO


CREATE TABLE [Stardust].[JobHistory](
	[JobId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[CreatedBy] nvarchar(500) NOT NULL,
	[Created] DateTime NOT NULL,
	[Started] DateTime NULL,
	[Ended] DateTime NULL,
	[Serialized] [nvarchar](max) NULL,
	[Type] [nvarchar](max) NULL,
	[SentTo] nvarchar(max) NULL,
	[Result] [nvarchar](max) NULL
CONSTRAINT [PK_JobHistory] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC
))

GO
ALTER TABLE [Stardust].[JobHistory] ADD CONSTRAINT
	DF_JobHistory_Created DEFAULT getutcdate() FOR Created
GO


CREATE TABLE [Stardust].[JobHistoryDetail](
	[Id] int NOT NULL IDENTITY (1, 1),
	[JobId] [uniqueidentifier] NOT NULL,
	[Created] DateTime NOT NULL,
	[Detail] [nvarchar](max) NULL
CONSTRAINT [PK_JobHistoryDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))

GO
ALTER TABLE [Stardust].[JobHistoryDetail] ADD CONSTRAINT
	DF_JobHistoryDetail_Created DEFAULT getutcdate() FOR Created
GO

  CREATE TABLE [Stardust].[Logging] (
    [Id] [int] IDENTITY (1, 1) NOT NULL,
    [Date] [datetime] NOT NULL,
    [Thread] [varchar] (255) NOT NULL,
    [Level] [varchar] (50) NOT NULL,
    [Logger] [varchar] (255) NOT NULL,
    [Message] [varchar] (4000) NOT NULL,
    [Exception] [varchar] (2000) NULL
CONSTRAINT [PK_Logging] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))
GO