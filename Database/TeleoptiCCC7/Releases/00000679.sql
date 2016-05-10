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
GO

INSERT INTO
	[dbo].[ProjectionVersion]
	(
		[Person],
		[Date],
		[Version]
	)
SELECT 
	[Person],
	[Date],
	0
FROM
	[dbo].[PersonAssignment]
GO