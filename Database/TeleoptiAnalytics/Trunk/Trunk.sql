----------------  
--Name: David Jonsson
--Date: 2013-12-18
--Desc: Bug #26329 - Adding index for faster report permissions
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_to_be_deleted')
CREATE NONCLUSTERED INDEX IX_dim_person_to_be_deleted
ON [mart].[dim_person] ([to_be_deleted])
INCLUDE ([person_id],[team_id],[business_unit_code])
GO
 
----------------  
--Name: David Jonsson
--Date: 2013-12-16
--Desc: bug #26204 - make hard join on null guid instead of isnull
----------------
--make sure we use a zero guid instead of dbNull
update mart.dim_absence
set absence_code='00000000-0000-0000-0000-000000000000'
where absence_id=-1

update mart.dim_activity
set activity_code='00000000-0000-0000-0000-000000000000'
where activity_id=-1

update mart.dim_overtime
set overtime_code='00000000-0000-0000-0000-000000000000'
where overtime_id=-1

update mart.dim_shift_category
set shift_category_code='00000000-0000-0000-0000-000000000000'
where shift_category_id =-1

--add supporting indexes to the stg_schedule view
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_activity]') AND name = N'IX_activity_code')
CREATE NONCLUSTERED INDEX [IX_activity_code] ON [mart].[dim_activity]
(
	[activity_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_absence]') AND name = N'IX_absence_code')
CREATE NONCLUSTERED INDEX [IX_absence_code] ON [mart].[dim_absence]
(
	[absence_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_scenario]') AND name = N'IX_scenario_code')
CREATE NONCLUSTERED INDEX [IX_scenario_code] ON [mart].[dim_scenario]
(
	[scenario_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_shift_category]') AND name = N'IX_shift_category_code')
CREATE NONCLUSTERED INDEX [IX_shift_category_code] ON [mart].[dim_shift_category]
(
	[shift_category_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_shift_length]') AND name = N'IX_shift_length_m')
CREATE NONCLUSTERED INDEX [IX_shift_length_m] ON [mart].[dim_shift_length]
(
	[shift_length_m] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_overtime]') AND name = N'IX_dim_overtime_code')
CREATE NONCLUSTERED INDEX [IX_dim_overtime_code] ON [mart].[dim_overtime]
(
	[overtime_code] ASC
)

GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_scenario_id')
DROP INDEX [IX_fact_schedule_scenario_id] ON [mart].[fact_schedule]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_scenario_shift_category')
DROP INDEX [IX_fact_schedule_scenario_shift_category] ON [mart].[fact_schedule]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'idx_fact_schedule_schedule_date_id')
DROP INDEX [idx_fact_schedule_schedule_date_id] ON [mart].[fact_schedule]
GO

----------------  
--Name: David Jonsson
--Date: 2013-12-18
--Desc: Bug #26329 - Adding index for faster report permissions
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_to_be_deleted')
CREATE NONCLUSTERED INDEX IX_dim_person_to_be_deleted
ON [mart].[dim_person] ([to_be_deleted])
INCLUDE ([person_id],[team_id],[business_unit_code])
GO

--messed up cross view target found in customer database
update mart.sys_crossdatabaseview_target
set target_defaultName='TeleoptiCCCAgg'
where target_id=4
GO
-- bug 28436 
UPDATE mart.report
set target = '_blank' WHERE ID = 'D45A8874-57E1-4EB9-826D-E216A4CBC45B'
GO

----------------  
--Name: David Jonsson
--Date: 2014-07-01
--Desc: Bug #27933 - Give the end user a posibility to cherrypick tables for update_stat
----------------
SET NOCOUNT OFF
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_updatestat_tables]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[sys_updatestat_tables](
	table_schema sysname NOT NULL,
	table_name sysname NOT NULL,
	options NVARCHAR(200) NULL
	CONSTRAINT [PK_sys_updatestat_tables] PRIMARY KEY CLUSTERED 
	(
		table_schema ASC,
		table_name ASC
	)
)
END

GO
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'sqlserver_updatestat' AND jobstep_id=87)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(87,N'sqlserver_updatestat')
GO


----------------  
--Name: David Jonsson
--Date: 2014-09-10
--Desc: Bug #27933 - setting preference_typ_id in code
----------------
DROP TABLE [stage].[stg_schedule_preference]
GO
CREATE TABLE [stage].[stg_schedule_preference](
            [person_restriction_code] [uniqueidentifier] NOT NULL,
            [restriction_date] [datetime] NOT NULL,
            [person_code] [uniqueidentifier] NOT NULL,
            [scenario_code] [uniqueidentifier] NOT NULL,
            [shift_category_code] [uniqueidentifier] NULL,
            [day_off_code] [uniqueidentifier] NULL,
            [day_off_name] [nvarchar](50) NULL,
            [day_off_shortname] [nvarchar](25) NULL,
            [preference_type_id] tinyint NOT NULL,
            [preference_fulfilled] [int] NULL,
            [preference_unfulfilled] [int] NULL,
            [business_unit_code] [uniqueidentifier] NOT NULL,
            [datasource_id] [smallint] NULL CONSTRAINT [DF_stg_schedule_preference_datasource_id]  DEFAULT ((1)),
            [insert_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_preference_insert_date]  DEFAULT (getdate()),
            [update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_preference_update_date]  DEFAULT (getdate()),
            [datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_preference_datasource_update_date]  DEFAULT (getdate()),
            [activity_code] [uniqueidentifier] NULL,
            [absence_code] [uniqueidentifier] NULL,
            [must_have] [int] NULL,
CONSTRAINT [PK_stg_schedule_preference] PRIMARY KEY CLUSTERED 
(
            [person_restriction_code] ASC,
            [scenario_code] ASC
)
) ON [PRIMARY]

GO

----------------  
--Name: Karin Jeppsson
--Date: 2014-09-17
--Desc: Bug #30395 Speed up load of dim_person to prevent deadlocks
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dimPerson_business_unit_code')
CREATE NONCLUSTERED INDEX [IX_dimPerson_business_unit_code] ON [mart].[dim_person] 
(
                             [business_unit_code] ASC
)
GO

----------------  
--Name: Karin Jeppsson
--Date: 2014-09-17
--Desc: Bug #30657 Speed up load of dim_person to prevent deadlocks, add two new columns valid_from_date_local , valid_to_date_local
----------------

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_person]') AND type in (N'U'))
BEGIN
	DROP TABLE [stage].[stg_person]
