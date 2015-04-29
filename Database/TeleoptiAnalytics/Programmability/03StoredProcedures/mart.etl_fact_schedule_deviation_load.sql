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
--				2014-02-10 #26422 Redesign with local date
--
-- =============================================
--exec mart.etl_fact_schedule_deviation_load @start_date='2013-02-04 00:00:00',@end_date='2013-02-06 00:00:00',@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA',@isIntraday=0
--exec mart.etl_fact_schedule_deviation_load @start_date='2014-02-17 00:00:00',@end_date='2014-02-23 00:00:00',@business_unit_code='9D812B66-A7BD-4FFF-A2D8-A2D90001CAF1',@isIntraday=1
--exec mart.etl_fact_schedule_deviation_load @start_date='2014-03-04 00:00:00',@end_date='2014-03-05 00:00:00',@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA',@isIntraday=1
--exec mart.etl_fact_schedule_deviation_load @start_date='2014-10-21 00:00:00',@end_date='2014-10-22 00:00:00',@business_unit_code='0B43736B-4ED5-4D9F-B2E4-A34200A7EF70',@isIntraday=1
--exec mart.etl_fact_schedule_deviation_load @start_date='2013-06-11 00:00:00',@end_date='2013-06-11 00:00:00',@business_unit_code='13DBFFE3-CA0A-4E7B-8CC8-A3E601034177',@isIntraday=2,@is_delayed_job=NULL,@now_utc=NULL
--exec mart.etl_fact_schedule_deviation_load @start_date='2013-06-11 00:00:00',@end_date='2013-06-11 00:00:00',@business_unit_code='057E2EE2-315E-4DED-B8DA-A3E700F71835',@isIntraday=2,@is_delayed_job=NULL,@now_utc='2013-06-15 00:00:00'
CREATE PROCEDURE [mart].[etl_fact_schedule_deviation_load]
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier,
@isIntraday tinyint = 0,
@is_delayed_job bit = 0,
@now_utc smalldatetime = null

AS
SET NOCOUNT ON
IF @now_utc is null
SET @now_utc = cast(getutcdate() as smalldatetime)
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
DECLARE @minutes_outside_shift int
DECLARE @intervals_outside_shift int
declare @interval_length_minutes int 

declare @now_date_date_utc smalldatetime
declare @now_interval_id_utc smallint
declare @now_date_id_utc int
declare @from_interval_id_utc smallint
declare @from_date_id_utc int
declare @from_date_date_utc smalldatetime
declare @intervals_back smallint
declare @detail_id int
SET @detail_id=4 --deviation

CREATE TABLE #fact_schedule_deviation(
	shift_startdate_local_id int,
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

CREATE TABLE #fact_schedule_deviation_merge(
	[shift_startdate_local_id] [int] NOT NULL,
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL,
	[scheduled_ready_time_s] [int] NULL,
	[ready_time_s] [int] NULL,
	[contract_time_s] [int] NULL,
	[deviation_schedule_s] [decimal](18, 4) NULL,
	[deviation_schedule_ready_s] [decimal](18, 4) NULL,
	[deviation_contract_s] [decimal](18, 4) NULL,
	[business_unit_id] [int] NULL,
	[is_logged_in] [bit] NOT NULL,
	[shift_startdate_id] [int] NULL,
	[shift_startinterval_id] [smallint] NULL
)

CREATE TABLE #fact_schedule (
	[shift_startdate_local_id] [int] NOT NULL,
	[schedule_date_id] [int] NOT NULL,
	[shift_startdate_id] [int] NOT NULL,
	[shift_startinterval_id] smallint, 
	shift_endinterval_id smallint, 
	[shift_endtime] smalldatetime,
	[interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL,
	[scheduled_ready_time_m] [int] NULL,
	[scheduled_contract_time_m] [int] NULL,
	[scheduled_time_m][int] NULL,
	[business_unit_id] [int] NULL,
	[person_code] [uniqueidentifier]
)

CREATE CLUSTERED INDEX [#CIX_fact_schedule] ON #fact_schedule
(
	[shift_startdate_local_id] ASC,
	[schedule_date_id] ASC,
	[interval_id] ASC
)

CREATE TABLE #stg_schedule_changed(
	[person_id] [int] NOT NULL,
	[shift_startdate_local_id] [int] NOT NULL,
	[shift_startdate_local] [smalldatetime] NOT NULL,
	[person_code] [uniqueidentifier]
)

