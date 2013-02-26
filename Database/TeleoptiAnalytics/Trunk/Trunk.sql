----------------  
--Name: CS
--Date: 2013-02-11  
--Desc: PBI #22024 
----------------  

-- Add column for activity preference

ALTER TABLE stage.stg_schedule_preference ADD
	activity_code uniqueidentifier NULL
GO

