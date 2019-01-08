CREATE NONCLUSTERED INDEX IX_SkillCombinationResource_SkillCombo_BU_Start_End
ON [ReadModel].[SkillCombinationResource] ([SkillCombinationId],[BusinessUnit],[StartDateTime],[EndDateTime])
INCLUDE ([Resource])