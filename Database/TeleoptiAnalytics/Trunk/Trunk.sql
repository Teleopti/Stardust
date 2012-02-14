/*
20120207 AF: Purge old data
*/

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 4 AND [configuration_name] = 'YearsToKeepFactAgent')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(4,'YearsToKeepFactAgent',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 5 AND [configuration_name] = 'YearsToKeepFactAgentQueue')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(5,'YearsToKeepFactAgentQueue',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 6 AND [configuration_name] = 'YearsToKeepFactForecastWorkload')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(6,'YearsToKeepFactForecastWorkload',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 7 AND [configuration_name] = 'YearsToKeepFactQueue')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(7,'YearsToKeepFactQueue',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 8 AND [configuration_name] = 'YearsToKeepFactRequest')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(8,'YearsToKeepFactRequest',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 9 AND [configuration_name] = 'YearsToKeepFactSchedule')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(9,'YearsToKeepFactSchedule',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 10 AND [configuration_name] = 'YearsToKeepFactScheduleDayCount')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(10,'YearsToKeepFactScheduleDayCount',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 11 AND [configuration_name] = 'YearsToKeepFactScheduleDeviation')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(11,'YearsToKeepFactScheduleDeviation',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 12 AND [configuration_name] = 'YearsToKeepFactScheduleForecastSkill')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(12,'YearsToKeepFactScheduleForecastSkill',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 13 AND [configuration_name] = 'YearsToKeepFactSchedulePreferences')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(13,'YearsToKeepFactSchedulePreferences',50)


----------------  
--Name: DavidJ
--Date: 2012-02-24
--Desc: Re-factor Request per Agent
----------------
--Implemented, now re-factored. Drop if exists
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_request]') AND type in (N'U'))
DROP TABLE [stage].[stg_request]

CREATE TABLE [stage].[stg_request](
	[request_code] [uniqueidentifier] NOT NULL,	
	[person_code] [uniqueidentifier] NOT NULL,
	[application_datetime] [smalldatetime] NOT NULL,--NEW		
	[request_date] [smalldatetime] NOT NULL,--renamed from: date_from_local
	[request_startdate] [smalldatetime] NOT NULL,--NEW		
	[request_enddate] [smalldatetime] NOT NULL,	--NEW	
	[request_type_code] [tinyint] NOT NULL,
	[request_status_code] [tinyint] NOT NULL,
	[request_start_date_count] [int] NOT NULL,
	[request_day_count] [int] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
	[is_deleted] [smallint] NOT NULL
)
ALTER TABLE [stage].[stg_request]
ADD CONSTRAINT PK_stg_request PRIMARY KEY CLUSTERED 
(
	[request_code],
	[person_code],
	[request_date],
	[request_type_code],
	[request_status_code]
)

--Implemented, now re-factored. Drop if exists
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_request]') AND type in (N'U'))
DROP TABLE [mart].[fact_request]

CREATE TABLE [mart].[fact_request](	
	[request_code] [uniqueidentifier] NOT NULL,
	[person_id] [int] NOT NULL,
	[request_start_date_id] [int] NOT NULL, --NEW
	[application_datetime] [smalldatetime] NOT NULL,--NEW
	[request_startdate] [smalldatetime] NOT NULL,--NEW		
	[request_enddate] [smalldatetime] NOT NULL,	--NEW	
	[request_type_id] [tinyint] NOT NULL,
	[request_status_id] [tinyint] NOT NULL,
	[request_day_count] [int] NOT NULL,	
	[request_start_date_count] [int] NOT NULL,
	[business_unit_id] [smallint] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL
 )

ALTER TABLE mart.fact_request ADD CONSTRAINT
	  PK_fact_request PRIMARY KEY CLUSTERED 
	  (
		request_code
	  )

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_date_startdate] FOREIGN KEY([request_start_date_id])
REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_date_startdate]
GO

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_person]
GO

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_request_type] FOREIGN KEY([request_type_id])
REFERENCES [mart].[dim_request_type] ([request_type_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_request_type]
GO

ALTER TABLE [mart].[fact_request]  WITH CHECK ADD  CONSTRAINT [FK_fact_request_dim_request_status] FOREIGN KEY([request_status_id])
REFERENCES [mart].[dim_request_status] ([request_status_id])
ALTER TABLE [mart].[fact_request] CHECK CONSTRAINT [FK_fact_request_dim_request_status]
GO

ALTER TABLE [mart].[fact_request] ADD  CONSTRAINT [DF_fact_request_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
ALTER TABLE [mart].[fact_request] ADD  CONSTRAINT [DF_fact_request_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [mart].[fact_request] ADD  CONSTRAINT [DF_fact_request_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

--Implemented, now re-factored. Drop if exists
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_requested_days]') AND type in (N'U'))
DROP TABLE [mart].[fact_requested_days]

CREATE TABLE [mart].[fact_requested_days](
	[request_code] [uniqueidentifier] NOT NULL,
	[person_id] [int] NOT NULL,
	[request_date_id] [int] NOT NULL,
	[request_type_id] [tinyint] NOT NULL,
	[request_status_id] [tinyint] NOT NULL,
	[request_day_count] [int] NOT NULL,
	[business_unit_id] [smallint] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL
 )


ALTER TABLE [mart].[fact_requested_days] ADD  CONSTRAINT [DF_fact_requested_days_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
ALTER TABLE [mart].[fact_requested_days] ADD  CONSTRAINT [DF_fact_requested_days_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [mart].[fact_requested_days] ADD  CONSTRAINT [DF_fact_requested_days_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

 ALTER TABLE mart.fact_requested_days ADD CONSTRAINT
	PK_fact_requested_days PRIMARY KEY CLUSTERED 
	(
	request_code,
	request_date_id
	)

ALTER TABLE [mart].[fact_requested_days]  WITH CHECK ADD  CONSTRAINT [FK_fact_requested_days_dim_date_local] FOREIGN KEY([request_date_id])
REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_requested_days] CHECK CONSTRAINT [FK_fact_requested_days_dim_date_local]
GO

ALTER TABLE [mart].[fact_requested_days]  WITH CHECK ADD  CONSTRAINT [FK_fact_requested_days_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
ALTER TABLE [mart].[fact_requested_days] CHECK CONSTRAINT [FK_fact_requested_days_dim_person]
GO

ALTER TABLE [mart].[fact_requested_days]  WITH CHECK ADD  CONSTRAINT [FK_fact_requested_days_dim_request_type] FOREIGN KEY([request_type_id])
REFERENCES [mart].[dim_request_type] ([request_type_id])
ALTER TABLE [mart].[fact_requested_days] CHECK CONSTRAINT [FK_fact_requested_days_dim_request_type]
GO

ALTER TABLE [mart].[fact_requested_days]  WITH CHECK ADD  CONSTRAINT [FK_fact_requested_days_dim_request_status] FOREIGN KEY([request_status_id])
REFERENCES [mart].[dim_request_status] ([request_status_id])
ALTER TABLE [mart].[fact_requested_days] CHECK CONSTRAINT [FK_fact_requested_days_dim_request_status]
GO

--Add index to support etl-load
CREATE NONCLUSTERED INDEX IX_dim_person_person_code_from_to
ON [mart].[dim_person] ([person_code],[valid_from_date],[valid_to_date])
INCLUDE ([person_id])
