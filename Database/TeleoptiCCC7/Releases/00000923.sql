ALTER TABLE [dbo].[ExternalPerformanceData]
DROP COLUMN  [Score]
GO

ALTER TABLE [dbo].[ExternalPerformanceData] ADD [Score] FLOAT NOT NULL
GO