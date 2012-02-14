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
--				2010-06-04 Need to consider scenario when doing calculation
--				2010-06-10 Added detection of is_logged_in, used when ready_time_m = 0 to show zero, else empty string in report
--				2010-09-20 Fix calculation: Ready Time vs. Scheduled Ready Time. Set 100% as soon a mixed interval is fullfilled. e.g Readytime >= ScheduleReadytime
--				2010-11-01 #11055 Refact of mart.fact_schedule_deviation, measures in seconds instead of minutes. KJ
--
--ToDo: --More robust calc of Adherance 
--		ALTER TABLE mart.fact_schedule_deviation ADD
--			scheduled_time_s int NULL
-- =============================================

--exec mart.etl_fact_schedule_deviation_load '2009-02-01 23:00:00','2009-02-03 23:00:00','928DD0BC-BF40-412E-B970-9B5E015AADEA' --Demo
CREATE PROCEDURE [mart].[etl_fact_schedule_deviation_load]
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier
AS

DECLARE @start_date_id int
DECLARE @end_date_id int
DECLARE @max_date_id int
DECLARE @min_date_id int
DECLARE @business_unit_id int
DECLARE @scenario_id int

CREATE TABLE #fact_schedule_deviation(
	date_id int,
	interval_id smallint,
	person_id int,
	scheduled_ready_time_s int default 0,
--	scheduled_time_s int default 0,
	ready_time_s int default 0,
	is_logged_in int default 0, 
	contract_time_s int default 0,
	business_unit_id int
)

CREATE TABLE #fact_schedule (
	[schedule_date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL,
	[scheduled_ready_time_m] [int] NULL,
	[scheduled_contract_time_m] [int] NULL,
	[business_unit_id] [int] NULL
)


/*Remove timestamp from datetime*/
SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,8),@start_date )))
SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,8),@end_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

SET @scenario_id	=	(SELECT scenario_id FROM mart.dim_scenario WHERE business_unit_code = @business_unit_code AND default_scenario = 1)

/*Get max and min date from fact_agent(can not calculate deviation unless data here)*/
--SELECT
--	@max_date_id= max(date_id),
--	@min_date_id= min(date_id)
--FROM
--	mart.fact_agent

/*Change date interval accordning to min and max date in fact_agent*/
--SET	@start_date_id = CASE WHEN @min_date_id > @start_date_id THEN @min_date_id ELSE @start_date_id END
--SET	@end_date_id	= CASE WHEN @max_date_id < @end_date_id	THEN @max_date_id ELSE @end_date_id	END

/*Get business unit id*/
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

/*Remove old data*/
DELETE FROM mart.fact_schedule_deviation
WHERE date_id between @start_date_id AND @end_date_id
	AND business_unit_id = @business_unit_id


--20090707 KJ
/*MAKE SURE THERE IS ONLY ONE ROW PER INTERVAL(IF SEVERAL ACTIVITIES PER INTERVAL)*/
INSERT INTO #fact_schedule
SELECT
	 fs.schedule_date_id, 
	 fs.interval_id,
	 fs.person_id, 
	sum(isnull(fs.scheduled_ready_time_m,0)) 'scheduled_ready_time_m',
--	sum(isnull(fs.scheduled_time_m,0))'scheduled_time_m',
	sum(isnull(fs.scheduled_contract_time_m,0))'scheduled_contract_time_m',
	fs.business_unit_id
FROM 
	mart.fact_schedule fs
WHERE fs.schedule_date_id BETWEEN @start_date_id AND @end_date_id
	AND fs.business_unit_id = @business_unit_id
	AND fs.scenario_id = @scenario_id
GROUP BY 
	fs.schedule_date_id, 
	fs.interval_id,
	fs.person_id,
	fs.business_unit_id



