drop table [ReadModel].[SkillForecast]
GO

CREATE TABLE [ReadModel].[SkillForecast](
	[SkillId] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[Agents] [float] NOT NULL,
	[AgentsWithShrinkage] [float] NOT NULL,
	[Calls] [float] NOT NULL,
	[AverageHandleTime] [float] NOT NULL,
	[IsBackOffice] bit NOT NULL,
	[InsertedOn] [datetime] NOT NULL	
)
GO

Alter table [ReadModel].[SkillForecast] Add CONSTRAINT [PK_SkillForecast] PRIMARY KEY CLUSTERED 
(
	[SkillId],
	[StartDateTime]
)
GO

ALTER TABLE [ReadModel].[SkillForecast] With NOCHECK ADD  CONSTRAINT [FK_SkillForecast_Skill] FOREIGN KEY([SkillId])
REFERENCES [dbo].[Skill] ([Id])
GO

ALTER TABLE [ReadModel].[SkillForecast] CHECK CONSTRAINT [FK_SkillForecast_Skill]
GO