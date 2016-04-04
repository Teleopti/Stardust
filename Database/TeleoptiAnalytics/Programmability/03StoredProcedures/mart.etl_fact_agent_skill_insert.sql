IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_skill_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_skill_insert]
GO
-- =============================================
-- Description:	Insert agent skills for person id
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_agent_skill_insert] 
@person_id int,
@skill_id int,
@active bit,
@business_unit int
AS

INSERT INTO mart.fact_agent_skill
	(person_id,
		skill_id,
		has_skill,
		active,
		business_unit_id,
		datasource_id)
	VALUES
	(@person_id,
	@skill_id,
	1,
	@active,
	@business_unit,
	1)

GO
