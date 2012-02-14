/* 
Trunk initiated: 
2011-01-13 
10:41
By: TOPTINET\davidj 
On INTEGRATION 
*/ 

----------------  
--Name: Jonas N
--Date: 2011-01-19  
--Desc: Add table dbo.PublicNote
----------------  
CREATE TABLE [dbo].[PublicNote](
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
	[BusinessUnit] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

ALTER TABLE dbo.PublicNote ADD CONSTRAINT
	PK_PublicNote PRIMARY KEY CLUSTERED 
	(
	Id
	) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])

ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_BusinessUnit]

ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_Person]

ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_Person_CreatedBy]

ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_Person_UpdatedBy]

ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])

ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_Scenario]

GO
ALTER TABLE dbo.WorkflowControlSet ADD
UseShiftCategoryFairness bit NOT NULL CONSTRAINT DF_WorkflowControlSet_UseShiftCategoryFairness DEFAULT 0
GO


----------------  
--Name: Ola
--Date: 2011-01-26  
--Desc: Add a new Application Function
--ViewShedulePeriodCalculation 

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
SELECT @ParentForeignId = '0019'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0060' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ViewSchedulePeriodCalculation' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxViewSchedulePeriodCalculation' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
GO
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (314,'7.1.314') 
