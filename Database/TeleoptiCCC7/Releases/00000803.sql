ALTER TABLE dbo.AgentState DROP CONSTRAINT PK_AgentState
GO

SP_RENAME '[dbo].[AgentState].[BatchId]', 'SnapshotId'
GO

SP_RENAME '[dbo].[AgentState].[DataSourceId]', 'SnapshotDataSourceId'
GO

DELETE a FROM 
dbo.agentstate a
WHERE a.DataSourceIdUserCode <> (
	SELECT MIN(DataSourceIdUserCode)
	FROM dbo.AgentState
	WHERE PersonId = a.PersonId
)

ALTER TABLE dbo.AgentState DROP COLUMN DataSourceIdUserCode
GO

DROP INDEX IX_AgentState_PersonId ON dbo.AgentState
GO

ALTER TABLE dbo.AgentState ADD CONSTRAINT PK_AgentState PRIMARY KEY CLUSTERED (PersonId)
GO
