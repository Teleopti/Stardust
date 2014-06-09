IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_deviation_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_deviation_load]
GO
-- =============================================
-- Author:		ChLu
-- Design/calculation: http://challenger/sites/DefaultCollection/matrix/Shared%20Documents/Design%20Specifications/Adherance.xlsx
-- Description:	Write schedule deviation from fact_agent,fact_schedule and fact_contract tables 
--				to data mart table 'fact_schedule_viation'.
-- Updates:		2008-09-02 Added parameters @start_date and @end_date
--				2008-12-03 Removed refernce to fact_contract, contract_time now from fact_schedule KJ
--				2009-02-11 New mart schema KJ
--				2009-07-07 Added new #-table for summarizing KJ
--				2009-09-16 Changed the way we treat deviation_m for over and under performance DJ
--				2009-10-01 Include rows for agents with Schedule but without agent statistics
--				2009-10-01 Removed isnull in calculation
--				2009-10-01 Special CASE WHEN scheduled_ready_time_m = 0 THEN SET deviation_schedule_m = ready_time_m
--				2010-06-04 Need to consider scenario when doing calculationr
--				2010-06-10 Added detection of is_logged_in, used when ready_time_m = 0 to show zero, else empty string in report
--				2010-09-20 Fix calculation: Ready Time vs. Scheduled Ready Time. Set 100% as soon a mixed interval is fullfilled. e.g Readytime >= ScheduleReadytime
--				2010-11-01 #11055 Refact of mart.fact_schedule_deviation, measures in seconds instead of minutes. KJ
--				2012-10-08 #20924 Fix Contract Deviation
--				2013-11-26 #25906 - make sure we put stats on correct adherance interval
--				2013-11-29 #25900 Fix for nights shift with person period change
--
-- =============================================
--exec mart.etl_fact_schedule_deviation_load @start_date='2013-07-02 00:00:00',@end_date='2013-07-02 00:00:00',@business_unit_code='70B18F45-1FF3-4BA7-AEB2-A13F00BDC738',@isIntraday=0
CREATE PROCEDURE [mart].[etl_fact_schedule_deviation_load]
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier,
@isIntraday bit = 0,
@is_delayed_job bit = 0
AS

-- temporary part of #24257 we can't just use the updated schedules to know if we should do anything
-- there could be updated statistcs too, maybe we can do both, update today and yesterday AND other days with changed schedules
set @isIntraday = 0

--Execute one delayed jobs, if any
if (@is_delayed_job=0 --only run once per ETL, dynamic SP will call using: @is_delayed_job=1
	and @isIntraday=0 --only run if Nightly
	and (select count(*) from mart.etl_job_delayed)>0 --only run if we accutally have delayed batches to execute
	)
EXEC mart.etl_execute_delayed_job @stored_procedure='mart.etl_fact_schedule_deviation_load'


DECLARE @start_date_id int
DECLARE @end_date_id int
DECLARE @max_date_id int
DECLARE @min_date_id int
DECLARE @business_unit_id int
DECLARE @scenario_id int
DECLARE @scenario_code uniqueidentifier
DECLARE @date_min smalldatetime
DECLARE @intervals_outside_shift int
declare @interval_length_minutes int 
SET @date_min='1900-01-01'

CREATE TABLE #fact_schedule_deviation(
	date_id int,
	shift_startdate_id int ,
	interval_id smallint,
	shift_startinterval_id smallint, 
	shift_endinterval_id smallint,
	acd_login_id int, --new 20131128
	person_id int,
	scheduled_ready_time_s int default 0,
	ready_time_s int default 0,
	is_logged_in int default 0, 
	contract_time_s int default 0,
	business_unit_id int,
	person_code uniqueidentifier
)

CREATE TABLE #fact_schedule (
	[schedule_date_id] [int] NOT NULL,
	[shift_startdate_id] [int] NOT NULL,
	[shift_startinterval_id] smallint, 
	[shift_endtime] smalldatetime,
	[interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL,
	[scheduled_ready_time_m] [int] NULL,
	[scheduled_contract_time_m] [int] NULL,
	[scheduled_time_m][int] NULL,
	[business_unit_id] [int] NULL
)

CREATE CLUSTERED INDEX [#CIX_fact_schedule] ON #fact_schedule
(
	[schedule_date_id] ASC,
	[interval_id] ASC
)

CREATE TABLE #intervals
(
	interval_id smallint not null,
	interval_start smalldatetime null,
	interval_end smalldatetime null
)

CREATE TABLE #stg_schedule_changed(
	[person_id] [int] NOT NULL,
	[shift_startdate_id] [int] NOT NULL
)

