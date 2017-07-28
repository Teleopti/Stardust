ALTER TABLE [ReadModel].[SkillCombination]  WITH CHECK ADD CONSTRAINT [FK_SkillCombination_Skill_Id] FOREIGN KEY([SkillId])
REFERENCES [dbo].[Skill] ([Id])
GO