DROP TABLE [ReadModel].[HistoricalAdherence]
GO
CREATE TABLE [ReadModel].[HistoricalAdherence](
	[PersonId] [uniqueidentifier] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[Adherence] [int] NOT NULL
)
GO
CREATE CLUSTERED INDEX CIX_Timestamp_PersonId
ON ReadModel.HistoricalAdherence([Timestamp], PersonId)
GO

DROP TABLE ReadModel.HistoricalChange;
GO
CREATE TABLE [ReadModel].[HistoricalChange](
	[PersonId] [uniqueidentifier] NULL,
	[BelongsToDate] [datetime] NULL,
	[Timestamp] [datetime] NULL,
	[StateName] [nvarchar](300) NULL,
	[StateGroupId] [uniqueidentifier] NULL,
	[ActivityName] [nvarchar](50) NULL,
	[ActivityColor] [int] NULL,
	[RuleName] [nvarchar](50) NULL,
	[RuleColor] [int] NULL,
	[Adherence] [int] NULL
) 
GO
CREATE CLUSTERED INDEX CIX_Timestamp_PersonId
ON ReadModel.HistoricalChange([Timestamp], PersonId)


