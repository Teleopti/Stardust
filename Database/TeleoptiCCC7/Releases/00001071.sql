----------------  
--Date: 2019-01-25
--Desc: Extend favorite search index with business unit
----------------  
DROP INDEX [IX_FavoriteSearch_Creator_Name_WfmArea] ON [dbo].[FavoriteSearch]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_FavoriteSearch_Creator_Name_WfmArea] ON [dbo].[FavoriteSearch]
	(
		[Creator] ASC,
		[BusinessUnit] ASC,
		[Name] ASC,
		[WfmArea] ASC
	)
GO
	