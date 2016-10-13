CREATE TABLE [ReadModel].[HistoricalAdherence] (
	PersonId uniqueidentifier not null,
	[Timestamp] datetime not null,
	Adherence int not null
)
GO

CREATE CLUSTERED INDEX CIX_PersonIdTimestamp ON [ReadModel].[HistoricalAdherence] (PersonId, [Timestamp])
GO