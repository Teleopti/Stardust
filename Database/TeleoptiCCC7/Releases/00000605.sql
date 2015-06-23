----------------  
--Name: Asad
--Desc: Added new fields to table for planning period. 
---------------- 
DELETE FROM dbo.PlanningPeriod
GO

ALTER TABLE dbo.PlanningPeriod
	ADD State int not null
	
GO