--Remove cascading deletes on shiftlayer -> personassignment fk

ALTER TABLE [dbo].[ShiftLayer] DROP CONSTRAINT FK_ShiftLayer_PersonAssignment
GO

ALTER TABLE [dbo].[ShiftLayer]
WITH CHECK ADD CONSTRAINT FK_ShiftLayer_PersonAssignment FOREIGN KEY(Parent)
REFERENCES [dbo].[PersonAssignment] (Id)
GO 