CREATE TABLE [ReadModel].[HistoricalOverview](
	[PersonId] [uniqueidentifier] NOT NULL,
	[Date] [smalldatetime] NOT NULL,
	[Adherence] [int] NULL,
	[WasLateForWork] [bit] NULL,
	[MinutesLateForWork] [int] NULL,
	[ShiftLength][int] NULL	
CONSTRAINT [PK_HistoricalOverview] PRIMARY KEY CLUSTERED 
	(
		[Date] ASC,
		[PersonId] ASC
	) 
) 
GO
