IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_forecast_skill_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_forecast_skill_load]
GO

--exec etl_fact_schedule_forecast_skill_load '2006-01-01','2006-01-31'

-- =============================================
-- Author:		KJ
-- Create date: 2008-09-10
-- Description:	Write schedule and foreacast resources per skill from staging table 'stg_schedule_forecast_skill'
--				to data mart table 'fact_schedule_forecast_skill'.
--				Delete and Load based on Full UTC days. Must only be full days loaded into stage by ETL-tool
-- Change date:	2008-09-30
-- By:			DJ
--				Checking delete/load process:
--				For Schedule the stage table will only contain "full UTC-day". Revert back to full delete in fact table.
--				Found possible logical error when using min_date, max_date. Removed for now. Always load/delete full period
-- 2009-02-09	Stage moved to mart db, removed view KJ
-- 2009-02-11	New mart schema KJ
-- 2013-03-01	Removed ABS() calculation from relative difference calculation KJ
-- 2013-09-20   Removed check on min/maxdate in stage
-- =============================================

CREATE PROCEDURE [mart].[etl_fact_schedule_forecast_skill_load] 
@start_date smalldatetime, --Always as start of day UTC: '2006-01-06' (no time!)
@end_date smalldatetime,	  --Always as end of day UTC: '2006-01-31' (no time!)
@business_unit_code uniqueidentifier

AS
--Delete and Insert based on intervall

--DECLARES
DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,8),@start_date )))
SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,8),@end_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

--Reset time frame to match the content in stage
--Need to find out how this affect mart-data
--Performace issues? => Will lead to that we don't delete stuff that accutally should have been deleted

--There must not be any timevalue since that will mess things up around midnight!
--Consider: DECLARE @end_date smalldatetime;SET @end_date = '2006-01-31 23:59:59';SELECT @end_date 
--SET @start_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), @start_date, 112)) --ISO yyyymmdd
--SET @end_date	= CONVERT(smalldatetime,CONVERT(nvarchar(30), @end_date, 112))

--Debug
SELECT date_date FROM mart.dim_date WHERE date_id IN (@end_date_id,@start_date_id)
SELECT COUNT(*) as rows_to_delete FROM mart.fact_schedule_forecast_skill WHERE date_id between @start_date_id AND @end_date_id
-----------------------------------------------------------------------------------
-- Delete rows matching dates
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

DELETE FROM mart.fact_schedule_forecast_skill
WHERE date_id between @start_date_id AND @end_date_id
AND business_unit_id = @business_unit_id


/*
DELETE FROM mart.fact_schedule_forecast_skill
WHERE date_id between @start_date_id AND @end_date_id
	AND business_unit_id = 
	(
		SELECT DISTINCT
			bu.business_unit_id
		FROM 
			Stage.stg_schedule_forecast_skill sfs
		INNER JOIN
			mart.dim_business_unit bu
		ON
			sfs.business_unit_code = bu.business_unit_code
	)
*/
-----------------------------------------------------------------------------------
-- Insert rows
INSERT INTO mart.fact_schedule_forecast_skill
	(
	date_id, 
	interval_id, 
	skill_id, 
	scenario_id, 
	forecasted_resources_m, 
	forecasted_resources, 
	forecasted_resources_incl_shrinkage_m, 
	forecasted_resources_incl_shrinkage, 
	scheduled_resources_m, 
	scheduled_resources, 
	scheduled_resources_incl_shrinkage_m, 
	scheduled_resources_incl_shrinkage, 
	intraday_deviation_m, 
	business_unit_id, 
	datasource_id, 
	update_date,
	forecasted_tasks,
	estimated_tasks_answered_within_sl
	)
SELECT
	date_id									= dsd.date_id, 
	interval_id								= di.interval_id, 
	skill_id								= dsk.skill_id,
	scenario_id								= ds.scenario_id, 
	forecasted_resources_m					= f.forecasted_resources_m, 
	forecasted_resources					= f.forecasted_resources, 
	forecasted_resources_incl_shrinkage_m	= f.forecasted_resources_incl_shrinkage_m, 
	forecasted_resources_incl_shrinkage		= f.forecasted_resources_incl_shrinkage, 
	scheduled_resources_m					= f.scheduled_resources_m, 
	scheduled_resources						= f.scheduled_resources, 
	scheduled_resources_m					= f.scheduled_resources_incl_shrinkage_m, 
	scheduled_resources						= f.scheduled_resources_incl_shrinkage, 
	intraday_deviation_m					= isnull(f.scheduled_resources_m,0) - isnull(f.forecasted_resources_m,0), --2013-03-01 Removed ABS() calculation KJ
	business_unit_id						= dsk.business_unit_id,
	datasource_id							= f.datasource_id, 
	update_date								= f.update_date,
	forecasted_tasks						= f.forecasted_tasks,
	estimated_tasks_answered_within_sl		= f.estimated_tasks_answered_within_sl
FROM (SELECT * FROM Stage.stg_schedule_forecast_skill WHERE date between @start_date and @end_date)f -- ADDED BY JONAS 2008-10-15
INNER JOIN
	mart.dim_skill		dsk
ON
	f.skill_code	=	dsk.skill_code
LEFT JOIN
	mart.dim_date		dsd
ON
	f.date	= dsd.date_date
LEFT JOIN
	mart.dim_interval	di
ON
	f.interval_id = di.interval_id
LEFT JOIN
	mart.dim_interval	dist
ON
	f.interval_id = dist.interval_id
INNER JOIN
	mart.dim_scenario	ds
ON
	f.scenario_code = ds.scenario_code

--David was here: Need comment
UPDATE mart.fact_schedule_forecast_skill
SET relative_difference=intraday_deviation_m/f.forecasted_resources_m
FROM mart.fact_schedule_forecast_skill f
WHERE f.forecasted_resources_m>0
AND date_id between @start_date_id AND @end_date_id
