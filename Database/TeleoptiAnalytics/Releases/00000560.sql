-- Add index to improve SP for adherence badge calculation (Refer to bug #40155: ETL Nightly - timeout on raptor_adherence_per_agent_by_date)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_shift_startdate_id_startinterval_id')
CREATE NONCLUSTERED INDEX [IX_fact_schedule_shift_startdate_id_startinterval_id] ON [mart].[fact_schedule] (
	[shift_startdate_id]
	,[shift_startinterval_id]
	) INCLUDE (
	[shift_startdate_local_id]
	,[schedule_date_id]
	,[person_id]
	,[interval_id]
	,[scenario_id]
	,[activity_id]
	,[absence_id]
	,[scheduled_time_m]
	,[scheduled_ready_time_m]
	,[business_unit_id]
	)
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_deviation]') AND name = N'IX_fact_schedule_deviation_date_id')
CREATE NONCLUSTERED INDEX [IX_fact_schedule_deviation_date_id] ON [mart].[fact_schedule_deviation] ([date_id]) INCLUDE (
	[shift_startdate_local_id]
	,[interval_id]
	,[person_id]
	,[contract_time_s]
	,[deviation_schedule_s]
	,[deviation_schedule_ready_s]
	,[deviation_contract_s]
	,[shift_startdate_id]
	,[shift_startinterval_id]
	)
GO
