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
--Desc: PBI21633
----------------  
ALTER TABLE mart.fact_schedule_deviation DROP COLUMN datasource_update_date
ALTER TABLE  mart.fact_schedule_deviation ADD shift_startdate_id int NULL
ALTER TABLE  mart.fact_schedule_deviation ADD shift_startinterval_id smallint NULL
GO

--will be re-loaded from SP-code
TRUNCATE TABLE mart.fact_schedule_deviation
GO

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
	
	ALTER TABLE [mart].[permission_report_active] ADD PRIMARY KEY CLUSTERED 
	(
		[lock] ASC
	)

	--only accept 'A' or 'B'
	ALTER TABLE [mart].[permission_report_active]  WITH CHECK ADD CONSTRAINT [CK_OnlyAorB] CHECK  (([is_active]='B' OR [is_active]='A'))
	--only accept one single row
	ALTER TABLE [mart].[permission_report_active]  WITH CHECK ADD CONSTRAINT [CK_OnlyOneRow] CHECK  (([lock]='x'))
	--default to 'x' for any new row
	ALTER TABLE [mart].[permission_report_active] ADD  DEFAULT ('x') FOR [lock]
		
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
