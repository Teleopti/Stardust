DROP TABLE [ReadModel].[PersonSkills]
CREATE TABLE [ReadModel].[PersonSkills] (
  PersonId uniqueidentifier NOT NULL,
  SkillId uniqueidentifier NOT NULL
  CONSTRAINT PK_PersonSkills PRIMARY KEY CLUSTERED
  (
  [PersonId] ASC,
  [SkillId] ASC
  )
) ON [PRIMARY]