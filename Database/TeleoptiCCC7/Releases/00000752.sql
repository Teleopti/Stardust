CREATE TABLE [ReadModel].[ScheduleForecastSkillChange](
	[SkillId] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[StaffingLevel] [float] NOT NULL,
	[InsertedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_ScheduleForecastSkillChange] PRIMARY KEY CLUSTERED 
(
	[SkillId] ASC,
	[StartDateTime] ASC,
	[EndDateTime] ASC,
	[InsertedOn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]