INSERT #intervals(interval_id,interval_start,interval_end)
SELECT interval_id= interval_id,
	interval_start= interval_start,
	interval_end = interval_end
FROM mart.dim_interval
ORDER BY interval_id

--remove one minute from last interval to be able to join shifts ending at UTC midnight
update #intervals 
set interval_end=dateadd(minute,-1,interval_end) 
where interval_end='1900-01-02 00:00:00'

--get the number of intervals outside shift to consider for adherence calc
SELECT @interval_length_minutes = 1440/COUNT(*) from mart.dim_interval 

SELECT @intervals_outside_shift = value/@interval_length_minutes FROM mart.sys_configuration WHERE [KEY]='AdherenceMinutesOutsideShift'  

/*Remove timestamp from datetime*/
SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,8),@start_date )))
SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,8),@end_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

SET @scenario_id	=	(SELECT scenario_id FROM mart.dim_scenario WHERE business_unit_code = @business_unit_code AND default_scenario = 1)
SET @scenario_code	=	(SELECT scenario_code FROM mart.dim_scenario WHERE business_unit_code = @business_unit_code AND default_scenario = 1)

/*Get business unit id*/
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

/*Remove old data*/
if (@isIntraday=0)
BEGIN
	INSERT INTO #fact_schedule
	SELECT
		 fs.schedule_date_id, 
		 fs.shift_startdate_id,
		 fs.shift_startinterval_id,
		 fs.shift_endtime,
		 fs.interval_id,
		 fs.person_id, 
		sum(isnull(fs.scheduled_ready_time_m,0)) 'scheduled_ready_time_m',
		sum(isnull(fs.scheduled_time_m,0))'scheduled_time_m',
		sum(isnull(fs.scheduled_contract_time_m,0))'scheduled_contract_time_m',
		fs.business_unit_id
	FROM 
		mart.fact_schedule fs
	WHERE fs.schedule_date_id BETWEEN @start_date_id AND @end_date_id
		AND fs.business_unit_id = @business_unit_id
		AND fs.scenario_id = @scenario_id
	GROUP BY 
		fs.schedule_date_id, 
		fs.shift_startdate_id,
		fs.shift_startinterval_id,
		fs.shift_endtime,
		fs.interval_id,
		fs.person_id,
		fs.business_unit_id

	/* a) Gather agent ready time */
	INSERT INTO #fact_schedule_deviation
		(
		date_id, 
		interval_id,
		acd_login_id, --new 20131128
		person_id, 
		ready_time_s,
		is_logged_in,
		business_unit_id,
		person_code
		)
	SELECT
		date_id					= fa.date_id, 
		interval_id				= fa.interval_id,
		acd_login_id			= fa.acd_login_id,--new 20131128
		person_id				= b.person_id, 
		ready_time_s			= fa.ready_time_s,
		is_logged_in			= 1, --marks that we do have logged in time
		business_unit_id		= b.business_unit_id,
		person_code				= p.person_code
	FROM 
		mart.bridge_acd_login_person b
	JOIN
		mart.fact_agent fa
	ON
		b.acd_login_id = fa.acd_login_id
	INNER JOIN 
		mart.dim_person p
	ON 
		p.person_id = b.person_id
		AND
			(
				(fa.date_id > p.valid_from_date_id AND fa.date_id < p.valid_to_date_id_maxDate)
					OR (fa.date_id = p.valid_from_date_id AND fa.interval_id >= p.valid_from_interval_id)
					OR (fa.date_id = p.valid_to_date_id_maxDate AND fa.interval_id <= p.valid_to_interval_id_maxDate)
			)
	WHERE
		fa.date_id BETWEEN @start_date_id AND @end_date_id
		AND b.business_unit_id = @business_unit_id

	DELETE FROM mart.fact_schedule_deviation
	WHERE date_id between @start_date_id AND @end_date_id
		AND business_unit_id = @business_unit_id
END

