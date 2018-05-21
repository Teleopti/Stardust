CREATE NONCLUSTERED INDEX IX_PersonRequest_Person_IsDeleted_BU
ON [dbo].[PersonRequest] ([Person],[IsDeleted],[BusinessUnit])
INCLUDE ([Id])