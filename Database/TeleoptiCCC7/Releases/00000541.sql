-- =============================================
-- Author: Micke Deig�rd
-- Description:	Added temporary storage for skill mappings
-- =============================================
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SkillMap_DEV](
	[Skill] [uniqueidentifier] NOT NULL,
	[MappedSkill] [uniqueidentifier] NULL,
 CONSTRAINT [PK_SkillMap_DEV] PRIMARY KEY CLUSTERED 
(
	[Skill] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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