----------------  
--Name: Dumpling
--Date: 2017-01-05
--Desc: Add new table for favorite search
---------------- 
IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[FavoriteSearch]'))
   DROP TABLE [dbo].[FavoriteSearch]
GO
CREATE TABLE [dbo].[FavoriteSearch](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[SearchTerm] [nvarchar](max) NULL,
	[TeamIds] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[Creator] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL
 CONSTRAINT [PK_FavoriteSearch] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[FavoriteSearch]  WITH CHECK ADD  CONSTRAINT [FK_FavoriteSearch_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id]);
Go

ALTER TABLE [dbo].[FavoriteSearch] CHECK CONSTRAINT [FK_FavoriteSearch_BusinessUnit]
GO

ALTER TABLE [dbo].[FavoriteSearch]  WITH CHECK ADD  CONSTRAINT [FK_FavoriteSearch_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[FavoriteSearch] CHECK CONSTRAINT [FK_FavoriteSearch_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[FavoriteSearch]  WITH CHECK ADD  CONSTRAINT [FK_FavoriteSearch_Person_Creator] FOREIGN KEY([Creator])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[FavoriteSearch] CHECK CONSTRAINT [FK_FavoriteSearch_Person_Creator]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_FavoriteSearch_Creator_Name] ON [dbo].[FavoriteSearch]
(
	[Name] ASC,
	[Creator] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
