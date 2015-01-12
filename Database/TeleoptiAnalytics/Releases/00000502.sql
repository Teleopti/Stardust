----------------  
--Name: JN
--Date: 2015-01-08 
--Desc: Add a toggle to be used from ETL code to toggle Intraday fact_schedule or event driven fact_schedule
----------------  
INSERT INTO mart.sys_configuration ([key], [value])
	VALUES ('ETL_SpeedUpETL_30791', 'False')
GO

----------------  
--Name: Real Time
--Comment: Make RTA ActualAgentState accept null values
----------------  

ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN StateId uniqueidentifier NULL
GO
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN ScheduledId uniqueidentifier NULL
GO
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN StateStart datetime NULL
GO
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN ScheduledNextId uniqueidentifier NULL
GO
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN NextStart datetime NULL
GO
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN AlarmId uniqueidentifier NULL
GO
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN Color INT NULL
GO
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN AlarmStart datetime NULL
GO
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN StaffingEffect float NULL
GO