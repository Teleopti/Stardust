ALTER TABLE dbo.PersonAbsence ADD 
[PersonRequest] [uniqueidentifier] NULL

GO

ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_PersonRequest] FOREIGN KEY ([PersonRequest])
REFERENCES [dbo].[PersonRequest]
GO

ALTER TABLE Auditing.PersonAbsence_AUD ADD 
[PersonRequest] [uniqueidentifier] NULL

GO