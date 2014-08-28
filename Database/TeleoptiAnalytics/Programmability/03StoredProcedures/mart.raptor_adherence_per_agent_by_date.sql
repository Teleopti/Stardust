IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_adherence_per_agent_by_date]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_adherence_per_agent_by_date]
GO

-- ======================================================================================================
-- Author:		Xinfeng
-- Create date: 2014-07-22
-- Update date: 
-- Description:	Gets the agent whose adherence over @threshold during @Date for specify @time_zone_code
-- Example: EXEC [mart].[raptor_adherence_per_agent_by_date]
--               @threshold=0.58, -- 58%
--               @local_date='2014-05-29',
--               @adherence_id=2,
--               @time_zone_code='UTC';
-- ======================================================================================================

CREATE PROCEDURE [mart].[raptor_adherence_per_agent_by_date]
@local_date smalldatetime,
@threshold decimal(3, 2),
@adherence_id int, --1, 2 or 3 from adherence_calculation table
@time_zone_code nvarchar(50)
AS
Begin

SET NOCOUNT ON 

------------
--Create all needed temp tables. Just for performance
------------
CREATE TABLE #fact_schedule_deviation_raw (
	shift_startdate_local_id int,
	shift_startdate_id INT,
	shift_startinterval_id int,
	date_id INT,
	interval_id INT,
	person_id INT,
	deviation_schedule_ready_s INT,
	deviation_schedule_s INT,
	deviation_contract_s INT,
	ready_time_s INT,
	is_logged_in INT,
	contract_time_s INT
)

CREATE CLUSTERED INDEX #CIX_fact_schedule_deviation_raw ON #fact_schedule_deviation_raw (
	[shift_startdate_id] ASC,
	[shift_startinterval_id] ASC
)

CREATE TABLE #fact_schedule_deviation (
	shift_startdate_local_id int,--NEW 20140218
	shift_startdate_id INT, --NEW 20130111
	shift_startinterval_id int,--NEW 20130111
	date_id INT,
	interval_id INT,
	person_id INT,
	deviation_schedule_ready_s INT,
	deviation_schedule_s INT,
	deviation_contract_s INT,
	ready_time_s INT,
	is_logged_in INT,
	contract_time_s INT
)

CREATE CLUSTERED INDEX #CIX_fact_schedule_deviation ON #fact_schedule_deviation (
	[shift_startdate_id] ASC,
	[shift_startinterval_id] ASC
)

CREATE TABLE #minmax (
	[minint] [int] NULL,
	[maxint] [int] NULL,
	[person_id] [int] NULL,
	[date]	[datetime] NULL,
	[shift_startdate_id] int,
	date_interval_min [datetime] NULL,
	date_interval_max [datetime] NULL
)

CREATE TABLE #team_adh_tot (
	[adherence_calc_s] [decimal](38, 3) NULL,
	[deviation_s] [decimal](38, 3) NULL
)

CREATE TABLE #team_adh (
	[interval_id] [int] NULL,
	[adherence_calc_s] [decimal](38, 3) NULL,
	[deviation_s] [decimal](38, 3) NULL
)

CREATE TABLE #person_adh (
	[date_id] [int] NULL,
	[person_id] [int] NULL,
	[adherence_calc_s] [decimal](38, 3) NULL,
	[deviation_s] [decimal](38, 3) NULL
)

CREATE TABLE #result (
	shift_startdate_local_id int,
	shift_startdate_id int, --new 20130111
	shift_startdate datetime,--new 20130111
	date_id int,
	date datetime,
	interval_id int,
	interval_name nvarchar(20),
	intervals_per_day int,
	site_id int,
	site_name nvarchar(100),
	team_id int,
	team_name nvarchar(100),
	person_code uniqueidentifier,
	person_id int,
	person_first_name nvarchar(30),
	person_last_name nvarchar(30),
	person_name	nvarchar(200),
	adherence decimal(18,3),
	adherence_tot decimal(18,3),
	deviation_s decimal(18,3),
	deviation_tot_s decimal(18,3),
	ready_time_s decimal(18,3),
	is_logged_in bit not null,
	activity_id int,
	absence_id int,
	display_color int,
	activity_absence_name nvarchar(100),
	adherence_calc_s decimal(18,3),
	team_adherence decimal(18,3),
	team_adherence_tot decimal(18,3),
	team_deviation_s decimal(18,3),
	team_deviation_tot_s decimal(18,3),
	adherence_type_selected nvarchar(100),
	hide_time_zone bit,
	count_activity_per_interval int,
	date_interval_counter int, --new 20130117
	person_min_shiftstart_interval int,
	person_max_shiftend_interval int, 
	shift_interval_id int
)

