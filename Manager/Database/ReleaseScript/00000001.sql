USE [$(DBNAME)]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE SCHEMA [Stardust]
GO 

CREATE TABLE [Stardust].[JobQueue](
	[JobId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Serialized] [nvarchar](max) NULL,
	[Type] [nvarchar](max) NULL,
	[CreatedBy] nvarchar(500) NOT NULL,
	[Created] [datetime] NOT NULL
 CONSTRAINT [PK_JobDefinition] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC
))
GO

CREATE TABLE [Stardust].[WorkerNode](
	[Id] [uniqueidentifier] NOT NULL,
	[Url] [nvarchar](450) NOT NULL,
	[Heartbeat] DateTime NULL,
	[Alive] Bit NULL
 CONSTRAINT [PK_WorkerNode] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_WorkerNodes_Url] ON [Stardust].[WorkerNode]
(
	[Url] ASC
)
GO


CREATE TABLE [Stardust].[Job](
	[JobId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[CreatedBy] nvarchar(500) NOT NULL,
	[Created] DateTime NOT NULL,
	[Started] DateTime NULL,
	[Ended] DateTime NULL,
	[Serialized] [nvarchar](max) NULL,
	[Type] [nvarchar](max) NULL,
	[SentToWorkerNodeUri] nvarchar(max) NULL,
	[Result] [nvarchar](max) NULL
CONSTRAINT [PK_Job] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC
))
GO

ALTER TABLE [Stardust].[Job] ADD CONSTRAINT
	DF_Job_Created DEFAULT getutcdate() FOR Created
GO


CREATE TABLE [Stardust].[JobDetail](
	[Id] int NOT NULL IDENTITY (1, 1),
	[JobId] [uniqueidentifier] NOT NULL,
	[Created] DateTime NOT NULL,
	[Detail] [nvarchar](max) NULL
CONSTRAINT [PK_JobDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))

GO
ALTER TABLE [Stardust].[JobDetail] ADD CONSTRAINT
	DF_JobDetail_Created DEFAULT getutcdate() FOR Created
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

CREATE TABLE [Stardust].[PerformanceTest](
	[Id] [int] IDENTITY (1, 1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Started] DateTime NOT NULL,
	[Ended] DateTime NOT NULL,
	[ElapsedInSeconds] float NOT NULL,
	[ElapsedInMinutes] float NOT NULL,
) ON [PRIMARY]
GO




