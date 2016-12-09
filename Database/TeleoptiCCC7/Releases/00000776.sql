ALTER TABLE dbo.PersonAbsence
	DROP CONSTRAINT FK_PersonAbsence_PersonRequest
GO
ALTER TABLE dbo.PersonAbsence
	DROP COLUMN PersonRequest
GO
