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