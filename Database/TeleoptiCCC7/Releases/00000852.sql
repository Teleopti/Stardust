----------------  
--Name: Chundan Xu
--Date: 2017-6-19
--Desc: create UserDevice table 
--to save user mobile device token on Firebase
---------------- 

CREATE TABLE [dbo].[UserDevice](
	[Id] [uniqueidentifier] NOT NULL,
	[Owner] [uniqueidentifier] NOT NULL,
	[Token] [nvarchar](255) NOT NULL
 CONSTRAINT [PK_UserDevice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[UserDevice]  WITH CHECK ADD  CONSTRAINT [FK_UserDevice_Person] FOREIGN KEY([Owner])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[UserDevice] CHECK CONSTRAINT [FK_UserDevice_Person]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_UserDevice_Token] ON [dbo].[UserDevice]
(
	[Token] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

GO