CREATE TABLE [ReadModel].[PersonSkills]
	(
	Id uniqueidentifier NOT NULL,
	PersonId uniqueidentifier NOT NULL,
	SkillId uniqueidentifier NOT NULL
	)
ON [PRIMARY]
ALTER TABLE [ReadModel].[PersonSkills] ADD CONSTRAINT PK_PersonSkills PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
ON [PRIMARY]