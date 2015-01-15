----------------  
--Name: Team Real Time
--Desc: Read model column names and new column
----------------  

DROP TABLE [ReadModel].[SiteAdherence]
GO

CREATE TABLE [ReadModel].[SiteOutOfAdherence](
	[SiteId] [uniqueidentifier] NULL,
	[BusinessUnitId] [uniqueidentifier] NULL,
	[Count] [int] NULL,
	[PersonIds] [varchar](MAX) NULL,
	CONSTRAINT [PK_Site] PRIMARY KEY CLUSTERED 
	(
		[SiteId] ASC
	)
)
GO

CREATE NONCLUSTERED INDEX [IX_SiteOutOfAdherence_BusinessUnitId] ON [ReadModel].[SiteOutOfAdherence] 
(
	[BusinessUnitId] ASC
)
GO

