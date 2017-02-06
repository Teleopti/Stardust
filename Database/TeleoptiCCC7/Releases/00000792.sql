----------------  
--Name: Dumpling
--Date: 2017-01-19
--Desc: Extend favorite search with wfm area
----------------  
SET NOCOUNT ON

ALTER TABLE dbo.FavoriteSearch ADD
	WfmArea int NOT NULL DEFAULT (1)	
GO
		
DROP INDEX [IX_FavoriteSearch_Creator_Name] ON [dbo].[FavoriteSearch]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_FavoriteSearch_Creator_Name_WfmArea] ON [dbo].[FavoriteSearch]
(
	[Name] ASC,
	[Creator] ASC,
	[WfmArea] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
	