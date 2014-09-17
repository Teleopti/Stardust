----------------  
--Name: Xinfeng Li
--Date: 2014-09-11  
--Desc: Add new application function "View Badge"
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
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy],[UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1, @SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0101' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ViewBadge' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxViewBadge' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

/*
--Name: Tamas Balog
--Date: 2014-09-16  
--Desc: Add backup column for UseShiftCategoryFairness in WorkflowControlSet
--and save the current UseShiftCategoryFairness data there
----------------  
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'UseShiftCategoryFairnessSave' AND [object_id] = OBJECT_ID(N'WorkflowControlSet'))
	ALTER TABLE [dbo].[WorkflowControlSet]
	ADD [UseShiftCategoryFairnessSave] bit NOT NULL
	CONSTRAINT DF_WorkflowControlSet_UseShiftCategoryFairnessSave DEFAULT ((0))
GO
UPDATE [dbo].[WorkflowControlSet]
	SET [UseShiftCategoryFairnessSave] = [UseShiftCategoryFairness] 
GO
UPDATE [dbo].[WorkflowControlSet]
	SET [UseShiftCategoryFairness] = 1 
GO
*/


--Name: Micke Deigård
--Date: 2014-09-18  
--Desc: Remove backup column for UseShiftCategoryFairness in WorkflowControlSet if someone already got it
----------------  
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'UseShiftCategoryFairnessSave' AND [object_id] = OBJECT_ID(N'WorkflowControlSet'))
	ALTER TABLE [dbo].[WorkflowControlSet]
	DROP COLUMN [UseShiftCategoryFairnessSave]
GO