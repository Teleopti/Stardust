 ----------------  
--Name: Ola H
--Date: 2011-10-12 
--Desc: Empty Logon information on deleted persons
---------------- 
UPDATE Person set 
[WindowsLogOnName] = ''
,[DomainName] = ''
,[ApplicationLogOnName] = ''
,[PartOfUnique]=id
WHERE IsDeleted = 1
GO


----------------  
--Name: Mathias Stenbom
--Date: 2011-10-20  
--Desc: Add the following new application function> Raptor/MyTimeWeb/TextRequests
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
SELECT @ParentForeignId = '0048'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0070' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'TextRequests' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxCreateTextRequest' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

---------------
--Name: CS
--Date: 2011-10-24
--Desc: Enable "new" Auditing, Remove of "old" AuditTrail
---------------
-- Drop Triggers OvertimeShift

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_OvertimeShift_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_OvertimeShift_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_OvertimeShift_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_OvertimeShift_UD]
END
GO

-- Drop Triggers OvertimeShiftActivityLayer

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_OvertimeShiftActivityLayer_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_OvertimeShiftActivityLayer_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_OvertimeShiftActivityLayer_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_OvertimeShiftActivityLayer_UD]
END
GO

-- Drop Triggers MainShiftActivityLayer

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_MainShiftActivityLayer_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_MainShiftActivityLayer_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_MainShiftActivityLayer_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_MainShiftActivityLayer_UD]
END
GO

-- Drop Triggers MainShift

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_MainShift_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_MainShift_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_MainShift_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_MainShift_UD]
END
GO

-- Drop Triggers PersonAssignment

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonAssignment_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonAssignment_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonAssignment_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonAssignment_UD]
END
GO

-- Drop Triggers PersonalShiftActivitylayer

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonalShiftActivityLayer_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonalShiftActivityLayer_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonalShiftActivityLayer_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonalShiftActivityLayer_UD]
END
GO

-- Drop Triggers PersonalShift

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonalShift_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonalShift_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonalShift_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonalShift_UD]
END
GO

-- Drop Triggers PersonAbsence

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonAbsence_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonAbsence_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonAbsence_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonAbsence_UD]
END
GO

-- Drop Triggers PersonDayOff

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonDayOff_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonDayOff_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_PersonDayOff_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_PersonDayOff_UD]
END
GO

-- Drop Triggers Meeting

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_Meeting_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_Meeting_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_Meeting_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_Meeting_UD]
END
GO

-- Drop Triggers MeetingPerson

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_MeetingPerson_I' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_MeetingPerson_I]
END
GO

IF EXISTS (SELECT name FROM [dbo].[sysobjects] WHERE name = 'tr_MeetingPerson_UD' AND type = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_MeetingPerson_UD]
END
GO

-- Drop procedure AuditTrailReport

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[AuditTrailReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AuditTrail].[AuditTrailReport]
GO

-- Drop procedure AuditTransactionCreate

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[AuditTransactionCreate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AuditTrail].[AuditTransactionCreate]
GO

-- Drop procedure PurgeTables

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[PurgeTables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AuditTrail].[PurgeTables]
GO

-- Drop procedure ScheduleReportGetAll

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[ScheduleReportGetAll]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AuditTrail].[ScheduleReportGetAll]
GO

-- Drop procedure ScheduleReportGetByAgent

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[ScheduleReportGetByAgent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AuditTrail].[ScheduleReportGetByAgent]
GO

-- Drop procedure ScheduleReportGetByOwner

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[ScheduleReportGetByOwner]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AuditTrail].[ScheduleReportGetByOwner]
GO

-- Drop table AuditTrail.AuditChangeSet

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AuditChangeSet_AuditStatus]') AND type = 'D')
BEGIN
ALTER TABLE [AuditTrail].[AuditChangeSet] DROP CONSTRAINT [DF_AuditChangeSet_AuditStatus]
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[AuditChangeSet]') AND type in (N'U'))
DROP TABLE [AuditTrail].[AuditChangeSet]
GO

-- Drop table AuditTrail.MainShift

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[MainShift]') AND type in (N'U'))
DROP TABLE [AuditTrail].[MainShift]
GO

-- Drop table AuditTrail.MainShiftActivityLayer

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[MainShiftActivityLayer]') AND type in (N'U'))
DROP TABLE [AuditTrail].[MainShiftActivityLayer]
GO

-- Drop table AuditTrail.Meeting

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[Meeting]') AND type in (N'U'))
DROP TABLE [AuditTrail].[Meeting]
GO

-- Drop table AuditTrail.MeetingPerson

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[MeetingPerson]') AND type in (N'U'))
DROP TABLE [AuditTrail].[MeetingPerson]
GO

-- Drop table AuditTrail.OvertimeShift

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[OvertimeShift]') AND type in (N'U'))
DROP TABLE [AuditTrail].[OvertimeShift]
GO

-- Drop table AuditTrail.OvertimeShiftActivityLayer

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[OvertimeShiftActivityLayer]') AND type in (N'U'))
DROP TABLE [AuditTrail].[OvertimeShiftActivityLayer]
GO

-- Drop table AuditTrail.PersonAbsence

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[PersonAbsence]') AND type in (N'U'))
DROP TABLE [AuditTrail].[PersonAbsence]
GO

-- Drop table AuditTrail.PersonalShift

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[PersonalShift]') AND type in (N'U'))
DROP TABLE [AuditTrail].[PersonalShift]
GO

-- Drop table AuditTrail.PersonalShiftActivityLayer

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[PersonalShiftActivityLayer]') AND type in (N'U'))
DROP TABLE [AuditTrail].[PersonalShiftActivityLayer]
GO

-- Drop table AuditTrail.PersonAssignment

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[PersonAssignment]') AND type in (N'U'))
DROP TABLE [AuditTrail].[PersonAssignment]
GO

-- Drop table AuditTrail.PersonDayOff

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AuditTrail].[PersonDayOff]') AND type in (N'U'))
DROP TABLE [AuditTrail].[PersonDayOff]
GO

-- DavidJ: We need to keep this table for a while: [dbo].[AuditTrailSetting]
-- Why?: in order to make bugfixes on this Stored Procedure: [Auditing].[InitAuditTables] and then use it for initial imports

-- Drop schema AuditTrail

IF  EXISTS (SELECT * FROM sys.schemas WHERE name = N'AuditTrail')
DROP SCHEMA [AuditTrail]
GO

----------------  
--Name: Anders F
--Date: 2011-10-27  
--Desc: Column need to be longer as we have concatenated sites and teams in it, and recursive groups within groups...
----------------
alter table dbo.GroupingReadOnly alter column GroupName nvarchar(200)
go

----------------  
--Name: TamasB
--Date: 2011-10-26
--Desc: Add AdjustTimeBankWithSeasonality and AdjustTimeBankWithPartTimePercentage column to Contract table 
----------------  
ALTER TABLE dbo.Contract ADD
	AdjustTimeBankWithSeasonality int NOT NULL CONSTRAINT DF_Contract_AdjustTimeBankWithSeasonality DEFAULT ((0)),
	AdjustTimeBankWithPartTimePercentage int NOT NULL CONSTRAINT DF_Contract_AdjustTimeBankWithPartTimePercentage DEFAULT ((0))
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (338,'7.1.338') 