if (@isIntraday=1)
BEGIN

	INSERT INTO #stg_schedule_changed
	select DISTINCT
		f.person_id,
		f.shift_startdate_id
	from mart.fact_schedule_deviation f
	inner join mart.dim_person p
		on p.person_id = f.person_id
	inner join mart.bridge_time_zone btz
		on f.shift_startdate_id = btz.date_id
		and f.shift_startinterval_id = btz.interval_id
		and p.time_zone_id = btz.time_zone_id
	inner join mart.dim_date dd
		on dd.date_id = btz.local_date_id
	inner join stage.stg_schedule_changed ch
		on ch.person_code = p.person_code
		and ch.schedule_date = dd.date_date
			AND --trim
			(
					(ch.schedule_date	>= p.valid_from_date_local)

				AND
					(ch.schedule_date <= p.valid_to_date_local)
			)
	where ch.scenario_code = @scenario_code

	--if in Intraday-mode, we have get the dates based on Agent stat job which is of no use.
	--Instead use min/max date from changed table
	SELECT
		@start_date_id	= min(shift_startdate_id),
		@end_date_id	= max(shift_startdate_id)
	FROM #stg_schedule_changed

	INSERT INTO #fact_schedule
	SELECT
		 fs.schedule_date_id, 
		 fs.shift_startdate_id,
		 fs.shift_startinterval_id,
		 fs.shift_endtime,
		 fs.interval_id,
		 fs.person_id, 
		sum(isnull(fs.scheduled_ready_time_m,0)) 'scheduled_ready_time_m',
		sum(isnull(fs.scheduled_time_m,0))'scheduled_time_m',
		sum(isnull(fs.scheduled_contract_time_m,0))'scheduled_contract_time_m',
		fs.business_unit_id
	FROM mart.fact_schedule fs
	INNER JOIN #stg_schedule_changed ch
		ON ch.shift_startdate_id = fs.shift_startdate_id 
		AND ch.person_id = fs.person_id
	WHERE fs.business_unit_id = @business_unit_id
	AND fs.scenario_id = @scenario_id
	GROUP BY 
		fs.schedule_date_id, 
		fs.shift_startdate_id,
		fs.shift_startinterval_id,
		fs.shift_endtime,
		fs.interval_id,
		fs.person_id,
		fs.business_unit_id

	/* a) Gather agent ready time */
	INSERT INTO #fact_schedule_deviation
		(
		date_id, 
		interval_id,
		person_id, 
		acd_login_id,--new 20131128
		ready_time_s,
		is_logged_in,
		business_unit_id,
		person_code
		)
	SELECT
		date_id					= fa.date_id, 
		interval_id				= fa.interval_id,
		acd_login_id			= fa.acd_login_id, --new 20131128
		person_id				= b.person_id, 
		ready_time_s			= fa.ready_time_s,
		is_logged_in			= 1, --marks that we do have logged in time
		business_unit_id		= b.business_unit_id,
		person_code				= p.person_code
	FROM mart.bridge_acd_login_person b
	INNER JOIN mart.fact_agent fa
		ON b.acd_login_id = fa.acd_login_id
	INNER JOIN mart.dim_person p
		ON p.person_id = b.person_id
		AND
			(
				(fa.date_id > p.valid_from_date_id AND fa.date_id < p.valid_to_date_id_maxDate)
					OR (fa.date_id = p.valid_from_date_id AND fa.interval_id >= p.valid_from_interval_id)
					OR (fa.date_id = p.valid_to_date_id_maxDate AND fa.interval_id <= p.valid_to_interval_id_maxDate)
			)
	INNER JOIN #stg_schedule_changed ch
		ON  ch.shift_startdate_id	= fa.date_id
		AND ch.person_id			= b.person_id
		AND b.acd_login_id			= fa.acd_login_id
		AND ch.shift_startdate_id	= fa.date_id
	WHERE b.business_unit_id = @business_unit_id

	DELETE fs
	FROM #stg_schedule_changed ch
	INNER JOIN mart.fact_schedule_deviation fs
		ON ch.person_id = fs.person_id
		AND ch.shift_startdate_id = fs.shift_startdate_id 
	WHERE fs.business_unit_id = @business_unit_id

END

--remove one minute from shifts ending at UTC midnight(00:00)
UPDATE #fact_schedule
SET shift_endtime= DATEADD(MINUTE,-1,shift_endtime)
WHERE DATEPART(HOUR,shift_endtime)=0 AND DATEPART(MINUTE,shift_endtime)=0 

/* b) Gather agent schedule time */
INSERT INTO #fact_schedule_deviation
	(
	date_id, 
	shift_startdate_id,
	shift_startinterval_id,
	shift_endinterval_id,
	interval_id,
	person_id,
	is_logged_in, 
	scheduled_ready_time_s,
	contract_time_s,
	business_unit_id,
	person_code
	)
