----------------  
--Name: Jonas & Maria
--Date: 2016-01-27
--Desc: Add new application functions "WebIntraday" and "WebModifySkillArea"
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
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy],[UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [Culture], [UiCulture], [IsDeleted], [WriteProtectionUpdatedOn], [PersonWriteProtectedDate], [WriteProtectionUpdatedBy], [WorkflowControlSet], [FirstDayOfWeek])
VALUES (@SuperUserId,1, @SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', NULL, NULL, 0, NULL, NULL, NULL, NULL, 1)

--get parent level
SELECT @ParentForeignId = '0080'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0127' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'WebIntraday' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxWebIntraday' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO



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
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy],[UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [Culture], [UiCulture], [IsDeleted], [WriteProtectionUpdatedOn], [PersonWriteProtectedDate], [WriteProtectionUpdatedBy], [WorkflowControlSet], [FirstDayOfWeek])
VALUES (@SuperUserId,1, @SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', NULL, NULL, 0, NULL, NULL, NULL, NULL, 1)

--get parent level
SELECT @ParentForeignId = '0127'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0128' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'WebModifySkillArea' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxWebModifySkillArea' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Jonas & Maria & Kunning
--Date: 2016-01-29
--Desc: Add two new tables for skill area
----------------  
CREATE TABLE [dbo].[SkillArea](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[Version] [int] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_SkillArea] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SkillArea]  WITH CHECK ADD  CONSTRAINT [FK_SkillArea_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[SkillArea] CHECK CONSTRAINT [FK_SkillArea_BusinessUnit]
GO

ALTER TABLE [dbo].[SkillArea]  WITH CHECK ADD  CONSTRAINT [FK_SkillArea_Person] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[SkillArea] CHECK CONSTRAINT [FK_SkillArea_Person]
GO


CREATE TABLE [dbo].[SkillAreaSkillCollection](
	[SkillArea] [uniqueidentifier] NOT NULL,
	[Skill] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SkillAreaSkillCollection]  WITH CHECK ADD  CONSTRAINT [FK_SkillAreaSkillCollection_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO

ALTER TABLE [dbo].[SkillAreaSkillCollection] CHECK CONSTRAINT [FK_SkillAreaSkillCollection_Skill]
GO

ALTER TABLE [dbo].[SkillAreaSkillCollection]  WITH CHECK ADD  CONSTRAINT [FK_SkillAreaSkillCollection_SkillArea] FOREIGN KEY([SkillArea])
REFERENCES [dbo].[SkillArea] ([Id])
GO

ALTER TABLE [dbo].[SkillAreaSkillCollection] CHECK CONSTRAINT [FK_SkillAreaSkillCollection_SkillArea]
GO

ALTER TABLE [dbo].[SkillAreaSkillCollection] ADD CONSTRAINT UQ_SkillAreaSkillCollection UNIQUE CLUSTERED 
(
	[SkillArea] ASC,
	[Skill] ASC
) ON [PRIMARY]
GO
