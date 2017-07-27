DROP TABLE [ReadModel].[SourceBpo]
GO

CREATE TABLE [BusinessProcessOutsourcer](
	[Id] [uniqueidentifier] NOT NULL,
	[Source] varchar(100) NOT NULL
 CONSTRAINT [PK_BusinessProcessOutsourcer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [ReadModel].[SkillCombinationResourceBpo]
ADD CONSTRAINT FK_SkillCombinationResourceBpo_BusinessProcessOutsourcer_Id
foreign key (SourceId) references [BusinessProcessOutsourcer] ([Id])
GO