ALTER TABLE [ReadModel].[SkillCombinationResourceBpo]
ADD	[BusinessUnit] [uniqueidentifier] NOT NULL

GO

ALTER TABLE [ReadModel].[SkillCombinationResourceBpo]  WITH CHECK ADD CONSTRAINT [FK_SkillCombinationResourceBpo_BusinessUnit_Id] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

