DROP INDEX [IX_AgentState_DataSourceIdUserCode] ON dbo.AgentState
GO

ALTER TABLE dbo.AgentState ADD DataSourceIdUserCode NVARCHAR(150) NULL
GO

UPDATE dbo.AgentState SET DataSourceIdUserCode = CAST(DataSourceId AS nvarchar) + '__' + UserCode
GO

UPDATE dbo.AgentState SET DataSourceIdUserCode = '' WHERE DataSourceIdUserCode IS NULL
GO

ALTER TABLE dbo.AgentState ALTER COLUMN DataSourceIdUserCode NVARCHAR(150) NOT NULL
GO

ALTER TABLE dbo.AgentState DROP COLUMN DataSourceId
GO

ALTER TABLE dbo.AgentState DROP COLUMN UserCode
GO

ALTER TABLE dbo.AgentState ADD CONSTRAINT PK_AgentState PRIMARY KEY CLUSTERED (DataSourceIdUserCode, PersonId)
GO