ALTER TABLE dbo.PersonAbsence ADD 
[AbsenceRequest] [uniqueidentifier] NULL

GO

ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_AbsenceRequest] FOREIGN KEY([AbsenceRequest])
REFERENCES [dbo].[AbsenceRequest] ([Request])
GO
