IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_skill_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_skill_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Description:	Loads skill from stg_sworkload to dim_skill.
-- Update date: 2010-11-05
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2010-11-05 Created a new stage table for Skills ROBINK
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_skill_load] 
WITH EXECUTE AS OWNER
AS
--------------------------------------------------------------------------
-- Not Defined skill
SET IDENTITY_INSERT mart.dim_skill ON

INSERT INTO mart.dim_skill
	(
	skill_id, 
	skill_name, 
	time_zone_id, 
	forecast_method_name, 
	business_unit_id,
	datasource_id
	)
SELECT 
	skill_id			= -1, 
	skill_name			= 'Not Defined', 
	time_zone_id		= -1, 
	forecast_method_name= 'Not Defined', 
	business_unit_id	= -1,
	datasource_id		= -1	
WHERE NOT EXISTS (SELECT * FROM mart.dim_skill where skill_id = -1)

SET IDENTITY_INSERT mart.dim_skill OFF


---------------------------------------------------------------------------
-- update changes on skills
UPDATE mart.dim_skill
SET 
	skill_name				= s.skill_name,
	time_zone_id			= z.time_zone_id,
	forecast_method_code	= s.forecast_method_code,
	forecast_method_name	= s.forecast_method_name,
	update_date				= getdate(),
	datasource_update_date	= s.datasource_update_date,
	is_deleted				= s.is_deleted
FROM
	Stage.stg_skill s
JOIN
	mart.dim_time_zone z
ON
	s.time_zone_code = z.time_zone_code
WHERE 
	s.skill_code = mart.dim_skill.skill_code

-- Insert new skills
INSERT INTO mart.dim_skill
	(
	skill_code, 
	skill_name, 
	time_zone_id, 
	forecast_method_code, 
	forecast_method_name, 
	business_unit_id,
	datasource_id, 
	datasource_update_date,
	is_deleted
	)
SELECT DISTINCT 
	skill_code				= s.skill_code, 
	skill_name				= s.skill_name, 
	time_zone_id			= z.time_zone_id, 
	forecast_method_code	= s.forecast_method_code, 
	forecast_method_name	= s.forecast_method_name,	 
	business_unit_id		= bu.business_unit_id,
	datasource_id			= 1, 
	datasource_update_date	= max(s.datasource_update_date),
	is_deleted				= s.is_deleted
FROM
	Stage.stg_skill s
JOIN
	mart.dim_business_unit	bu
ON
	s.business_unit_code	= bu.business_unit_code
JOIN
	mart.dim_time_zone	z
ON
	s.time_zone_code = z.time_zone_code
WHERE 
	NOT EXISTS (SELECT skill_code FROM mart.dim_skill d WHERE d.skill_code = s.skill_code and d.datasource_id=1)
GROUP BY 
	skill_code, 
	skill_name, 
	time_zone_id, 
	forecast_method_code, 
	forecast_method_name,
	bu.business_unit_id,
	s.is_deleted
