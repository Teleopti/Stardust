
----------------  
--Name: David
--Date: 2013-0-18
--Desc: bug #22699 - Support [Queue].[PeekMessage]
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Queue].[Messages]') AND name = N'IX_Message_QueueId_Processed_ProcessingUntil')
CREATE NONCLUSTERED INDEX IX_Message_QueueId_Processed_ProcessingUntil
ON [Queue].[Messages] ([QueueId],[Processed],[ProcessingUntil])
INCLUDE ([CreatedAt],[ExpiresAt])
GO

----------------  
--Name: David
--Date: 2013-04-05
--Desc: bug #22969 - unwanted load of bridge_time_zone
----------------
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'dim_time_zone delete data' AND jobstep_id=82)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(82,N'dim_time_zone delete data')
GO

----------------  
--Name: Erik Sundberg
--Date: 2013-03-18
--Desc: PBI New RTA infrastructure
----------------
--remove IF EXISTS before deployment
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[ActualAgentState]') AND type in (N'U'))
DROP TABLE [RTA].[ActualAgentState]
GO

CREATE TABLE [RTA].[ActualAgentState](

 [PersonId] [uniqueidentifier] NOT NULL,
 [StateCode] [nvarchar](500) NOT NULL,
 [PlatformTypeId] [uniqueidentifier] NOT NULL,
 [State] [nvarchar](500) NOT NULL,
 [StateId] [uniqueidentifier] NOT NULL,
 [Scheduled] [nvarchar](500) NOT NULL,
 [ScheduledId] [uniqueidentifier] NOT NULL,
 [StateStart] [datetime] NOT NULL,
 [ScheduledNext] [nvarchar](500) NOT NULL,
 [ScheduledNextId] [uniqueidentifier] NOT NULL,
 [NextStart] [datetime] NOT NULL,
 [AlarmName] [nvarchar](500) NOT NULL,
 [AlarmId] [uniqueidentifier] NOT NULL,
 [Color] [int] NOT NULL,
 [AlarmStart] [datetime] NOT NULL,
 [StaffingEffect] [float] NOT NULL,
 [ReceivedTime] [datetime] NOT NULL,
 CONSTRAINT [PK_ActualAgentState] PRIMARY KEY CLUSTERED 
(
 [PersonId] ASC
)
)

GO

----------------  
--Name: David
--Date: 2013-03-19
--Desc: Int -> BigInt
----------------
IF (select DATA_TYPE from INFORMATION_SCHEMA.COLUMNS IC where TABLE_SCHEMA='Queue' AND TABLE_NAME = 'Messages' AND COLUMN_NAME = 'MessageId') = 'int'
BEGIN

	EXEC dbo.sp_rename @objname = N'[Queue].[Messages]', @newname = N'Messages_old', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[Queue].[Messages_old].[PK_Messages]', @newname = N'PK_Messages_old', @objtype =N'INDEX'

	CREATE TABLE [Queue].[Messages](
		[MessageId] [bigint] IDENTITY(1,1) NOT NULL,
		[QueueId] [int] NOT NULL,
		[CreatedAt] [datetime] NOT NULL,
		[ProcessingUntil] [datetime] NOT NULL,
		[ExpiresAt] [datetime] NULL,
		[Processed] [bit] NOT NULL,
		[Headers] [nvarchar](2000) NULL,
		[Payload] [varbinary](max) NULL,
		[ProcessedCount] [int] NOT NULL,
	 CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED 
	(
		[MessageId] ASC
	)
	)

	SET IDENTITY_INSERT [Queue].[Messages] ON
	INSERT INTO [Queue].[Messages] (MessageId, QueueId, CreatedAt, ProcessingUntil, ExpiresAt, Processed, Headers, Payload, ProcessedCount)
	SELECT MessageId, QueueId, CreatedAt, ProcessingUntil, ExpiresAt, Processed, Headers, Payload, ProcessedCount
	FROM [Queue].[Messages_old]
	SET IDENTITY_INSERT [Queue].[Messages] OFF

	DROP TABLE [Queue].[Messages_old] 

	ALTER TABLE [Queue].[Messages] ADD  CONSTRAINT [DF_Messages_CreatedAt]  DEFAULT (getdate()) FOR [CreatedAt]
	ALTER TABLE [Queue].[Messages] ADD  CONSTRAINT [DF_Messages_ProcessingUntil]  DEFAULT (getdate()) FOR [ProcessingUntil]
	ALTER TABLE [Queue].[Messages] ADD  CONSTRAINT [DF_Messages_Processed]  DEFAULT ((0)) FOR [Processed]
	ALTER TABLE [Queue].[Messages] ADD  CONSTRAINT [DF_Messages_ProcessedCount]  DEFAULT ((1)) FOR [ProcessedCount]

	CREATE NONCLUSTERED INDEX IX_Message_QueueId_Processed_ProcessingUntil
	ON [Queue].[Messages] ([QueueId],[Processed],[ProcessingUntil])
	INCLUDE ([CreatedAt],[ExpiresAt])

END

----------------  
--Name: David
--Date: 2013-03-19
--Desc: Purge table
----------------
--remove IF EXISTS before deployment
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Queue].[MessagesPurged]') AND type in (N'U'))
DROP TABLE [Queue].[MessagesPurged]
GO

CREATE TABLE [Queue].[MessagesPurged](
	[Id] [bigint] IDENTITY(-9223372036854775808,1) NOT NULL,
	[PurgedAt] [datetime] NOT NULL,
	[MessageId] [bigint] NOT NULL,
	[QueueId] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[ProcessingUntil] [datetime] NOT NULL,
	[ExpiresAt] [datetime] NULL,
	[Processed] [bit] NOT NULL,
	[Headers] [nvarchar](2000) NULL,
	[Payload] [varbinary](max) NULL,
	[ProcessedCount] [int] NOT NULL,
 CONSTRAINT [PK_MessagesPurged] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

ALTER TABLE [Queue].[MessagesPurged] ADD  CONSTRAINT [DF_PurgedAt_CreatedAt]  DEFAULT (getutcdate()) FOR [PurgedAt]
GO



----------------  
--Name: David
--Date: 2013-0-18
--Desc: bug #22699 - Support [Queue].[PeekMessage]
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Queue].[Messages]') AND name = N'IX_Message_QueueId_Processed_ProcessingUntil')
CREATE NONCLUSTERED INDEX IX_Message_QueueId_Processed_ProcessingUntil
ON [Queue].[Messages] ([QueueId],[Processed],[ProcessingUntil])
INCLUDE ([CreatedAt],[ExpiresAt])

----------------  
--Name: Ola & David
--Date: 2013-04-12
--Desc: PBI #22523 - Speed up ETL Intrady load
----------------
CREATE TABLE [stage].[stg_schedule_changed](
	[schedule_date] [datetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL
 CONSTRAINT [PK_stg_schedule_deleted] PRIMARY KEY CLUSTERED 
(
	[schedule_date] ASC,
	[person_code] ASC,
	[scenario_code] ASC
))
GO