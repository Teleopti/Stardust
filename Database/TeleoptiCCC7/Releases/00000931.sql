ALTER TABLE [dbo].[ShiftLayer] 
ADD CONSTRAINT CHK_ShiftLayer_OrderIndex
CHECK (OrderIndex >=0)
GO
