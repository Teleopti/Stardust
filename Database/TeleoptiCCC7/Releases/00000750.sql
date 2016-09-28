ALTER TABLE [dbo].[ProjectionVersion] DROP CONSTRAINT [PK_ProjectionVersion]
GO
ALTER TABLE [dbo].[ProjectionVersion] ADD  CONSTRAINT [PK_ProjectionVersion] PRIMARY KEY CLUSTERED 
(
	[Person] ASC,
	[Date] ASC
)
GO