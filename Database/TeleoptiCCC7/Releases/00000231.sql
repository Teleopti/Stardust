/* 
Trunk initiated: 
2010-04-14 
15:12
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  



----------------  
--Name: Henry and Robin
--Date: 2010-04-15
--Desc: PBI 10101  
----------------  
CREATE TABLE [dbo].[WorkflowControlSet](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_BusinessUnit]
GO

ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_Person_CreatedBy]
GO

ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_Person_UpdatedBy]
GO

-- Person
ALTER TABLE Person ADD [WorkflowControlSet] [uniqueidentifier] NULL
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_Person_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO

ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_Person_WorkflowControlSet]
GO

----------------  
--Name: Zoë
--Date: 2010-04-16
--Desc: PBI 10138  
----------------  

CREATE TABLE [dbo].[Note](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[NoteDate] [datetime] NOT NULL,
	[ScheduleNote] [nvarchar](255) NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_BusinessUnit]
GO

ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_Person]
GO

ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_Person_CreatedBy]
GO

ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO

ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_Scenario]

----------------  
--Name: CS  
--Date: 2010-04-19 
--Desc: Adding MinTimeSchedulePeriod to Contract 
----------------  
ALTER TABLE dbo.Contract ADD
MinTimeSchedulePeriod bigint NULL

GO

UPDATE  dbo.Contract set MinTimeSchedulePeriod = 0

GO 

ALTER TABLE Contract ALTER COLUMN MinTimeSchedulePeriod bigint NOT NULL
GO 

----------------  
--Name: jonas n
--Date: 2010-04-19  
--Desc: Add new application function Options/AbsenceRequests
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
	INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
		VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1) 

--get parent level
SELECT @ParentForeignId = '0017'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0053' --Foreign id of the function that is hardcoded	
SELECT @FunctionCode = 'AbsenceRequests' --Name of the function that is hardcoded
SELECT @FunctionDescription = 'xxAbsenceRequest' --Description of the function that is hardcoded

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
	INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
		VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO



----------------  
--Name: Henry and Robin
--Date: 2010-04-20
--Desc: PBI 10106
----------------  
CREATE TABLE [dbo].[AbsenceRequestOpenPeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[PeriodType] [nvarchar](255) NOT NULL,
	[Absence] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OpenMinimum] [datetime] NOT NULL,
	[OpenMaximum] [datetime] NOT NULL,
	[PersonAccountValidator] [int] NOT NULL,
	[StaffingThresholdValidator] [int] NOT NULL,
	[DaysMinimum] [int] NULL,
	[DaysMaximum] [int] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[AbsenceRequestOpenPeriod]  WITH CHECK ADD  CONSTRAINT [FK_AbsenceRequestOpenPeriod_Absence] FOREIGN KEY([Absence])
REFERENCES [dbo].[Absence] ([Id])
GO

ALTER TABLE [dbo].[AbsenceRequestOpenPeriod] CHECK CONSTRAINT [FK_AbsenceRequestOpenPeriod_Absence]
GO

ALTER TABLE [dbo].[AbsenceRequestOpenPeriod]  WITH CHECK ADD  CONSTRAINT [FK_AbsenceRequestOpenPeriod_WorkflowControlSet] FOREIGN KEY([Parent])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO

ALTER TABLE [dbo].[AbsenceRequestOpenPeriod] CHECK CONSTRAINT [FK_AbsenceRequestOpenPeriod_WorkflowControlSet]
GO


 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (231,'7.1.231') 
