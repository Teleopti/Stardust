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

--ADD LOAD SP HERE--
--<

--/>


GO
PRINT 'Adherance data will now we re-loaded. This will take some time (DBManager time out = 15 min)'
PRINT 'Please do not close this Windows!'
GO

--reload data
DECLARE @min_date smalldatetime
DECLARE @max_date smalldatetime
SET @min_date = (SELECT date_date from mart.dim_date where date_id in (Select MIN(date_id) from mart.fact_schedule_deviation))
SET @max_date = (SELECT date_date from mart.dim_date where date_id in (Select MAX(date_id) from mart.fact_schedule_deviation))

DECLARE @id uniqueidentifier
DECLARE @business_unit_code uniqueidentifier 

DECLARE business_unit_Cursor CURSOR FOR
	SELECT business_unit_code
	FROM mart.dim_business_unit
	WHERE  business_unit_id<>-1
	ORDER BY business_unit_id
OPEN business_unit_Cursor;
FETCH NEXT FROM business_unit_Cursor
INTO @business_unit_code
WHILE @@FETCH_STATUS = 0
   BEGIN 
		exec mart.etl_fact_schedule_deviation_load @min_date,@max_date,@business_unit_code
		FETCH NEXT FROM business_unit_Cursor
		INTO @business_unit_code
   END;
CLOSE business_unit_Cursor;
DEALLOCATE business_unit_Cursor;

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
