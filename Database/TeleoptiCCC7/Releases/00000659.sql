ALTER TABLE ReadModel.SiteOutOfAdherence
ADD [Version] int NULL
GO
UPDATE ReadModel.SiteOutOfAdherence SET [Version] = 0
GO
