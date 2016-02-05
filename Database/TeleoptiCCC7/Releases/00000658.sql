ALTER TABLE ReadModel.TeamOutOfAdherence
ADD [Version] int NULL
GO
UPDATE ReadModel.TeamOutOfAdherence SET [Version] = 0
GO
