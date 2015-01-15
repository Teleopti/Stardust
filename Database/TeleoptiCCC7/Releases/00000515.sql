----------------  
--Name: Team Real Time
--Desc: Read model column names and new column
----------------  

DROP TABLE [ReadModel].[TeamAdherence]
GO

CREATE TABLE [ReadModel].[TeamOutOfAdherence](
	[SiteId] [uniqueidentifier] NULL,
	[TeamId] [uniqueidentifier] NULL,
	[Count] [int] NULL,
	[PersonIds] [varchar](MAX) NULL
)
GO