SELECT
	date_id					= fs.schedule_date_id, 
	shift_startdate_id		= fs.shift_startdate_id,
	shift_startinterval_id	= fs.shift_startinterval_id,
	shift_endinterval_id	= di.interval_id,
	interval_id				= fs.interval_id,
	person_id				= fs.person_id,
	is_logged_in			= 0, --Mark schedule rows as Not loggged in 
	scheduled_ready_time_s	= fs.scheduled_ready_time_m*60,
	contract_time_s			= fs.scheduled_contract_time_m*60,
	business_unit_id		= fs.business_unit_id,
	person_code				= p.person_code
FROM 
	#fact_schedule fs
INNER JOIN 
	mart.dim_person p
ON
	p.person_id = fs.person_id
	AND
		(
				(fs.shift_startdate_id > p.valid_from_date_id AND fs.shift_startdate_id < p.valid_to_date_id_maxDate)
				OR (fs.shift_startdate_id = p.valid_from_date_id AND fs.shift_startinterval_id >= p.valid_from_interval_id)
				OR (fs.shift_startdate_id = p.valid_to_date_id_maxDate AND fs.shift_startinterval_id <= p.valid_to_interval_id_maxDate)
		)
INNER JOIN 
	#intervals di
ON 
	dateadd(hour,DATEPART(hour,fs.shift_endtime),@date_min)+ dateadd(minute,DATEPART(minute,fs.shift_endtime),@date_min) > di.interval_start
	and dateadd(hour,DATEPART(hour,fs.shift_endtime),@date_min)+ dateadd(minute,DATEPART(minute,fs.shift_endtime),@date_min) <= di.interval_end
WHERE
	fs.schedule_date_id BETWEEN @start_date_id AND @end_date_id
	AND fs.business_unit_id = @business_unit_id


/*#26421 Update schedule data with acd_login_id to handle nights shifts and person_period change*/
UPDATE #fact_schedule_deviation
SET acd_login_id=b.acd_login_id
FROM mart.bridge_acd_login_person b 
INNER JOIN #fact_schedule_deviation shifts
ON shifts.person_id=b.person_id
WHERE shifts.shift_startdate_id IS NOT NULL --only schedule data
AND shifts.acd_login_id IS NULL 
AND b.acd_login_id in (select acd_login_id from #fact_schedule_deviation stat where stat.shift_startdate_id IS NULL and stat.acd_login_id is not null)--pick only when acd_login exists


/*#25900:20131128 Handle night shifts and person_period change, must update person_id in stat from correct shift*/
/*#28059 Handle shared logins as well(only update when same person_code*/
UPDATE stat
SET person_id=shifts.person_id--update agent stat data
FROM #fact_schedule_deviation shifts
INNER JOIN #fact_schedule_deviation stat
ON stat.date_id=shifts.date_id AND stat.interval_id=shifts.interval_id AND shifts.acd_login_id=stat.acd_login_id
WHERE stat.person_id<>shifts.person_id AND shifts.acd_login_id IS NOT NULL AND stat.person_code=shifts.person_code --where diff on person_id but same acd_login and same person_code
AND shifts.shift_startdate_id IS NOT NULL  --get from schedule data
AND stat.shift_startdate_id IS NULL


--UPDATE ALL ROWS WITH KNOWN SHIFT_STARTDATE_ID
UPDATE stat
SET shift_startdate_id = shifts.shift_startdate_id, shift_startinterval_id=shifts.shift_startinterval_id
FROM #fact_schedule_deviation shifts
INNER JOIN #fact_schedule_deviation stat
ON stat.date_id=shifts.date_id AND stat.interval_id=shifts.interval_id AND stat.person_id=shifts.person_id
WHERE shifts.shift_startdate_id IS NOT NULL

--ALL ROWS BEFORE SHIFT WITH NO SHIFT_STARTDATE_ID TO NEAREST SHIFT +-SOMETHING 
UPDATE stat
SET shift_startdate_id = shifts.shift_startdate_id, shift_startinterval_id=shifts.shift_startinterval_id
FROM #fact_schedule_deviation shifts
INNER JOIN #fact_schedule_deviation stat
ON stat.date_id=shifts.date_id AND stat.person_id=shifts.person_id
WHERE stat.shift_startdate_id IS NULL 
AND stat.interval_id < shifts.shift_startinterval_id 
AND stat.interval_id >= shifts.shift_startinterval_id - @intervals_outside_shift-- ONLY 2 Hours back
AND stat.date_id <= shifts.shift_startdate_id
AND stat.date_id <= shifts.date_id --make sure the stat intervals are before shift
AND stat.interval_id <= shifts.interval_id

--ALL ROWS AFTER SHIFT WITH NO SHIFT_STARTDATE_ID TO NEAREST SHIFT +-SOMETHING 
UPDATE stat
SET shift_startdate_id = shifts.shift_startdate_id,shift_startinterval_id=shifts.shift_startinterval_id
FROM #fact_schedule_deviation shifts
INNER JOIN #fact_schedule_deviation stat
ON stat.date_id=shifts.date_id AND stat.person_id=shifts.person_id
WHERE stat.shift_startdate_id IS NULL 
AND stat.interval_id > shifts.shift_endinterval_id
AND stat.interval_id <= shifts.shift_endinterval_id + @intervals_outside_shift -- ONLY 2 Hours ahead
AND stat.date_id >= shifts.shift_startdate_id
AND stat.date_id >= shifts.date_id --make sure the stat intervals are after shift
AND stat.interval_id >= shifts.interval_id


DELETE FROM #fact_schedule_deviation WHERE shift_startdate_id IS NULL


/* Insert of new data */
INSERT INTO mart.fact_schedule_deviation
	(
	date_id, 
	shift_startdate_id,
	shift_startinterval_id,
	interval_id,
	person_id, 
	scheduled_ready_time_s,
	ready_time_s,
	is_logged_in,
	contract_time_s,
	business_unit_id,
	datasource_id, 
	insert_date, 
	update_date
	)
SELECT
	date_id					= date_id, 
	shift_startdate_id		= MAX(shift_startdate_id),
	shift_startinterval_id	= max(shift_startinterval_id),
	interval_id				= interval_id,
	person_id				= person_id, 
	scheduled_ready_time_s	= sum(isnull(scheduled_ready_time_s,0)),
	ready_time_s			= sum(isnull(ready_time_s,0)),
	is_logged_in			= sum(is_logged_in), --Calculated bit value
	contract_time_s			= sum(isnull(contract_time_s,0)),
	business_unit_id		= business_unit_id,
	datasource_id			= -1, 
	insert_date				= getdate(), 
	update_date				= getdate()
FROM 
	#fact_schedule_deviation
GROUP BY 
	date_id, 
	interval_id,
	person_id,
	business_unit_id

--If agents are logged in, but have no Schedule_Time_s then fake time as @interval_length.
--Used later when doing Adherance calculation

--We count the three deviation_m in two ways a) or b)
--We also "move" the activities during a shift in favor of the Agent, since we can't say exactly when they actually occurred during the interval
--a) If Ready Time is less or equal to ScheduleReadyTime (agent under performed)
--In this case the Deviation will be the actual Diff between ScheduleReadyTime vs. ActualReadyTime
--As: Deviation_m = scheduled_ready_time_m-ready_time_m

