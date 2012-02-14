---------------- 
--Name: DJ
--Date: 2010-09-24
--Desc: Adding Audit Trail
----------------
GO
CREATE SCHEMA [AuditTrail]
GO
------------
--Setting table
------------
CREATE TABLE [dbo].[AuditTrailSetting](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[DaysToKeep] [int] NOT NULL,
	[IsRunning] [bit] NOT NULL)
ON [PRIMARY]

ALTER TABLE dbo.AuditTrailSetting ADD CONSTRAINT
	PK_AuditTrailSetting PRIMARY KEY CLUSTERED 
	(
	Id
	)

ALTER TABLE [dbo].[AuditTrailSetting]  WITH CHECK ADD  CONSTRAINT [FK_AuditTrailSetting_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[AuditTrailSetting] CHECK CONSTRAINT [FK_AuditTrailSetting_Person_CreatedBy]

ALTER TABLE [dbo].[AuditTrailSetting]  WITH CHECK ADD  CONSTRAINT [FK_AuditTrailSetting_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[AuditTrailSetting] CHECK CONSTRAINT [FK_AuditTrailSetting_Person_UpdatedBy]

--insert default data = triggers Off
DECLARE @SuperUser UNIQUEIDENTIFIER
SET @SuperUser = '3F0886AB-7B25-4E95-856A-0D726EDC2A67'

INSERT INTO [dbo].[AuditTrailSetting]
SELECT NEWID(),@SuperUser,@SuperUser,GETDATE(),GETDATE(),14,0

------------
--central table for transactions, status, date
------------
CREATE TABLE [AuditTrail].[AuditChangeSet](
	[Id] int NOT NULL IDENTITY (1, 1),
	[AuditChangeSet] varchar(255) NOT NULL,
	[AuditDatetime] datetime NOT NULL,
	[AuditOwner] [uniqueidentifier] NOT NULL,
	[AuditStatus] char(1) NOT NULL
) ON [PRIMARY]

ALTER TABLE AuditTrail.AuditChangeSet ADD CONSTRAINT
	PK_AuditChangeSet PRIMARY KEY NONCLUSTERED 
	(
	Id
	) ON [PRIMARY]

ALTER TABLE AuditTrail.AuditChangeSet ADD CONSTRAINT
	DF_AuditChangeSet_AuditStatus DEFAULT 0 FOR AuditStatus

CREATE CLUSTERED INDEX [CIX_AuditChangeSet] ON [AuditTrail].[AuditChangeSet] 
(
	[AuditChangeSet] ASC
) ON [PRIMARY]
GO
------------
--mirror tables
------------
CREATE TABLE [AuditTrail].OvertimeShift (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
Parent uniqueidentifier NOT NULL,
OrderIndex int NOT NULL,
			 CONSTRAINT [PK_OvertimeShift] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_OvertimeShift] ON [AuditTrail].[OvertimeShift]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].OvertimeShiftActivityLayer (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
payLoad uniqueidentifier NOT NULL,
Minimum datetime NOT NULL,
Maximum datetime NOT NULL,
DefinitionSet uniqueidentifier NOT NULL,
Parent uniqueidentifier NOT NULL,
OrderIndex int NOT NULL,
			 CONSTRAINT [PK_OvertimeShiftActivityLayer] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_OvertimeShiftActivityLayer] ON [AuditTrail].[OvertimeShiftActivityLayer]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].MainShiftActivityLayer (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
payLoad uniqueidentifier NOT NULL,
Minimum datetime NOT NULL,
Maximum datetime NOT NULL,
Parent uniqueidentifier NOT NULL,
OrderIndex int NOT NULL,
			 CONSTRAINT [PK_MainShiftActivityLayer] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_MainShiftActivityLayer] ON [AuditTrail].[MainShiftActivityLayer]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].MainShift (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
Name nvarchar(100) NOT NULL,
ShiftCategory uniqueidentifier NOT NULL,
			 CONSTRAINT [PK_MainShift] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_MainShift] ON [AuditTrail].[MainShift]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].PersonAssignment (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
Version int NOT NULL,
CreatedBy uniqueidentifier NOT NULL,
UpdatedBy uniqueidentifier NOT NULL,
CreatedOn datetime NOT NULL,
UpdatedOn datetime NOT NULL,
Person uniqueidentifier NOT NULL,
Scenario uniqueidentifier NOT NULL,
BusinessUnit uniqueidentifier NOT NULL,
			 CONSTRAINT [PK_PersonAssignment] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_PersonAssignment] ON [AuditTrail].[PersonAssignment]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].PersonalShiftActivityLayer (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
payLoad uniqueidentifier NOT NULL,
Minimum datetime NOT NULL,
Maximum datetime NOT NULL,
Parent uniqueidentifier NOT NULL,
OrderIndex int NOT NULL,
			 CONSTRAINT [PK_PersonalShiftActivityLayer] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_PersonalShiftActivityLayer] ON [AuditTrail].[PersonalShiftActivityLayer]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].PersonalShift (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
Parent uniqueidentifier NOT NULL,
OrderIndex int NOT NULL,
			 CONSTRAINT [PK_PersonalShift] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_PersonalShift] ON [AuditTrail].[PersonalShift]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].PersonAbsence (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
Version int NOT NULL,
CreatedBy uniqueidentifier NOT NULL,
UpdatedBy uniqueidentifier NOT NULL,
CreatedOn datetime NOT NULL,
UpdatedOn datetime NOT NULL,
LastChange datetime NULL,
Person uniqueidentifier NOT NULL,
Scenario uniqueidentifier NOT NULL,
PayLoad uniqueidentifier NOT NULL,
Minimum datetime NOT NULL,
Maximum datetime NOT NULL,
BusinessUnit uniqueidentifier NOT NULL,
			 CONSTRAINT [PK_PersonAbsence] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_PersonAbsence] ON [AuditTrail].[PersonAbsence]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].PersonDayOff (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
Version int NOT NULL,
CreatedBy uniqueidentifier NOT NULL,
UpdatedBy uniqueidentifier NOT NULL,
CreatedOn datetime NOT NULL,
UpdatedOn datetime NOT NULL,
Anchor datetime NOT NULL,
Person uniqueidentifier NOT NULL,
Scenario uniqueidentifier NOT NULL,
BusinessUnit uniqueidentifier NOT NULL,
TargetLength bigint NOT NULL,
Flexibility bigint NOT NULL,
Name nvarchar(100) NOT NULL,
ShortName nvarchar(50) NULL,
DisplayColor int NOT NULL,
			 CONSTRAINT [PK_PersonDayOff] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_PersonDayOff] ON [AuditTrail].[PersonDayOff]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].Meeting (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
Version int NOT NULL,
CreatedBy uniqueidentifier NOT NULL,
UpdatedBy uniqueidentifier NULL,
CreatedOn datetime NOT NULL,
UpdatedOn datetime NULL,
Organizer uniqueidentifier NOT NULL,
Scenario uniqueidentifier NOT NULL,
Subject nvarchar(200) NOT NULL,
Description nvarchar(4000) NOT NULL,
Location nvarchar(200) NOT NULL,
Activity uniqueidentifier NOT NULL,
StartDate datetime NOT NULL,
EndDate datetime NOT NULL,
BusinessUnit uniqueidentifier NOT NULL,
StartTime bigint NOT NULL,
EndTime bigint NOT NULL,
TimeZone nvarchar(200) NOT NULL,
			 CONSTRAINT [PK_Meeting] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_Meeting] ON [AuditTrail].[Meeting]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

