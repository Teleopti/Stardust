--#37543, #38300 make sure no duplicates from event based updates
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PK_SkillCode]') AND type in (N'UQ'))
BEGIN
	ALTER TABLE [mart].[dim_skill]
	ADD CONSTRAINT [PK_SkillCode] UNIQUE ([skill_code])
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PK_ScenarioCode]') AND type in (N'UQ'))
BEGIN
	ALTER TABLE [mart].[dim_scenario]
	ADD CONSTRAINT [PK_ScenarioCode] UNIQUE ([scenario_code])
END
