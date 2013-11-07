----------------  
--Name: David J
--Date: 2013-09-09
--Desc: Dropping CreatedBy + CreatedOn from database
---------------- 
/*
--Columns
select 'ALTER TABLE '+ sch.name +'.' + so.name + ' drop column ' + sc.name
from sys.objects so
inner join sys.schemas sch
	on so.schema_id = sch.schema_id
inner join sys.columns sc
	on so.object_id = sc.object_id
where so.type_desc='USER_TABLE'
and sc.name in ('CreatedBy','CreatedOn')
and so.name not in ('PersonRequest','PushMessage','PushMessageDialogue','PayrollExport')

--FKs
select 'ALTER TABLE ' +sch.name +'.' + so.name + ' DROP CONSTRAINT ['+ object_name(fk.constraint_object_id) +']'
from sys.foreign_key_columns fk
inner join sys.columns sc
	on fk.parent_object_id = sc.object_id
	and fk.parent_column_id = sc.column_id
inner join sys.objects so
	on so.object_id = sc.object_id
inner join sys.schemas sch
	on so.schema_id = sch.schema_id
where so.type_desc='USER_TABLE'
and sc.name in ('CreatedBy','CreatedOn')
and so.name not in ('PersonRequest','PushMessage','PushMessageDialogue','PayrollExport')
order by sch.name,so.name,sc.name
*/
--Drop indexes dynamicly depending on columns to be dropped
DECLARE @ownername SYSNAME 
DECLARE @tablename SYSNAME 
DECLARE @indexname SYSNAME 
DECLARE @sql NVARCHAR(4000) 

DECLARE dropindexes CURSOR FOR 
SELECT DISTINCT
     ind.name 
    ,t.name
	,sc.name
FROM sys.indexes ind 

INNER JOIN sys.index_columns ic 
    ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 
INNER JOIN sys.columns col 
    ON ic.object_id = col.object_id and ic.column_id = col.column_id 
INNER JOIN sys.tables t 
    ON ind.object_id = t.object_id
INNER JOIN sys.schemas sc
	ON sc.schema_id = t.schema_id 
WHERE (1=1) 
    AND ind.is_primary_key = 0 
    AND ind.is_unique = 0 
    AND ind.is_unique_constraint = 0 
    AND t.is_ms_shipped = 0 
	AND col.name in ('CreatedBy','CreatedOn')
	AND t.name not in ('PersonRequest','PushMessage','PushMessageDialogue','PayrollExport')

OPEN dropindexes 
FETCH NEXT FROM dropindexes INTO @indexname, @tablename, @ownername 
WHILE @@fetch_status = 0 
BEGIN 
	SET @sql = N'DROP INDEX '+ QUOTENAME(@indexname) +' ON '+  QUOTENAME(@ownername)+'.'+QUOTENAME(@tablename)
	EXEC sp_executesql @sql   
	FETCH NEXT FROM dropindexes INTO @indexname, @tablename, @ownername 
END 
CLOSE dropindexes 
DEALLOCATE dropindexes


