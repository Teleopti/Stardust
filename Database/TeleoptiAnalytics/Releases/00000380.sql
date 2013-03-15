-- Robin Karlsson, 2013-01-17
-- Drop everything related to the message broker
--Stored Procedures
declare @sql varchar(max)
set @sql = (
select 'DROP PROCEDURE [' + routine_schema + '].[' + routine_name + '] ' 
from information_schema.routines where routine_schema = 'msg' and routine_type = 'PROCEDURE'
FOR XML PATH ('')
)
exec (@sql)

--views
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[msg].[vCurrentUsers]'))
DROP VIEW [msg].[vCurrentUsers]
GO

--tables
declare @sql varchar(max)
set @sql = (
select 'DROP TABLE [' + table_schema + '].[' + table_name + '] ' 
from information_schema.TABLES
where table_schema= 'msg' and table_type='BASE TABLE'
FOR XML PATH ('')
)
exec (@sql)
GO

--schema
IF  EXISTS (SELECT * FROM sys.schemas WHERE name = N'msg')
DROP SCHEMA [msg]
GO

----------------  
--Name: Karin and David
--Date: 2013-01-09
--Desc: PBI21633 + #22446
----------------  
CREATE TABLE mart.etl_job_delayed
	(
		Id int identity (1,1) not null,
		stored_procedured nvarchar(300) not null,
		parameter_string nvarchar(1000) not null,
		insert_date smalldatetime not null,
		execute_date smalldatetime null
	)
ALTER TABLE mart.etl_job_delayed ADD CONSTRAINT
	PK_etl_job_delayed PRIMARY KEY CLUSTERED 
	(
	Id
	)
ALTER TABLE [mart].[etl_job_delayed] ADD  CONSTRAINT [DF_etl_job_delayed_insert_date]  DEFAULT (getdate()) FOR [insert_date]

ALTER TABLE mart.fact_schedule_deviation DROP COLUMN datasource_update_date
ALTER TABLE  mart.fact_schedule_deviation ADD shift_startdate_id int NULL
ALTER TABLE  mart.fact_schedule_deviation ADD shift_startinterval_id smallint NULL
GO

