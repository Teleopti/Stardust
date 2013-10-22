

----------------  
--Name: Robin Karlsson
--Date: 2013-09-26
--Desc: Bug #24859 - Fix messed up values in shifts
---------------- 
update ActivityExtender
set earlystart = 0 where EarlyStart = 1

update ActivityExtender
set LateStart = 0 where LateStart = 1
GO

----------------  
--Name: David Jonsson
--Date: 2013-10-02
--Desc: Bug #24989 - Make database compatible with SQL 2005
---------------- 
ALTER TABLE [dbo].[OvertimeAvailability]
ALTER COLUMN [DateOfOvertime] [datetime] NOT NULL


----------------  
--Name: Erik Sundberg
--Date: 2013-10-03
--Desc: Bug #25008 - RTA does not differentiate between no scheduled activity and no alarm
---------------- 
ALTER TABLE dbo.StateGroupActivityAlarm ALTER COLUMN AlarmType uniqueidentifier NULL
GO

----------------  
--Name:CS
--Date: 2013-10-11
--Desc: Bug #25079 - Unclear permission setting for overtime availability
---------------- 
update dbo.ApplicationFunction
set FunctionCode = 'ModifyAvailabilities' where ForeignId = '0087'

update dbo.ApplicationFunction
set FunctionDescription = 'xxModifyAvailabilities' where ForeignId = '0087'
GO

----------------  
--Name: David J
--Date: 2013-10-22
--Desc: Bug #24969: Timeout during upgrade when running EXEC [dbo].[DayOffConverter]
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn')
CREATE NONCLUSTERED INDEX [IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn]
ON [dbo].[PersonAssignment] ([Scenario])
INCLUDE ([UpdatedBy],[UpdatedOn])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonDayOff]') AND name = N'IX_PersonDayOff_BU_Name_Anchor_Person_Scenario')
CREATE NONCLUSTERED INDEX [IX_PersonDayOff_BU_Name_Anchor_Person_Scenario]
ON [dbo].[PersonDayOff] ([BusinessUnit],[Name])
INCLUDE ([Anchor],[Person],[Scenario])
GO