--FKs depending on columns
ALTER TABLE dbo.OptionalColumn DROP CONSTRAINT [FK_OptionalColumn_Person_CreatedBy]
ALTER TABLE dbo.MultisiteDay DROP CONSTRAINT [FK_MultisiteDay_Person_CreatedBy]
ALTER TABLE dbo.ApplicationRole DROP CONSTRAINT [FK_ApplicationRole_Person_CreatedBy]
ALTER TABLE dbo.BudgetGroup DROP CONSTRAINT [FK_BudgetGroup_Person_CreatedBy]
ALTER TABLE dbo.AgentDayScheduleTag DROP CONSTRAINT [FK_AgentDayScheduleTag_Person_CreatedBy]
ALTER TABLE dbo.BusinessUnit DROP CONSTRAINT [FK_BusinessUnit_Person_CreatedBy]
ALTER TABLE dbo.BudgetDay DROP CONSTRAINT [FK_BudgetDay_Person_CreatedBy]
ALTER TABLE dbo.RtaStateGroup DROP CONSTRAINT [FK_RtaStateGroup_Person_CreatedBy]
ALTER TABLE dbo.WorkShiftRuleSet DROP CONSTRAINT [FK_WorkShiftRuleSet_Person_CreatedBy]
ALTER TABLE dbo.ApplicationFunction DROP CONSTRAINT [FK_ApplicationFunction_Person_CreatedBy]
ALTER TABLE dbo.Contract DROP CONSTRAINT [FK_Contract_Person_CreatedBy]
ALTER TABLE dbo.Absence DROP CONSTRAINT [FK_Absence_Person_CreatedBy]
ALTER TABLE dbo.KeyPerformanceIndicator DROP CONSTRAINT [FK_KeyPerformanceIndicator_Person_CreatedBy]
ALTER TABLE dbo.SkillDay DROP CONSTRAINT [FK_SkillDay_Person_CreatedBy]
ALTER TABLE dbo.StateGroupActivityAlarm DROP CONSTRAINT [FK_StateGroupActivityAlarm_Person_CreatedBy]
ALTER TABLE dbo.PersonAvailability DROP CONSTRAINT [FK_PersonAvailability_Person_CreatedBy]
ALTER TABLE dbo.License DROP CONSTRAINT [FK_License_Person_CreatedBy]
ALTER TABLE dbo.AlarmType DROP CONSTRAINT [FK_AlarmType_Person_CreatedBy]
ALTER TABLE dbo.DayOffTemplate DROP CONSTRAINT [FK_DayOff_Person_CreatedBy]
ALTER TABLE dbo.SkillType DROP CONSTRAINT [FK_SkillType_Person_CreatedBy]
ALTER TABLE dbo.ShiftCategory DROP CONSTRAINT [FK_ShiftCategory_Person_CreatedBy]
ALTER TABLE dbo.WorkflowControlSet DROP CONSTRAINT [FK_WorkflowControlSet_Person_CreatedBy]
ALTER TABLE dbo.PublicNote DROP CONSTRAINT [FK_PublicNote_Person_CreatedBy]
ALTER TABLE dbo.KpiTarget DROP CONSTRAINT [FK_KpiTarget_Person_CreatedBy]
ALTER TABLE dbo.Note DROP CONSTRAINT [FK_Note_Person_CreatedBy]
ALTER TABLE dbo.SystemRoleApplicationRoleMapper DROP CONSTRAINT [FK_SystemRoleApplicationRoleMapper_Person_CreatedBy]
ALTER TABLE dbo.PartTimePercentage DROP CONSTRAINT [FK_PartTimePercentage_Person_CreatedBy]
ALTER TABLE dbo.PersonDayOff DROP CONSTRAINT [FK_PersonDayOff_Person_CreatedBy]
ALTER TABLE dbo.PersonAbsenceAccount DROP CONSTRAINT [FK_PersonAbsenceAccount_Person_CreatedBy]
ALTER TABLE dbo.Meeting DROP CONSTRAINT [FK_Meeting_Person_CreatedBy]
ALTER TABLE dbo.PersonWriteProtectionInfo DROP CONSTRAINT [FK_WriteProtection_Person_CreatedBy]
ALTER TABLE dbo.PersonAssignment DROP CONSTRAINT [FK_PersonAssignment_Person_CreatedBy]
ALTER TABLE dbo.ContractSchedule DROP CONSTRAINT [FK_ContractSchedule_Person_CreatedBy]
ALTER TABLE dbo.GlobalSettingData DROP CONSTRAINT [FK_GlobalSetting_Person_CreatedBy]
ALTER TABLE dbo.PersonRotation DROP CONSTRAINT [FK_PersonRotation_Person_CreatedBy]
ALTER TABLE dbo.ScheduleTag DROP CONSTRAINT [FK_ScheduleTag_Person_CreatedBy]
ALTER TABLE dbo.Outlier DROP CONSTRAINT [FK_Outlier_Person_CreatedBy]
ALTER TABLE dbo.AvailableData DROP CONSTRAINT [FK_AvailableData_Person_CreatedBy]
ALTER TABLE dbo.PayrollResult DROP CONSTRAINT [FK_PayrollResult_Person_CreatedBy]
ALTER TABLE dbo.PersonAbsence DROP CONSTRAINT [FK_PersonAbsence_Person_CreatedBy]
ALTER TABLE dbo.Team DROP CONSTRAINT [FK_Team_Person_CreatedBy]
ALTER TABLE dbo.ValidatedVolumeDay DROP CONSTRAINT [FK_ValidatedVolumeDay_Person_CreatedBy]
ALTER TABLE dbo.ExtendedPreferenceTemplate DROP CONSTRAINT [FK_ExtendedPreferenceTemplate_Person_CreatedBy]
ALTER TABLE dbo.Site DROP CONSTRAINT [FK_Site_Person_CreatedBy]
ALTER TABLE dbo.Skill DROP CONSTRAINT [FK_Skill_Person_CreatedBy]
ALTER TABLE dbo.GroupPage DROP CONSTRAINT [FK_GroupPage_Person_CreatedBy]
ALTER TABLE dbo.AvailabilityRotation DROP CONSTRAINT [FK_Availability_Person_CreatedBy]
ALTER TABLE dbo.GroupingAbsence DROP CONSTRAINT [FK_GroupingAbsence_Person_CreatedBy]
ALTER TABLE dbo.PreferenceDay DROP CONSTRAINT [FK_PreferenceDay_Person_CreatedBy]
ALTER TABLE dbo.ExternalLogOn DROP CONSTRAINT [FK_ExternalLogOn_Person_CreatedBy]
ALTER TABLE dbo.Scorecard DROP CONSTRAINT [FK_Scorecard_Person_CreatedBy]
ALTER TABLE dbo.GroupingActivity DROP CONSTRAINT [FK_GroupingActivity_Person_CreatedBy]
ALTER TABLE dbo.Workload DROP CONSTRAINT [FK_Workload_Person_CreatedBy]
ALTER TABLE dbo.QueueSource DROP CONSTRAINT [FK_QueueSource_Person_CreatedBy]
ALTER TABLE dbo.ForecastFile DROP CONSTRAINT [FK_ForecastFile_Person_CreatedBy]
ALTER TABLE dbo.Multiplicator DROP CONSTRAINT [FK_Multiplicator_Person_CreatedBy]
ALTER TABLE dbo.JobResult DROP CONSTRAINT [FK_JobResult_Person_CreatedBy]
ALTER TABLE dbo.RuleSetBag DROP CONSTRAINT [FK_RuleSetBag_Person_CreatedBy]
ALTER TABLE dbo.StudentAvailabilityDay DROP CONSTRAINT [FK_StudentAvailabilityDay_Person_CreatedBy]
ALTER TABLE dbo.Activity DROP CONSTRAINT [FK_Activity_Person_CreatedBy]
ALTER TABLE dbo.MultiplicatorDefinitionSet DROP CONSTRAINT [FK_MultiplicatorDefinitionSet_Person_CreatedBy]
ALTER TABLE dbo.Rotation DROP CONSTRAINT [FK_Rotation_Person_CreatedBy]
ALTER TABLE dbo.Person DROP CONSTRAINT [FK_Person_Person_CreatedBy]
ALTER TABLE dbo.Scenario DROP CONSTRAINT [FK_Scenario_Person_CreatedBy]

