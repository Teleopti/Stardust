ALTER TABLE ReadModel.AdherenceDetails
ADD [Version] int NULL
GO
UPDATE ReadModel.AdherenceDetails SET [Version] = 0
GO
