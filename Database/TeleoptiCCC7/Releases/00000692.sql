ALTER TABLE [dbo].[PersonAbsence]  DROP CONSTRAINT [FK_PersonAbsence_AbsenceRequest] 
GO

ALTER TABLE dbo.PersonAbsence DROP COLUMN [AbsenceRequest]
GO

ALTER TABLE Auditing.PersonAbsence_AUD DROP COLUMN [AbsenceRequest]
GO