--get the number of intervals outside shift to consider for adherence calc
SELECT @interval_length_minutes = 1440/COUNT(*) from mart.dim_interval 
SELECT @minutes_outside_shift = value FROM mart.sys_configuration WHERE [KEY]='AdherenceMinutesOutsideShift'  

SELECT @intervals_outside_shift = ISNULL(@minutes_outside_shift,120)/@interval_length_minutes 

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
		 fs.shift_startdate_local_id,
		 fs.schedule_date_id, 
		 fs.shift_startdate_id,
		 fs.shift_startinterval_id,
		 fs.shift_endinterval_id,
		 fs.shift_endtime,
		 fs.interval_id,
		 fs.person_id, 
		sum(isnull(fs.scheduled_ready_time_m,0)) 'scheduled_ready_time_m',
		sum(isnull(fs.scheduled_contract_time_m,0))'scheduled_contract_time_m',
		sum(isnull(fs.scheduled_time_m,0))'scheduled_time_m',
		fs.business_unit_id,
		p.person_code
	FROM 
		mart.fact_schedule fs
	INNER JOIN 
		mart.dim_person p 
	ON p.person_id=fs.person_id
	WHERE fs.shift_startdate_local_id BETWEEN @start_date_id AND @end_date_id
		AND fs.scenario_id = @scenario_id
	GROUP BY 
		fs.shift_startdate_local_id,
		fs.schedule_date_id, 
		fs.shift_startdate_id,
		fs.shift_startinterval_id,
		fs.shift_endinterval_id,
		fs.shift_endtime,
		fs.interval_id,
		fs.person_id,
		fs.business_unit_id,
		p.person_code

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
					OR (fa.date_id = p.valid_to_date_id_maxdate AND fa.interval_id <= p.valid_to_interval_id_maxdate)
			)
	WHERE
		fa.date_id BETWEEN @start_date_id-1 AND @end_date_id+1 --extend stat to cover local date
		AND b.business_unit_id = @business_unit_id
