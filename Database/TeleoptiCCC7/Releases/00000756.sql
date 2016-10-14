--bug #40731 - Remove NULL values
--Becuase the later statments in this script would generate yet another null value when concatinating NULL + srting = NULL
DELETE FROM dbo.AgentState WHERE DataSourceId IS NULL
DELETE FROM dbo.AgentState WHERE StateCode IS NULL
GO

--bug #41161 - Remove any remaining duplicates
WITH cte as(
  SELECT ROW_NUMBER() OVER (PARTITION BY PersonId,DataSourceId,StateCode
                            ORDER BY  ReceivedTime DESC ) RN
  FROM   dbo.AgentState
  )
delete from cte where RN>1


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