--recreate one index and make it clustered
DROP INDEX [IX_PersonId] ON [ReadModel].[ExternalLogon]
GO
CREATE CLUSTERED INDEX [CIX_PersonId] ON [ReadModel].[ExternalLogon]
(
	[PersonId] ASC
)
GO



