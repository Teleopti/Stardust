-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-19
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

DROP INDEX CIX_RuleSetRuleSetBag_RuleSet ON dbo.RuleSetRuleSetBag
GO
ALTER TABLE dbo.RuleSetRuleSetBag ADD CONSTRAINT
	PK_RuleSetRuleSetBag PRIMARY KEY CLUSTERED 
	(
		RuleSet,
		RuleSetBag
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.RuleSetRuleSetBag SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
ALTER TABLE dbo.SchedulePeriodShiftCategoryLimitation
	DROP CONSTRAINT UQ_SchedulePeriodShiftCategoryLimitation
GO
ALTER TABLE dbo.SchedulePeriodShiftCategoryLimitation ADD CONSTRAINT
	PK_SchedulePeriodShiftCategoryLimitation PRIMARY KEY CLUSTERED 
	(
		SchedulePeriod,
		ShiftCategory
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.SchedulePeriodShiftCategoryLimitation SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
ALTER TABLE dbo.SkillAreaSkillCollection
	DROP CONSTRAINT UQ_SkillAreaSkillCollection
GO
ALTER TABLE dbo.SkillAreaSkillCollection ADD CONSTRAINT
	PK_SkillAreaSkillCollection PRIMARY KEY CLUSTERED 
	(
		SkillArea,
		Skill
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.SkillAreaSkillCollection SET (LOCK_ESCALATION = TABLE)
GO
