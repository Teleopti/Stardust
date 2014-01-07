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
----------------  
--Name: Karin
--Date: 2013-04-25
--Desc: Alter mart fact requested days table
----------------
ALTER TABLE mart.fact_requested_days ADD absence_id int NULL
GO
UPDATE mart.fact_requested_days
SET absence_id=-1
WHERE absence_id IS NULL
GO
ALTER TABLE [mart].[fact_requested_days]  WITH CHECK ADD  CONSTRAINT [FK_fact_requested_days_dim_absence] FOREIGN KEY([absence_id])
REFERENCES [mart].[dim_absence] ([absence_id])
GO

ALTER TABLE [mart].[fact_requested_days] CHECK CONSTRAINT [FK_fact_requested_days_dim_absence]
GO

----------------  
--Name: Anders
--Date: 2013-04-25
--Desc: Purge agg data
----------------
if not exists(select 1 from [mart].[etl_maintenance_configuration] where configuration_id = 14)
	insert into [mart].[etl_maintenance_configuration] values(14,'YearsToKeepAggQueueStats',50)

if not exists(select 1 from [mart].[etl_maintenance_configuration] where configuration_id = 15)
	insert into [mart].[etl_maintenance_configuration] values(15,'YearsToKeepAggAgentStats',50)
GO

----------------  
--Name: David
--Date: 2013-04-29
--Desc: bug #23283 - wrong SP name
----------------
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_time_zone_delete.sql]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_time_zone_delete.sql]
GO
----------------  
--Name: Karin
--Date: 2013-04-29
--Desc: Add new column must_have in [stage].[stg_schedule_preference]
----------------
ALTER TABLE stage.stg_schedule_preference 
ADD absence_code uniqueidentifier NULL,
	must_have int NULL
GO

----------------  
--Name: Karin
--Date: 2013-04-29
--Desc: Add new column must_haves and absence_id in [mart].[fact_schedule_preference]

--Name: David
--Date: 2014-01-07
--Desc: #26271 - We can't drop PK in Azure for a table holding data
----------------
CREATE TABLE [mart].[fact_schedule_preference_new](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[preference_type_id] [int] NOT NULL,
	[shift_category_id] [int] NOT NULL,
	[day_off_id] [int] NOT NULL,
	[preferences_requested] [int] NULL,
	[preferences_fulfilled] [int] NULL,
	[preferences_unfulfilled] [int] NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[must_haves] [int] NULL,
	[absence_id] [int] NOT NULL
 CONSTRAINT [PK_fact_schedule_preference_new] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[person_id] ASC,
	[scenario_id] ASC,
	[preference_type_id] ASC,
	[shift_category_id] ASC,
	[day_off_id] ASC,
	[absence_id] ASC
)
)
GO
INSERT INTO [mart].[fact_schedule_preference_new]
(
	[date_id]
	,[interval_id]
	,[person_id]
	,[scenario_id]
	,[preference_type_id]
	,[shift_category_id]
	,[day_off_id]
	,[preferences_requested]
	,[preferences_fulfilled]
	,[preferences_unfulfilled]
	,[business_unit_id]
	,[datasource_id]
	,[insert_date]
	,[update_date]
	,[datasource_update_date]
	,[must_haves]
	,[absence_id]
)
SELECT [date_id]
      ,[interval_id]
      ,[person_id]
      ,[scenario_id]
      ,[preference_type_id]
      ,[shift_category_id]
      ,[day_off_id]
      ,[preferences_requested]
      ,[preferences_fulfilled]
      ,[preferences_unfulfilled]
      ,[business_unit_id]
      ,[datasource_id]
      ,[insert_date]
      ,[update_date]
      ,[datasource_update_date]
      ,0 --must_haves=0
      ,-1 --absence_id=-1
  FROM [mart].[fact_schedule_preference]
GO
DROP TABLE [mart].[fact_schedule_preference]
GO
EXEC dbo.sp_rename @objname = N'[mart].[fact_schedule_preference_new]', @newname = N'fact_schedule_preference', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[mart].[fact_schedule_preference].[PK_fact_schedule_preference_new]', @newname = N'PK_fact_schedule_preference', @objtype =N'INDEX'

