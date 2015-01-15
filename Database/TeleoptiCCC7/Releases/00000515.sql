----------------  
--Name: Team Real Time
--Desc: Read model column names and new column
----------------  

DROP TABLE [ReadModel].[TeamAdherence]
GO

CREATE TABLE [ReadModel].[TeamOutOfAdherence](
	[TeamId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NULL,
	[Count] [int] NULL,
	[PersonIds] [varchar](MAX) NULL,
	CONSTRAINT [PK_TeamOutOfAdherence] PRIMARY KEY CLUSTERED 
	(
		[TeamId] ASC
	)
)
GO

CREATE NONCLUSTERED INDEX [IX_TeamOutOfAdherence_SiteId] ON [ReadModel].[TeamOutOfAdherence] 
(
	[SiteId] ASC
)
GO

