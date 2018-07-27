
ALTER TABLE [ReadModel].[AgentState] DROP COLUMN [HasName]
GO

UPDATE
	ReadModel.AgentState
SET
	HasAssociation = 0
FROM 
	ReadModel.AgentState rm
JOIN Person p ON p.Id = rm.PersonId
WHERE 
	p.IsDeleted = 1 AND
	rm.HasAssociation = 1

DELETE PersonAssociationCheckSum
GO

UPDATE ReadModel.KeyValueStore SET Value = 'True' WHERE [Key] = 'PersonAssociationChangedPublishTrigger'
GO
