/* 
Trunk initiated: 
2011-04-04 
14:14
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2011-04-05
--Desc: Adding clustered indexes to all tables (Azure pre-req)
---------------- 
--update existing
declare @indexname nvarchar(2000)
select @indexname=name from sys.indexes
where object_name(object_id)= 'SchedulePeriodShiftCategoryLimitation'
and is_unique = 1

exec sp_rename @objname = @indexname, @newname = 'UQ_SchedulePeriodShiftCategoryLimitation', @objtype = 'OBJECT'
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SchedulePeriodShiftCategoryLimitation]') AND name = N'UQ_SchedulePeriodShiftCategoryLimitation')
ALTER TABLE [dbo].[SchedulePeriodShiftCategoryLimitation] DROP CONSTRAINT [UQ_SchedulePeriodShiftCategoryLimitation]
GO

ALTER TABLE [dbo].[SchedulePeriodShiftCategoryLimitation] ADD  CONSTRAINT [UQ_SchedulePeriodShiftCategoryLimitation] UNIQUE CLUSTERED 
(
	[SchedulePeriod] ASC,
	[ShiftCategory] ASC
) ON [PRIMARY]
GO

declare @indexname nvarchar(2000)
select @indexname=name from sys.indexes
where object_name(object_id)= 'MultiplicatorDefinitionSetCollection'
and is_unique = 1

exec sp_rename @objname = @indexname, @newname = 'UQ_MultiplicatorDefinitionSetCollection', @objtype = 'OBJECT'
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MultiplicatorDefinitionSetCollection]') AND name = N'UQ_MultiplicatorDefinitionSetCollection')
ALTER TABLE [dbo].[MultiplicatorDefinitionSetCollection] DROP CONSTRAINT [UQ_MultiplicatorDefinitionSetCollection]
GO

ALTER TABLE [dbo].[MultiplicatorDefinitionSetCollection] ADD  CONSTRAINT [UQ_MultiplicatorDefinitionSetCollection] UNIQUE CLUSTERED 
(
	[Contract] ASC,
	[MultiplicatorDefinitionSet] ASC
) ON [PRIMARY]
GO




IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationFunctionInRole]') AND name = N'UQ_ApplicationFunctionInRole_ApplicationRole')
ALTER TABLE [dbo].[ApplicationFunctionInRole] DROP CONSTRAINT [UQ_ApplicationFunctionInRole_ApplicationRole]
GO

ALTER TABLE [dbo].[ApplicationFunctionInRole] ADD UNIQUE CLUSTERED 
(
	[ApplicationRole] ASC,
	[ApplicationFunction] ASC
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonInApplicationRole]') AND name = N'UQ_PersonInApplicationRole_Person')
ALTER TABLE [dbo].[PersonInApplicationRole] DROP CONSTRAINT [UQ_PersonInApplicationRole_Person]
GO

ALTER TABLE [dbo].[PersonInApplicationRole] ADD UNIQUE CLUSTERED 
(
	[Person] ASC,
	[ApplicationRole] ASC
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ExternalLogOnCollection]') AND name = N'UQ_ExternalLogOnCollection_PersonPeriod')
ALTER TABLE [dbo].[ExternalLogOnCollection] DROP CONSTRAINT [UQ_ExternalLogOnCollection_PersonPeriod]
GO

ALTER TABLE [dbo].[ExternalLogOnCollection] ADD UNIQUE CLUSTERED 
(
	[PersonPeriod] ASC,
	[ExternalLogOn] ASC
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QueueSourceCollection]') AND name = N'UQ_QueueSourceCollection_Workload')
ALTER TABLE [dbo].[QueueSourceCollection] DROP CONSTRAINT [UQ_QueueSourceCollection_Workload]
GO

ALTER TABLE [dbo].[QueueSourceCollection] ADD UNIQUE CLUSTERED 
(
	[Workload] ASC,
	[QueueSource] ASC
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KeyPerformanceIndicatorCollection]') AND name = N'UQ_KeyPerformanceIndicatorCollection_Scorecard')
ALTER TABLE [dbo].[KeyPerformanceIndicatorCollection] DROP CONSTRAINT [UQ_KeyPerformanceIndicatorCollection_Scorecard]
GO

ALTER TABLE [dbo].[KeyPerformanceIndicatorCollection] ADD UNIQUE CLUSTERED 
(
	[Scorecard] ASC,
	[KeyPerformanceIndicator] ASC
) ON [PRIMARY]
GO



--none existing
CREATE CLUSTERED INDEX [CIX_PersonGroup_PersonGroup] ON [dbo].[PersonGroup] 
(
	[PersonGroup] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_RuleSetRuleSetBag_RuleSet] ON [dbo].[RuleSetRuleSetBag] 
(
	[RuleSet] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_WorkflowControlSetAllowedShiftCategories_WorkflowControlSet] ON [dbo].[WorkflowControlSetAllowedShiftCategories] 
(
	[WorkflowControlSet] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_WorkflowControlSetSkills_WorkflowControlSet] ON [dbo].[WorkflowControlSetSkills]
(
	[WorkflowControlSet] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_WorkflowControlSetAllowedDayOffs_WorkflowControlSet] ON [dbo].[WorkflowControlSetAllowedDayOffs]
(
	[WorkflowControlSet] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_AccessibilityDaysOfWeek_RuleSet] ON [dbo].[AccessibilityDaysOfWeek]
(
	[RuleSet] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_AccessibilityDates_RuleSet] ON [dbo].[AccessibilityDates]
(
	[RuleSet] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_OutlierDates_Parent] ON [dbo].[OutlierDates]
(
	[Parent] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_ReplyOptions_Id] ON [dbo].[ReplyOptions]
(
	[Id] ASC
) ON [PRIMARY]
GO


CREATE CLUSTERED INDEX [CIX_AvailableUnitsInApplicationRole_AvailableData] ON [dbo].[AvailableUnitsInApplicationRole]
(
	[AvailableData] ASC
) ON [PRIMARY]
GO


CREATE CLUSTERED INDEX [CIX_AvailablePersonsInApplicationRole_AvailableData] ON [dbo].[AvailablePersonsInApplicationRole]
(
	[AvailableData] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_RecurrentWeeklyMeetingWeekDays_RecurrentWeeklyMeeting] ON [dbo].[RecurrentWeeklyMeetingWeekDays]
(
	[RecurrentWeeklyMeeting] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_MasterActivityCollection_MasterActivity] ON [dbo].[MasterActivityCollection]
(
	[MasterActivity] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_SkillCollection_BudgetGroup] ON [dbo].[SkillCollection]
(
	[BudgetGroup] ASC
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_stg_queue_date] ON [stage].[stg_queue]
(
	[date] ASC
) ON [PRIMARY]
GO
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (323,'7.1.323') 
