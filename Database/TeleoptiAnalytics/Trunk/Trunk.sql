--Improve delete in: mart.etl_fact_schedule_load
--Adding on 358 and default => IF NOT EXISTS
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_Shift_startdate_id')
CREATE NONCLUSTERED INDEX [IX_fact_schedule_Shift_startdate_id]
ON [mart].[fact_schedule] ([shift_startdate_id])
INCLUDE ([schedule_date_id],[person_id],[interval_id],[activity_starttime],[scenario_id],[business_unit_id])
GO

