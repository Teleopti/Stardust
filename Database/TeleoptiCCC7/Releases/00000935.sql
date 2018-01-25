ALTER TABLE ReadModel.SkillCombinationResourceDelta
ADD  BusinessUnit uniqueidentifier NULL
GO
UPDATE d SET BusinessUnit = s.BusinessUnit
FROM ReadModel.SkillCombinationResourceDelta d
 INNER JOIN ReadModel.SkillCombination c ON d.SkillCombinationId = c.Id
 INNER JOIN Skill s ON s.Id = c.SkillId
GO
 ALTER TABLE ReadModel.SkillCombinationResourceDelta
ALTER COLUMN  BusinessUnit uniqueidentifier NOT NULL





