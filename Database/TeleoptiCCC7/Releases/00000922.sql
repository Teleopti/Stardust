ALTER TABLE [dbo].[AgentState]
DROP COLUMN  [SnapshotId]
GO

ALTER TABLE [dbo].[AgentState]
ADD [SnapshotId] BIGINT NULL
GO