END
ELSE
BEGIN
	if (@isIntraday=1)
	BEGIN
		INSERT INTO #stg_schedule_changed
		SELECT  p.person_id,
				dd.date_id,
				ch.schedule_date_local,
				p.person_code
		FROM stage.stg_schedule_changed ch
		INNER JOIN mart.dim_person p
			ON p.person_code = ch.person_code
				AND --trim
				(
						(ch.schedule_date_local	>= p.valid_from_date_local)
					AND
						(ch.schedule_date_local <= p.valid_to_date_local)
				)
		INNER JOIN mart.dim_date dd
			ON dd.date_date = ch.schedule_date_local
		WHERE ch.scenario_code = @scenario_code
		AND ch.schedule_date_local <= dateadd(dd,1,@now_utc)

		INSERT INTO #fact_schedule
		--First get changed day via #stg_schedule_changed (full local agent day)
		SELECT
			 fs.shift_startdate_local_id,
			 fs.schedule_date_id, 
			 fs.shift_startdate_id,
			 fs.shift_startinterval_id,
			 fs.shift_endinterval_id,
			 fs.shift_endtime,
			 fs.interval_id,
			 fs.person_id, 
			sum(isnull(fs.scheduled_ready_time_m,0)) 'scheduled_ready_time_m',
			sum(isnull(fs.scheduled_contract_time_m,0))'scheduled_contract_time_m',
			sum(isnull(fs.scheduled_time_m,0))'scheduled_time_m',
			fs.business_unit_id,
			ch.person_code
		FROM mart.fact_schedule fs
		INNER JOIN #stg_schedule_changed ch
			ON ch.shift_startdate_local_id = fs.shift_startdate_local_id 
			AND ch.person_id = fs.person_id
		WHERE fs.scenario_id = @scenario_id
		GROUP BY 
			fs.shift_startdate_local_id,
			fs.schedule_date_id, 
			fs.shift_startdate_id,
			fs.shift_startinterval_id,
			fs.shift_endinterval_id,
			fs.shift_endtime,
			fs.interval_id,
			fs.person_id,
			fs.business_unit_id,
			ch.person_code

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

		SELECT DISTINCT
			date_id					= fa.date_id, 
			interval_id				= fa.interval_id,
			person_id				= b.person_id,
			acd_login_id			= fa.acd_login_id, --new 20131128
			ready_time_s			= fa.ready_time_s,
			is_logged_in			= 1, --marks that we do have logged in time
			business_unit_id		= b.business_unit_id,
			person_code				= ch.person_code
		FROM mart.bridge_acd_login_person b
		INNER JOIN mart.fact_agent fa
			ON b.acd_login_id = fa.acd_login_id
		INNER JOIN #stg_schedule_changed ch
			ON fa.date_id BETWEEN ch.shift_startdate_local_id-1 AND ch.shift_startdate_local_id+1 --extend stat to cover local date
			AND ch.person_id			= b.person_id
			AND b.acd_login_id			= fa.acd_login_id
			AND EXISTS (SELECT 1 FROM #stg_schedule_changed tmp WHERE tmp.person_id = b.person_id)
	END

	if (@isIntraday=2)
	BEGIN
		select
			@now_date_id_utc	=date_id,
			@now_date_date_utc	=CONVERT(DATE,@now_utc)
		from mart.dim_date
		where date_date=CONVERT(DATE,@now_utc)

		select
			@now_interval_id_utc=interval_id
		from mart.dim_interval
		where CONVERT(TIME, @now_utc) between CONVERT(TIME,interval_start) and CONVERT(TIME,dateadd(minute,-1,interval_end))

		SELECT
			@from_date_date_utc=target_date,
			@from_interval_id_utc=target_interval,
			@intervals_back=intervals_back
		FROM [mart].[etl_job_intraday_settings] ds WITH (TABLOCKX) --Block any other process from even reading this data. Wait until ETL is done processing!
		WHERE business_unit_id = @business_unit_id
		AND detail_id = @detail_id

		IF (
			@from_date_date_utc IS NULL
			OR
			@from_interval_id_utc IS NULL
			)
		Raiserror ('@from_date_id_utc or @from_interval_id_utc values inside [mart].[etl_fact_schedule_deviation_load] contained null values. Transaction should be aborted by ADO.NET!',16,1) WITH NOWAIT

		--if any "go back number of intervals"
		SELECT
			@from_date_id_utc=d.date_id,
			@from_interval_id_utc=d_i.interval_id
		FROM [mart].[SubtractInterval](@from_date_date_utc,@from_interval_id_utc,@intervals_back) d_i
		INNER JOIN mart.dim_date d
		 on d_i.date_from = d.date_date

		IF (
			@now_date_id_utc IS NULL
			OR
			@now_interval_id_utc IS NULL
			)
		Raiserror ('@now_date_id_utc or @now_interval_id_utc values inside [mart].[etl_fact_schedule_deviation_load] contained null values. Transaction should be aborted by ADO.NET!',16,1) WITH NOWAIT

		--get last n intervals according to mart.sys_datasource_detail
		INSERT INTO #fact_schedule
		SELECT
			 fs.shift_startdate_local_id,
			 fs.schedule_date_id, 
			 fs.shift_startdate_id,
			 fs.shift_startinterval_id,
			 fs.shift_endinterval_id,
			 fs.shift_endtime,
			 fs.interval_id,
			 fs.person_id, 
			sum(isnull(fs.scheduled_ready_time_m,0)) 'scheduled_ready_time_m',
			sum(isnull(fs.scheduled_contract_time_m,0))'scheduled_contract_time_m',
			sum(isnull(fs.scheduled_time_m,0))'scheduled_time_m',
			fs.business_unit_id,
			p.person_code
		FROM mart.fact_schedule fs
		INNER JOIN mart.dim_person p
			ON fs.person_id = p.person_id
		WHERE shift_startdate_local_id between @from_date_id_utc-1 and @now_date_id_utc+1
		AND
		(
				(schedule_date_id = @from_date_id_utc AND interval_id >= @from_interval_id_utc)
				OR (schedule_date_id between @from_date_id_utc+1 AND @now_date_id_utc-1)
				OR (schedule_date_id = @now_date_id_utc AND schedule_date_id > @from_date_id_utc)
		)
		AND fs.scenario_id = @scenario_id
		GROUP BY 
			fs.shift_startdate_local_id,
			fs.schedule_date_id, 
			fs.shift_startdate_id,
			fs.shift_startinterval_id,
			fs.shift_endinterval_id,
			fs.shift_endtime,
			fs.interval_id,
			fs.person_id,
			fs.business_unit_id,
			p.person_code
		
		--remove intervals for today in the future
		DELETE FROM #fact_schedule WHERE schedule_date_id = @now_date_id_utc AND interval_id > @now_interval_id_utc
		
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
			person_id				= b.person_id,
			acd_login_id			= fa.acd_login_id, --new 20131128
			ready_time_s			= fa.ready_time_s,
			is_logged_in			= 1, --marks that we do have logged in time
			business_unit_id		= b.business_unit_id,
			person_code				= p.person_code
		FROM mart.fact_agent fa
		INNER JOIN mart.bridge_acd_login_person b
			ON b.acd_login_id = fa.acd_login_id
			AND b.business_unit_id=@business_unit_id
		INNER JOIN 
			mart.dim_person p
		ON 
		p.person_id = b.person_id
		AND
			(
				(fa.date_id > p.valid_from_date_id AND fa.date_id < p.valid_to_date_id_maxDate)
					OR (fa.date_id = p.valid_from_date_id AND fa.interval_id >= p.valid_from_interval_id)
					OR (fa.date_id = p.valid_to_date_id_maxdate AND fa.interval_id <= p.valid_to_interval_id_maxdate)
			)
		WHERE (date_id between @from_date_id_utc+1 AND @now_date_id_utc)
		OR
		(
			date_id=@from_date_id_utc
			AND interval_id >= @from_interval_id_utc
		)
	END

	if (@isIntraday=3)--service bus changes
	BEGIN

		INSERT INTO #stg_schedule_changed
		SELECT  p.person_id,
				dd.date_id,
				ch.schedule_date_local,
				p.person_code
		FROM stage.stg_schedule_changed_servicebus ch
		INNER JOIN mart.dim_person p
			ON p.person_code = ch.person_code
				AND --trim
				(
						(ch.schedule_date_local	>= p.valid_from_date_local)
					AND
						(ch.schedule_date_local <= p.valid_to_date_local)
				)
		INNER JOIN mart.dim_date dd
			ON dd.date_date = ch.schedule_date_local
		WHERE ch.scenario_code = @scenario_code
		
		--delete rows from stage
		DELETE stage
		FROM #stg_schedule_changed ch
		INNER JOIN stage.stg_schedule_changed_servicebus stage
		ON ch.person_code = stage.person_code
		AND ch.shift_startdate_local = stage.schedule_date_local

		--REMOVE DATES IN THE FUTURE
		DELETE FROM #stg_schedule_changed
		WHERE shift_startdate_local > dateadd(dd,1,@now_utc)


		INSERT INTO #fact_schedule
		--First get changed day via #stg_schedule_changed (full local agent day)
		SELECT
			 fs.shift_startdate_local_id,
			 fs.schedule_date_id, 
			 fs.shift_startdate_id,
			 fs.shift_startinterval_id,
			 fs.shift_endinterval_id,
			 fs.shift_endtime,
			 fs.interval_id,
			 fs.person_id, 
			sum(isnull(fs.scheduled_ready_time_m,0)) 'scheduled_ready_time_m',
			sum(isnull(fs.scheduled_contract_time_m,0))'scheduled_contract_time_m',
			sum(isnull(fs.scheduled_time_m,0))'scheduled_time_m',
			fs.business_unit_id,
			ch.person_code
		FROM mart.fact_schedule fs
		INNER JOIN #stg_schedule_changed ch
			ON ch.shift_startdate_local_id = fs.shift_startdate_local_id 
			AND ch.person_id = fs.person_id
		WHERE fs.scenario_id = @scenario_id
		GROUP BY 
			fs.shift_startdate_local_id,
			fs.schedule_date_id, 
			fs.shift_startdate_id,
			fs.shift_startinterval_id,
			fs.shift_endinterval_id,
			fs.shift_endtime,
			fs.interval_id,
			fs.person_id,
			fs.business_unit_id,
			ch.person_code

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

		SELECT DISTINCT
			date_id					= fa.date_id, 
			interval_id				= fa.interval_id,
			person_id				= b.person_id,
			acd_login_id			= fa.acd_login_id, --new 20131128
			ready_time_s			= fa.ready_time_s,
			is_logged_in			= 1, --marks that we do have logged in time
			business_unit_id		= b.business_unit_id,
			person_code				= ch.person_code
		FROM mart.bridge_acd_login_person b
		INNER JOIN mart.fact_agent fa
			ON b.acd_login_id = fa.acd_login_id
		INNER JOIN #stg_schedule_changed ch
			ON fa.date_id BETWEEN ch.shift_startdate_local_id-1 AND ch.shift_startdate_local_id+1 --extend stat to cover local date
			AND ch.person_id			= b.person_id
			AND b.acd_login_id			= fa.acd_login_id
			AND EXISTS (SELECT 1 FROM #stg_schedule_changed tmp WHERE tmp.person_id = b.person_id)
	END
END

/* b) Gather agent schedule time */
INSERT INTO #fact_schedule_deviation
	(
	shift_startdate_local_id,
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
	shift_startdate_local_id =fs.shift_startdate_local_id,
	date_id					= fs.schedule_date_id, 
	shift_startdate_id		= fs.shift_startdate_id,
	shift_startinterval_id	= fs.shift_startinterval_id,
	shift_endinterval_id	= fs.shift_endinterval_id,
	interval_id				= fs.interval_id,
	person_id				= fs.person_id,
	is_logged_in			= 0, --Mark schedule rows as Not loggged in 
	scheduled_ready_time_s	= fs.scheduled_ready_time_m*60,
	contract_time_s			= fs.scheduled_contract_time_m*60,
	business_unit_id		= fs.business_unit_id,
	person_code				= fs.person_code
FROM #fact_schedule fs



/*#26421 Update schedule data with acd_login_id to handle nights shifts and person_period change*/
UPDATE #fact_schedule_deviation
SET acd_login_id=b.acd_login_id
FROM mart.bridge_acd_login_person b 
INNER JOIN #fact_schedule_deviation shifts
ON shifts.person_id=b.person_id
WHERE shifts.shift_startdate_local_id IS NOT NULL --only schedule data
AND shifts.acd_login_id IS NULL 
AND b.acd_login_id in (select acd_login_id from #fact_schedule_deviation stat where stat.shift_startdate_local_id IS NULL and stat.acd_login_id is not null)--pick only when acd_login exists

/*#25900:20131128 Handle night shifts and person_period change, must update person_id in stat from correct shift*/
UPDATE stat
SET person_id=shifts.person_id--update agent stat data
FROM #fact_schedule_deviation shifts
INNER JOIN #fact_schedule_deviation stat
ON stat.date_id=shifts.date_id AND stat.interval_id=shifts.interval_id AND shifts.acd_login_id=stat.acd_login_id
WHERE stat.person_id<>shifts.person_id AND shifts.acd_login_id IS NOT NULL AND shifts.person_code=stat.person_code--where diff on person but same acd_login (and same person_code #24433)
AND shifts.shift_startdate_local_id IS NOT NULL  --get from schedule data
AND stat.shift_startdate_local_id IS NULL

--UPDATE ALL STATISTICS ROWS WITH KNOWN SHIFT_STARTDATE_ID, (KEEP shift_startdate_local_id AS NULL FOR NEXT UPDATE)
UPDATE stat
SET shift_startdate_id = shifts.shift_startdate_id, shift_startinterval_id=shifts.shift_startinterval_id
FROM #fact_schedule_deviation shifts
INNER JOIN #fact_schedule_deviation stat
ON stat.date_id=shifts.date_id AND stat.interval_id=shifts.interval_id AND stat.person_id=shifts.person_id
WHERE shifts.shift_startdate_local_id IS NOT NULL
AND stat.shift_startdate_local_id IS NULL

--UPDATE ALL SHIFT ROWS WITH STATISTICS TO BE ABLE TO HANDLE OVERLAPPING SHIFTS AND DUPLICATE LOGINS(#28433)
UPDATE shifts
SET ready_time_s=stat.ready_time_s,is_logged_in=stat.is_logged_in
FROM #fact_schedule_deviation shifts
INNER JOIN
(
  SELECT date_id, interval_id, person_id, sum(ready_time_s)ready_time_s, max(is_logged_in)is_logged_in
  FROM #fact_schedule_deviation
  WHERE shift_startdate_local_id IS NULL
  GROUP BY  date_id, interval_id, person_id
) stat ON stat.date_id=shifts.date_id AND stat.interval_id=shifts.interval_id AND stat.person_id=shifts.person_id
WHERE shifts.shift_startdate_local_id IS NOT NULL 

--ALL ROWS BEFORE SHIFT WITH NO SHIFT_STARTDATE_ID TO NEAREST SHIFT +-SOMETHING 
UPDATE stat
SET shift_startdate_local_id=shifts.shift_startdate_local_id,shift_startdate_id = shifts.shift_startdate_id, shift_startinterval_id=shifts.shift_startinterval_id
FROM #fact_schedule_deviation shifts
INNER JOIN #fact_schedule_deviation stat
ON stat.date_id=shifts.date_id AND stat.person_id=shifts.person_id
WHERE stat.shift_startdate_id IS NULL 
AND stat.interval_id < shifts.shift_startinterval_id 
AND stat.interval_id >= shifts.shift_startinterval_id - @intervals_outside_shift-- ONLY 2 Hours back
AND stat.date_id <= shifts.shift_startdate_id
AND stat.date_id <= shifts.date_id --make sure the stat intervals are before shift
AND stat.interval_id <= shifts.interval_id
AND shifts.shift_startdate_local_id IS NOT NULL 


--ALL ROWS AFTER SHIFT WITH NO SHIFT_STARTDATE_ID TO NEAREST SHIFT +-SOMETHING 
UPDATE stat
SET shift_startdate_local_id=shifts.shift_startdate_local_id,shift_startdate_id = shifts.shift_startdate_id,shift_startinterval_id=shifts.shift_startinterval_id
FROM #fact_schedule_deviation shifts
INNER JOIN #fact_schedule_deviation stat
ON stat.date_id=shifts.date_id AND stat.person_id=shifts.person_id
WHERE stat.shift_startdate_id IS NULL 
AND stat.interval_id > shifts.shift_endinterval_id
AND stat.interval_id <= shifts.shift_endinterval_id + @intervals_outside_shift -- ONLY 2 Hours ahead
AND stat.date_id >= shifts.shift_startdate_id
AND stat.date_id >= shifts.date_id --make sure the stat intervals are after shift
AND stat.interval_id >= shifts.interval_id
AND shifts.shift_startdate_local_id IS NOT NULL 

DELETE FROM #fact_schedule_deviation WHERE shift_startdate_local_id IS NULL

/*Merge data*/
INSERT INTO #fact_schedule_deviation_merge
	(
	shift_startdate_local_id,
	date_id, 
	shift_startdate_id,
	shift_startinterval_id,
	interval_id,
	person_id, 
	scheduled_ready_time_s,
	ready_time_s,
	is_logged_in,
	contract_time_s,
	business_unit_id
	)
SELECT
	shift_stardate_local_id	= shift_startdate_local_id,
	date_id					= date_id, 
	shift_startdate_id		= MAX(shift_startdate_id),
	shift_startinterval_id	= max(shift_startinterval_id),
	interval_id				= interval_id,
	person_id				= person_id, 
	scheduled_ready_time_s	= sum(isnull(scheduled_ready_time_s,0)),
	ready_time_s			= sum(isnull(ready_time_s,0)),
	is_logged_in			= sum(is_logged_in), --Calculated bit value
	contract_time_s			= sum(isnull(contract_time_s,0)),
	business_unit_id		= business_unit_id
FROM 
	#fact_schedule_deviation
GROUP BY 
	shift_startdate_local_id,
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
UPDATE #fact_schedule_deviation_merge
SET
	deviation_schedule_ready_s = 0
WHERE scheduled_ready_time_s=0

--2 Deviation from schedule, i.e all scheduled time, including breaks and lunch
-- Note: You will get punished if you over perform e.i beeing logged in during breaks, lunch etc.
--		 You will also get punished if you are logged in on a interval with no scheduled_ready_time_s at all
UPDATE #fact_schedule_deviation_merge
SET deviation_schedule_s = ABS(scheduled_ready_time_s-ready_time_s)

--1 Deviation_schedule_ready, only intervals where agents are scheduled to be ready are included
-- a) Under performance
UPDATE	#fact_schedule_deviation_merge
SET	deviation_schedule_ready_s = scheduled_ready_time_s-ready_time_s
WHERE scheduled_ready_time_s>0
AND scheduled_ready_time_s>=ready_time_s

-- b) Over performance
UPDATE	#fact_schedule_deviation_merge
SET	deviation_schedule_ready_s = 0 --corrected
WHERE scheduled_ready_time_s>0
AND scheduled_ready_time_s<ready_time_s

--3 Calculated as 2) i.e punish for over performance. But only time where agents are contracted to be working are included
UPDATE #fact_schedule_deviation_merge
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

/* DELETE mart.fact_schedule_deviation */
SET NOCOUNT OFF
if @isIntraday = 0 --NIGHTLY
BEGIN
	DELETE FROM mart.fact_schedule_deviation
	WHERE shift_startdate_local_id between @start_date_id AND @end_date_id
	AND business_unit_id = @business_unit_id
END
IF @isIntraday = 1 --changed day
BEGIN
 	DELETE fs
	FROM #stg_schedule_changed ch
 	INNER JOIN mart.fact_schedule_deviation fs
		ON ch.person_id = fs.person_id
		AND ch.shift_startdate_local_id = fs.shift_startdate_local_id
END
IF @isIntraday = 2 --last 10 intervals
BEGIN --INTRADAY
	DELETE fs
	FROM #fact_schedule_deviation_merge d
	INNER JOIN mart.fact_schedule_deviation fs
		ON d.person_id = fs.person_id
		AND d.shift_startdate_local_id = fs.shift_startdate_local_id
		AND d.date_id = fs.date_id
		AND d.interval_id = fs.interval_id
END 
IF @isIntraday = 3 --changed day by SB
BEGIN
 	DELETE fs
	FROM #stg_schedule_changed ch
 	INNER JOIN mart.fact_schedule_deviation fs
		ON ch.person_id = fs.person_id
		AND ch.shift_startdate_local_id = fs.shift_startdate_local_id
END

/* Insert of new data */
INSERT INTO mart.fact_schedule_deviation
	(shift_startdate_local_id, 
	date_id, 
	interval_id, 
	person_id, 
	scheduled_ready_time_s, 
	ready_time_s, 
	contract_time_s, 
	deviation_schedule_s, 
	deviation_schedule_ready_s, 
	deviation_contract_s, 
	business_unit_id, 
	datasource_id, 
	insert_date, 
	update_date, 
	is_logged_in, 
	shift_startdate_id, 
	shift_startinterval_id
	)
SELECT
	shift_startdate_local_id	= shift_startdate_local_id, 
	date_id						= date_id, 
	interval_id					= interval_id, 
	person_id					= person_id, 
	scheduled_ready_time_s		= scheduled_ready_time_s, 
	ready_time_s				= ready_time_s, 
	contract_time_s				= contract_time_s, 
	deviation_schedule_s		= deviation_schedule_s, 
	deviation_schedule_ready_s	= deviation_schedule_ready_s, 
	deviation_contract_s		= deviation_contract_s, 
	business_unit_id			= business_unit_id, 
	datasource_id				= -1, 
	insert_date					= getdate(), 
	update_date					= getdate(),
	is_logged_in				= is_logged_in, 
	shift_startdate_id			= shift_startdate_id, 
	shift_startinterval_id		= shift_startinterval_id
FROM 
	#fact_schedule_deviation_merge


--finally
--update with now interval
IF @isIntraday = 2
BEGIN
	UPDATE [mart].[etl_job_intraday_settings]
	SET
		target_date		= @now_date_date_utc,
		target_interval	= @now_interval_id_utc
	WHERE business_unit_id = @business_unit_id
	AND detail_id = @detail_id
END
GO
