CREATE TABLE [ReadModel].[ScheduleForecastSkill](
	[SkillId] [uniqueidentifier] NOT NULL,
	[BelongsToDate] [date] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[Forecast] [float] NOT NULL,
	[StaffingLevel] [float] NOT NULL,
	[InsertedOn] [datetime] NOT NULL CONSTRAINT [DF_ScheduleForecastSkill_InsertedOn]  DEFAULT (getutcdate()),
 CONSTRAINT [PK_ScheduleForecastSkill] PRIMARY KEY CLUSTERED 
(
	[SkillId] ASC,
	[StartDateTime] ASC
)) ON [PRIMARY]
GO
