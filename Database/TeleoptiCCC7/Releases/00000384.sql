----------------  
--Name: tamasb, kunning
--Date: 2013-03-25 
--Desc: add new appliation function MyTimeWeb/TeamSchedule/ViewAllGroupPages
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
SELECT @ParentForeignId = '0072'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0084' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ViewAllGroupPages' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxViewAllGroupPages' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO
----------------  
--Name: Robin Karlsson
--Date: 2013-04-24
--Desc: Bug #23047. Empty read models to consider deleted schedule when done.
----------------  
--TRUNCATE TABLE [ReadModel].[ScheduleDay] nope! dont => #23288 //DavidJ
TRUNCATE TABLE [ReadModel].[ScheduleProjectionReadOnly]
GO

----------------  
--Name: Ola H
--Date: 2013-04-27
--Desc: Table for logging log in attempts.
---------------- 
CREATE TABLE [Auditing].[Security](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DateTimeUtc] [datetime] NOT NULL,
	[Result] [nchar](100) NOT NULL,
	[UserCredentials] [nchar](100) NOT NULL,
	[Provider] [nchar](100) NOT NULL,
	[Client] [nchar](100) NOT NULL,
	[ClientIp] [nchar](100) NOT NULL,
	[PersonId] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

GO

ALTER TABLE [Auditing].[Security] ADD  CONSTRAINT [DF_Security_Time]  DEFAULT (getutcdate()) FOR [DateTimeUtc]
GO

----------------  
--Name: David J
--Date: 2013-04-29
--Desc: re-factor purge settings
---------------- 
EXEC dbo.sp_rename @objname = N'[dbo].[PurgeSetting]', @newname = N'PurgeSetting_old', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[dbo].[PurgeSetting_old].[PK_PurgeSetting]', @newname = N'PK_PurgeSetting_old', @objtype =N'INDEX'
GO
CREATE TABLE [dbo].[PurgeSetting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](50) NOT NULL,
	[Value] [int] NOT NULL,
 CONSTRAINT [PK_PurgeSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO

--Save old data
INSERT INTO dbo.PurgeSetting ([Key],Value)
SELECT 'YearsToKeep'+[Key],KeepYears
FROM dbo.PurgeSetting_old

--new setting
INSERT INTO dbo.PurgeSetting ([Key],Value)
VALUES('DaysToKeepSecurityAudit',30)
GO

----------------  
--Name: Ola & David
--Date: 2013-04-12
--Desc: PBI #22523 - Speed up ETL Intrady load
----------------
CREATE TABLE [mart].[LastUpdatedPerStep](
	[StepName] [varchar](500) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NULL,
	[Date] [datetime] NOT NULL
) 

GO

--Track changes on schedule, default scenario only
ALTER TABLE ReadModel.ScheduleDay ADD NotScheduled bit NULL
GO
UPDATE ReadModel.ScheduleDay SET NotScheduled = 0
GO
ALTER TABLE ReadModel.ScheduleDay ALTER COLUMN NotScheduled bit NOT NULL
GO

ALTER TABLE ReadModel.ScheduleDay ADD CONSTRAINT DF_ScheduleDay_InsertedOn DEFAULT getutcdate() FOR InsertedOn
ALTER TABLE ReadModel.ScheduleDay ADD CONSTRAINT DF_ScheduleDay_NotScheduled DEFAULT 0 FOR NotScheduled
GO

--track changes on Permissions
ALTER TABLE dbo.ApplicationFunctionInRole ADD InsertedOn datetime NULL
GO
UPDATE dbo.ApplicationFunctionInRole SET InsertedOn = '1900-01-01'
ALTER TABLE dbo.ApplicationFunctionInRole ALTER COLUMN InsertedOn datetime NOT NULL
ALTER TABLE dbo.ApplicationFunctionInRole ADD  CONSTRAINT [DF_ApplicationFunctionInRole_InsertedOn]  DEFAULT (getutcdate()) FOR [InsertedOn]
GO

ALTER TABLE dbo.AvailablePersonsInApplicationRole ADD InsertedOn datetime NULL
GO
UPDATE dbo.AvailablePersonsInApplicationRole SET InsertedOn = '1900-01-01'
ALTER TABLE dbo.AvailablePersonsInApplicationRole ALTER COLUMN InsertedOn datetime NOT NULL
ALTER TABLE dbo.AvailablePersonsInApplicationRole ADD  CONSTRAINT [DF_AvailablePersonsInApplicationRole_InsertedOn]  DEFAULT (getutcdate()) FOR [InsertedOn]
GO

ALTER TABLE dbo.PersonInApplicationRole ADD InsertedOn datetime NULL
GO
UPDATE dbo.PersonInApplicationRole SET InsertedOn = '1900-01-01'
ALTER TABLE dbo.PersonInApplicationRole ALTER COLUMN InsertedOn datetime NOT NULL
ALTER TABLE dbo.PersonInApplicationRole ADD  CONSTRAINT [DF_PersonInApplicationRole_InsertedOn]  DEFAULT (getutcdate()) FOR [InsertedOn]
GO

--track changes on PersonRequest (db time)
ALTER TABLE dbo.PersonRequest ADD UpdatedOnServerUtc datetime NULL
GO
UPDATE dbo.PersonRequest SET UpdatedOnServerUtc = GETUTCDATE()
GO
ALTER TABLE dbo.PersonRequest ALTER COLUMN UpdatedOnServerUtc datetime NOT NULL
GO

ALTER TABLE dbo.PersonRequest ADD CONSTRAINT DF_PersonRequest_UpdatedOnServerUtc DEFAULT getutcdate() FOR UpdatedOnServerUtc
GO
GO

----------------  
--Name: David Jonsson
--Date: 2013-05-04
--Desc: Bug #23349 Speed up rotations
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonRotation]') AND name = N'IX_PersonRotation_Person_IsDeleted_BusinessUnit_StartDate')
CREATE NONCLUSTERED INDEX [IX_PersonRotation_Person_IsDeleted_BusinessUnit_StartDate]
ON [dbo].[PersonRotation] ([Person],[IsDeleted],[BusinessUnit],[StartDate])
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (384,'7.4.384') 