--decieded how many rows should be part of each extra ETL-load
	DECLARE @min_date smalldatetime
	DECLARE @tmp_min_date smalldatetime
	DECLARE @max_date smalldatetime
	DECLARE @min_schedule int
	DECLARE @max_schedule int
	DECLARE @min_agent int
	DECLARE @max_agent int
	DECLARE @chunkSizeDays int 
	DECLARE @schedule_rows int
	DECLARE @number_of_bu int
	DECLARE @business_unit_code uniqueidentifier
	DECLARE @parameter_string nvarchar(1000)
	DECLARE @stored_procedured nvarchar(300)
	DECLARE @max_schedule_rows int
	
	SET @max_schedule_rows = 250000
	SELECT @number_of_bu = COUNT(*) FROM mart.dim_business_unit
	SELECT @schedule_rows=ddps.row_count 
	FROM sys.objects so
	JOIN sys.indexes si ON si.OBJECT_ID = so.OBJECT_ID
	JOIN sys.dm_db_partition_stats AS ddps ON si.OBJECT_ID = ddps.OBJECT_ID  AND si.index_id = ddps.index_id
	WHERE si.index_id < 2  AND so.is_ms_shipped = 0
	and so.name = 'fact_schedule'
	
	SELECT @max_schedule=MAX(schedule_date_id), @min_schedule=MIN(schedule_date_id) FROM mart.fact_schedule
	SELECT @max_agent=MAX(date_id), @min_agent=MIN(date_id) FROM mart.fact_agent
	SELECT @min_date=MAX(d.date_date) --Get largest @min_date
		from (select @min_schedule as date_id union all	select @min_agent as date_id) as minDate
		inner join mart.dim_date d on minDate.date_id = d.date_id
	SELECT @max_date = MIN(d.date_date) --Get smallest @max_date
		from (select @max_schedule as date_id union all select @max_agent as date_id) as maxDate
		inner join mart.dim_date d on maxDate.date_id = d.date_id

	--will be re-loaded as part of "mart.etl_job_delayed"
	TRUNCATE TABLE mart.fact_schedule_deviation

	SELECT @chunkSizeDays =
		CASE
			WHEN  (@schedule_rows/@max_schedule_rows) > 0 THEN @number_of_bu * (DATEDIFF(dd,@min_date,@max_date)/(@schedule_rows/@max_schedule_rows))
			ELSE 100
		END

	SELECT @tmp_min_date=@max_date
	
	--If we need to Chunk- add a delayed job for periods older than @chunkSizeDays
		WHILE @tmp_min_date > DATEADD(dd,-@chunkSizeDays,@min_date)
		BEGIN

			INSERT INTO mart.etl_job_delayed(stored_procedured,parameter_string)
			SELECT 'mart.etl_fact_schedule_deviation_load','@start_date=''' + CONVERT(nvarchar(30), @tmp_min_date, 126) + ''',@end_date=''' + CONVERT(nvarchar(30),(DATEADD(dd,@chunkSizeDays,@tmp_min_date)), 126) + ''',@business_unit_code='''+ cast(business_unit_code as nvarchar(36))+ ''''
			FROM mart.dim_business_unit
			WHERE business_unit_id <> -1
			AND business_unit_id is not null
			
			SET @tmp_min_date = DATEADD(dd,-@chunkSizeDays,@tmp_min_date)
		END
	PRINT 'Delayed job added for: mart.etl_fact_schedule_deviation_load'
GO

--============================

IF NOT EXISTS(SELECT 1 FROM mart.sys_configuration WHERE [key]='AdherenceMinutesOutsideShift')
	INSERT INTO mart.sys_configuration([key], value, insert_date)
	SELECT 'AdherenceMinutesOutsideShift', 120, GETDATE()

UPDATE mart.report_control_collection
SET control_name_resource_key = 'ResShiftStartDateColon'
WHERE collection_id=37 AND control_id =6
AND control_name_resource_key ='ResDateColon'

UPDATE mart.report_control_collection
SET control_name_resource_key = 'ResShiftStartDateFromColon'
WHERE collection_id=42 AND control_id =1
AND control_name_resource_key ='ResDateFromColon'

UPDATE mart.report_control_collection
SET control_name_resource_key = 'ResShiftStartDateToColon'
WHERE collection_id=42 AND control_id =2
AND control_name_resource_key ='ResDateToColon'




----------------  
--Name: David
--Date: 2013-02-07
--Desc: bug #22206
----------------  
--Check if previous table exists, in that case BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[permission_report]') AND type in (N'U'))
BEGIN
	CREATE TABLE [mart].[permission_report_A](
		[person_code] uniqueidentifier NOT NULL,
		[team_id] int NOT NULL,
		[my_own] bit NOT NULL,
		[business_unit_id] int NOT NULL,
		[datasource_id] smallint NOT NULL,
		[ReportId] uniqueidentifier NOT NULL,
		[datasource_update_date] smalldatetime NULL,
		[table_name] char(1) NOT NULL,
	CONSTRAINT [PK_permission_report_A] PRIMARY KEY CLUSTERED 
		(
			table_name ASC,
			[person_code] ASC,
			[team_id] ASC,
			[my_own] ASC,
			[business_unit_id] ASC,
			[ReportId] ASC
		),
	CONSTRAINT [CK_permission_report_A] CHECK
		(
		table_name = 'A'
		)
	)

	ALTER TABLE [mart].[permission_report_A] ADD  CONSTRAINT [DF_permission_report_A_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]

	CREATE TABLE [mart].[permission_report_B](
		[person_code] uniqueidentifier NOT NULL,
		[team_id] int NOT NULL,
		[my_own] bit NOT NULL,
		[business_unit_id] int NOT NULL,
		[datasource_id] smallint NOT NULL,
		[ReportId] uniqueidentifier NOT NULL,
		[datasource_update_date] smalldatetime NULL,
		[table_name] char(1) NOT NULL,
	CONSTRAINT [PK_permission_report_B] PRIMARY KEY CLUSTERED 
		(
			table_name ASC,
			[person_code] ASC,
			[team_id] ASC,
			[my_own] ASC,
			[business_unit_id] ASC,
			[ReportId] ASC
		),
	CONSTRAINT [CK_permission_report_B] CHECK
		(
		table_name = 'B'
		)
	)

	ALTER TABLE [mart].[permission_report_B] ADD  CONSTRAINT [DF_permission_report_B_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]

	CREATE TABLE [mart].[permission_report_active](
		[lock] [char](1) NOT NULL,
		[is_active] [char](1) NOT NULL
	)
	
	ALTER TABLE [mart].[permission_report_active] ADD CONSTRAINT PK_permission_report_active PRIMARY KEY CLUSTERED 
	(
		[lock] ASC
	)

	--only accept 'A' or 'B'
	ALTER TABLE [mart].[permission_report_active]  WITH CHECK ADD CONSTRAINT [CK_OnlyAorB] CHECK  (([is_active]='B' OR [is_active]='A'))
	--only accept one single row
	ALTER TABLE [mart].[permission_report_active]  WITH CHECK ADD CONSTRAINT [CK_OnlyOneRow] CHECK  (([lock]='x'))
	--default to 'x' for any new row
	ALTER TABLE [mart].[permission_report_active] ADD CONSTRAINT [DF_permission_report_active_lock] DEFAULT ('x') FOR [lock]
		
	INSERT INTO [mart].[permission_report_A]
		(person_code, team_id, my_own, business_unit_id, datasource_id, ReportId, datasource_update_date, table_name)
	SELECT [person_code]
		  ,[team_id]
		  ,[my_own]
		  ,[business_unit_id]
		  ,[datasource_id]
		  ,[ReportId]
		  ,[datasource_update_date]
		  ,'A'
	  FROM [mart].[permission_report]

	--set A as active
	INSERT INTO [mart].[permission_report_active] (is_active)
	SELECT 'A'

	DROP TABLE [mart].[permission_report]
END	
GO

--Drop obsolete SP
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_permission_data_check_test]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_permission_data_check_test]
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (380,'7.3.380') 
