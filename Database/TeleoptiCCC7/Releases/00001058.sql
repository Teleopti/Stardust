ALTER TABLE [ReadModel].[SkillCombinationResourceDelta] DROP CONSTRAINT PK_SkillCombinationResourceDelta

ALTER TABLE [ReadModel].[SkillCombinationResourceDelta] ADD CONSTRAINT [PK_SkillCombinationResourceDelta] PRIMARY KEY CLUSTERED 
(
       [SkillCombinationId],
       [BusinessUnit],
       [StartDateTime],
       [EndDateTime],
       [id]
)