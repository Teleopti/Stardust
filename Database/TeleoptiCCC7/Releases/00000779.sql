CREATE TABLE [ReadModel].[SkillCombinationResource](
	[SkillCombinationId] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[Resource] [float] NOT NULL,
	[InsertedOn] [datetime] NOT NULL
 CONSTRAINT [PK_SkillCombinationResource] PRIMARY KEY CLUSTERED 
(
	[SkillCombinationId] ASC,
	[StartDateTime] ASC
) )

GO

CREATE TABLE [ReadModel].[SkillCombination](
	[Id] [uniqueidentifier] NOT NULL,
	[SkillId] [uniqueidentifier] NOT NULL,
	[InsertedOn] [datetime] NOT NULL
 CONSTRAINT [PK_SkillCombination] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[SkillId] ASC
)) 

GO

