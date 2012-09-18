
----------------  
--Name: Xianwei Shen
--Date: 2012-08-21
--Desc: Add options for whether contract time should come from contract of schedule period
----------------  	
ALTER TABLE dbo.Contract ADD
	WorkTimeSource int NOT NULL CONSTRAINT DF_Contract_WorkTimeSource DEFAULT 0
GO

----------------  
--Name: Asad MIrza
--Date: 2012-08-29
--Desc: Added an extra column in activity to indicate if we can overwrite it or not
---------------- 
ALTER TABLE dbo.Activity ADD
	AllowOverwrite bit NOT NULL CONSTRAINT DF_Activity_AllowOverwrite DEFAULT 1
GO

update  dbo.activity SET AllowOverwrite = 0 where InWorkTime  = 0


GO
GO
----------------  
----------------  
--Name: David J + Andreas S
--Date: 2012-00-11
--Desc: prepare Table for PS Tech custom SMS bridge
----------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[CustomTables].[SMS]') AND type in (N'U'))
BEGIN
	CREATE TABLE [CustomTables].[SMS](
		[Id] [uniqueidentifier] NOT NULL,
		[PhoneNumber] [nvarchar](50) NULL,
		[Msg] nvarchar(max) NULL,
		[RecivedTime] [datetime] NULL,
		[DeliveredTime] [datetime] NULL,
		[LastTriedTime] [datetime] NULL,
		[IsSent] [bit] NOT NULL DEFAULT 0,
		[ToBeSent] [bit] NOT NULL DEFAULT 1,
		[Status] [nvarchar](200) NOT NULL DEFAULT 'init'
		)
	           
	ALTER TABLE CustomTables.SMS ADD CONSTRAINT
		PK_SMS PRIMARY KEY CLUSTERED 
		(
		Id
		)
END
GO
--Name: Ola
--Date: 2012-08-31
--Desc: Add new ReadModel
---------------- 
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduleDay]') AND type in (N'U'))
BEGIN
CREATE TABLE [ReadModel].[ScheduleDay](
		[Id] [uniqueidentifier] NOT NULL,
		[PersonId] [uniqueidentifier] NOT NULL,
		[BelongsToDate] [smalldatetime] NOT NULL,
		[StartDateTime] [datetime] NOT NULL,
		[EndDateTime] [datetime] NOT NULL,
		[Workday] [bit] NOT NULL,
		[WorkTime] [bigint] NOT NULL,
		[ContractTime] [bigint] NOT NULL,
		[Label] [nvarchar](50) NOT NULL,
		[DisplayColor] [int] NOT NULL,
		[InsertedOn] [datetime] NOT NULL
	)
	
	
	ALTER TABLE ReadModel.[ScheduleDay] ADD CONSTRAINT
		PK_ScheduleDayReadOnly PRIMARY KEY NONCLUSTERED 
		(
		Id
		)

	CREATE CLUSTERED INDEX [CIX_ScheduleDayReadOnly] ON [ReadModel].[ScheduleDay] 
	(
		[PersonId] ASC,
		[BelongsToDate] ASC	
	)

	ALTER TABLE [ReadModel].[ScheduleDay] ADD  CONSTRAINT [DF_ScheduleDayReadOnly_Id]  DEFAULT (newid()) FOR [Id]
END
GO


----------------  
--Name: Jonas N
--Date: 2012-09-17  
--Desc: Add the following new application function Agent Schedule Messenger (MyTimeWeb)
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
SELECT @ForeignId = '0078' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'AgentScheduleMessenger' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxAgentScheduleMessenger' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Kunning Mao
--Date: 2012-09-11  
--Desc: Add the following new application function> ExtendedPreferencesWeb
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
SELECT @ForeignId = '0079' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ExtendedPreferencesWeb' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxExtendedPreferencesWeb' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO
