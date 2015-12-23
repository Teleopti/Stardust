--Drop tables related to staffing view model as it hasn't hit production in a year
DROP TABLE [ReadModel].[ActivitySkillCombination]
DROP TABLE [ReadModel].[PeriodSkillEfficiencies]
DROP TABLE [ReadModel].[ScheduledResources]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[AddResources]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [ReadModel].[AddResources]
END

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[AddSkillEfficiency]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [ReadModel].[AddSkillEfficiency]
END

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[RemoveResources]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [ReadModel].[RemoveResources]
END

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[RemoveSkillEfficiency]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [ReadModel].[RemoveSkillEfficiency]
END

