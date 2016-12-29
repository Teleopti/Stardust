IF OBJECT_ID('dbo.DeltaSkillCombinationResource', 'U') IS NOT NULL 
  DROP TABLE dbo.Scores; 

  CREATE TABLE [ReadModel].[SkillCombinationResourceDelta](
	[SkillCombinationId] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[InsertedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_SkillCombinationResourceDelta] PRIMARY KEY CLUSTERED 
(
	[SkillCombinationId] ASC,
	[StartDateTime] ASC,
	[InsertedOn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO