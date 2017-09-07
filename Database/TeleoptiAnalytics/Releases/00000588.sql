ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD [forecasted_tasks_incl_shrinkage] decimal(28,4)
ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD [estimated_tasks_answered_within_sl_incl_shrinkage] decimal(28,4)


ALTER TABLE [stage].[stg_schedule_forecast_skill] ADD [forecasted_tasks_incl_shrinkage] decimal(28,4)
ALTER TABLE [stage].[stg_schedule_forecast_skill] ADD [estimated_tasks_answered_within_sl_incl_shrinkage] decimal(28,4)