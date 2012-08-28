
----------------  
--Name: Xianwei Shen
--Date: 2012-08-21
--Desc: Add options for whether contract time should come from contract of schedule period
----------------  	
ALTER TABLE dbo.Contract ADD
	IsWorkTimeFromContract int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromContract DEFAULT 1,
	IsWorkTimeFromSchedulePeriod int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromSchedulePeriod DEFAULT 0
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduleDay]') AND type in (N'U'))
BEGIN
CREATE TABLE [ReadModel].[ScheduleDay](
		[Id] [uniqueidentifier] NOT NULL,
		[PersonId] [uniqueidentifier] NOT NULL,
		[BelongsToDate] [smalldatetime] NOT NULL,
		[StartDateTime] [datetime] NOT NULL,
		[EndDateTime] [datetime] NOT NULL,
		[WorkDay] [bit] NOT NULL,
		[WorkTime] [bigint] NOT NULL,
		[ContractTime] [bigint] NOT NULL,
		[Name] [nvarchar](50) NOT NULL,
		[ShortName] [nvarchar](25) NULL,
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
		[BelongsToDate] ASC,
		[PersonId] ASC
	)

	ALTER TABLE [ReadModel].[ScheduleDay] ADD  CONSTRAINT [DF_ScheduleDayReadOnly_Id]  DEFAULT (newid()) FOR [Id]
END
GO