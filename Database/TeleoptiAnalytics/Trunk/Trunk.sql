--Improve delete in: mart.etl_fact_schedule_load
--Adding on 358 and default => IF NOT EXISTS
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_Shift_startdate_id')
CREATE NONCLUSTERED INDEX [IX_fact_schedule_Shift_startdate_id]
ON [mart].[fact_schedule] ([shift_startdate_id])
INCLUDE ([schedule_date_id],[person_id],[interval_id],[activity_starttime],[scenario_id],[business_unit_id])
GO

----------------  
--Name: AndersF
--Date: 2012-03-29
--Desc: #18790 - Performance: etl fact report permissions is performing bad
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[permission_report]') AND name = N'IX_Permission_Report_BusinessUnitId')
CREATE NONCLUSTERED INDEX [IX_Permission_Report_BusinessUnitId]
ON [mart].[permission_report] ([business_unit_id])
INCLUDE ([person_code],[team_id],[my_own],[ReportId])
GO
