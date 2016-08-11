----------------  
--Name: Anders
--Desc: bug #40027
---------------- 
--No need to rename before changing the table as it can be re-created easily
DROP TABLE [ReadModel].[FindPerson]

CREATE TABLE [ReadModel].[FindPerson](
	[PersonId] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[EmploymentNumber] [nvarchar](50) NOT NULL,
	[Note] [nvarchar](1024) NOT NULL,
	[TerminalDate] [datetime] NULL,
	[SearchValue] [nvarchar](max) NULL,
	[SearchType] [nvarchar](200) NOT NULL,
	[TeamId] [uniqueidentifier] NULL,
	[SiteId] [uniqueidentifier] NULL,
	[BusinessUnitId] [uniqueidentifier] NULL,
	[SearchValueId] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NULL,
 CONSTRAINT [PK_FindPerson] PRIMARY KEY CLUSTERED 
(
[PersonId] ASC, [StartDateTime] ASC, [SearchType] ASC, [SearchValueId] ASC
)
)
--The readmodel will be re-populated when the proc is scripted in
