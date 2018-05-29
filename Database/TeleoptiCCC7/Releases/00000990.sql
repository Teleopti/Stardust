CREATE TABLE [dbo].[ExtensiveLogsSettings](
	[Id] [uniqueidentifier] NOT NULL,
	[Setting] [varchar]  (400) NOT NULL,
	[Value] [varchar]  (200) NOT NULL,
	[TimeoutInMin] [int] NOT NULL,
	[StartedLoggingAt] [datetime] NULL
		
 CONSTRAINT [PK_ExtensiveLogsSettings] PRIMARY KEY CLUSTERED 
(
	[Id] 
))

GO


insert into ExtensiveLogsSettings(Id,Setting,value,TimeoutInMin) values(NEWID(),'EnableRequestLogging','false',10)

GO

CREATE TABLE [dbo].[ExtensiveLogs](
	[Id] [uniqueidentifier] NOT NULL,
	[ObjectId] [uniqueidentifier] NULL,
	[Person] [uniqueidentifier]  NULL,
	[BusinessUnit] [uniqueidentifier] NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[RawData] nvarchar (max) NOT NULL,
	[EntityType] nvarchar (1000) NOT NULL,
	[IpAddress] [nvarchar] (400) NOT NULL,
	[HostName] [nvarchar] (1000) NOT NULL
		
 CONSTRAINT [PK_ExtensiveLogs] PRIMARY KEY CLUSTERED 
(
	[Id] 
))