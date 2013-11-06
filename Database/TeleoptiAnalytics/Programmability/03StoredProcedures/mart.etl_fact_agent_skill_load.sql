/****** Object:  StoredProcedure [mart].[etl_fact_agent_skill_load]    Script Date: 2013-11-01 10:08:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_skill_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_skill_load]
GO
-- =============================================
-- Author:		KJ
-- Create date: 2013-10-30
-- Description:	Loads agent skills
-- Update date: 
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_agent_skill_load] 
@business_unit_code uniqueidentifier
WITH EXECUTE AS OWNER	
AS

/*Get business unit id*/
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

/*Deleting data*/
DELETE FROM mart.fact_agent_skill
WHERE business_unit_id = @business_unit_id
	OR business_unit_id = -1


INSERT  mart.fact_agent_skill
SELECT  person_id			= dp.person_id,
		skill_id			= ds.skill_id,
		has_skill			= 1,
		active				=active,
		business_unit_id	= dp.business_unit_id,
		datasource_id		= stg.datasource_id
FROM 
stage.stg_agent_skill stg
INNER JOIN mart.dim_skill ds 
	ON ds.skill_code=stg.skill_code
INNER JOIN mart.dim_person dp 
	ON stg.person_code=dp.person_code
	AND dp.valid_from_date= stg.date_from
WHERE dp.business_unit_id=@business_unit_id
ORDER BY dp.person_id,ds.skill_id,stg.date_from

GO