/* Prepare insert of new data in two steps, a) and b). */
/* a) Gather agent ready time */
INSERT INTO #fact_schedule_deviation
	(
	date_id, 
	interval_id,
	person_id, 
	ready_time_s,
	is_logged_in,
	business_unit_id
	)
SELECT
	date_id					= fa.date_id, 
	interval_id				= fa.interval_id,
	person_id				= b.person_id, 
	ready_time_s			= fa.ready_time_s,
	is_logged_in			= 1, --marks that we do have logged in time
	business_unit_id		= b.business_unit_id
FROM 
	mart.bridge_acd_login_person b
JOIN
	mart.fact_agent fa
ON
	b.acd_login_id = fa.acd_login_id
INNER JOIN 
	mart.DimPersonAdapted() p
ON 
	p.person_id = b.person_id
	AND
		(
			(fa.date_id > p.valid_from_date_id AND fa.date_id < p.valid_to_date_id)
				OR (fa.date_id = p.valid_from_date_id AND fa.interval_id >= p.valid_from_interval_id)
				OR (fa.date_id = p.valid_to_date_id AND fa.interval_id <= p.valid_to_interval_id)
		)
WHERE
	fa.date_id BETWEEN @start_date_id AND @end_date_id
	AND b.business_unit_id = @business_unit_id

/* b) Gather agent schedule time */
INSERT INTO #fact_schedule_deviation
	(
	date_id, 
	interval_id,
	person_id,
	is_logged_in, 
	scheduled_ready_time_s,
--	scheduled_time_s,
	contract_time_s,
	business_unit_id
	)
SELECT
	date_id					= fs.schedule_date_id, 
	interval_id				= fs.interval_id,
	person_id				= fs.person_id,
	is_logged_in			= 0, --Mark schedule rows as Not loggged in 
	scheduled_ready_time_s	= fs.scheduled_ready_time_m*60,
--	scheduled_time_s		= fs.scheduled_time_m*60,
	contract_time_s			= fs.scheduled_contract_time_m*60,
	business_unit_id		= fs.business_unit_id
FROM 
	#fact_schedule fs
INNER JOIN 
	mart.DimPersonAdapted() p
ON
	p.person_id = fs.person_id
	AND
		(
			(fs.schedule_date_id > p.valid_from_date_id AND fs.schedule_date_id < p.valid_to_date_id)
				OR (fs.schedule_date_id = p.valid_from_date_id AND fs.interval_id >= p.valid_from_interval_id)
				OR (fs.schedule_date_id = p.valid_to_date_id AND fs.interval_id <= p.valid_to_interval_id)
		)
WHERE
	fs.schedule_date_id BETWEEN @start_date_id AND @end_date_id
	AND fs.business_unit_id = @business_unit_id

/* Insert of new data */
INSERT INTO mart.fact_schedule_deviation
	(
	date_id, 
	interval_id,
	person_id, 
	scheduled_ready_time_s,
--	scheduled_time_s,
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
	interval_id				= interval_id,
	person_id				= person_id, 
	scheduled_ready_time_s	= sum(isnull(scheduled_ready_time_s,0)),
--	scheduled_time_s		= sum(isnull(scheduled_time_s,0)),
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
UPDATE	mart.fact_schedule_deviation
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

--3 Deviation_contract, only time where agents are contracted to be working are included
-- a + b) Under performance and Over Performance calculated in same way
UPDATE	mart.fact_schedule_deviation
SET
	deviation_contract_s = 0
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND contract_time_s=0

UPDATE mart.fact_schedule_deviation
SET deviation_contract_s =
	CASE
		WHEN ready_time_s > contract_time_s THEN ABS(contract_time_s-scheduled_ready_time_s) --This one is a bit odd, check Excel to understand/see it working!
		ELSE ABS(scheduled_ready_time_s-ready_time_s) --same way as 1)
	END
WHERE mart.fact_schedule_deviation.date_id BETWEEN @start_date_id AND @end_date_id
AND contract_time_s>0

GO