--Actual columns
ALTER TABLE dbo.OptionalColumn drop column CreatedBy
ALTER TABLE dbo.OptionalColumn drop column CreatedOn
ALTER TABLE dbo.MultisiteDay drop column CreatedBy
ALTER TABLE dbo.MultisiteDay drop column CreatedOn
ALTER TABLE dbo.ApplicationRole drop column CreatedBy
ALTER TABLE dbo.ApplicationRole drop column CreatedOn
ALTER TABLE dbo.BudgetGroup drop column CreatedBy
ALTER TABLE dbo.BudgetGroup drop column CreatedOn
ALTER TABLE dbo.AgentDayScheduleTag drop column CreatedBy
ALTER TABLE dbo.AgentDayScheduleTag drop column CreatedOn
ALTER TABLE dbo.BusinessUnit drop column CreatedBy
ALTER TABLE dbo.BusinessUnit drop column CreatedOn
ALTER TABLE dbo.BudgetDay drop column CreatedBy
ALTER TABLE dbo.BudgetDay drop column CreatedOn
ALTER TABLE dbo.RtaStateGroup drop column CreatedBy
ALTER TABLE dbo.RtaStateGroup drop column CreatedOn
ALTER TABLE dbo.WorkShiftRuleSet drop column CreatedBy
ALTER TABLE dbo.WorkShiftRuleSet drop column CreatedOn
ALTER TABLE dbo.ApplicationFunction drop column CreatedBy
ALTER TABLE dbo.ApplicationFunction drop column CreatedOn
ALTER TABLE dbo.Contract drop column CreatedBy
ALTER TABLE dbo.Contract drop column CreatedOn
ALTER TABLE dbo.Absence drop column CreatedBy
ALTER TABLE dbo.Absence drop column CreatedOn
ALTER TABLE dbo.KeyPerformanceIndicator drop column CreatedBy
ALTER TABLE dbo.KeyPerformanceIndicator drop column CreatedOn
ALTER TABLE dbo.SkillDay drop column CreatedBy
ALTER TABLE dbo.SkillDay drop column CreatedOn
ALTER TABLE dbo.StateGroupActivityAlarm drop column CreatedBy
ALTER TABLE dbo.StateGroupActivityAlarm drop column CreatedOn
ALTER TABLE dbo.PersonAvailability drop column CreatedBy
ALTER TABLE dbo.PersonAvailability drop column CreatedOn
ALTER TABLE dbo.License drop column CreatedBy
ALTER TABLE dbo.License drop column CreatedOn
ALTER TABLE dbo.AlarmType drop column CreatedBy
ALTER TABLE dbo.AlarmType drop column CreatedOn
ALTER TABLE dbo.DayOffTemplate drop column CreatedBy
ALTER TABLE dbo.DayOffTemplate drop column CreatedOn
ALTER TABLE dbo.SkillType drop column CreatedBy
ALTER TABLE dbo.SkillType drop column CreatedOn
ALTER TABLE dbo.PersonAbsence_Backup drop column CreatedBy
ALTER TABLE dbo.PersonAbsence_Backup drop column CreatedOn
ALTER TABLE dbo.ShiftCategory drop column CreatedBy
ALTER TABLE dbo.ShiftCategory drop column CreatedOn
ALTER TABLE dbo.WorkflowControlSet drop column CreatedBy
ALTER TABLE dbo.WorkflowControlSet drop column CreatedOn
ALTER TABLE dbo.PublicNote drop column CreatedBy
ALTER TABLE dbo.PublicNote drop column CreatedOn
ALTER TABLE dbo.KpiTarget drop column CreatedBy
ALTER TABLE dbo.KpiTarget drop column CreatedOn
ALTER TABLE dbo.Note drop column CreatedBy
ALTER TABLE dbo.Note drop column CreatedOn
ALTER TABLE dbo.SystemRoleApplicationRoleMapper drop column CreatedBy
ALTER TABLE dbo.SystemRoleApplicationRoleMapper drop column CreatedOn
ALTER TABLE dbo.PartTimePercentage drop column CreatedBy
ALTER TABLE dbo.PartTimePercentage drop column CreatedOn
ALTER TABLE dbo.OvertimeAvailability drop column CreatedBy
ALTER TABLE dbo.OvertimeAvailability drop column CreatedOn
ALTER TABLE dbo.PersonDayOff drop column CreatedBy
ALTER TABLE dbo.PersonDayOff drop column CreatedOn
ALTER TABLE dbo.PersonAbsenceAccount drop column CreatedBy
ALTER TABLE dbo.PersonAbsenceAccount drop column CreatedOn
ALTER TABLE dbo.Meeting drop column CreatedBy
ALTER TABLE dbo.Meeting drop column CreatedOn
ALTER TABLE dbo.PersonWriteProtectionInfo drop column CreatedBy
ALTER TABLE dbo.PersonWriteProtectionInfo drop column CreatedOn
ALTER TABLE dbo.PersonAssignment drop column CreatedBy
ALTER TABLE dbo.PersonAssignment drop column CreatedOn
ALTER TABLE dbo.ContractSchedule drop column CreatedBy
ALTER TABLE dbo.ContractSchedule drop column CreatedOn
ALTER TABLE dbo.GlobalSettingData drop column CreatedBy
ALTER TABLE dbo.GlobalSettingData drop column CreatedOn
ALTER TABLE dbo.PersonRotation drop column CreatedBy
ALTER TABLE dbo.PersonRotation drop column CreatedOn
ALTER TABLE dbo.ScheduleTag drop column CreatedBy
ALTER TABLE dbo.ScheduleTag drop column CreatedOn
ALTER TABLE dbo.Outlier drop column CreatedBy
ALTER TABLE dbo.Outlier drop column CreatedOn
ALTER TABLE dbo.AvailableData drop column CreatedBy
ALTER TABLE dbo.AvailableData drop column CreatedOn
ALTER TABLE dbo.PayrollResult drop column CreatedBy
ALTER TABLE dbo.PayrollResult drop column CreatedOn
ALTER TABLE dbo.PersonAbsence drop column CreatedBy
ALTER TABLE dbo.PersonAbsence drop column CreatedOn
ALTER TABLE dbo.Team drop column CreatedBy
ALTER TABLE dbo.Team drop column CreatedOn
ALTER TABLE dbo.ValidatedVolumeDay drop column CreatedBy
ALTER TABLE dbo.ValidatedVolumeDay drop column CreatedOn
ALTER TABLE dbo.ExtendedPreferenceTemplate drop column CreatedBy
ALTER TABLE dbo.ExtendedPreferenceTemplate drop column CreatedOn
ALTER TABLE dbo.Site drop column CreatedBy
ALTER TABLE dbo.Site drop column CreatedOn
ALTER TABLE dbo.Skill drop column CreatedBy
ALTER TABLE dbo.Skill drop column CreatedOn
ALTER TABLE dbo.GroupPage drop column CreatedBy
ALTER TABLE dbo.GroupPage drop column CreatedOn
ALTER TABLE dbo.AvailabilityRotation drop column CreatedBy
ALTER TABLE dbo.AvailabilityRotation drop column CreatedOn
ALTER TABLE dbo.GroupingAbsence drop column CreatedBy
ALTER TABLE dbo.GroupingAbsence drop column CreatedOn
ALTER TABLE dbo.PreferenceDay drop column CreatedBy
ALTER TABLE dbo.PreferenceDay drop column CreatedOn
ALTER TABLE dbo.ExternalLogOn drop column CreatedBy
ALTER TABLE dbo.ExternalLogOn drop column CreatedOn
ALTER TABLE dbo.Scorecard drop column CreatedBy
ALTER TABLE dbo.Scorecard drop column CreatedOn
ALTER TABLE dbo.GroupingActivity drop column CreatedBy
ALTER TABLE dbo.GroupingActivity drop column CreatedOn
ALTER TABLE dbo.Workload drop column CreatedBy
ALTER TABLE dbo.Workload drop column CreatedOn
ALTER TABLE dbo.QueueSource drop column CreatedBy
ALTER TABLE dbo.QueueSource drop column CreatedOn
ALTER TABLE dbo.ForecastFile drop column CreatedBy
ALTER TABLE dbo.ForecastFile drop column CreatedOn
ALTER TABLE dbo.Multiplicator drop column CreatedBy
ALTER TABLE dbo.Multiplicator drop column CreatedOn
ALTER TABLE dbo.JobResult drop column CreatedBy
ALTER TABLE dbo.JobResult drop column CreatedOn
ALTER TABLE dbo.RuleSetBag drop column CreatedBy
ALTER TABLE dbo.RuleSetBag drop column CreatedOn
ALTER TABLE dbo.StudentAvailabilityDay drop column CreatedBy
ALTER TABLE dbo.StudentAvailabilityDay drop column CreatedOn
ALTER TABLE dbo.Activity drop column CreatedBy
ALTER TABLE dbo.Activity drop column CreatedOn
ALTER TABLE dbo.MultiplicatorDefinitionSet drop column CreatedBy
ALTER TABLE dbo.MultiplicatorDefinitionSet drop column CreatedOn
ALTER TABLE dbo.Rotation drop column CreatedBy
ALTER TABLE dbo.Rotation drop column CreatedOn
ALTER TABLE dbo.Person drop column CreatedBy
ALTER TABLE dbo.Person drop column CreatedOn
ALTER TABLE dbo.Scenario drop column CreatedBy
ALTER TABLE dbo.Scenario drop column CreatedOn

