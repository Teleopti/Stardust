CREATE TABLE [ReadModel].[FindPerson](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[EmploymentNumber] [nvarchar](50) NOT NULL,
	[Note] [nvarchar](1024) NOT NULL,
	[TerminalDate] [datetime] NULL,
	[SearchValue] [nvarchar](max) NULL,
	[SearchType] [nvarchar](200) NOT NULL,
	[TeamId] [uniqueidentifier]  NULL,
	[SiteId] [uniqueidentifier]  NULL,
	[BusinessUnitId] [uniqueidentifier]  NULL
	)

GO

ALTER TABLE ReadModel.[FindPerson] ADD CONSTRAINT
	PK_FindPerson PRIMARY KEY NONCLUSTERED 
	(
	Id
	)
GO

CREATE CLUSTERED INDEX [CIX_FindPerson] ON [ReadModel].[FindPerson] 
(
	[TerminalDate] ASC
)
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (348,'7.1.348') 
