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

--#41161 - Make sure there are no duplicates before adding the PK
WITH cte as(
  SELECT ROW_NUMBER() OVER (PARTITION BY PersonId,DataSourceIdUserCode
                            ORDER BY  ReceivedTime DESC ) RN
  FROM   dbo.AgentState
  )
delete from cte where RN>1
GO

ALTER TABLE dbo.AgentState ADD CONSTRAINT PK_AgentState PRIMARY KEY CLUSTERED (DataSourceIdUserCode, PersonId)
GO