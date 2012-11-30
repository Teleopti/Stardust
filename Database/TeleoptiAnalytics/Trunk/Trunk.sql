-----------------  
---Name: Jonas
---Date: 2012-11-30  
---Desc: Bug #21668 Change column for day off ShortName to nullable in stage.stg_schedule_preference table.
-----------------  
TRUNCATE TABLE stage.stg_schedule_preference
ALTER TABLE stage.stg_schedule_preference ALTER COLUMN day_off_shortname NVARCHAR(25) NULL