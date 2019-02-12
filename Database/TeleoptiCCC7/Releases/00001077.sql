CREATE TABLE [dbo].[SkillForecastJobStartTime](
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[LockTimestamp] [datetime] NULL,
 CONSTRAINT [PK_SkillForecastJobStartTime] PRIMARY KEY CLUSTERED 
(
	[BusinessUnit] ASC
)
) ON [PRIMARY]


