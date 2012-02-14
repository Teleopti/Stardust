/* 
Trunk initiated: 
2011-01-31 
11:14
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Anders F  
--Date: 2011-02-14  
--Desc: Because invalid rotations may have been saved to db already.  
----------------  
update RotationRestriction
set StartTimeMaximum = StartTimeMinimum
where StartTimeMaximum < StartTimeMinimum
and StartTimeMaximum is not null
and StartTimeMinimum is not null

update RotationRestriction
set EndTimeMaximum = EndTimeMinimum
where EndTimeMaximum < EndTimeMinimum
and EndTimeMaximum is not null
and EndTimeMinimum is not null

update RotationRestriction
set WorkTimeMaximum = WorkTimeMinimum
where WorkTimeMaximum < WorkTimeMinimum
and WorkTimeMaximum is not null
and WorkTimeMinimum is not null


----------------  
--Name: Robin&David 
--Date: 2011-02-15 
--Desc: Normalize XMLResult to separate table  
--Changing how payroll results are handled to enable lazy loading of the results part.
---------------- 
--Changing how payroll results are handled to enable lazy loading of the results part.
declare @sql varchar(max)
--start, DDL
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[XmlResult]') AND type in (N'U'))
AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PayrollResult]') AND name=N'Result')
BEGIN
	--Add table
	CREATE TABLE [dbo].[XmlResult](
		[Id] [uniqueidentifier] NOT NULL,
		[XPathNavigable] [xml] NULL,
		[Temporary_PayrollResultId] [uniqueidentifier] NOT NULL
	) ON [PRIMARY]
	
	--Add PK
	ALTER TABLE dbo.[XmlResult] ADD CONSTRAINT
		PK_XmlResult PRIMARY KEY CLUSTERED 
		(
		Id
		) ON [PRIMARY]

	--Add bit column in parent table. Default to 'false' = 0
	ALTER TABLE dbo.PayrollResult ADD 
		[FinishedOk] [bit] NOT NULL DEFAULT 0
		
	--Add the new child column
	ALTER TABLE dbo.PayrollResult ADD 
		[XmlResult] [uniqueidentifier] NULL
		
	--Add FK to child table
	ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_XmlResult] FOREIGN KEY([XmlResult])
	REFERENCES [dbo].[XmlResult] ([Id])

	ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_XmlResult]

	--move data into new child table
	SELECT @sql = 'INSERT INTO dbo.XmlResult SELECT NEWID(),Result,Id FROM dbo.PayrollResult'
	execute(@sql)

	--update parent to use GUID ref. instead
	SELECT @sql = 'UPDATE dbo.PayrollResult
	SET XmlResult = XmlResult.Id
	FROM dbo.XmlResult INNER JOIN dbo.PayrollResult
		ON dbo.PayrollResult.Id=dbo.XmlResult.Temporary_PayrollResultId'
	execute(@sql)
	
	--change data type
	ALTER TABLE dbo.PayrollResult ALTER COLUMN XmlResult [uniqueidentifier] NOT NULL
	
END

--split transaction
GO
declare @sql varchar(max)
--Continue and run DML
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[XmlResult]') AND type in (N'U'))
AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PayrollResult]') AND name=N'Result')
BEGIN
	--update bit =1. i.e true for success story
	SELECT @sql = 'UPDATE dbo.PayrollResult
	SET FinishedOk = 1 WHERE Result is not null'
	execute(@sql)

	--Clean up
	ALTER TABLE dbo.PayrollResult DROP COLUMN Result
	ALTER TABLE dbo.XmlResult DROP COLUMN Temporary_PayrollResultId
	
END
GO

----------------  
--Name: Ola
--Date: 2011-02-17  
--Desc: Add a new Application Function
--SetPlanningTimeBank 

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
SELECT @ForeignId = '0063' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'SetPlanningTimeBank' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxSetPlanningTimeBank' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
GO
ALTER TABLE Contract
Add [PlanningTimeBankMax] [bigint] NOT NULL Default 0,
	[PlanningTimeBankMin] [bigint] NOT NULL Default 0
GO

/*
   den 19 februari
   User: cs
   Database: new column in Scenario -> Restricted
*/
ALTER TABLE dbo.Scenario ADD
	Restricted bit NOT NULL CONSTRAINT DF_Scenario_Restricted DEFAULT 0
GO

----------------  
--Name: CS
--Date: 2011-02-19  
--Desc: Add a new Application Function
--ViewRestrictedScenario 

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
SELECT @ParentForeignId = '0023'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0061' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ViewRestrictedScenario' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxViewRestrictedScenario' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
GO

----------------  
--Name: CS
--Date: 2011-02-19  
--Desc: Add a new Application Function
--ModifyRestrictedScenario 

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
SELECT @ParentForeignId = '0023'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0062' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ModifyRestrictedScenario' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxModifyRestrictedScenario' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
GO
----------------  
--Name: David
--Date: 2011-02-25
--Desc: Add a new Matrix Report
----------------  
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
SELECT @ParentForeignId = '0006'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE FunctionCode='Reports' AND ForeignSource = 'Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '23' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ResReportScheduledOvertimePerAgent' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxResReportScheduledOvertimePerAgent' --Description of the function > hardcoded

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Matrix' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Matrix', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Matrix' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction]
SET [ForeignId]=@ForeignId, [Parent]=@ParentId
WHERE ForeignSource='Matrix' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

