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

TRUNCATE TABLE mart.fact_schedule_deviation

ALTER TABLE  mart.fact_schedule_deviation ADD shift_startdate_id int NULL


ALTER TABLE mart.fact_schedule_deviation DROP COLUMN datasource_update_date

IF NOT EXISTS(SELECT 1 FROM mart.sys_configuration WHERE [key]='AdherenceMinutesOutsideShift')
	INSERT INTO mart.sys_configuration([key], value, insert_date)
	SELECT 'AdherenceMinutesOutsideShift', 120, GETDATE()

