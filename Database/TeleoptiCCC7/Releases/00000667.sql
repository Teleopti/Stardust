
IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[Stardust].[JobDefinitions]'))
   DROP TABLE [Stardust].[JobDefinitions]

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

IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[Stardust].[WorkerNodes]'))
   DROP TABLE [Stardust].[WorkerNodes]

CREATE TABLE [Stardust].[WorkerNodes](
	[Id] [uniqueidentifier] NOT NULL,
	[Url] [nvarchar](450) NOT NULL,
	[Heartbeat] DateTime NULL,
	[Alive] [nvarchar](max) NULL
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