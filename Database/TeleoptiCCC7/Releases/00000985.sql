ALTER TABLE readmodel.SkillCombinationResourceBpo
ADD ImportFilename VARCHAR(500), PersonId uniqueidentifier
GO
ALTER TABLE [ReadModel].[SkillCombinationResourceBpo]  WITH NOCHECK ADD  CONSTRAINT [FK_SkillCombinationResourceBpo_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
GO