CREATE TABLE #counter (
	shift_startdate_local_id int,
	date_id int,
	interval_id smallint,
	date_interval_counter int IDENTITY(1,1)
)

--NEW 20130111
CREATE TABLE #fact_schedule_raw (
	shift_startdate_local_id int,
	shift_startdate_id INT,
	shift_startinterval_id int,
	schedule_date_id INT,
	interval_id INT,
	person_id INT,
	activity_id int,
	absence_id int,
	scheduled_time_m INT,
	scheduled_ready_time_m INT
)

CREATE TABLE #fact_schedule (
	shift_startdate_local_id int,
	shift_startdate_id INT, --NEW 20130111
	shift_startinterval_id int,--NEW 20130111
	schedule_date_id INT,
	interval_id INT,
	person_id INT,
	activity_id int,
	absence_id int,
	scheduled_time_s INT,
	scheduled_ready_time_s INT,
	count_activity_per_interval int
)

--Table to hold agents to view
CREATE TABLE #person_id (
	person_id int
)

CREATE TABLE #bridge_time_zone (
	local_date_id int,
	local_interval_id int,
	date_id int,
	interval_id int,
	date_date datetime --new 20130111
)

------------
--Declares
------------
DECLARE @intervals_per_day INT
DECLARE @intervals_length_s INT
DECLARE @hide_time_zone bit
DECLARE @selected_adherence_type nvarchar(100)
DECLARE @date TABLE (date_from_id INT, date_to_id INT)
DECLARE @nowutcDateOnly smalldatetime
DECLARE @nowutcInterval smalldatetime
DECLARE @nowLocalDateId int
DECLARE @nowLocalIntervalId smallint
DECLARE @minUtcDateId int
DECLARE @maxUtcDateId int

SELECT @nowutcDateOnly = DATEADD(dd, 0, DATEDIFF(dd, 0, GETUTCDATE()))
SELECT @nowutcInterval = DATEADD(minute,DATEPART(minute, GETUTCDATE()),DATEADD(hour, DATEPART(hour, GETUTCDATE()),'1900-01-01 00:00:00'))

------------
--Init
------------
SELECT @selected_adherence_type= adherence_name FROM mart.adherence_calculation WHERE adherence_id=@adherence_id
SELECT @intervals_per_day = COUNT(interval_id) FROM mart.dim_interval

--handle empty Analytics
IF @intervals_per_day = 0 SET @intervals_per_day=96

--how many seconds per interval
SELECT @intervals_length_s = 1440*60/@intervals_per_day

--Get needed dates and intervals from bridge time zone into temp table
INSERT INTO #bridge_time_zone
SELECT 
	local_date_id		= d.date_id,
	local_interval_id	= i.interval_id,
	date_id				= b.date_id,
	interval_id			= b.interval_id,
	date_date			= d.date_date		
FROM mart.bridge_time_zone b
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date = @local_date
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
INNER JOIN mart.dim_time_zone t
	ON b.time_zone_id = t.time_zone_id
WHERE t.time_zone_code = @time_zone_code

--Get the min/max UTC date_id
SELECT
	@minUtcDateId = MIN(b.date_id),
	@maxUtcDateId = MAX(b.date_id)
FROM #bridge_time_zone b

--Get Now() in local date/interval Id variables
SELECT
	@nowLocalDateId = b.local_date_id,
	@nowLocalIntervalId = b.local_interval_id
FROM bridge_time_zone b
INNER JOIN mart.dim_date d 
	ON b.date_id = d.date_id
	AND d.date_date = @nowUtcDateOnly
INNER JOIN mart.dim_interval i
	ON b.interval_id = i.interval_id
	AND @nowUtcInterval between i.interval_start and i.interval_end
INNER JOIN mart.dim_time_zone t
	ON b.time_zone_id = t.time_zone_id
WHERE t.time_zone_code = @time_zone_code

--Multiple Time zones?
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

-----------
--Start
-----------

INSERT INTO #person_id
select distinct
       p.person_id
  From mart.dim_person p
 inner join mart.bridge_time_zone tz
    on tz.time_zone_id = p.time_zone_id
 inner join mart.dim_date d
    on tz.local_date_id = d.date_id
 INNER JOIN mart.dim_time_zone t
	ON tz.time_zone_id = t.time_zone_id
 where d.date_date = @local_date
   and t.time_zone_code = @time_zone_code;

--Create UTC table from: mart.fact_schedule_deviation
INSERT INTO #fact_schedule_deviation_raw(shift_startdate_local_id,shift_startdate_id,shift_startinterval_id,date_id,interval_id,person_id,deviation_schedule_ready_s,deviation_schedule_s,deviation_contract_s,ready_time_s,is_logged_in,contract_time_s)
SELECT 
	fsd.shift_startdate_local_id,--new20140218
	fsd.shift_startdate_id,--new20130111
	fsd.shift_startinterval_id,--new20130111
	fsd.date_id,
	fsd.interval_id,
	fsd.person_id,
	deviation_schedule_ready_s,
	deviation_schedule_s,
	deviation_contract_s,
	ready_time_s,
	is_logged_in,
	contract_time_s
FROM mart.fact_schedule_deviation fsd
WHERE fsd.date_id between @minUtcDateId-1 AND @maxUtcDateId+1
AND fsd.person_id IN (SELECT person_id FROM #person_id)

INSERT INTO #fact_schedule_deviation(shift_startdate_local_id,shift_startdate_id,shift_startinterval_id,date_id,interval_id,person_id,deviation_schedule_ready_s,deviation_schedule_s,deviation_contract_s,ready_time_s,is_logged_in,contract_time_s)
SELECT 
	fsd.shift_startdate_local_id,
	fsd.shift_startdate_id,--new20130111
	fsd.shift_startinterval_id,--new20130111
	fsd.date_id,
	fsd.interval_id,
	fsd.person_id,
	deviation_schedule_ready_s,
	deviation_schedule_s,
	deviation_contract_s,
	ready_time_s,
	is_logged_in,
	contract_time_s
FROM #fact_schedule_deviation_raw fsd
INNER JOIN #bridge_time_zone b
	ON	fsd.shift_startinterval_id= b.interval_id
	AND fsd.shift_startdate_id=b.date_id --NEW 20130111

--Get all fact_schedule-data for the day in question.
--Note: local date e.g. incl. time zone
--step 1 get raw data for speed
INSERT #fact_schedule_raw(shift_startdate_local_id,shift_startdate_id,shift_startinterval_id,schedule_date_id,interval_id,person_id,activity_id,absence_id,scheduled_time_m,scheduled_ready_time_m)
SELECT 
		shift_startdate_local_id,
		shift_startdate_id,
		shift_startinterval_id,
		schedule_date_id,
		fs.interval_id,
		fs.person_id,
		activity_id,
		absence_id,
		scheduled_time_m,
		scheduled_ready_time_m
FROM 
	mart.fact_schedule fs
--INNER JOIN #person_id a
--	ON fs.person_id = a.person_id
INNER JOIN #bridge_time_zone b
   ON fs.shift_startinterval_id= b.interval_id
  AND fs.shift_startdate_id = b.date_id
INNER JOIN mart.dim_business_unit bu
   ON fs.business_unit_id = bu.business_unit_id
INNER JOIN mart.dim_scenario sc
   ON sc.business_unit_id = bu.business_unit_id
  AND fs.scenario_id = sc.scenario_id
  AND sc.default_scenario = 1
WHERE fs.person_id in(SELECT person_id FROM #person_id)

INSERT #fact_schedule(shift_startdate_local_id,shift_startdate_id,shift_startinterval_id,schedule_date_id,interval_id,person_id,scheduled_time_s,scheduled_ready_time_s,count_activity_per_interval)
SELECT
	fs.shift_startdate_local_id,
	fs.shift_startdate_id,
	fs.shift_startinterval_id,
	fs.schedule_date_id,
	fs.interval_id,
	fs.person_id,
	SUM(fs.scheduled_time_m)*60,
	SUM(fs.scheduled_ready_time_m)*60,
	COUNT(fs.interval_id)		
FROM #fact_schedule_raw fs
--INNER JOIN #person_id a
--	ON fs.person_id = a.person_id
INNER JOIN #bridge_time_zone b
	ON	fs.shift_startinterval_id= b.interval_id
	AND fs.shift_startdate_id= b.date_id
	Where fs.person_id in(SELECT person_id FROM  #person_id)
GROUP BY fs.shift_startdate_local_id,fs.shift_startdate_id,fs.shift_startinterval_id,fs.schedule_date_id,fs.person_id,fs.interval_id

--Update with activity.
--a) In case there are multiple activities per interval, we use activities where in_ready_time = 1
UPDATE #fact_schedule
SET activity_id= fs.activity_id
FROM #fact_schedule_raw fs
	INNER JOIN #fact_schedule SchTemp
	ON SchTemp.schedule_date_id=fs.schedule_date_id
	AND SchTemp.person_id=fs.person_id
	AND SchTemp.interval_id=fs.interval_id
	AND SchTemp.shift_startdate_local_id=fs.shift_startdate_local_id--in case overlapping shifts
INNER JOIN mart.dim_activity a
	ON a.activity_id=fs.activity_id
WHERE a.in_ready_time=1

-- b) in case there is no ready time; just take any one
UPDATE #fact_schedule
SET activity_id= fs.activity_id
FROM #fact_schedule_raw fs
	INNER JOIN #fact_schedule SchTemp
	ON SchTemp.schedule_date_id=fs.schedule_date_id
	AND SchTemp.person_id=fs.person_id
	AND SchTemp.interval_id=fs.interval_id
	AND SchTemp.shift_startdate_local_id=fs.shift_startdate_local_id--in case overlapping shifts
INNER JOIN mart.dim_activity a
	ON a.activity_id=fs.activity_id
WHERE SchTemp.activity_id IS NULL

--Update with absence code
UPDATE #fact_schedule
SET absence_id= fs.absence_id
FROM #fact_schedule_raw fs
INNER JOIN #fact_schedule SchTemp
	ON SchTemp.schedule_date_id=fs.schedule_date_id
	AND SchTemp.person_id=fs.person_id
	AND SchTemp.interval_id=fs.interval_id
	AND SchTemp.shift_startdate_local_id=fs.shift_startdate_local_id--in case overlapping shifts
WHERE SchTemp.absence_id IS NULL

--Start creating the result set
--a) insert agent statistics matching scheduled time
INSERT #result(shift_startdate_local_id,shift_startdate_id,shift_startdate,date_id,date,interval_id,interval_name,intervals_per_day,site_id,site_name,team_id,team_name,person_code,person_id,
person_first_name,person_last_name,person_name,deviation_s,ready_time_s,is_logged_in,activity_id,absence_id,adherence_calc_s,
adherence_type_selected,hide_time_zone,count_activity_per_interval)
	SELECT	fs.shift_startdate_local_id,
			b1.date_id, --shift_startdate_id
			b1.date_date,
			d.date_id,
			d.date_date,
			i.interval_id,
			i.interval_name,
			@intervals_per_day,
			p.site_id,
			p.site_name,
			p.team_id,
			p.team_name,
			p.person_code,
			p.person_id,
			p.first_name,
			p.last_name,
			p.person_name,
			CASE @adherence_id 
				WHEN 1 THEN  isnull(fsd.deviation_schedule_ready_s,0)
				WHEN 2 THEN  isnull(fsd.deviation_schedule_s,0)
				WHEN 3 THEN isnull(fsd.deviation_contract_s,0)
			END AS 'deviation_s',
			isnull(fsd.ready_time_s,0) 'ready_time_s',
			isnull(fsd.is_logged_in,0) as 'is_logged_in',
			isnull(fs.activity_id,-1), --isnull = not defined
			isnull(fs.absence_id,-1), --isnull = not defined
			CASE @adherence_id 
				WHEN 1 THEN isnull(fs.scheduled_ready_time_s,0)
				WHEN 2 THEN isnull(fs.scheduled_time_s,0)
				WHEN 3 THEN isnull(fsd.contract_time_s,0)
			END AS 'adherence_calc_s',
			@selected_adherence_type,
			@hide_time_zone,
			isnull(count_activity_per_interval,2) --fake a mixed shift = white color	
	FROM mart.dim_person p
	INNER JOIN #fact_schedule fs
		ON fs.person_id=p.person_id
	LEFT JOIN #fact_schedule_deviation fsd
		ON fsd.person_id=fs.person_id
		AND fsd.date_id=fs.schedule_date_id
		AND fsd.interval_id=fs.interval_id
		AND fsd.shift_startdate_local_id=fs.shift_startdate_local_id
	INNER JOIN #bridge_time_zone b1
		ON	fs.shift_startinterval_id= b1.interval_id
		AND fs.shift_startdate_id=b1.date_id
	INNER JOIN bridge_time_zone b2
		ON	fs.interval_id= b2.interval_id
		AND fs.schedule_date_id= b2.date_id
	INNER JOIN mart.dim_interval i
		ON b2.local_interval_id = i.interval_id			
	INNER JOIN mart.dim_date d 
		ON b2.local_date_id = d.date_id
	INNER JOIN mart.dim_time_zone t
		ON t.time_zone_code = @time_zone_code 
		AND b2.time_zone_id = t.time_zone_id

