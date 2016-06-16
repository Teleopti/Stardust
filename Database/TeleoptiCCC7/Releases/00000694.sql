CREATE TABLE [ReadModel].[PersonSkills]
	(
	PersonId uniqueidentifier NOT NULL,
	SkillId uniqueidentifier NOT NULL
	)
ON [PRIMARY]
ALTER TABLE  [ReadModel].[PersonSkills] ADD CONSTRAINT [UQ_Person_Skill] UNIQUE NONCLUSTERED 
(
	[PersonId] ASC,
	[SkillId] ASC
)
ON [PRIMARY]