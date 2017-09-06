ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD [forecasted_tasks] decimal(28,4)
ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD [estimated_tasks_answered_within_sl] decimal(28,4)


ALTER TABLE [stage].[stg_schedule_forecast_skill] ADD [forecasted_tasks] decimal(28,4)
ALTER TABLE [stage].[stg_schedule_forecast_skill] ADD [estimated_tasks_answered_within_sl] decimal(28,4)