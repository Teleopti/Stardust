
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Skill
	DROP CONSTRAINT FK_Skill_BusinessUnit
GO
ALTER TABLE dbo.BusinessUnit SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Skill
	DROP CONSTRAINT FK_Skill_Activity
GO
ALTER TABLE dbo.Activity SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Skill
	DROP CONSTRAINT FK_Skill_SkillType
GO
ALTER TABLE dbo.SkillType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Skill
	DROP CONSTRAINT FK_Skill_Person_UpdatedBy
GO
ALTER TABLE dbo.Person SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Skill
	DROP CONSTRAINT DF_Skill_UnderstaffingFor
GO
CREATE TABLE dbo.Tmp_Skill
	(
	Id uniqueidentifier NOT NULL,
	Version int NOT NULL,
	UpdatedBy uniqueidentifier NOT NULL,
	UpdatedOn datetime NOT NULL,
	Name nvarchar(50) NOT NULL,
	DisplayColor int NOT NULL,
	Description nvarchar(1024) NOT NULL,
	DefaultResolution int NOT NULL,
	SkillType uniqueidentifier NOT NULL,
	Activity uniqueidentifier NOT NULL,
	TimeZone nvarchar(50) NOT NULL,
	MidnightBreakOffset bigint NOT NULL,
	SeriousUnderstaffing float(53) NOT NULL,
	Understaffing float(53) NOT NULL,
	Overstaffing float(53) NOT NULL,
	BusinessUnit uniqueidentifier NOT NULL,
	IsDeleted bit NOT NULL,
	Priority int NOT NULL,
	OverstaffingFactor float(53) NOT NULL,
	UnderstaffingFor float(53) NOT NULL,
	MaxParallelTasks int NOT NULL,
	CascadingIndex int NULL,
	AbandonRate float(53) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Skill SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Skill ADD CONSTRAINT
	DF_Skill_UnderstaffingFor DEFAULT ((1)) FOR UnderstaffingFor
GO
IF EXISTS(SELECT * FROM dbo.Skill)
	 EXEC('INSERT INTO dbo.Tmp_Skill (Id, Version, UpdatedBy, UpdatedOn, Name, DisplayColor, Description, DefaultResolution, SkillType, Activity, TimeZone, MidnightBreakOffset, SeriousUnderstaffing, Understaffing, Overstaffing, BusinessUnit, IsDeleted, Priority, OverstaffingFactor, UnderstaffingFor, MaxParallelTasks, CascadingIndex, AbandonRate)
		SELECT Id, Version, UpdatedBy, UpdatedOn, Name, DisplayColor, Description, DefaultResolution, SkillType, Activity, TimeZone, MidnightBreakOffset, SeriousUnderstaffing, Understaffing, Overstaffing, BusinessUnit, IsDeleted, Priority, OverstaffingFactor, UnderstaffingFor, MaxParallelTasks, CascadingIndex, 0 FROM dbo.Skill WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE dbo.MultisiteDay
	DROP CONSTRAINT FK_MultisiteDay_MultisiteSkill
GO
ALTER TABLE ReadModel.SkillCombination
	DROP CONSTRAINT FK_SkillCombination_Skill_Id
GO
ALTER TABLE dbo.SkillDay
	DROP CONSTRAINT FK_SkillDayTemplateReference_Skill
GO
ALTER TABLE dbo.SkillDay
	DROP CONSTRAINT FK_SkillDay_Skill
GO
ALTER TABLE dbo.ChildSkill
	DROP CONSTRAINT FK_ChildSkill_Skill
GO
ALTER TABLE dbo.WorkflowControlSetSkills
	DROP CONSTRAINT FK_WorkflowControlSetSkills_Skill
GO
ALTER TABLE dbo.MultisiteSkill
	DROP CONSTRAINT FK_MultisiteSkill_Skill
GO
ALTER TABLE dbo.Workload
	DROP CONSTRAINT FK_Workload_Skill
GO
ALTER TABLE dbo.SkillFilter
	DROP CONSTRAINT FK_SkillFilter_Skill
GO
ALTER TABLE dbo.PersonSkill
	DROP CONSTRAINT FK_PersonSkill_Skill
GO
ALTER TABLE dbo.SkillAreaSkillCollection
	DROP CONSTRAINT FK_SkillAreaSkillCollection_Skill
GO
ALTER TABLE dbo.MultisiteDayTemplate
	DROP CONSTRAINT FK_MultisiteDayTemplate_Skill
GO
ALTER TABLE dbo.SkillDayTemplate
	DROP CONSTRAINT FK_SkillDayTemplate_Skill
GO
ALTER TABLE dbo.SkillCollection
	DROP CONSTRAINT FK_SkillCollection_Skill
GO
ALTER TABLE dbo.OutboundCampaign
	DROP CONSTRAINT FK_OutboundCampaign_Skill
GO
DROP TABLE dbo.Skill
GO
EXECUTE sp_rename N'dbo.Tmp_Skill', N'Skill', 'OBJECT' 
GO
ALTER TABLE dbo.Skill ADD CONSTRAINT
	PK_Skill PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Skill WITH NOCHECK ADD CONSTRAINT
	FK_Skill_Person_UpdatedBy FOREIGN KEY
	(
	UpdatedBy
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Skill WITH NOCHECK ADD CONSTRAINT
	FK_Skill_SkillType FOREIGN KEY
	(
	SkillType
	) REFERENCES dbo.SkillType
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Skill WITH NOCHECK ADD CONSTRAINT
	FK_Skill_Activity FOREIGN KEY
	(
	Activity
	) REFERENCES dbo.Activity
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Skill WITH NOCHECK ADD CONSTRAINT
	FK_Skill_BusinessUnit FOREIGN KEY
	(
	BusinessUnit
	) REFERENCES dbo.BusinessUnit
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.OutboundCampaign ADD CONSTRAINT
	FK_OutboundCampaign_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.OutboundCampaign SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.SkillCollection WITH NOCHECK ADD CONSTRAINT
	FK_SkillCollection_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.SkillCollection SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.SkillDayTemplate WITH NOCHECK ADD CONSTRAINT
	FK_SkillDayTemplate_Skill FOREIGN KEY
	(
	Parent
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.SkillDayTemplate SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.MultisiteDayTemplate ADD CONSTRAINT
	FK_MultisiteDayTemplate_Skill FOREIGN KEY
	(
	Parent
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.MultisiteDayTemplate SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.SkillAreaSkillCollection ADD CONSTRAINT
	FK_SkillAreaSkillCollection_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.SkillAreaSkillCollection SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.PersonSkill ADD CONSTRAINT
	FK_PersonSkill_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PersonSkill SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.SkillFilter ADD CONSTRAINT
	FK_SkillFilter_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.SkillFilter SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Workload WITH NOCHECK ADD CONSTRAINT
	FK_Workload_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Workload SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.MultisiteSkill ADD CONSTRAINT
	FK_MultisiteSkill_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.MultisiteSkill SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.WorkflowControlSetSkills ADD CONSTRAINT
	FK_WorkflowControlSetSkills_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.WorkflowControlSetSkills SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ChildSkill ADD CONSTRAINT
	FK_ChildSkill_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ChildSkill SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.SkillDay WITH NOCHECK ADD CONSTRAINT
	FK_SkillDayTemplateReference_Skill FOREIGN KEY
	(
	TemplateReferenceSkill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.SkillDay WITH NOCHECK ADD CONSTRAINT
	FK_SkillDay_Skill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.SkillDay SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE ReadModel.SkillCombination ADD CONSTRAINT
	FK_SkillCombination_Skill_Id FOREIGN KEY
	(
	SkillId
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE ReadModel.SkillCombination SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.MultisiteDay ADD CONSTRAINT
	FK_MultisiteDay_MultisiteSkill FOREIGN KEY
	(
	Skill
	) REFERENCES dbo.Skill
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.MultisiteDay SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
