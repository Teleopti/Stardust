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

