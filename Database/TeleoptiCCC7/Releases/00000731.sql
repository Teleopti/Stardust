-- Convert to seconds
UPDATE [dbo].[RtaRule] SET ThresholdTime = ThresholdTime / 10000000
GO

ALTER TABLE [dbo].[RtaRule] ALTER COLUMN [ThresholdTime] int NOT NULL
