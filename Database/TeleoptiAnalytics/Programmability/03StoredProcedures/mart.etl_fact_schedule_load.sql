/****** Object:  StoredProcedure [mart].[etl_fact_schedule_load]    Script Date: 12/03/2008 15:37:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_load]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-16
-- Description:	Write schedule activities from staging table 'stg_schedule'
--				to data mart table 'fact_schedule'.
-- Updates:		2008-09-02	Changes on deletestatements using start_starttime instead of id:s KJ
--				2008-09-30	Checking delete/load process:
--							For Schedules we make the delete based on the Shift_StartTime
--							The stage table must only contain "Full UTC-day" loaded from ETL-tool.
--				2008-10-07	Revert back and use max/min shift_starttime from stage
--				2008-12-03	New columns scheduled_time_absence_m,scheduled_time_activity_m,scheduled_contract_time_m etc.
--				2009-02-09 Stage moved to mart db, removed view KJ
--				2009-02-11 New mart schema KJ
--				2010-11-09	#12320 fix personperiods vs. Schedule_date_id JN+DJ
--				2011-02-25	Added overtime id Mattias E
--				2013-09-20   Removed check on min/maxdate in stage
-- Interface:	smalldatetime, with only datepart! No time allowed
-- =============================================
--exec mart.etl_fact_schedule_load '2009-02-02','2009-02-03'
--exec mart.etl_fact_schedule_load @start_date='2010-10-30 00:00:00',@end_date='2010-11-03 00:00:00'
--set statistics io on
--exec mart.etl_fact_schedule_load @start_date='2014-02-03 00:00:00',@end_date='2014-02-06 00:00:00',@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE PROCEDURE [mart].[etl_fact_schedule_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier
	
AS
----------------------------------------------------------------------------------
DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

--EXEC [mart].[stage_schedule_remove_overlapping_shifts]

--There must not be any timevalue on the interface values, since that will mess things up around midnight!
--Consider: DECLARE @end_date smalldatetime;SET @end_date = '2006-01-31 23:59:30';SELECT @end_date
SET @start_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), @start_date, 112)) --ISO yyyymmdd
SET @end_date	= CONVERT(smalldatetime,CONVERT(nvarchar(30), @end_date, 112))

--Not currently needed since we now delete on shift_starttime (instead of _id)
SET @start_date_id =	(SELECT date_id FROM mart.dim_date WHERE @start_date = date_date)
SET @end_date_id	 =	(SELECT date_id FROM mart.dim_date WHERE @end_date = date_date)

--Debug
--SELECT @start_date, @end_date
--SELECT count(*) FROM mart.fact_schedule  WHERE shift_starttime between @start_date AND @end_date
-----------------------------------------------------------------------------------
-- Delete rows based on shift_starttime
--DELETE FROM mart.fact_schedule  WHERE shift_starttime between @start_date AND @end_date
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

DELETE FROM fs
FROM mart.fact_schedule fs
INNER JOIN mart.dim_scenario ds
	ON ds.scenario_id = fs.scenario_id
	AND ds.business_unit_id = @business_unit_id
WHERE shift_startdate_local_id between @start_date_id AND @end_date_id  --Will be "Hash Match JOIN" => 6940 reads
--using "Nested Loop JOIN" => 2300 reads
--AND EXISTS (SELECT * FROM mart.dim_scenario ds WHERE fs.business_unit_id = ds.business_unit_id and ds.business_unit_id = @business_unit_id)

-- Insert rows
INSERT INTO mart.fact_schedule
	(
	shift_startdate_local_id,
	schedule_date_id, 
	person_id, 
	interval_id, 
	activity_starttime, 
	scenario_id, 
	activity_id, 
	absence_id, 
	activity_startdate_id, 
	activity_enddate_id, 
	activity_endtime, 
	shift_startdate_id, 
	shift_starttime, 
	shift_enddate_id, 
	shift_endtime, 
	shift_startinterval_id, 
	shift_endinterval_id, 
	shift_category_id, 
	shift_length_id, 
	scheduled_time_m, 
	scheduled_time_absence_m,
	scheduled_time_activity_m,
	scheduled_contract_time_m,
	scheduled_contract_time_activity_m,
	scheduled_contract_time_absence_m,
	scheduled_work_time_m, 
	scheduled_work_time_activity_m,
	scheduled_work_time_absence_m,
	scheduled_over_time_m, 
	scheduled_ready_time_m,
	scheduled_paid_time_m,
	scheduled_paid_time_activity_m,
	scheduled_paid_time_absence_m,
	last_publish, 
	business_unit_id,
	datasource_id, 
	datasource_update_date,
	overtime_id
	)

SELECT * FROM Stage.v_stg_Schedule_load
WHERE shift_startdate_local_id between @start_date_id AND @end_date_id

GO
