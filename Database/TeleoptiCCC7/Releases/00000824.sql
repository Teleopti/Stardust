alter table [ReadModel].[SkillCombinationResourceDelta] add id UniqueIdentifier NOT NULL default newid() with values  

ALTER TABLE [ReadModel].[SkillCombinationResourceDelta] DROP CONSTRAINT PK_SkillCombinationResourceDelta

ALTER TABLE [ReadModel].[SkillCombinationResourceDelta] ADD CONSTRAINT PK_SkillCombinationResourceDelta PRIMARY KEY (id)