----------------  
--Name: Ola
--Date: 2011-02-25
--Desc: 
----------------  
CREATE TABLE [dbo].[PersonWriteProtectionInfo](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[PersonWriteProtectedDate] [datetime] NULL
) ON [PRIMARY]

ALTER TABLE dbo.PersonWriteProtectionInfo ADD CONSTRAINT
	PK_PersonWriteProtectionInfo PRIMARY KEY CLUSTERED 
	(
	Id
	) ON [PRIMARY]

ALTER TABLE [dbo].[PersonWriteProtectionInfo]  WITH CHECK ADD  CONSTRAINT [FK_WriteProtection_Person] FOREIGN KEY([Id])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[PersonWriteProtectionInfo] CHECK CONSTRAINT [FK_WriteProtection_Person]

ALTER TABLE [dbo].[PersonWriteProtectionInfo]  WITH CHECK ADD  CONSTRAINT [FK_WriteProtection_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[PersonWriteProtectionInfo] CHECK CONSTRAINT [FK_WriteProtection_Person_CreatedBy]

ALTER TABLE [dbo].[PersonWriteProtectionInfo]  WITH CHECK ADD  CONSTRAINT [FK_WriteProtection_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[PersonWriteProtectionInfo] CHECK CONSTRAINT [FK_WriteProtection_Person_UpdatedBy]

INSERT INTO [PersonWriteProtectionInfo]
SELECT Id, CreatedBy, ISNULL(WriteProtectionUpdatedBy, UpdatedBy) , CreatedOn, ISNULL(WriteProtectionUpdatedOn, UpdatedOn),
PersonWriteProtectedDate
FROM Person

----------------  
--Name: David J  
--Date: 2011-03-01
--Desc: Not a huge difference but a few secs (with no/less impact on inserts)
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriod]') AND name = N'IX_PersonPeriod_Parent')
CREATE NONCLUSTERED INDEX [IX_PersonPeriod_Parent]
ON [dbo].[PersonPeriod] ([Parent])



----------------  
--Name: Roger K  
--Date: 2011-03-04
--Desc: Envers audit tables for internal testing
----------------  
CREATE TABLE [dbo].[REVINFO](
	[REV] [int] IDENTITY(1,1) NOT NULL,
	[REVTSTMP] [datetime] NULL,
CONSTRAINT PK_REVINFO PRIMARY KEY CLUSTERED 
(
	[REV] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[PersonAssignment_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[CreatedOn] [datetime] NULL,
	[UpdatedOn] [datetime] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[MainShift] [uniqueidentifier] NULL,
	[Person] [uniqueidentifier] NULL,
	[Scenario] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NULL,
CONSTRAINT PK_PersonAssignment_AUD PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[PersonAssignment_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignmentAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])

ALTER TABLE [dbo].[PersonAssignment_AUD] CHECK CONSTRAINT FK_PersonAssignmentAUD_REV

CREATE TABLE [dbo].[PersonalShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL,
CONSTRAINT PK_PersonalShiftActivityLayer_AUD PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[PersonalShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftActivityLayerAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])

ALTER TABLE [dbo].[PersonalShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_PersonalShiftActivityLayerAUD_REV]

CREATE TABLE [dbo].[PersonalShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[OrderIndex] [int] NULL,
	[Parent] [uniqueidentifier] NULL,
CONSTRAINT PK_PersonalShift_AUD PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[PersonalShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])

ALTER TABLE [dbo].[PersonalShift_AUD] CHECK CONSTRAINT [FK_PersonalShiftAUD_REV]

CREATE TABLE [dbo].[OvertimeShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[DefinitionSet] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL,
CONSTRAINT PK_OvertimeShiftActivityLayer_AUD PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[OvertimeShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayerAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])

ALTER TABLE [dbo].[OvertimeShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayerAUD_REV]

CREATE TABLE [dbo].[OvertimeShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[OrderIndex] [int] NULL,
	[Parent] [uniqueidentifier] NULL,
CONSTRAINT PK_OvertimeShift_AUD PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[OvertimeShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])

ALTER TABLE [dbo].[OvertimeShift_AUD] CHECK CONSTRAINT [FK_OvertimeShiftAUD_REV]

CREATE TABLE [dbo].[MainShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[PayLoad] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL,
CONSTRAINT PK_MainShiftActivityLayer_AUD PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[MainShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayerAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])

ALTER TABLE [dbo].[MainShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_MainShiftActivityLayerAUD_REV]

CREATE TABLE [dbo].[MainShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[Name] [nvarchar](50) NULL,
	[RefId] [uniqueidentifier] NULL,
	[ShiftCategory] [uniqueidentifier] NULL,
CONSTRAINT PK_MainShift_AUD PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[MainShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])

ALTER TABLE [dbo].[MainShift_AUD] CHECK CONSTRAINT [FK_MainShiftAUD_REV] 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (316,'7.1.316') 
