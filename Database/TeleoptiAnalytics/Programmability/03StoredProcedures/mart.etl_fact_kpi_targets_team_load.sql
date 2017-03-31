IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_kpi_targets_team_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_kpi_targets_team_load]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-18
-- Description:	Write KPI targets for teams from staging table 'stg_kpi_targets_team'
--				to data mart table 'fact_kpi_targets_team'.
--
-- Change:		2008-09-30 DJ Just checking the delete/load process (UTC-date). Nothing to change.
--				2009-02-09 Stage moved to mart db, removed view KJ
--				2009-02-11 New mart schema KJ
--				2009-04-01 Insert 0 for teams and teams not selected (otherwise KPI in cube not working) KJ
--				2009-04-28 Verfication that team exists in dim person otherwise error in cube KJ
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_kpi_targets_team_load]
@business_unit_code uniqueidentifier	
AS

-----------------------------------------------------------------------------------
-- Delete rows
/*Get business unit id*/
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

DELETE FROM mart.fact_kpi_targets_team
WHERE business_unit_id = @business_unit_id

/*
DELETE FROM mart.fact_kpi_targets_team
WHERE business_unit_id = 
	(
		SELECT DISTINCT	business_unit_id
		FROM 
			Stage.stg_kpi_targets_team ktm
		INNER JOIN
			mart.dim_business_unit bu
		ON
			ktm.business_unit_code = bu.business_unit_code
	)
*/
-----------------------------------------------------------------------------------
-- Insert rows

INSERT INTO mart.fact_kpi_targets_team
	(
	kpi_id, 
	team_id, 
	business_unit_id,
	target_value, 
	min_value, 
	max_value, 
	between_color, 
	lower_than_min_color, 
	higher_than_max_color, 
	datasource_id, 
	datasource_update_date
	)
SELECT
	kpi_id					= dk.kpi_id, 
	team_id					= dt.team_id, 
	business_unit_id		= db.business_unit_id,
	target_value			= stg.target_value,
	min_value				= stg.min_value, 
	max_value				= stg.max_value, 
	between_color			= stg.between_color, 
	lower_than_min_color	= stg.lower_than_min_color, 
	higher_than_max_color	= stg.higher_than_max_color, 
	datasource_id			= stg.datasource_id, 
	datasource_update_date	= stg.datasource_update_date
FROM 
	Stage.stg_kpi_targets_team stg
INNER JOIN 
	mart.dim_kpi dk	
ON	
	dk.kpi_code	= stg.kpi_code
INNER JOIN	
	mart.dim_team dt	
ON	
	dt.team_code = stg.team_code
INNER JOIN 
	mart.dim_business_unit db
ON 
	db.business_unit_code=stg.business_unit_code
WHERE dt.team_id IN (SELECT DISTINCT team_id FROM mart.dim_person) --20090428 Must exist in dim_person
ORDER BY dk.kpi_id,dt.team_id

--fill with 0 on teams with no selected targets
INSERT INTO mart.fact_kpi_targets_team
	(
	kpi_id, 
	team_id, 
	business_unit_id,
	target_value, 
	min_value, 
	max_value, 
	between_color, 
	lower_than_min_color, 
	higher_than_max_color, 
	datasource_id, 
	datasource_update_date
	)
SELECT
	kpi_id					= dk.kpi_id, 
	team_id					= dt.team_id, 
	business_unit_id		= db.business_unit_id,
	target_value			= 0,
	min_value				= 0, 
	max_value				= 0, 
	between_color			= -1, 
	lower_than_min_color	= -1, 
	higher_than_max_color	= -1, 
	datasource_id			= dt.datasource_id, 
	datasource_update_date	= NULL
FROM 	mart.dim_kpi dk	
CROSS JOIN mart.dim_team dt
INNER JOIN 
	mart.dim_business_unit db
ON 
	db.business_unit_id=dt.business_unit_id
WHERE NOT EXISTS
(
	SELECT * FROM 
	mart.fact_kpi_targets_team ktt
	WHERE dk.kpi_id=ktt.kpi_id
	AND dt.team_id=ktt.team_id

)
AND dt.team_id>0
AND dt.team_id IN (SELECT DISTINCT team_id FROM mart.dim_person)--20090428 Must exist in dim_person
GO

