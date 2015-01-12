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