----------------  
--Name: Roger Kratz
--Date: 2013-09-11
--Desc: Dropping BusinessUnit from PersonAssignment
---------------- 
ALTER TABLE dbo.PersonAssignment DROP CONSTRAINT [FK_PersonAssignment_BusinessUnit]
alter table dbo.PersonAssignment drop column BusinessUnit
alter table auditing.PersonAssignment_AUD drop column BusinessUnit

----------------  
--Name: Roger Kratz
--Date: 2013-09-11
--Desc: Dropping BusinessUnit from PersonAbsence
---------------- 
ALTER TABLE dbo.PersonAbsence DROP CONSTRAINT [FK_PersonAbsence_BusinessUnit]
alter table dbo.PersonAbsence drop column BusinessUnit
alter table auditing.PersonAbsence_AUD drop column BusinessUnit

---------------- 
--Name: Kunning
--Date: 2013-08-29 
--Desc: add new appliation function MyTimeWeb/OvertimeAvailabilityWeb
----------------  
SET NOCOUNT ON
	
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0089' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'OvertimeAvailabilityWeb' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxOvertimeAvailabilityWeb' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

---------------- 
--Name: Kunning
--Date: 2013-09-18 
--Desc: add IsDeleted tag to OvertimeAvailability
---------------- 
ALTER TABLE OvertimeAvailability
ADD IsDeleted bit NULL
GO
UPDATE OvertimeAvailability
SET IsDeleted = 0
GO
ALTER TABLE OvertimeAvailability
ALTER COLUMN IsDeleted bit NOT NULL
GO

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
--Name: Mathias Stenbom
--Date: 2013-09-30
--Desc: Renaming ReadModel.PersonScheduleDay.Shift to Model to match class property name
---------------- 

