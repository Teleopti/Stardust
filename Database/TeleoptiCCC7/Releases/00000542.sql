-----------------  
---Name: MD
---Date: 2015-02-12
---Desc: trying to fix azure
----------------- 
--DROP TABLE [dbo].[SkillMap_DEV]

CREATE TABLE [dbo].[SkillMap_DEV](
	[Skill] [uniqueidentifier] NOT NULL,
	[MappedSkill] [uniqueidentifier] NULL,
 CONSTRAINT [PK_SkillMap_DEV] PRIMARY KEY CLUSTERED 
(
	[Skill] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SkillMap_DEV]  WITH CHECK ADD  CONSTRAINT [FK_Skill_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO

ALTER TABLE [dbo].[SkillMap_DEV] CHECK CONSTRAINT [FK_Skill_Skill]
GO

ALTER TABLE [dbo].[SkillMap_DEV]  WITH CHECK ADD  CONSTRAINT [FK_MappedSkill_Skill] FOREIGN KEY([MappedSkill])
REFERENCES [dbo].[Skill] ([Id])
GO

ALTER TABLE [dbo].[SkillMap_DEV] CHECK CONSTRAINT [FK_MappedSkill_Skill]
GO


