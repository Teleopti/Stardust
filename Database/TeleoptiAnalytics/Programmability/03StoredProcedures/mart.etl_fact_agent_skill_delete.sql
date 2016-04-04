IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_skill_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_skill_delete]
GO
-- =============================================
-- Description:	Delete agent skills for person id
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_agent_skill_delete] 
@person_id int
AS

DELETE FROM mart.fact_agent_skill
WHERE person_id = @person_id

GO