--b) insert agent statistics outside shift
INSERT #result(shift_startdate_local_id,shift_startdate_id,shift_startdate,date_id,date,interval_id,interval_name,intervals_per_day,site_id,site_name,team_id,team_name,person_code,person_id,
person_first_name,person_last_name,person_name,deviation_s,ready_time_s,is_logged_in,activity_id,absence_id,adherence_calc_s,
adherence_type_selected,hide_time_zone,count_activity_per_interval)
	SELECT	fsd.shift_startdate_local_id,
			b1.date_id,
			b1.date_date,
			d.date_id,
			d.date_date,
			i.interval_id,
			i.interval_name,
			@intervals_per_day,
			p.site_id,
			p.site_name,
			p.team_id,
			p.team_name,
			p.person_code,
			p.person_id,
			p.first_name,
			p.last_name,
			p.person_name,
			CASE @adherence_id 
				WHEN 1 THEN  isnull(fsd.deviation_schedule_ready_s,0)
				WHEN 2 THEN  isnull(fsd.deviation_schedule_s,0)
				WHEN 3 THEN isnull(fsd.deviation_contract_s,0)
			END AS 'deviation_s',
			isnull(fsd.ready_time_s,0) 'ready_time_s',
			isnull(fsd.is_logged_in,0) as 'is_logged_in',
			-1, --isnull = not defined
			-1, --isnull = not defined
			CASE @adherence_id 
				WHEN 1 THEN 0
				WHEN 2 THEN 0
				WHEN 3 THEN isnull(fsd.contract_time_s,0)
			END AS 'adherence_calc_s',
			@selected_adherence_type,
			@hide_time_zone,
			2 --fake a mixed shift = white color	
	FROM mart.dim_person p
	INNER JOIN #fact_schedule_deviation fsd
		ON fsd.person_id=p.person_id
	INNER JOIN #bridge_time_zone b1
		ON	fsd.shift_startinterval_id= b1.interval_id
		AND fsd.shift_startdate_id=b1.date_id
	INNER JOIN bridge_time_zone b2
		ON	fsd.interval_id= b2.interval_id
		AND fsd.date_id= b2.date_id
	INNER JOIN mart.dim_interval i
		ON b2.local_interval_id = i.interval_id			
	INNER JOIN mart.dim_date d 
		ON b2.local_date_id = d.date_id
	INNER JOIN mart.dim_time_zone t
		ON t.time_zone_code = @time_zone_code
		AND b2.time_zone_id= t.time_zone_id
	WHERE NOT EXISTS (SELECT 1 FROM #result r where r.person_id=fsd.person_id and r.interval_id=i.interval_id and r.date_id=d.date_id)--WHERE NO MATCH ON SCHEDULE

----------
--remove deviation and schedule_ready_time for every interval > Now(), but keep color and activity for display
----------
update #result
set
	adherence_calc_s=NULL,
	deviation_s = NULL
where 	date_id > @nowLocalDateId 

update #result
set
	adherence_calc_s=NULL,
	deviation_s = NULL
where 	date_id = @nowLocalDateId 
and interval_id > @nowLocalIntervalId

----------
--calculation of Agent adherence, team adherence
----------
--per person and interval
UPDATE #result
SET adherence=
CASE
		WHEN adherence_calc_s = 0 AND @adherence_id <> 2 THEN 1
		WHEN adherence_calc_s = 0 AND @adherence_id = 2 THEN 0
		WHEN deviation_s > adherence_calc_s THEN 0
		ELSE (adherence_calc_s - deviation_s)/ adherence_calc_s
	END
FROM #result

--per person total
INSERT INTO #person_adh
SELECT shift_startdate_local_id,person_id,sum(adherence_calc_s)'adherence_calc_s',sum(deviation_s)'deviation_s'
FROM #result
GROUP by shift_startdate_local_id,person_id

UPDATE #result
SET adherence_tot=
	CASE
		WHEN a.adherence_calc_s = 0 AND @adherence_id <> 2 THEN 1
		WHEN a.adherence_calc_s = 0 AND @adherence_id = 2 THEN 0
		WHEN a.deviation_s > a.adherence_calc_s THEN 0
		ELSE (a.adherence_calc_s - a.deviation_s )/ a.adherence_calc_s
	END,
deviation_tot_s=a.deviation_s
FROM #person_adh a
INNER JOIN #result r ON r.shift_startdate_local_id=a.date_id AND r.person_id=a.person_id

--per interval
INSERT INTO #team_adh
SELECT interval_id,sum(adherence_calc_s)'adherence_calc_s',sum(deviation_s)'deviation_s'
FROM #result
GROUP by interval_id

UPDATE #result
SET team_adherence=
	CASE
		WHEN a.adherence_calc_s = 0 AND @adherence_id <> 2 THEN 1
		WHEN a.adherence_calc_s = 0 AND @adherence_id = 2 THEN 0
		WHEN a.deviation_s > a.adherence_calc_s THEN 0
		ELSE (a.adherence_calc_s - a.deviation_s )/ a.adherence_calc_s
	END
,team_deviation_s=a.deviation_s
FROM #team_adh a
INNER JOIN #result r ON r.interval_id=a.interval_id

--total(for all selected)
INSERT INTO #team_adh_tot
SELECT sum(adherence_calc_s)'adherence_calc_s',sum(deviation_s)'deviation_s'
FROM #result

UPDATE #result
SET team_adherence_tot=
	CASE
		WHEN a.adherence_calc_s = 0 AND @adherence_id <> 2 THEN 1
		WHEN a.adherence_calc_s = 0 AND @adherence_id = 2 THEN 0
		WHEN a.deviation_s > a.adherence_calc_s THEN 0
		ELSE (a.adherence_calc_s - a.deviation_s )/ a.adherence_calc_s
	END
,team_deviation_tot_s=a.deviation_s
FROM #team_adh_tot a

/*Set display color and name on activity or absence*/
UPDATE #result
SET display_color=a.display_color,activity_absence_name=absence_name
FROM mart.dim_absence a 
INNER JOIN #result r
	ON r.absence_id=a.absence_id AND r.activity_id = -1

UPDATE #result
SET display_color=a.display_color,activity_absence_name=activity_name
FROM mart.dim_activity a 
INNER JOIN #result r
	ON r.activity_id=a.activity_id and r.absence_id = -1

--If more the one activity per interval, then we display the "not defined" color
UPDATE #result
SET display_color=a.display_color,activity_absence_name=activity_name
FROM #result  r
INNER JOIN mart.dim_activity a on a.activity_id =-1
WHERE r.count_activity_per_interval >1

--maximum and minimum intervals check
update #result
set shift_interval_id= interval_id

update #result
set shift_interval_id= interval_id+intervals_per_day
where shift_startdate_id<date_id

INSERT INTO #minmax(minint,maxint,person_id,shift_startdate_id)
SELECT  min(r.shift_interval_id) minint ,max(r.shift_interval_id) maxint, person_id, shift_startdate_id
FROM #result r
GROUP BY shift_startdate_id,person_id

UPDATE #result 
SET person_min_shiftstart_interval= minint
FROM #minmax 
INNER JOIN #result ON #result.shift_startdate_id=#minmax.shift_startdate_id AND #result.person_id=#minmax.person_id

UPDATE #result 
SET person_max_shiftend_interval= maxint
FROM #minmax 
INNER JOIN #result ON #result.shift_startdate_id=#minmax.shift_startdate_id AND #result.person_id=#minmax.person_id

--add unique id per date_id and interval_id
INSERT #counter(shift_startdate_local_id,date_id,interval_id)
select shift_startdate_local_id,date_id,interval_id
from #result
order by shift_startdate_local_id,date_id,interval_id

update #result
set date_interval_counter=c.date_interval_counter
from #counter c
inner join #result r
	on r.date_id=c.date_id
	AND r.interval_id=c.interval_id
	AND r.shift_startdate_local_id = c.shift_startdate_local_id

select person_code, [date], adherence_tot as Adherence
  from #result
 where adherence_tot > @threshold
   and date = @local_date
 group by person_code, [date], adherence_tot
end