END
GO

CREATE TABLE [stage].[stg_person](
	[person_code] [uniqueidentifier] NOT NULL,
	[valid_from_date] [smalldatetime] NOT NULL,
	[valid_to_date] [smalldatetime] NOT NULL,
	[valid_from_interval_id] [int] NOT NULL CONSTRAINT [DF_stg_person_valid_from_interval_id]  DEFAULT ((0)),
	[valid_to_interval_id] [int] NOT NULL CONSTRAINT [DF_stg_person_valid_to_interval_id]  DEFAULT ((0)),
	[valid_to_interval_start] [smalldatetime] NULL,
	[person_period_code] [uniqueidentifier] NULL,
	[person_name] [nvarchar](200) NOT NULL,
	[person_first_name] [nvarchar](25) NOT NULL,
	[person_last_name] [nvarchar](25) NOT NULL,
	[team_code] [uniqueidentifier] NOT NULL,
	[team_name] [nvarchar](50) NOT NULL,
	[scorecard_code] [uniqueidentifier] NULL,
	[site_code] [uniqueidentifier] NOT NULL,
	[site_name] [nvarchar](50) NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[email] [nvarchar](200) NULL,
	[note] [nchar](1024) NULL,
	[employment_number] [nvarchar](50) NULL,
	[employment_start_date] [smalldatetime] NOT NULL,
	[employment_end_date] [smalldatetime] NOT NULL,
	[time_zone_code] [nvarchar](50) NULL,
	[is_agent] [bit] NULL,
	[is_user] [bit] NULL,
	[contract_code] [uniqueidentifier] NULL,
	[contract_name] [nvarchar](50) NULL,
	[parttime_code] [uniqueidentifier] NULL,
	[parttime_percentage] [nvarchar](50) NULL,
	[employment_type] [nvarchar](50) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_stg_person_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_stg_person_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_person_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_person_datasource_update_date]  DEFAULT (getdate()),
	[windows_domain] [nvarchar](50) NULL,
	[windows_username] [nvarchar](50) NULL,
	[valid_from_date_local] [smalldatetime] NULL,
	[valid_to_date_local] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_person] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[valid_from_date] ASC
)
)

