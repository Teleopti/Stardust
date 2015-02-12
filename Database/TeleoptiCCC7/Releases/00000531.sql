DELETE [ReadModel].[AdherencePercentage]
GO

ALTER TABLE [ReadModel].[AdherencePercentage] ADD [ShiftEndTime] [datetime] NOT NULL
GO