--b) If ReadyTime is more then ScheduleReadyTime (agent over performed)
--Deviation will be set to zero for ScheduledReadyTime + contract calculation
--Deviation will be set to ABS(scheduled_ready_time_s-ready_time_s) for scheduled time calculation


--See: http://challenger/sites/DefaultCollection/matrix/Shared%20Documents/Design%20Specifications/Adherance.xlsx
--First handle a special case for [deviation_schedule_ready_s]
--If we have no Scheduled Ready Time there can be no deviation
UPDATE mart.fact_schedule_deviation
SET
	deviation_schedule_ready_s = 0
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s=0

--2 Deviation from schedule, i.e all scheduled time, including breaks and lunch
-- Note: You will get punished if you over perform e.i beeing logged in during breaks, lunch etc.
--		 You will also get punished if you are logged in on a interval with no scheduled_ready_time_s at all
UPDATE mart.fact_schedule_deviation
SET deviation_schedule_s = ABS(scheduled_ready_time_s-ready_time_s)
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id

--1 Deviation_schedule_ready, only intervals where agents are scheduled to be ready are included
-- a) Under performance
UPDATE	mart.fact_schedule_deviation
SET	deviation_schedule_ready_s = scheduled_ready_time_s-ready_time_s
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s>0
AND scheduled_ready_time_s>=ready_time_s

-- b) Over performance
UPDATE	mart.fact_schedule_deviation
SET	deviation_schedule_ready_s = 0 --corrected
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND scheduled_ready_time_s>0
AND scheduled_ready_time_s<ready_time_s

--3 Calculated as 2) i.e punish for over performance. But only time where agents are contracted to be working are included
UPDATE mart.fact_schedule_deviation
SET 	deviation_contract_s = ABS(
	(
	CASE
		WHEN scheduled_ready_time_s>contract_time_s then contract_time_s
		ELSE scheduled_ready_time_s
	END
	)
-
	(
	CASE
		WHEN ready_time_s>contract_time_s then contract_time_s
		ELSE ready_time_s
	END
	)
)
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
GO
