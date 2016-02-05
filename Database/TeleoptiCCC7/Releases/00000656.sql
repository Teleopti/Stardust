ALTER TABLE ReadModel.AdherencePercentage
ADD [Version] int NULL
GO
UPDATE ReadModel.AdherencePercentage SET [Version] = 0
GO