EXEC sp_rename 'ReadModel.PersonScheduleDay.Shift','Model','COLUMN'
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
--Name: Mathias Stenbom
--Date: 2013-10-08
--Desc: Renaming ReadModel.PersonScheduleDay.ShiftStart to Start to match class property name
---------------- 

EXEC sp_rename 'ReadModel.PersonScheduleDay.ShiftStart','Start','COLUMN'
GO
EXEC sp_rename 'ReadModel.PersonScheduleDay.ShiftEnd','End','COLUMN'
GO

----------------  
--Name: Roger Kratz
--Date: 2013-10-11
--Desc: Removing unused table
---------------- 

DROP TABLE [dbo].[OutlierDateProviderBase]
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
--Name: Roger Kratz
--Date: 2013-10-17
--Desc: Removing unused table GroupingAbsence
---------------- 
ALTER TABLE [dbo].[Absence] DROP CONSTRAINT [FK_Absence_GroupingAbsence]
GO
alter table [dbo].[Absence]
drop column GroupingAbsence
GO
DROP TABLE [dbo].[GroupingAbsence]
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

----------------  
--Name: David Jonsson
--Date: 2013-10-29
--Desc: Bug #25358 - re-design clustered key on ReadModel.PersonScheduleDay
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ReadModel].[PersonScheduleDay]') AND name = N'CIX_PersonScheduleDay')
BEGIN
	CREATE TABLE [ReadModel].[New_PersonScheduleDay](
		[Id] [uniqueidentifier] NOT NULL,
		[PersonId] [uniqueidentifier] NOT NULL,
		[TeamId] [uniqueidentifier] NOT NULL,
		[BelongsToDate] [smalldatetime] NOT NULL,
		[Start] [datetime] NULL,
		[End] [datetime] NULL,
		[SiteId] [uniqueidentifier] NOT NULL,
		[BusinessUnitId] [uniqueidentifier] NOT NULL,
		[InsertedOn] [datetime] NOT NULL,
		[Model] [nvarchar](max) NOT NULL
	)

	ALTER TABLE [ReadModel].[New_PersonScheduleDay] ADD  CONSTRAINT [new_PK_PersonScheduleDay] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)

	CREATE CLUSTERED INDEX [CIX_PersonScheduleDay] ON [ReadModel].[New_PersonScheduleDay]
	(
		[PersonId] ASC,
		[BelongsToDate] ASC
	)

	ALTER TABLE [ReadModel].[PersonScheduleDay] DROP CONSTRAINT [DF_PersonScheduleDay_InsertedOn]
	ALTER TABLE [ReadModel].[New_PersonScheduleDay] ADD  CONSTRAINT [DF_PersonScheduleDay_InsertedOn]  DEFAULT (getutcdate()) FOR [InsertedOn]

	--Re-create data
	INSERT INTO [ReadModel].[New_PersonScheduleDay]
	SELECT * FROM [ReadModel].[PersonScheduleDay]

	--drop old table
	DROP TABLE [ReadModel].[PersonScheduleDay]

	EXEC dbo.sp_rename @objname = N'[ReadModel].[New_PersonScheduleDay]', @newname = N'PersonScheduleDay', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[ReadModel].[PersonScheduleDay].[new_PK_PersonScheduleDay]', @newname = N'PK_PersonScheduleDay', @objtype =N'INDEX'
