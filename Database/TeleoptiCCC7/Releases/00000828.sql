CREATE TABLE [ReadModel].[HistoricalChange] (
	PersonId uniqueidentifier,
	BelongsToDate datetime,
	[Timestamp] datetime,
	StateName nvarchar(300),
	StateGroupId uniqueidentifier,
	ActivityName nvarchar(50),
	ActivityColor int,
	RuleName nvarchar(50),
	RuleColor int,
	Adherence int
)
GO

CREATE CLUSTERED INDEX IX_PersonId ON [ReadModel].[HistoricalChange] (PersonId)
GO
