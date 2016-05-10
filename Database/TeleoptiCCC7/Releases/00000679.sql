CREATE TABLE [dbo].[ProjectionVersion](
	[Person] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Version] [int] NULL

CONSTRAINT [PK_ProjectionVersion] PRIMARY KEY NONCLUSTERED 
(
	[Person] ASC,
	[Date] ASC
)
)