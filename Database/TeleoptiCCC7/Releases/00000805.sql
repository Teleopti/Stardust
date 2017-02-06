ALTER TABLE dbo.AgentState DROP COLUMN StateCode
GO

ALTER TABLE ReadModel.RuleMappings ADD IsLoggedOut bit
GO

UPDATE m
SET m.IsLoggedOut = g.IsLogOutState
FROM ReadModel.RuleMappings m
JOIN RtaStateGroup g ON g.Id = m.StateGroupId
GO

UPDATE ReadModel.KeyValueStore SET Value = 'True' WHERE [Key] = 'RuleMappingsInvalido'
GO
