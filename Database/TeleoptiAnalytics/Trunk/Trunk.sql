----------------  
--Name: Karin and David
--Date: 2012-12-12
--Desc: PBI21517
----------------  
UPDATE mart.report
SET help_key = REPLACE(help_key,'.html','')
WHERE RIGHT(help_key,5) = '.html'

UPDATE mart.report
SET help_key = REPLACE(help_key,'f01_','f01:')
WHERE LEFT(help_key,4) = 'f01_'

UPDATE mart.report
SET help_key = REPLACE(help_key,'_','+')
WHERE SUBSTRING(help_key,11,1) = '_'

UPDATE mart.report
SET help_key='f01:Report+AbsenceTimePerAbsence'
WHERE Id='D45A8874-57E1-4EB9-826D-E216A4CBC45B'

UPDATE mart.report
SET help_key='f01:Report+AbsenceTimePerAgent'
WHERE Id='C5B88862-F7BE-431B-A63F-3DD5FF8ACE54'


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