ALTER TABLE [mart].[fact_schedule_preference] ADD  CONSTRAINT [DF_fact_preferences_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
ALTER TABLE [mart].[fact_schedule_preference] ADD  CONSTRAINT [DF_fact_preferences_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [mart].[fact_schedule_preference] ADD  CONSTRAINT [DF_fact_preferences_update_date]  DEFAULT (getdate()) FOR [update_date]
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_absence] FOREIGN KEY([absence_id]) REFERENCES [mart].[dim_absence] ([absence_id])
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_absence]
ALTER TABLE [mart].[fact_schedule_preference]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_date] FOREIGN KEY([date_id]) REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_date]
ALTER TABLE [mart].[fact_schedule_preference]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_day_off] FOREIGN KEY([day_off_id]) REFERENCES [mart].[dim_day_off] ([day_off_id])
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_day_off]
ALTER TABLE [mart].[fact_schedule_preference]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_interval] FOREIGN KEY([interval_id]) REFERENCES [mart].[dim_interval] ([interval_id])
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_interval]
ALTER TABLE [mart].[fact_schedule_preference]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_person] FOREIGN KEY([person_id]) REFERENCES [mart].[dim_person] ([person_id])
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_person]
ALTER TABLE [mart].[fact_schedule_preference]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_preference_type] FOREIGN KEY([preference_type_id]) REFERENCES [mart].[dim_preference_type] ([preference_type_id])
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_preference_type]
ALTER TABLE [mart].[fact_schedule_preference]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_scenario] FOREIGN KEY([scenario_id]) REFERENCES [mart].[dim_scenario] ([scenario_id])
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_scenario]
ALTER TABLE [mart].[fact_schedule_preference]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_shift_category] FOREIGN KEY([shift_category_id]) REFERENCES [mart].[dim_shift_category] ([shift_category_id])
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_shift_category]
GO

----------------  
--Name: Karin
--Date: 2013-04-29
--Desc: Add "Absence" as new type in dim_preference_type
----------------
--add preference type absence in [mart].[dim_preference_type]
INSERT mart.dim_preference_type(preference_type_id, preference_type_name, resource_key)
SELECT 4,'Absence','ResAbsence'
GO

----------------  
--Name: Karin
--Date: 2013-04-29
--Desc: Rename columns in preference table
----------------

EXEC sp_RENAME 'mart.fact_schedule_preference.preferences_accepted_count' , 'preferences_fulfilled', 'COLUMN'
GO
EXEC sp_RENAME 'mart.fact_schedule_preference.preferences_declined_count' , 'preferences_unfulfilled', 'COLUMN'
GO
EXEC sp_RENAME 'mart.fact_schedule_preference.preferences_requested_count' , 'preferences_requested', 'COLUMN'
GO
EXEC sp_RENAME 'stage.stg_schedule_preference.preference_accepted' , 'preference_fulfilled', 'COLUMN'
GO
EXEC sp_RENAME 'stage.stg_schedule_preference.preference_declined' , 'preference_unfulfilled', 'COLUMN'
GO

----------------  
--Name: Karin
--Date: 2013-05-03
--Desc: Drop unused column calulated calls in fact_forecast_workload
----------------
ALTER TABLE [mart].[fact_forecast_workload] DROP COLUMN [calculated_calls]
GO
----------------  
--Name: Ola & David
--Date: 2013-04-12
--Desc: PBI #22523 - Speed up ETL Intrady load
----------------
CREATE TABLE [stage].[stg_schedule_changed](
            [schedule_date] [datetime] NOT NULL,
            [person_code] [uniqueidentifier] NOT NULL,
            [scenario_code] [uniqueidentifier] NOT NULL,
            [business_unit_code] [uniqueidentifier] NOT NULL,
            [datasource_id] [smallint] NOT NULL,
            [datasource_update_date] [smalldatetime] NOT NULL
CONSTRAINT [PK_stg_schedule_changed] PRIMARY KEY CLUSTERED 
(
            [schedule_date] ASC,
            [person_code] ASC,
            [scenario_code] ASC
)
)

GO

CREATE TABLE Stage.stg_schedule_updated_personLocal (
	person_id int not null,
	time_zone_id int not null,
	person_code uniqueidentifier not null,
	valid_from_date_local smalldatetime not null,
	valid_to_date_local smalldatetime not null
CONSTRAINT [PK_stg_schedule_updated_personLocal] PRIMARY KEY CLUSTERED 
(
            [person_id]
)
)

CREATE TABLE Stage.stg_schedule_updated_ShiftStartDateUTC (
	person_id int not null,
	shift_startdate_id int not null
CONSTRAINT [PK_stg_schedule_updated_ShiftStartDateUTC] PRIMARY KEY CLUSTERED 
(
            [person_id]
)
)
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (384,'7.4.384') 
