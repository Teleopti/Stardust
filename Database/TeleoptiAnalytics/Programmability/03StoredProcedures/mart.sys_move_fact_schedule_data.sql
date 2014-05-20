IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_move_fact_schedule_data]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_move_fact_schedule_data]
GO

CREATE PROCEDURE [mart].[sys_move_fact_schedule_data] 
	
AS
SET NOCOUNT ON
	---move out from trunk script
--exec [mart].[sys_move_fact_schedule_data] 
	--Prepare intervals for new column

DECLARE @date_min smalldatetime
SET @date_min='1900-01-01'

CREATE TABLE #intervals
(
	interval_id smallint not null,
	interval_start smalldatetime null,
	interval_end smalldatetime null
)

INSERT #intervals(interval_id,interval_start,interval_end)
SELECT interval_id= interval_id,
	interval_start= interval_start,
	interval_end = interval_end
FROM mart.dim_interval
ORDER BY interval_id

--remove one minute from last interval to be able to join shifts ending at UTC midnight
UPDATE #intervals 
SET interval_end=dateadd(minute,-1,interval_end) 
WHERE interval_end=dateadd(day,1,@date_min)

SET NOCOUNT OFF
----INSERT DATA FROM OLD FACT_SCHEDULE
INSERT [mart].[fact_schedule] WITH(TABLOCK)
(shift_startdate_local_id, schedule_date_id, person_id, interval_id, activity_starttime, scenario_id, activity_id, absence_id, activity_startdate_id, activity_enddate_id, activity_endtime, shift_startdate_id, shift_starttime, shift_enddate_id, shift_endtime, shift_startinterval_id, shift_endinterval_id, shift_category_id, shift_length_id, scheduled_time_m, scheduled_time_absence_m, scheduled_time_activity_m, scheduled_contract_time_m, scheduled_contract_time_activity_m, scheduled_contract_time_absence_m, scheduled_work_time_m, scheduled_work_time_activity_m, scheduled_work_time_absence_m, scheduled_over_time_m, scheduled_ready_time_m, scheduled_paid_time_m, scheduled_paid_time_activity_m, scheduled_paid_time_absence_m, business_unit_id, datasource_id, insert_date, update_date, datasource_update_date, overtime_id)
SELECT top 100000 
	shift_startdate_local_id			=	btz.local_date_id, 
	schedule_date_id					=	f.schedule_date_id, 
	person_id							=	f.person_id, 
	interval_id							=	f.interval_id, 
	activity_starttime					=	f.activity_starttime, 
	scenario_id							=	f.scenario_id, 
	activity_id							=	f.activity_id, 
	absence_id							=	f.absence_id, 
	activity_startdate_id				=	f.activity_startdate_id, 
	activity_enddate_id					=	f.activity_enddate_id, 
	activity_endtime					=	f.activity_endtime, 
	shift_startdate_id					=	f.shift_startdate_id, 
	shift_starttime						=	f.shift_starttime, 
	shift_enddate_id					=	f.shift_enddate_id, 
	shift_endtime						=	f.shift_endtime, 
	shift_startinterval_id				=	f.shift_startinterval_id, 
	shift_endinterval_id				=	di.interval_id, 
	shift_category_id					=	f.shift_category_id, 
	shift_length_id						=	f.shift_length_id, 
	scheduled_time_m					=	f.scheduled_time_m, 
	scheduled_time_absence_m			=	f.scheduled_time_absence_m, 
	scheduled_time_activity_m			=	f.scheduled_time_activity_m, 
	scheduled_contract_time_m			=	f.scheduled_contract_time_m, 
	scheduled_contract_time_activity_m	=	f.scheduled_contract_time_activity_m, 
	scheduled_contract_time_absence_m	=	f.scheduled_contract_time_absence_m, 
	scheduled_work_time_m				=	f.scheduled_work_time_m, 
	scheduled_work_time_activity_m		=	f.scheduled_work_time_activity_m, 
	scheduled_work_time_absence_m		=	f.scheduled_work_time_absence_m, 
	scheduled_over_time_m				=	f.scheduled_over_time_m, 
	scheduled_ready_time_m				=	f.scheduled_ready_time_m, 
	scheduled_paid_time_m				=	f.scheduled_paid_time_m, 
	scheduled_paid_time_activity_m		=	f.scheduled_paid_time_activity_m, 
	scheduled_paid_time_absence_m		=	f.scheduled_paid_time_absence_m, 
	business_unit_id					=	f.business_unit_id, 
	datasource_id						=	f.datasource_id, 
	insert_date							=	f.insert_date, 
	update_date							=	f.update_date, 
	datasource_update_date				=	f.datasource_update_date, 
	overtime_id							=	f.overtime_id
FROM [mart].[fact_schedule_old] f
INNER JOIN mart.bridge_time_zone btz 
	ON f.shift_startdate_id=btz.date_id 
	AND f.shift_startinterval_id=btz.interval_id
INNER JOIN mart.dim_person dp
	ON f.person_id=dp.person_id
	AND btz.time_zone_id=dp.time_zone_id
INNER JOIN #intervals di
	ON	dateadd(hour,DATEPART(hour,f.shift_endtime),@date_min)+ dateadd(minute,DATEPART(minute,f.shift_endtime),@date_min) > di.interval_start
	AND	dateadd(hour,DATEPART(hour,f.shift_endtime),@date_min)+ dateadd(minute,DATEPART(minute,f.shift_endtime),@date_min) <= di.interval_end

SET NOCOUNT ON
--DELETE MOVED DATA FROM OLD
DELETE old
FROM mart.fact_schedule_old old
INNER JOIN mart.fact_schedule new 
	ON new.schedule_date_id=old.schedule_date_id 
	AND new.person_id=old.person_id 
	AND new.interval_id=old.interval_id 
	AND new.activity_starttime=old.activity_starttime 
	AND new.scenario_id=old.scenario_id

GO


