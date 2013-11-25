IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_forecast_workload_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_forecast_workload_intraday_load]
GO


-- =============================================
-- Author:		David
-- Create date: 2013-11-21
-- Description:	Loads forecasts per workload for ETL.Intraday
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_forecast_workload_intraday_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier
	
AS
SET NOCOUNT ON
CREATE TABLE #delete(from_date_id int, to_date_id int, workload_id int, scenario_id int)

INSERT INTO #delete
SELECT
	min(d.date_id),
	max(d.date_id),
	w.workload_id,
	s.scenario_id
FROM stage.stg_forecast_workload stg
INNER JOIN mart.dim_date d
	ON d.date_date = stg.date
INNER JOIN mart.dim_workload w
	ON w.workload_code = stg.workload_code
INNER JOIN mart.dim_scenario s
	ON s.scenario_code = stg.scenario_code
GROUP BY w.workload_id,s.scenario_id

SET NOCOUNT OFF
DELETE f
FROM mart.fact_forecast_workload f
INNER JOIN #delete d
ON f.date_id between d.from_date_id and d.to_date_id
AND f.workload_id = d.workload_id
AND f.scenario_id = d.scenario_id

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
	(SELECT * FROM Stage.stg_forecast_workload) f
INNER JOIN
	mart.dim_date		fd
ON
	f.date	= fd.date_date
INNER JOIN
	mart.dim_workload	dw
ON
	f.workload_code = dw.workload_code
INNER JOIN
	mart.dim_scenario	ds
ON
	f.scenario_code = ds.scenario_code

GO

