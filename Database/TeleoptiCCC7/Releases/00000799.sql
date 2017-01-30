ALTER TABLE dbo.AgentState DROP COLUMN SourceId
GO

ALTER TABLE dbo.AgentState ADD DataSourceId INT
GO

UPDATE dbo.AgentState SET DataSourceId = CAST(SUBSTRING(DataSourceIdUserCode, 0, CHARINDEX('__', DataSourceIdUserCode)) AS INT)
GO