END
GO

----------------  
--Name: David Jonsson
--Date: 2013-10-29
--Desc: Bug #25370 - re-design clustered key on dbo.OvertimeShiftActivityLayer
----------------
--No, not on this version. Tables is removed as part of "0000000386.sql"
GO

----------------  
--Name: David Jonsson
--Date: 2013-11-01
--Desc: Bug #25310 - Crash In ETL tool on stg_request, timout
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Request]') AND name = N'IX_Request_StartDateTime_Id_Parent')
CREATE NONCLUSTERED INDEX [IX_Request_StartDateTime_Id_Parent] ON [dbo].[Request]
(
	[StartDateTime] ASC
)
INCLUDE (
[Id],
[Parent]
)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Request]') AND name = N'IX_Request_Parent')
CREATE NONCLUSTERED INDEX [IX_Request_Parent] ON [dbo].[Request]
(
	[Parent] ASC
)
INCLUDE ( 	[Id],
	[StartDateTime],
	[EndDateTime]
	)
GO

----------------  
--Name: David Jonsson
--Date: 2013-11-05
--Desc: Bug #25599 - ReadModel.GetNextActivityStartTime is super sloooow
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduleProjectionReadOnly]') AND name = N'IX_ScheduleProjectionReadOnly_PersonId_EndDateTime_StartDateTime')
CREATE NONCLUSTERED INDEX IX_ScheduleProjectionReadOnly_PersonId_EndDateTime_StartDateTime
ON [ReadModel].[ScheduleProjectionReadOnly]
(
	[PersonId],
	[EndDateTime]
)
INCLUDE
(
	[StartDateTime]
)
GO

