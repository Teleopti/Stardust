----------------  
--Name: Robin Karlsson
--Date: 2013-02-13
--Desc: Bug #22194. Constraint violation error in PersonSkill
----------------  
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonSkill]') AND name = N'UC_Parent_Skill')
	EXEC('ALTER TABLE [dbo].[PersonSkill] DROP CONSTRAINT [UC_Parent_Skill]')
GO

----------------  
--Name: Tamasb
--Date: 2012-11-26  
--Desc: Bugfix 22336 correct the following application function: AdminWeb
--Note: The application function has been once added in v. 378.sql, but there the foreignid is already occupied, so the application function
--      was not added at all. Adding the function again will correct that bug
----------------  
UPDATE [ApplicationFunction]
SET [ForeignId] = '0083'
WHERE [FunctionCode] = 'ShiftTradeRequests'
AND [ForeignId] = '0080'
GO

----------------  
--Name: tamasb
--Date: 2012-11-28  
--Desc: Bugfix 22336 : readd the following application function: ShiftTradeRequests
--Note: The application function has been once added in v. 378.sql, but there the foreignid is already occupied, so the application function
--      changed the other function by mistake. Adding the function again will correct that bug
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
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0083' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ShiftTradeRequests' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxShiftTradeRequests' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO



----------------  
--Name: Tamasb
--Date: 2012-11-26  
--Desc: Bugfix 22336 readd the following application function: Anywhere
--Note: The application function has been once added in v. 375.sql, but there the foreignid is already occupied, so the application function
--      was not added at all. Adding the function again will correct that bug
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
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0001'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0080'
SELECT @FunctionCode = 'Anywhere'
SELECT @FunctionDescription = 'xxAnywhere'
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0)
 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId, [FunctionCode]=@FunctionCode, [FunctionDescription]=@FunctionDescription 
WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: tamasb
--Date: 2012-11-28  
--Desc: Bugfix 22336 : readd the following application function: SchedulesAnywhere
--Note: The application function has been once added in v. 375.sql, but later its parent was overriden by mistake by another function 
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
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0080'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0081'
SELECT @FunctionCode = 'SchedulesAnywhere'
SELECT @FunctionDescription = 'xxSchedules'
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 

SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId, [FunctionCode]=@FunctionCode, [FunctionDescription]=@FunctionDescription 
WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Robin Karlsson
--Date: 2013-02-21
--Desc: Extend the length of the read model to fit large projections.
----------------  
EXEC('ALTER TABLE ReadModel.PersonScheduleDay ADD Shift2 nvarchar(MAX) NOT NULL DEFAULT('''')')
GO
	
EXEC('UPDATE ReadModel.PersonScheduleDay SET Shift2 = Shift')
GO

DECLARE @d_name nvarchar(400)
SELECT @d_name=name FROM dbo.sysobjects WHERE parent_obj = OBJECT_ID(N'[ReadModel].[PersonScheduleDay]') AND type = 'D' AND name LIKE 'DF__PersonSch__%'

IF @d_name IS NOT NULL
BEGIN
	EXEC('ALTER TABLE [ReadModel].[PersonScheduleDay] DROP CONSTRAINT ['+@d_name+']')
END
GO

EXEC('ALTER TABLE ReadModel.PersonScheduleDay DROP COLUMN Shift')
GO

sp_rename 'ReadModel.PersonScheduleDay.Shift2','Shift','COLUMN'

----------------  
--Name: David Jonsson
--Date: 2013-01-08
--Desc: PBI #22158 - analysis of readtrace.exe and some missing index reports
--Note: this change can be applied on 374 if needed, therefor IF NOT EXISTS
----------------
--=============
-- OptionalColumnValue - move clustered key
--=============
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OptionalColumnValue]') AND name = N'CIX_OptionalColumnValue_ReferenceId')
BEGIN
	EXEC dbo.sp_rename @objname = N'[dbo].[OptionalColumnValue]', @newname = N'OptionalColumnValue_old', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[dbo].[OptionalColumnValue_old].[PK_OptionalColumnValue]', @newname = N'PK_OptionalColumnValue_old', @objtype =N'INDEX'

	CREATE TABLE [dbo].[OptionalColumnValue](
		[Id] [uniqueidentifier] NOT NULL,
		[Description] [nvarchar](255) NOT NULL,
		[ReferenceId] [uniqueidentifier] NULL,
		[Parent] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_OptionalColumnValue] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	)

	--better access
	CREATE CLUSTERED INDEX CIX_OptionalColumnValue_ReferenceId
	ON [dbo].[OptionalColumnValue] ([ReferenceId]) --References person Id most of the times, even though no relation exists

	--get data
	INSERT INTO [dbo].[OptionalColumnValue] (Id, Description, ReferenceId, Parent)
	SELECT Id, Description, ReferenceId, Parent
	FROM [dbo].[OptionalColumnValue_old]

	--create index to support parent (FK) as well, include all columns
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OptionalColumnValue]') AND name = N'IX_OptionalColumnValue_AK1')
	CREATE NONCLUSTERED INDEX IX_OptionalColumnValue_AK1
	ON [dbo].[OptionalColumnValue] ([Parent])
	INCLUDE (Id, Description, ReferenceId)

	DROP TABLE [dbo].[OptionalColumnValue_old]

	ALTER TABLE [dbo].[OptionalColumnValue]  WITH CHECK ADD  CONSTRAINT [FK_OptionalColumnValue_OptionalColumn] FOREIGN KEY([Parent])
	REFERENCES [dbo].[OptionalColumn] ([Id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[OptionalColumnValue] CHECK CONSTRAINT [FK_OptionalColumnValue_OptionalColumn]
END
GO

--=============
-- SchedulePeriod - add index to support FK
--=============
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SchedulePeriod]') AND name = N'IX_SchedulePeriod_Parent')
CREATE NONCLUSTERED INDEX IX_SchedulePeriod_Parent
ON [dbo].[SchedulePeriod] ([Parent])
GO

--=============
-- PersonSkill - move clustered key
--=============
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonSkill]') AND name = N'CIX_PersonSkill_Parent')
BEGIN
	EXEC dbo.sp_rename @objname = N'[dbo].[PersonSkill]', @newname = N'PersonSkill_old', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[dbo].[PersonSkill_old].[PK_PersonSkill]', @newname = N'PK_PersonSkill_old', @objtype =N'INDEX'

	CREATE TABLE [dbo].[PersonSkill](
		[Id] [uniqueidentifier] NOT NULL,
		[Parent] [uniqueidentifier] NOT NULL,
		[Skill] [uniqueidentifier] NOT NULL,
		[Value] [float] NOT NULL,
		[Active] [bit] NOT NULL,
	 CONSTRAINT [PK_PersonSkill] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	)

	CREATE CLUSTERED INDEX CIX_PersonSkill_Parent
	ON [dbo].[PersonSkill] ([Parent])

	INSERT INTO [dbo].[PersonSkill] (Id, Parent, Skill, Value, Active)
	SELECT Id, Parent, Skill, Value, Active
	FROM [dbo].[PersonSkill_old]

	DROP TABLE [dbo].[PersonSkill_old]

	ALTER TABLE [dbo].[PersonSkill] ADD  CONSTRAINT [DF_PersonSkill_Active]  DEFAULT ((1)) FOR [Active]

	ALTER TABLE [dbo].[PersonSkill]  WITH CHECK ADD  CONSTRAINT [FK_PersonSkill_PersonPeriod] FOREIGN KEY([Parent])
	REFERENCES [dbo].[PersonPeriod] ([Id])
	ALTER TABLE [dbo].[PersonSkill] CHECK CONSTRAINT [FK_PersonSkill_PersonPeriod]

	ALTER TABLE [dbo].[PersonSkill]  WITH CHECK ADD  CONSTRAINT [FK_PersonSkill_Skill] FOREIGN KEY([Skill])
	REFERENCES [dbo].[Skill] ([Id])
	ALTER TABLE [dbo].[PersonSkill] CHECK CONSTRAINT [FK_PersonSkill_Skill]

END
GO

--=============
-- AgentDayScheduleTag - move clustered key
--=============
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AgentDayScheduleTag]') AND name = N'CIX_AgentDayScheduleTag')
BEGIN
	EXEC dbo.sp_rename @objname = N'[dbo].[AgentDayScheduleTag]', @newname = N'AgentDayScheduleTag_old', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[dbo].[AgentDayScheduleTag_old].[PK_AgentDayScheduleTag]', @newname = N'PK_AgentDayScheduleTag_old', @objtype =N'INDEX'

	CREATE TABLE [dbo].[AgentDayScheduleTag](
		[Id] [uniqueidentifier] NOT NULL,
		[Version] [int] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[UpdatedBy] [uniqueidentifier] NOT NULL,
		[CreatedOn] [datetime] NOT NULL,
		[UpdatedOn] [datetime] NOT NULL,
		[Person] [uniqueidentifier] NOT NULL,
		[TagDate] [datetime] NOT NULL,
		[ScheduleTag] [uniqueidentifier] NOT NULL,
		[Scenario] [uniqueidentifier] NOT NULL,
		[BusinessUnit] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_AgentDayScheduleTag] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	)

	CREATE CLUSTERED INDEX CIX_AgentDayScheduleTag
	ON [dbo].[AgentDayScheduleTag] ([Person], [Scenario], [BusinessUnit],[TagDate])

	INSERT INTO [dbo].[AgentDayScheduleTag] (Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Person, TagDate, ScheduleTag, Scenario, BusinessUnit)
	SELECT Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Person, TagDate, ScheduleTag, Scenario, BusinessUnit
	FROM [dbo].[AgentDayScheduleTag_old]

	DROP TABLE [dbo].[AgentDayScheduleTag_old]

	ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_BusinessUnit] FOREIGN KEY([BusinessUnit])
	REFERENCES [dbo].[BusinessUnit] ([Id])
	ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_BusinessUnit]

	ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_Person] FOREIGN KEY([Person])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_Person]

	ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_Person_CreatedBy] FOREIGN KEY([CreatedBy])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_Person_CreatedBy]

	ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_Person_UpdatedBy]

	ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_Scenario] FOREIGN KEY([Scenario])
	REFERENCES [dbo].[Scenario] ([Id])
	ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_Scenario]

	ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_ScheduleTag] FOREIGN KEY([ScheduleTag])
	REFERENCES [dbo].[ScheduleTag] ([Id])
	ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_ScheduleTag]
END

--=============
-- PersonAssignment - PersonAssignment with a full coverage index
--=============
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'IX_PersonAssignment_AK1')
CREATE INDEX IX_PersonAssignment_AK1
ON [dbo].[PersonAssignment] ([Person], [Scenario], [BusinessUnit],[Minimum], [Maximum]) INCLUDE ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn])
GO

--=============
-- Some other indexes, based on missing index stats
--=============
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriod]') AND name = N'IX_PersonPeriod_Team_StartDate')
CREATE INDEX IX_PersonPeriod_Team_StartDate ON [dbo].[PersonPeriod] ([Team],[StartDate]) INCLUDE ([Parent])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Auditing].[PersonDayOff_AUD]') AND name = N'IX_PersonDayOff_AUD_Person_Anchor')
CREATE INDEX IX_PersonDayOff_AUD_Person_Anchor ON [Auditing].[PersonDayOff_AUD] ([Person],[Anchor]) INCLUDE ([REV])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Auditing].[PersonAssignment_AUD]') AND name = N'IX_PersonAssignment_AUD_Person_min_max')
CREATE INDEX IX_PersonAssignment_AUD_Person_min_max ON [Auditing].[PersonAssignment_AUD] ([Person],[Minimum], [Maximum]) INCLUDE ([REV])
GO
