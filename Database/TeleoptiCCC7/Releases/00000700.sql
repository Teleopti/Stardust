ALTER TABLE [ReadModel].[AdherencePercentage] DROP COLUMN [NightShift]

GO

ALTER TABLE [ReadModel].[AdherencePercentage] ADD [ShiftStartTime] DateTime NULL

GO

ALTER TABLE [ReadModel].[AdherencePercentage] ADD [ShiftEndTime] DateTime NULL

