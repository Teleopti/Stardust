----------------  
--Name: Robin
--Date: 2011-11-17
--Desc: Adds schema for read models
----------------
CREATE SCHEMA [ReadModel] AUTHORIZATION [dbo]
GO

ALTER SCHEMA ReadModel TRANSFER [dbo].[GroupingReadOnly];
ALTER SCHEMA ReadModel TRANSFER [dbo].[UpdateGroupingReadModel];
GO

CREATE TABLE [ReadModel].[ScheduleProjectionReadOnly](
	[Id] [uniqueidentifier] NOT NULL,
	[ScenarioId] [uniqueidentifier] NOT NULL,
	[PersonId] [uniqueidentifier] NOT NULL,
	[BelongsToDate] [smalldatetime] NOT NULL,
	[PayloadId] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[WorkTime] [bigint] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DisplayColor] [int] NOT NULL,
	[PayrollCode] [nvarchar](20) NULL,
	[InsertedOn] [datetime] NOT NULL
)
GO

ALTER TABLE ReadModel.ScheduleProjectionReadOnly ADD CONSTRAINT
	PK_ScheduleProjectionReadOnly PRIMARY KEY NONCLUSTERED 
	(
	Id
	)
GO

CREATE CLUSTERED INDEX [CIX_ScheduleProjectionReadOnly] ON [ReadModel].[ScheduleProjectionReadOnly] 
(
	[BelongsToDate] ASC,
	[PersonId] ASC,
	[ScenarioId] ASC
)

ALTER TABLE [ReadModel].[ScheduleProjectionReadOnly] ADD CONSTRAINT [DF_ScheduleProjectionReadOnly_Id]  DEFAULT (newid()) FOR [Id]
GO

CREATE TABLE dbo.DenormalizationQueue
	(
	Id uniqueidentifier NOT NULL,
	BusinessUnit uniqueidentifier NOT NULL,
	Timestamp datetime NOT NULL,
	Message nvarchar(2000) NOT NULL,
	Type nvarchar(250) NOT NULL
	) 
GO

ALTER TABLE dbo.DenormalizationQueue ADD CONSTRAINT	[DF_DenormalizationQueue_Id] DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE dbo.DenormalizationQueue ADD CONSTRAINT
	PK_DenormalizationQueue PRIMARY KEY CLUSTERED 
	(
	Id
	) 
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (346,'7.1.346') 
