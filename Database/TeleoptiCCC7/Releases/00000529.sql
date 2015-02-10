DELETE [ReadModel].[AdherencePercentage]
GO

ALTER TABLE [ReadModel].[AdherencePercentage] ADD [State] nvarchar(MAX) NULL
GO