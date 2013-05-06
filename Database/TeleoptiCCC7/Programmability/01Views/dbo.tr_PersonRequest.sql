if exists (select * from sysobjects where id = object_id(N'[dbo].[tr_PersonRequest_UpdatedOn]') and OBJECTPROPERTY(id, N'IsTrigger') = 1)
    drop trigger [dbo].[tr_PersonRequest_UpdatedOn]
GO

CREATE TRIGGER tr_PersonRequest_UpdatedOn ON dbo.PersonRequest
AFTER UPDATE 
AS
SET NOCOUNT ON
UPDATE dbo.PersonRequest
SET UpdatedOnServerUtc = getutcdate()
FROM INSERTED i
WHERE i.Id = dbo.PersonRequest.id
GO