CREATE TABLE [AuditTrail].MeetingPerson (
	[AuditId] int NOT NULL identity(1,1),
	[AuditType] char(1) NOT NULL,
	[AuditChangeSet] varchar(255) NOT NULL,Id uniqueidentifier NOT NULL,
Person uniqueidentifier NOT NULL,
Parent uniqueidentifier NOT NULL,
Optional bit NOT NULL,
			 CONSTRAINT [PK_MeetingPerson] PRIMARY KEY NONCLUSTERED 
			(
				[AuditId] ASC
			)
			) ON [PRIMARY]
			CREATE CLUSTERED INDEX [CIX_MeetingPerson] ON [AuditTrail].[MeetingPerson]
		(
			[AuditChangeSet] ASC
		) ON [PRIMARY]

----------------  
--Name: David Jonsson
--Date: 2010-09-30 
--Desc: Adding ApplicationFunction for Audit Trail
----------------
SET NOCOUNT ON

DECLARE @SuperUserId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--hard coded GUID for @SuperUserId
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

--set parent level for the new function
SELECT @ParentForeignId = '0017'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0058' -- Foreign id of the function that is hardcoded	
SELECT @FunctionCode = 'AuditTrailSettings' -- Name of the function that is hardcoded
SELECT @FunctionDescription = 'xxAuditTrailSettings' -- Description of the function that is hardcoded
SELECT @ParentId = @ParentId

--insert, check if exists
IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 

--if value already exists
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')


----------------  
--Name: Micke Deigård
--Date: 2010-10-02 
--Desc: Masteractivities
----------------
/****** Object:  Table [dbo].[MasterActivityCollection]    Script Date: 09/20/2010 12:48:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE dbo.Activity ADD
	IsMaster nvarchar(1) NOT NULL CONSTRAINT DF_Activity_IsMaster DEFAULT 0
	
GO
CREATE TABLE [dbo].[MasterActivityCollection](
	[MasterActivity] [uniqueidentifier] NOT NULL,
	[Activity] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[MasterActivityCollection]  WITH CHECK ADD  CONSTRAINT [FK_MasterActivity_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO

ALTER TABLE [dbo].[MasterActivityCollection] CHECK CONSTRAINT [FK_MasterActivity_Activity]
GO

ALTER TABLE [dbo].[MasterActivityCollection]  WITH CHECK ADD  CONSTRAINT [FK_MasterActivity_Master] FOREIGN KEY([MasterActivity])
REFERENCES [dbo].[Activity] ([Id])
GO

ALTER TABLE [dbo].[MasterActivityCollection] CHECK CONSTRAINT [FK_MasterActivity_Master]
GO


/* 
Trunk initiated: 
2010-09-24 
10:00
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2010-xx-xx  
--Desc: Because ...  
----------------  
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (296,'7.1.296') 
