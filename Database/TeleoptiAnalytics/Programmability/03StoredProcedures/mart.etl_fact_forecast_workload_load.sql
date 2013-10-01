IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_forecast_workload_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_forecast_workload_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-03-13
-- Description:	Loads forecasts per workload.
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2013-09-20  Removed check on min/maxdate in stage
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_forecast_workload_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier
	
AS
----------------------------------------------------------------------------------
DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,8),@start_date )))
SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,8),@end_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

--SELECT @start_date

-----------------------------------------------------------------------------------
-- Delete rows
-- only the scenarios we have fetched
CREATE TABLE #scenarios(id int)
INSERT INTO #scenarios SELECT DISTINCT scenario_id FROM mart.dim_scenario ds 
INNER JOIN
	Stage.stg_forecast_workload stg
ON
	stg.scenario_code = ds.scenario_code

/*Get business unit id*/
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

DELETE FROM mart.fact_forecast_workload
WHERE date_id between @start_date_id AND @end_date_id
AND business_unit_id = @business_unit_id
AND scenario_id IN (SELECT id FROM #scenarios)

/*
DELETE FROM mart.fact_forecast_workload
WHERE date_id between @start_date_id AND @end_date_id
	AND business_unit_id = 
	(
		SELECT DISTINCT
			bu.business_unit_id
		FROM 
			Stage.stg_forecast_workload fw
		INNER JOIN
			mart.dim_business_unit bu
		ON
			fw.business_unit_code = bu.business_unit_code
	)
*/
-----------------------------------------------------------------------------------
-- Insert rows

INSERT INTO mart.fact_forecast_workload 
	(
	date_id, 
	interval_id, 
	start_time, 
	workload_id, 
	scenario_id, 
	end_time, 
	skill_id, 
	forecasted_calls, 
	forecasted_emails, 
	forecasted_backoffice_tasks, 
	forecasted_campaign_calls, 
	forecasted_calls_excl_campaign, 
	forecasted_talk_time_s, 
	forecasted_campaign_talk_time_s, 
	forecasted_talk_time_excl_campaign_s, 
	forecasted_after_call_work_s, 
	forecasted_campaign_after_call_work_s, 
	forecasted_after_call_work_excl_campaign_s, 
	forecasted_handling_time_s, 
	forecasted_campaign_handling_time_s, 
	forecasted_handling_time_excl_campaign_s, 
	period_length_min,
	business_unit_id,
	datasource_id,
	datasource_update_date
	)
SELECT
	date_id									= fd.date_id, 
	interval_id								= f.interval_id, 
	start_time								= f.start_time, 
	workload_id								= dw.workload_id, 
	scenario_id								= ds.scenario_id, 
	end_time								= f.end_time, 
	skill_id								= dw.skill_id, 
	forecasted_calls						= f.forecasted_calls, 
	forecasted_emails						= f.forecasted_emails,
	forecasted_backoffice_tasks				= f.forecasted_backoffice_tasks, 
	forecasted_campaign_calls				= f.forecasted_campaign_calls, 
	forecasted_calls_excl_campaign			= f.forecasted_calls_excl_campaign, 
	forecasted_talk_time_s					= f.forecasted_talk_time_s, 
	forecasted_campaign_talk_time_s			= f.forecasted_campaign_talk_time_s, 
	forecasted_talk_time_excl_campaign_s	= f.forecasted_talk_time_excl_campaign_s, 
	forecasted_after_call_work_s			= f.forecasted_after_call_work_s, 
	forecasted_campaign_after_call_work_s	= f.forecasted_campaign_after_call_work_s,
	forecasted_after_call_work_excl_campaign_s	= f.forecasted_after_call_work_excl_campaign_s, 
	forecasted_handling_time_s				= f.forecasted_handling_time_s, 
	forecasted_campaign_handling_time_s		= f.forecasted_campaign_handling_time_s, 
	forecasted_handling_time_excl_campaign_s= f.forecasted_handling_time_excl_campaign_s, 
	period_length_min						= f.period_length_min,
	business_unit_id						= dw.business_unit_id,
	datasource_id							= 1, 
	datasource_update_date					= f.datasource_update_date
FROM
	(SELECT * FROM Stage.stg_forecast_workload WHERE date between @start_date and @end_date) f
LEFT JOIN
	mart.dim_date		fd
ON
	f.date	= fd.date_date
LEFT JOIN
	mart.dim_workload	dw
ON
	f.workload_code = dw.workload_code
LEFT JOIN
	mart.dim_scenario	ds
ON
	f.scenario_code = ds.scenario_code

GO

