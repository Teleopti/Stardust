ALTER TABLE [mart].[dim_group_page] DROP CONSTRAINT [AK_group_id]

ALTER TABLE [mart].[dim_group_page] DROP COLUMN [group_id]

ALTER TABLE [mart].[dim_group_page] ADD [group_id] AS [group_page_id]

ALTER TABLE [mart].[dim_group_page] ADD CONSTRAINT [AK_group_id] UNIQUE NONCLUSTERED 
(
	[group_id] ASC
)
GO

