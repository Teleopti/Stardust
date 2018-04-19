-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-19
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

DROP INDEX CIX_WorkflowControlSetAllowedAbsences_WorkflowControlSet ON dbo.WorkflowControlSetAllowedAbsences
GO
ALTER TABLE dbo.WorkflowControlSetAllowedAbsences ADD CONSTRAINT
	PK_WorkflowControlSetAllowedAbsences PRIMARY KEY CLUSTERED 
	(
		WorkflowControlSet,
		Absence
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.WorkflowControlSetAllowedAbsences SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
DROP INDEX CIX_WorkflowControlSetAllowedDayOffs_WorkflowControlSet ON dbo.WorkflowControlSetAllowedDayOffs
GO
ALTER TABLE dbo.WorkflowControlSetAllowedDayOffs ADD CONSTRAINT
	PK_WorkflowControlSetAllowedDayOffs PRIMARY KEY CLUSTERED 
	(
		WorkflowControlSet,
		DayOffTemplate
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.WorkflowControlSetAllowedDayOffs SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
DROP INDEX CIX_WorkflowControlSetAllowedShiftCategories_WorkflowControlSet ON dbo.WorkflowControlSetAllowedShiftCategories
GO
ALTER TABLE dbo.WorkflowControlSetAllowedShiftCategories ADD CONSTRAINT
	PK_WorkflowControlSetAllowedShiftCategories PRIMARY KEY CLUSTERED 
	(
		WorkflowControlSet,
		ShiftCategory
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.WorkflowControlSetAllowedShiftCategories SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
DROP INDEX CIX_WorkflowControlSetSkills_WorkflowControlSet ON dbo.WorkflowControlSetSkills
GO
ALTER TABLE dbo.WorkflowControlSetSkills ADD CONSTRAINT
	PK_WorkflowControlSetSkills PRIMARY KEY CLUSTERED 
	(
		WorkflowControlSet,
		Skill
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.WorkflowControlSetSkills SET (LOCK_ESCALATION = TABLE)
GO
