----------------  
--Name: Xinfeng Li
--Date: 2014-10-31
--Desc: Add new application function "Absence Report"
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
SELECT @ForeignId = '0103' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'AbsenceReport' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxAbsenceReport' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Henrik Andersson
--Date: 2014-011-03
--Desc: Add new table for adherence percentage
---------------- 

DROP TABLE [ReadModel].[AdherencePercentage]

CREATE TABLE [ReadModel].[AdherencePercentage](
	[PersonId] [uniqueidentifier] NOT NULL,
	[BelongsToDate] [smalldatetime] NOT NULL,
	[LastTimestamp] [datetime] NULL,
	[TimeInAdherence] [bigint] NULL,
	[TimeOutOfAdherence] [bigint] NULL,
	[IsLastTimeInAdherence] [bit] NULL,
 CONSTRAINT [PK_AdherencePercentage] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC,
	[BelongsToDate] ASC
)
)

GO

----------------  
--Name: Xinfeng Li
--Date: 2014-11-06
--Desc: Add new table for allowed absences for report in workflow controlset
----------------  
CREATE TABLE [dbo].[WorkflowControlSetAllowedAbsencesForReport](
	[WorkflowControlSet] [uniqueidentifier] NOT NULL,
	[Absence] [uniqueidentifier] NOT NULL
 CONSTRAINT [PK_WorkflowControlSetAllowedAbsencesForReport] PRIMARY KEY CLUSTERED 
(
	[WorkflowControlSet] ASC,
	[Absence] ASC
))

GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedAbsencesForReport]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedAbsencesForReport_Absence] FOREIGN KEY([Absence])
REFERENCES [dbo].[Absence] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedAbsencesForReport] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedAbsencesForReport_Absence]
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedAbsencesForReport]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedAbsencesForReport_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSetAllowedAbsencesForReport] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedAbsencesForReport_WorkflowControlSet]
GO