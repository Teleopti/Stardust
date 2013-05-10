IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_intraday_load]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2013-04-11
-- Description:	Write schedule activities from staging table 'stg_schedule'
--				to data mart table 'fact_schedule'.
-- Updates:
------------------------------------------------
--	Date	Who		Why

-- =============================================
--exec mart.etl_fact_schedule_intraday_load '2009-02-02','2009-02-03'
--exec mart.etl_fact_schedule_intraday_load 'CEC854E6-B4A8-4BD5-BB12-26E8A3D9E0BA''
exec [mart].[etl_fact_schedule_intraday_load] @business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE PROCEDURE [mart].[etl_fact_schedule_intraday_load]
@business_unit_code uniqueidentifier
AS

--if no @scenario, no data then break
DECLARE @scenario_code uniqueidentifier
SELECT TOP 1 @scenario_code=scenario_code FROM Stage.stg_schedule_changed
IF @scenario_code IS NULL
BEGIN
	RETURN 0
END

--Get first row scenario in stage table, currently this must(!) be the default scenario, else RAISERROR
if (select count(*)
	from mart.dim_scenario
	where business_unit_code = @business_unit_code
	and scenario_code = @scenario_code
	) <> 1
BEGIN
	DECLARE @ErrorMsg nvarchar(4000)
	SELECT @ErrorMsg  = 'This is not a default scenario, or muliple default scenarios exists!'
	RAISERROR (@ErrorMsg,16,1)
	RETURN 0
END

--fill "temporary" tables used for later Intraday loads
exec [stage].[etl_stg_schedule_updated_special_load]

DELETE fs
FROM Stage.stg_schedule_changed stg
INNER JOIN Stage.stg_schedule_updated_personLocal dp
	ON stg.person_code		=	dp.person_code
	AND --trim
		(
				(stg.schedule_date	>= dp.valid_from_date_local)

			AND
				(stg.schedule_date < dp.valid_to_date_local)
		)
INNER JOIN mart.dim_scenario ds
	ON stg.scenario_code = ds.scenario_code
	AND stg.scenario_code = @scenario_code  --remove this if we are to handle multiple scenarios
INNER JOIN mart.fact_schedule fs
	ON dp.person_id = fs.person_id
 	AND ds.scenario_id = fs.scenario_id
INNER JOIN Stage.stg_schedule_updated_ShiftStartDateUTC dd
	ON dd.shift_startdate_id = fs.shift_startdate_id
WHERE stg.business_unit_code = @business_unit_code

--insert new and updated
INSERT INTO mart.fact_schedule
	(
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
GO
