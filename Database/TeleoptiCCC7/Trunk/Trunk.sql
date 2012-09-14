
----------------  
--Name: Xianwei Shen
--Date: 2012-08-21
--Desc: Add options for whether contract time should come from contract of schedule period
----------------  	
ALTER TABLE dbo.Contract ADD
	IsWorkTimeFromContract int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromContract DEFAULT 1,
	IsWorkTimeFromSchedulePeriod int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromSchedulePeriod DEFAULT 0
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