----------------  
--Name: David Jonsson
--Date: 2013-11-05
--Desc: Bug #25359 - Prepare (redesign) the tables and optimize for the initial load of [ReadModel].[ScheduledResources] and [ReadModel].[ActivitySkillCombination]
-- Intial load of theese are disabled until we need decided on when to deploy "AnyWhere"
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduledResources]') AND name = N'CIX_ScheduledResources')
BEGIN
	CREATE TABLE [ReadModel].[ScheduledResources_new](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[ActivitySkillCombinationId] [int] NOT NULL,
		[Resources] [float] NOT NULL,
		[Heads] [float] NOT NULL,
		[PeriodStart] [datetime] NOT NULL,
		[PeriodEnd] [datetime] NOT NULL,
	 CONSTRAINT [PK_ScheduledResources_new] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	)

	CREATE CLUSTERED INDEX [CIX_ScheduledResources] ON [ReadModel].[ScheduledResources_new]
	(
		[ActivitySkillCombinationId] ASC,
		[PeriodStart] ASC
	)

	ALTER TABLE [ReadModel].[ScheduledResources] DROP CONSTRAINT [DF_ScheduledResources_Resources]
	ALTER TABLE [ReadModel].[ScheduledResources_new] ADD  CONSTRAINT [DF_ScheduledResources_Resources]  DEFAULT ((0)) FOR [Resources]

	ALTER TABLE [ReadModel].[ScheduledResources] DROP CONSTRAINT [DF_ScheduledResources_Heads]
	ALTER TABLE [ReadModel].[ScheduledResources_new] ADD  CONSTRAINT [DF_ScheduledResources_Heads]  DEFAULT ((0)) FOR [Heads]

	--Re-create data
	SET IDENTITY_INSERT [ReadModel].[ScheduledResources_new] ON
	INSERT INTO [ReadModel].[ScheduledResources_new](Id, ActivitySkillCombinationId, Resources, Heads, PeriodStart, PeriodEnd)
	SELECT Id, ActivitySkillCombinationId, Resources, Heads, PeriodStart, PeriodEnd
	FROM [ReadModel].[ScheduledResources]
	SET IDENTITY_INSERT [ReadModel].[ScheduledResources_new] OFF
	
	--drop old table
	DROP TABLE [ReadModel].[ScheduledResources]

	EXEC dbo.sp_rename @objname = N'[ReadModel].[ScheduledResources_new]', @newname = N'ScheduledResources', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[ReadModel].[ScheduledResources].[PK_ScheduledResources_new]', @newname = N'PK_ScheduledResources', @objtype =N'INDEX'
END
GO
--And a supporting index on [ReadModel].[ActivitySkillCombination]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ReadModel].[ActivitySkillCombination]') AND name = N'IX_ActivitySkillCombination')
	CREATE NONCLUSTERED INDEX [IX_ActivitySkillCombination] ON [ReadModel].[ActivitySkillCombination]
	(
		[Activity] ASC
	)
	INCLUDE ([Skills]) 
GO

--Re-design indexes on [dbo].[PersonAbsence]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAbsence]') AND name = N'CIX_PersonAbsence')
BEGIN
	CREATE TABLE [dbo].[PersonAbsence_new](
		[Id] [uniqueidentifier] NOT NULL,
		[Version] [int] NOT NULL,
		[UpdatedBy] [uniqueidentifier] NOT NULL,
		[UpdatedOn] [datetime] NOT NULL,
		[LastChange] [datetime] NULL,
		[Person] [uniqueidentifier] NOT NULL,
		[Scenario] [uniqueidentifier] NOT NULL,
		[PayLoad] [uniqueidentifier] NOT NULL,
		[Minimum] [datetime] NOT NULL,
		[Maximum] [datetime] NOT NULL,
	 CONSTRAINT [PK_PersonAbsence_new] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	)

	CREATE CLUSTERED INDEX [CIX_PersonAbsence] ON [dbo].[PersonAbsence_new]
	(
		[Person] ASC
	)

	--Re-create data
	INSERT INTO [dbo].[PersonAbsence_new]
	SELECT * FROM [dbo].[PersonAbsence]
	
	--drop old table
	DROP TABLE [dbo].[PersonAbsence]

	EXEC dbo.sp_rename @objname = N'[dbo].[PersonAbsence_new]', @newname = N'PersonAbsence', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[dbo].[PersonAbsence].[PK_PersonAbsence_new]', @newname = N'PK_PersonAbsence', @objtype =N'INDEX'

	--Add additional indexes
	CREATE NONCLUSTERED INDEX [IX_PersonAbsence_Scenario] ON [dbo].[PersonAbsence]
	(
		[Scenario] ASC
	)

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Absence] FOREIGN KEY([PayLoad])
	REFERENCES [dbo].[Absence] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Absence]

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_BusinessUnit] FOREIGN KEY([BusinessUnit])
	REFERENCES [dbo].[BusinessUnit] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_BusinessUnit]

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Person_UpdatedBy]

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Person3] FOREIGN KEY([Person])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Person3]

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Scenario] FOREIGN KEY([Scenario])
	REFERENCES [dbo].[Scenario] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Scenario]
END