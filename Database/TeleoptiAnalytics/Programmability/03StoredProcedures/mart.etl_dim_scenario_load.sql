IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_scenario_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_scenario_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-31
-- Description:	Loads scenario from stg_scenario to dim_scenario
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 20090401 Load only default scenario KJ
-- 20010726 Adding is_deleted DJ
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_scenario_load] 
WITH EXECUTE AS OWNER
AS
--------------------------------------------------------------------------
-- Not Defined Activity
SET IDENTITY_INSERT mart.dim_scenario ON

INSERT INTO mart.dim_scenario
	(
	scenario_id, 
	scenario_name, 
	business_unit_id,
	business_unit_name, 
	datasource_id,
	is_deleted
	)
SELECT 
	scenario_id			= -1, 
	scenario_name		= 'Not Defined', 
	business_unit_id	= -1,
	business_unit_name	= 'Not Defined', 
	datasource_id		= -1,
	is_deleted			=  0
WHERE NOT EXISTS (SELECT * FROM mart.dim_scenario where scenario_id = -1)

SET IDENTITY_INSERT mart.dim_scenario OFF

---------------------------------------------------------------------------
-- update changes on scenario
UPDATE mart.dim_scenario
SET 
	scenario_name		= s.scenario_name, 
	default_scenario	= s.default_scenario, 
	business_unit_id	= bu.business_unit_id,
	business_unit_code	= S.business_unit_code, 
	business_unit_name	= S.business_unit_name, 
	update_date			= getdate(), 
	datasource_update_date =s.datasource_update_date,
	is_deleted			= s.is_deleted
FROM
	Stage.stg_scenario s
JOIN
	mart.dim_business_unit bu
ON
	s.business_unit_code	= bu.business_unit_code
WHERE 
	s.scenario_code = mart.dim_scenario.scenario_code

-- Insert new scenario
INSERT INTO mart.dim_scenario
	(
	scenario_code, 
	scenario_name, 
	default_scenario, 
	business_unit_id,
	business_unit_code, 
	business_unit_name, 
	datasource_id, 
	datasource_update_date,
	is_deleted
	)
SELECT 
	scenario_code		= s.scenario_code, 
	scenario_name		= s.scenario_name, 
	default_scenario	= s.default_scenario, 
	business_unit_id	= bu.business_unit_id,
	business_unit_code	= s.business_unit_code, 
	business_unit_name	= s.business_unit_name, 
	datasource_id		= 1, 
	datasource_update_date = s.datasource_update_date,
	is_deleted			= s.is_deleted	
FROM
	Stage.stg_scenario s
JOIN
	mart.dim_business_unit bu
ON
	s.business_unit_code	= bu.business_unit_code
WHERE 
	NOT EXISTS (SELECT scenario_id FROM mart.dim_scenario d WHERE d.scenario_code = s.scenario_code AND datasource_id=1)
	AND s.is_deleted= 0


-----------------------------------------------------------------------------
-- JN: This should not be able to happen I guess. I´ll comment this away
-- insert from stg_schedule
--INSERT INTO mart.dim_scenario
--	(
--	scenario_code, 
--	datasource_id,
--	datasource_update_date
--	)
--SELECT 
--	scenario_code			= s.scenario_code,
--	datasource_id			= 1,
--	datasource_update_date	= s.datasource_update_date
--FROM
--	(SELECT scenario_code,datasource_update_date=max(datasource_update_date) FROM Stage.stg_schedule GROUP BY scenario_code) s
--WHERE 
--	NOT EXISTS (SELECT scenario_id FROM mart.dim_scenario d WHERE d.scenario_code = s.scenario_code and d.datasource_id=1)

