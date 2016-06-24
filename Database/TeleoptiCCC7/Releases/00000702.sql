ALTER TABLE [dbo].[PersonAbsence]  DROP CONSTRAINT [FK_PersonAbsence_PersonRequest] 
GO

ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_PersonRequest] FOREIGN KEY ([PersonRequest])
REFERENCES [dbo].[PersonRequest]
ON DELETE SET NULL
GO


