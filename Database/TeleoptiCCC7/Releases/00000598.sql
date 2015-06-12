----------------  
--Name: Asad
--Desc: Added new fields to table for planning period. 
---------------- 
DELETE FROM dbo.PlanningPeriod
GO

ALTER TABLE dbo.PlanningPeriod
	ADD PeriodType int not null
	
ALTER TABLE dbo.PlanningPeriod
	ADD [Number] int not null
	
GO