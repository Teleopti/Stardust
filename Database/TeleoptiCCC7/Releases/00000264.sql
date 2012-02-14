/* 
Trunk initiated: 
2010-05-14 12:55 
by: /tamasb
On TELEOPTI494 
*/ 
----------------  
--Name: tamasb
--Date: 2010-05-14 12:55 
--Desc: Add the following new application function> Online Reports
--		Online Reports application function
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
--Not needed. DBManager should catch and report this error if we run in to it!
/*
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, 'System', 'System', 'UTC', '_Super User', 0, 1) 
*/

--get parent level
SELECT @ParentForeignId = '0001'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0054' -- Foreign id of the function that is hardcoded	
SELECT @FunctionCode = 'OnlineReports' -- Name of the function that is hardcoded
SELECT @FunctionDescription = 'xxOnlineReports' -- Description of the function that is hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

-- end of script

----------------  
--Name: Tamasb
--Date: 2010-05-14 16:15 
--Desc: Add the following new application function> Scheduled Time per Activity
--      Scheduled Time per Activity Report application function
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
--Not needed. DBManager should catch and report this error if we run in to it!
/*
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, 'System', 'System', 'UTC', '_Super User', 0, 1) 
*/

--get parent level
SELECT @ParentForeignId = '0054'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0055' -- Foreign id of the function that is hardcoded	
SELECT @FunctionCode = 'ScheduledTimePerActivityReport' -- Name of the function that is hardcoded
SELECT @FunctionDescription = 'xxScheduledTimePerActivityReport' -- Description of the function that is hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Robin
--Date: 2010-05-12
--Desc: Rename @SuperUserId login as: System System
----------------  
UPDATE Person SET FirstName = 'System', LastName='System' WHERE Id = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

----------------  
--Name: David
--Date: 2010-05-17
--Desc: Remove Datamart user from the application
----------------  
UPDATE person SET IsDeleted = 1 WHERE ApplicationLogOnName = 'datamart' AND FirstName = 'datamart' AND LastName = 'datamart'

----------------  
--Name: Zoë
--Date: 2010-05-19
--Desc: New column in WorkflowControlSet
----------------  
ALTER TABLE [dbo].[WorkflowControlSet] ADD [AllowedPreferenceActivity] [uniqueidentifier] NULL
GO
ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_Activity_AllowedPreferenceActivity] FOREIGN KEY([AllowedPreferenceActivity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_Activity_AllowedPreferenceActivity]
GO

----------------  
--Name: Ola
--Date: 2010-05-19
--Desc: Move columns (not data) from Team to WorkflowControlSet
alter TABLE WorkflowControlSet
ADD [SchedulePublishedToDate] [datetime] NULL,
	[SchedulePreferenceDate] [datetime] NULL,
	[WriteProtection] [int] NULL
	
GO
ALTER TABLE dbo.Team
	DROP COLUMN SchedulePublishedToDate, SchedulePreferenceDate, WriteProtection
GO	


----------------  
--Name: Ola
--Date: 2010-05-21
--Desc: New column in WorkflowControlSet
ALTER TABLE [dbo].[WorkflowControlSet] ADD [ShiftTradeTargetTimeFlexibility] [bigint] NULL

GO

----------------  
--Name: Ola
--Date: 2010-05-21
--Desc: New table to connect skills to WorkflowControlSet

CREATE TABLE [dbo].[WorkflowControlSetSkills](
	[WorkflowControlSet] [uniqueidentifier] NOT NULL,
	[Skill] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[WorkflowControlSetSkills]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetSkills_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetSkills] CHECK CONSTRAINT [FK_WorkflowControlSetSkills_Skill]
GO

ALTER TABLE [dbo].[WorkflowControlSetSkills]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetSkills_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetSkills] CHECK CONSTRAINT [FK_WorkflowControlSetSkills_WorkflowControlSet]
GO

----------------  
--Name: Ola
--Date: 2010-05-21
--Desc: New column in WorkflowControlSet
ALTER TABLE [dbo].[WorkflowControlSet] ADD [ShiftTradeOpenPeriodDaysForwardMinimum] [int] NULL,
	[ShiftTradeOpenPeriodDaysForwardMaximum] [int] NULL
GO

UPDATE [WorkflowControlSet] SET [ShiftTradeOpenPeriodDaysForwardMinimum] = 0
UPDATE [WorkflowControlSet] SET [ShiftTradeOpenPeriodDaysForwardMaximum] = 0

ALTER TABLE [dbo].[WorkflowControlSet] ALTER COLUMN [ShiftTradeOpenPeriodDaysForwardMinimum] [int] NOT  NULL
ALTER TABLE [dbo].[WorkflowControlSet] ALTER COLUMN [ShiftTradeOpenPeriodDaysForwardMaximum] [int] NOT NULL

GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (264,'7.1.264') 