GO
--------------
--New table to store custom Agg views
--------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossdatabaseview_custom]') AND type in (N'U'))
BEGIN
	CREATE TABLE [mart].[sys_crossdatabaseview_custom](
		[view_id] [int] IDENTITY(1,1) NOT NULL,
		[view_name] [varchar](100) NOT NULL,
		[view_definition] [varchar](4000) NOT NULL,
		[target_id] [int] NOT NULL,
	 CONSTRAINT [PK_sys_crossdatabaseview_custom] PRIMARY KEY CLUSTERED 
	(
		[view_id] ASC
	)
	)
	ALTER TABLE [mart].[sys_crossdatabaseview_custom]  WITH NOCHECK ADD  CONSTRAINT [FK_sys_crossdatabaseview_custom_sys_crossdatabaseview_target] FOREIGN KEY([target_id])
	REFERENCES [mart].[sys_crossdatabaseview_target] ([target_id])
	ALTER TABLE [mart].[sys_crossdatabaseview_custom] CHECK CONSTRAINT [FK_sys_crossdatabaseview_custom_sys_crossdatabaseview_target]
END
GO
----------------  
--Name: KJ
--Date: 2014-12-12 
--Desc: Remove timezone as a parameter from Agent Skill report
----------------  
DELETE FROM mart.report_control_collection WHERE collection_id=46 and control_collection_id=495
GO
----------------  
--Name: KJ
--Date: 2015-04-30
--Desc: RESEED mart.dim_scenario OR recreate table if Azure v11
----------------
DECLARE @Version numeric(18,10)
SET @Version = CAST(LEFT(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)),CHARINDEX('.',CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max))) - 1) + '.' + REPLACE(RIGHT(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)), LEN(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max))) - CHARINDEX('.',CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)))),'.','') AS numeric(18,10))

