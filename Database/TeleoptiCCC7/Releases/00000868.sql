CREATE TABLE [ReadModel].[SourceBpo](
	[Id] [uniqueidentifier] NOT NULL,
	[Source] varchar(100) NOT NULL
 CONSTRAINT [PK_SourceBpo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[Source] ASC
)
) ON [PRIMARY]

GO

 CREATE TABLE [ReadModel].[SkillCombinationResourceBpo](
	[SkillCombinationId] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[InsertedOn] [datetime] NOT NULL,
	[Resources] [float] NOT NULL,
	[SourceId] [uniqueidentifier] NOT NULL
 CONSTRAINT [PK_SkillCombinationResourceBpo] PRIMARY KEY CLUSTERED 
(
	[SkillCombinationId] ASC,
	[StartDateTime] ASC,
	[SourceId] ASC
)
) ON [PRIMARY]

GO