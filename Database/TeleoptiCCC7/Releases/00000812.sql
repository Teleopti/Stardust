  ALTER TABLE [ReadModel].[SkillCombinationResourceDelta] ADD DeltaResource float NULL
GO
UPDATE [ReadModel].[SkillCombinationResourceDelta] SET DeltaResource = -1
GO
ALTER TABLE[ReadModel].[SkillCombinationResourceDelta] ALTER COLUMN DeltaResource float NOT NULL
GO