IF SERVERPROPERTY('EngineEdition') = 5 AND @Version < 12
BEGIN
	--create temp table
	CREATE TABLE [mart].[dim_scenario_temp](
	[scenario_id] [smallint] NOT NULL,
	[scenario_code] [uniqueidentifier] NULL,
	[scenario_name] [nvarchar](50) NOT NULL ,
	[default_scenario] [bit] NULL,
	[business_unit_id] [int] NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](50) NOT NULL ,
	[datasource_id] [smallint] NOT NULL ,
	[insert_date] [smalldatetime] NOT NULL ,
	[update_date] [smalldatetime] NOT NULL ,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL ,
	CONSTRAINT [PK_dim_scenario_temp] PRIMARY KEY CLUSTERED 
	(
	[scenario_id] ASC
	)
	) 
	--insert data into temp table
	INSERT [mart].[dim_scenario_temp](scenario_id, scenario_code, scenario_name, default_scenario, business_unit_id, business_unit_code, business_unit_name, datasource_id, insert_date, update_date, datasource_update_date, is_deleted)
	SELECT scenario_id, scenario_code, scenario_name, default_scenario, business_unit_id, business_unit_code, business_unit_name, datasource_id, insert_date, update_date, datasource_update_date, is_deleted
	FROM [mart].[dim_scenario]
	--drop FKs
	ALTER TABLE [mart].[fact_forecast_workload] DROP CONSTRAINT [FK_fact_forecast_workload_dim_scenario]
	ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_scenario]
	ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [FK_fact_schedule_day_count_dim_scenario]
	ALTER TABLE [mart].[fact_schedule_forecast_skill] DROP CONSTRAINT [FK_fact_schedule_forecast_skill_dim_scenario]
	ALTER TABLE [mart].[fact_schedule_preference] DROP CONSTRAINT [FK_fact_schedule_preference_dim_scenario]
	ALTER TABLE [mart].[fact_hourly_availability] DROP CONSTRAINT [FK_fact_hourly_availability_dim_scenario]
	--drop old table
	DROP TABLE [mart].[dim_scenario]

	--create new scenario table
	CREATE TABLE [mart].[dim_scenario](
		[scenario_id] [smallint] IDENTITY(1,1) NOT NULL,
		[scenario_code] [uniqueidentifier] NULL,
		[scenario_name] [nvarchar](50) NOT NULL CONSTRAINT [DF_dim_scenario_scenario_name]  DEFAULT (N'Not Defined'),
		[default_scenario] [bit] NULL,
		[business_unit_id] [int] NULL,
		[business_unit_code] [uniqueidentifier] NULL,
		[business_unit_name] [nvarchar](50) NOT NULL CONSTRAINT [DF_dim_scenario_business_unit_name]  DEFAULT (N'Not Defined'),
		[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_scenario_datasource_id]  DEFAULT ((-1)),
		[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_scenario_insert_date]  DEFAULT (getdate()),
		[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_scenario_update_date]  DEFAULT (getdate()),
		[datasource_update_date] [smalldatetime] NULL,
		[is_deleted] [bit] NOT NULL CONSTRAINT [DF_dim_scenario_is_deleted]  DEFAULT ((0)),
	 CONSTRAINT [PK_dim_scenario] PRIMARY KEY CLUSTERED 
	(
		[scenario_id] ASC
	)
	) 
	--insert rows again
	SET IDENTITY_INSERT mart.dim_scenario ON
	INSERT [mart].[dim_scenario](scenario_id, scenario_code, scenario_name, default_scenario, business_unit_id, business_unit_code, business_unit_name, datasource_id, insert_date, update_date, datasource_update_date, is_deleted)
	SELECT scenario_id, scenario_code, scenario_name, default_scenario, business_unit_id, business_unit_code, business_unit_name, datasource_id, insert_date, update_date, datasource_update_date, is_deleted
	FROM [mart].[dim_scenario_temp]
	SET IDENTITY_INSERT mart.dim_scenario OFF

	--ADD FKs
	ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_scenario] FOREIGN KEY([scenario_id])
	REFERENCES [mart].[dim_scenario] ([scenario_id])
	ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_scenario]

	ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_scenario] FOREIGN KEY([scenario_id])
	REFERENCES [mart].[dim_scenario] ([scenario_id])
	ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_scenario]

	ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_scenario] FOREIGN KEY([scenario_id])
	REFERENCES [mart].[dim_scenario] ([scenario_id])
	ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_scenario]

	ALTER TABLE [mart].[fact_schedule_forecast_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_forecast_skill_dim_scenario] FOREIGN KEY([scenario_id])
	REFERENCES [mart].[dim_scenario] ([scenario_id])
	ALTER TABLE [mart].[fact_schedule_forecast_skill] CHECK CONSTRAINT [FK_fact_schedule_forecast_skill_dim_scenario]

	ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_scenario] FOREIGN KEY([scenario_id])
	REFERENCES [mart].[dim_scenario] ([scenario_id])

	ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_scenario]
	ALTER TABLE [mart].[fact_hourly_availability]  WITH CHECK ADD  CONSTRAINT [FK_fact_hourly_availability_dim_scenario] FOREIGN KEY([scenario_id])
	REFERENCES [mart].[dim_scenario] ([scenario_id])
	
	ALTER TABLE [mart].[fact_hourly_availability] CHECK CONSTRAINT [FK_fact_hourly_availability_dim_scenario]
	--remove temp table
	DROP TABLE [mart].[dim_scenario_temp]
END
ELSE
BEGIN
	DECLARE  @maxIdentityValue INT
	DECLARE @sqlstring nvarchar(4000)
	SET @sqlstring = ''
	SET @maxIdentityValue = (SELECT MAX(scenario_id) FROM [mart].[dim_scenario])
	SELECT @sqlstring = 'DBCC CHECKIDENT(''mart.dim_scenario'', RESEED,'+ convert(nvarchar(4),@maxIdentityValue) +')'
	EXEC sp_executesql @sqlstring
END


GO
