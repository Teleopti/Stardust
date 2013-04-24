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
--Name: Karin
--Date: 2013-04-15
--Desc: Alter stage request table
----------------
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_request]') AND type in (N'U'))
DROP TABLE stage.stg_request
GO

CREATE TABLE [stage].[stg_request](
	[request_code] [uniqueidentifier] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[application_datetime] [smalldatetime] NOT NULL,
	[request_date] [smalldatetime] NOT NULL,
	[request_startdate] [smalldatetime] NOT NULL,
	[request_enddate] [smalldatetime] NOT NULL,
	[request_type_code] [tinyint] NOT NULL,
	[request_status_code] [tinyint] NOT NULL,
	[request_start_date_count] [int] NOT NULL,
	[request_day_count] [int] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
	[is_deleted] [smallint] NOT NULL,
	[request_starttime] [smalldatetime] NOT NULL,
	[request_endtime] [smalldatetime] NOT NULL,
	[absence_code] [uniqueidentifier] NULL
 CONSTRAINT [PK_stg_request] PRIMARY KEY CLUSTERED 
(
	[request_code] ASC,
	[person_code] ASC,
	[request_date] ASC,
	[request_type_code] ASC,
	[request_status_code] ASC
)
)
GO

----------------  
--Name: Karin
--Date: 2013-04-15
--Desc: Alter mart fact request table
----------------
ALTER TABLE mart.fact_request ADD
	absence_id int NULL,
	request_starttime smalldatetime NULL,
	request_endtime smalldatetime NULL,
	requested_time_m int NULL
GO

UPDATE mart.fact_request 
SET absence_id=-1,
request_starttime='1900-01-01 00:00:00',
request_endtime='1900-01-01 00:00:00',
requested_time_m =0 
WHERE absence_id IS NULL
GO

ALTER TABLE mart.fact_request alter column absence_id int NOT NULL
GO
ALTER TABLE  mart.fact_request alter column request_starttime smalldatetime NOT NULL
GO
ALTER TABLE  mart.fact_request alter column request_endtime smalldatetime NOT NULL
GO
ALTER TABLE  mart.fact_request alter column requested_time_m int NOT NULL
GO

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_absence] FOREIGN KEY([absence_id])
REFERENCES [mart].[dim_absence] ([absence_id])
GO

ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_absence]
GO
