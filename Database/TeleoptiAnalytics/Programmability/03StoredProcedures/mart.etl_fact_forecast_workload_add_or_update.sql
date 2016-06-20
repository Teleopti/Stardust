IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_forecast_workload_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_forecast_workload_add_or_update]
GO
-- =============================================
-- Description:	add or update fact_forecast_workload
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_forecast_workload_add_or_update] 
		@date_id int
		,@interval_id smallint
		,@start_time smalldatetime
		,@workload_id int
		,@scenario_id int
		,@end_time smalldatetime
		,@skill_id int
		,@forecasted_calls decimal(28,4)
		,@forecasted_emails decimal(28,4)
		,@forecasted_backoffice_tasks decimal(28,4)
		,@forecasted_campaign_calls decimal(28,4)
		,@forecasted_calls_excl_campaign decimal(28,4)
		,@forecasted_talk_time_s decimal(28,4)
		,@forecasted_campaign_talk_time_s decimal(28,4)
		,@forecasted_talk_time_excl_campaign_s decimal(28,4)
		,@forecasted_after_call_work_s decimal(28,4)
		,@forecasted_campaign_after_call_work_s decimal(28,4)
		,@forecasted_after_call_work_excl_campaign_s decimal(28,4)
		,@forecasted_handling_time_s decimal(28,4)
		,@forecasted_campaign_handling_time_s decimal(28,4)
		,@forecasted_handling_time_excl_campaign_s decimal(28,4)
		,@period_length_min decimal(28,4)
		,@business_unit_id int
		,@datasource_update_date smalldatetime
AS
declare @rows int

INSERT INTO mart.fact_forecast_workload
SELECT 
	@date_id
	,@interval_id
	,@start_time
	,@workload_id
	,@scenario_id
	,@end_time
	,@skill_id
	,@forecasted_calls
	,@forecasted_emails
	,@forecasted_backoffice_tasks
	,@forecasted_campaign_calls
	,@forecasted_calls_excl_campaign
	,@forecasted_talk_time_s
	,@forecasted_campaign_talk_time_s
	,@forecasted_talk_time_excl_campaign_s
	,@forecasted_after_call_work_s
	,@forecasted_campaign_after_call_work_s
	,@forecasted_after_call_work_excl_campaign_s
	,@forecasted_handling_time_s
	,@forecasted_campaign_handling_time_s
	,@forecasted_handling_time_excl_campaign_s
	,@period_length_min
	,@business_unit_id
	,1
	,GETUTCDATE()
	,GETUTCDATE()
	,@datasource_update_date
WHERE NOT EXISTS(SELECT 1 FROM mart.fact_forecast_workload 
	WHERE date_id=@date_id
		AND interval_id=@interval_id
		AND start_time=@start_time
		AND workload_id=@workload_id
		AND scenario_id=@scenario_id)

SET @rows = (SELECT @@ROWCOUNT)
IF @rows = 0
BEGIN
	UPDATE mart.fact_forecast_workload
	SET
		end_time = @end_time
		,skill_id = @skill_id
		,forecasted_calls = @forecasted_calls
		,forecasted_emails = @forecasted_emails
		,forecasted_backoffice_tasks = @forecasted_backoffice_tasks
		,forecasted_campaign_calls = @forecasted_campaign_calls
		,forecasted_calls_excl_campaign = @forecasted_calls_excl_campaign
		,forecasted_talk_time_s = @forecasted_talk_time_s
		,forecasted_campaign_talk_time_s = @forecasted_campaign_talk_time_s
		,forecasted_talk_time_excl_campaign_s = @forecasted_talk_time_excl_campaign_s
		,forecasted_after_call_work_s = @forecasted_after_call_work_s
		,forecasted_campaign_after_call_work_s = @forecasted_campaign_after_call_work_s
		,forecasted_after_call_work_excl_campaign_s = @forecasted_after_call_work_excl_campaign_s
		,forecasted_handling_time_s = @forecasted_handling_time_s
		,forecasted_campaign_handling_time_s = @forecasted_campaign_handling_time_s
		,forecasted_handling_time_excl_campaign_s = @forecasted_handling_time_excl_campaign_s
		,period_length_min = @period_length_min
		,datasource_update_date = @datasource_update_date
	WHERE
		date_id=@date_id
		AND interval_id=@interval_id
		AND start_time=@start_time
		AND workload_id=@workload_id
		AND scenario_id=@scenario_id
END
GO



		
