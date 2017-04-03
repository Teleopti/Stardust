IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_adherence_per_agent_by_date]') AND type in (N'P', N'PC'))
   DROP PROCEDURE [mart].[raptor_adherence_per_agent_by_date]
GO

-- ======================================================================================================
-- Author:      Xinfeng
-- Create date: 2014-07-22
-- Example: EXEC [mart].[raptor_adherence_per_agent_by_date]
--               @threshold=0.58, --58%
--               @local_date='2014-05-29',
--               @adherence_id=2,
--               @time_zone_code='UTC';
--               @business_unit_code = '48C43F5E-EB5A-485B-82FF-A1B000B75497'
-- ======================================================================================================

CREATE PROCEDURE [mart].[raptor_adherence_per_agent_by_date]
    @local_date smalldatetime,
    @threshold decimal(3, 2),
    @adherence_id int, --1, 2 or 3 from adherence_calculation table
    @time_zone_code nvarchar(50),
    @business_unit_code uniqueidentifier
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
    utc_date_id INT,
    utc_interval_id INT,
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
    shift_startdate_local_id int,
    shift_startdate_id INT,
    shift_startinterval_id int,
    utc_date_id INT,
    utc_interval_id INT,
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

CREATE TABLE #person_adh (
    [person_id] int NULL,
    [person_code] uniqueidentifier NULL,
    [adherence_calc_s_tot] [decimal](38, 3) NULL,
    [adherence_tot] decimal(18,3),
    [deviation_s_tot] decimal(18,3)
)

CREATE TABLE #adherence_detail (
    shift_startdate_local_id int,
    shift_startdate_id int,
    shift_startdate datetime,
    date_id int,
    [date] datetime,
    interval_id int,
    interval_name nvarchar(20),
    intervals_per_day int,
    person_code uniqueidentifier,
    person_id int,
    adherence decimal(18,3),
    adherence_tot decimal(18,3),
    deviation_s decimal(18,3),
    deviation_tot_s decimal(18,3),
    adherence_calc_s decimal(18,3),
    adherence_type_selected nvarchar(100)
)

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
    shift_startdate_id INT,
    shift_startinterval_id int,
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
    utc_date_id int,
    utc_interval_id int,
    date_date datetime
)

------------
--Declares
------------
DECLARE @intervals_per_day INT
DECLARE @selected_adherence_type nvarchar(100)
DECLARE @nowutcDateOnly smalldatetime
DECLARE @nowutcInterval smalldatetime
DECLARE @nowLocalDateId int
DECLARE @nowLocalIntervalId smallint
DECLARE @minLocalDateId int
DECLARE @maxLocalDateId int

SELECT @nowutcDateOnly = DATEADD(dd, 0, DATEDIFF(dd, 0, GETUTCDATE()))
SELECT @nowutcInterval = DATEADD(minute,DATEPART(minute, GETUTCDATE()),DATEADD(hour, DATEPART(hour, GETUTCDATE()),'1900-01-01 00:00:00'))

------------
--Init
------------
SELECT @selected_adherence_type = adherence_name FROM [mart].[adherence_calculation] WHERE adherence_id = @adherence_id
SELECT @intervals_per_day = COUNT(interval_id) FROM [mart].[dim_interval]

--handle empty Analytics
IF @intervals_per_day = 0 SET @intervals_per_day = 96

--Get needed dates and intervals from bridge time zone into temp table
INSERT INTO #bridge_time_zone
SELECT local_date_id       = d.date_id,
       local_interval_id   = i.interval_id,
       utc_date_id         = b.date_id,
       utc_interval_id     = b.interval_id,
       date_date           = d.date_date
  FROM [mart].[bridge_time_zone] b
 INNER JOIN [mart].[dim_date] d
    ON b.local_date_id = d.date_id
   AND d.date_date = @local_date
 INNER JOIN [mart].[dim_interval] i
    ON b.local_interval_id = i.interval_id
 INNER JOIN [mart].[dim_time_zone] t
    ON b.time_zone_id = t.time_zone_id
 WHERE t.time_zone_code = @time_zone_code

--Get the min/max local date_id
SELECT @minLocalDateId = MIN(b.local_date_id),
       @maxLocalDateId = MAX(b.local_date_id)
  FROM #bridge_time_zone b

--Get Now() in local date/interval Id variables
SELECT @nowLocalDateId     = b.local_date_id,
       @nowLocalIntervalId = b.local_interval_id
  FROM [mart].[bridge_time_zone] b
 INNER JOIN [mart].[dim_date] d
    ON b.date_id = d.date_id
   AND d.date_date = @nowUtcDateOnly
 INNER JOIN [mart].[dim_interval] i
    ON b.interval_id = i.interval_id
   AND @nowUtcInterval between i.interval_start and i.interval_end
 INNER JOIN [mart].[dim_time_zone] t
    ON b.time_zone_id = t.time_zone_id
 WHERE t.time_zone_code = @time_zone_code

-----------
--Start
-----------
INSERT INTO #person_id
SELECT DISTINCT p.person_id
  From [mart].[dim_person] p WITH (NOLOCK)
 INNER JOIN [mart].[bridge_time_zone] tz
    ON tz.time_zone_id = p.time_zone_id
 INNER JOIN [mart].[dim_date] d
    ON tz.local_date_id = d.date_id
   AND d.date_date = @local_date
 INNER JOIN [mart].[dim_time_zone] t
    ON tz.time_zone_id = t.time_zone_id
 WHERE p.business_unit_code = @business_unit_code and d.date_date = @local_date
   AND t.time_zone_code = @time_zone_code
   AND @local_date BETWEEN p.valid_from_date and p.valid_to_date;

--Create UTC table from: [mart].[fact_schedule_deviation]
INSERT INTO #fact_schedule_deviation_raw(
       shift_startdate_local_id,
       shift_startdate_id,
       shift_startinterval_id,
       utc_date_id,
       utc_interval_id,
       person_id,
       deviation_schedule_ready_s,
       deviation_schedule_s,
       deviation_contract_s,
       ready_time_s,
       is_logged_in,
       contract_time_s)
SELECT fsd.shift_startdate_local_id,
       fsd.shift_startdate_id,
       fsd.shift_startinterval_id,
       fsd.date_id,
       fsd.interval_id,
       fsd.person_id,
       deviation_schedule_ready_s,
       deviation_schedule_s,
       deviation_contract_s,
       ready_time_s,
       is_logged_in,
       contract_time_s
  FROM [mart].[fact_schedule_deviation] fsd
 INNER JOIN #person_id ON fsd.person_id = #person_id.person_id
 WHERE fsd.shift_startdate_local_id between @minLocalDateId - 2 AND @maxLocalDateId + 2

INSERT INTO #fact_schedule_deviation(
       shift_startdate_local_id,
       shift_startdate_id,
       shift_startinterval_id,
       utc_date_id,
       utc_interval_id,
       person_id,
       deviation_schedule_ready_s,
       deviation_schedule_s,
       deviation_contract_s,
       ready_time_s,
       is_logged_in,
       contract_time_s)
SELECT fsd.shift_startdate_local_id,
       fsd.shift_startdate_id,
       fsd.shift_startinterval_id,
       fsd.utc_date_id,
       fsd.utc_interval_id,
       fsd.person_id,
       deviation_schedule_ready_s,
       deviation_schedule_s,
       deviation_contract_s,
       ready_time_s,
       is_logged_in,
       contract_time_s
  FROM #fact_schedule_deviation_raw fsd
 INNER JOIN #bridge_time_zone b
    ON fsd.shift_startinterval_id = b.utc_interval_id
   AND fsd.shift_startdate_id = b.utc_date_id

--Get all fact_schedule-data for the day in question.
--Note: local date e.g. incl. time zone
--step 1 get raw data for speed
INSERT #fact_schedule_raw(
       shift_startdate_local_id,
       shift_startdate_id,
       shift_startinterval_id,
       schedule_date_id,
       interval_id,
       person_id,
       activity_id,
       absence_id,
       scheduled_time_m,
       scheduled_ready_time_m)
SELECT shift_startdate_local_id,
       shift_startdate_id,
       shift_startinterval_id,
       schedule_date_id,
       fs.interval_id,
       fs.person_id,
       activity_id,
       absence_id,
       scheduled_time_m,
       scheduled_ready_time_m
  FROM [mart].[fact_schedule] fs
 INNER JOIN #person_id
    ON fs.person_id = #person_id.person_id
 INNER JOIN #bridge_time_zone b
    ON fs.shift_startdate_local_id = b.local_date_id
   AND fs.shift_startinterval_id = b.utc_interval_id
 INNER JOIN [mart].[dim_business_unit] bu
    ON fs.business_unit_id = bu.business_unit_id
 INNER JOIN [mart].[dim_scenario] sc
    ON sc.business_unit_id = bu.business_unit_id
   AND fs.scenario_id = sc.scenario_id
   AND sc.default_scenario = 1

INSERT #fact_schedule(
       shift_startdate_local_id,
       shift_startdate_id,
       shift_startinterval_id,
       schedule_date_id,
       interval_id,
       person_id,
       scheduled_time_s,
       scheduled_ready_time_s,
       count_activity_per_interval)
SELECT fs.shift_startdate_local_id,
       fs.shift_startdate_id,
       fs.shift_startinterval_id,
       fs.schedule_date_id,
       fs.interval_id,
       fs.person_id,
       SUM(fs.scheduled_time_m) * 60,
       SUM(fs.scheduled_ready_time_m) * 60,
       COUNT(fs.interval_id)
  FROM #fact_schedule_raw fs
 INNER JOIN #person_id
    ON fs.person_id = #person_id.person_id
 INNER JOIN #bridge_time_zone b
    ON fs.shift_startinterval_id = b.utc_interval_id
   AND fs.shift_startdate_id = b.utc_date_id
 GROUP BY fs.shift_startdate_local_id,fs.shift_startdate_id,fs.shift_startinterval_id,fs.schedule_date_id,fs.person_id,fs.interval_id

--Update with activity.
--a) In case there are multiple activities per interval, we use activities where in_ready_time = 1
UPDATE #fact_schedule
   SET activity_id = fs.activity_id
  FROM #fact_schedule_raw fs
 INNER JOIN #fact_schedule SchTemp
    ON SchTemp.schedule_date_id = fs.schedule_date_id
   AND SchTemp.person_id = fs.person_id
   AND SchTemp.interval_id = fs.interval_id
   AND SchTemp.shift_startdate_local_id = fs.shift_startdate_local_id --in case overlapping shifts
 INNER JOIN [mart].[dim_activity] a
    ON a.activity_id = fs.activity_id
 WHERE a.in_ready_time = 1

-- b) in case there is no ready time; just take any one
UPDATE #fact_schedule
   SET activity_id = fs.activity_id
  FROM #fact_schedule_raw fs
 INNER JOIN #fact_schedule SchTemp
    ON SchTemp.schedule_date_id = fs.schedule_date_id
   AND SchTemp.person_id = fs.person_id
   AND SchTemp.interval_id = fs.interval_id
   AND SchTemp.shift_startdate_local_id = fs.shift_startdate_local_id --in case overlapping shifts
 INNER JOIN [mart].[dim_activity] a
    ON a.activity_id = fs.activity_id
 WHERE SchTemp.activity_id IS NULL

--Update with absence code
UPDATE #fact_schedule
   SET absence_id = fs.absence_id
  FROM #fact_schedule_raw fs
 INNER JOIN #fact_schedule SchTemp
    ON SchTemp.schedule_date_id = fs.schedule_date_id
   AND SchTemp.person_id = fs.person_id
   AND SchTemp.interval_id = fs.interval_id
   AND SchTemp.shift_startdate_local_id = fs.shift_startdate_local_id --in case overlapping shifts
 WHERE SchTemp.absence_id IS NULL

--Start creating the adherence detail set
--a) insert agent statistics matching scheduled time
INSERT #adherence_detail(shift_startdate_local_id,
       shift_startdate_id,
       shift_startdate,
       date_id,
       [date],
       interval_id,
       interval_name,
       intervals_per_day,
       person_code,
       person_id,
       deviation_s,
       adherence_calc_s,
       adherence_type_selected)
SELECT fs.shift_startdate_local_id,
       b1.utc_date_id,
       b1.date_date,
       d.date_id,
       d.date_date,
       i.interval_id,
       i.interval_name,
       @intervals_per_day,
       p.person_code,
       p.person_id,
       CASE @adherence_id
           WHEN 1 THEN isnull(fsd.deviation_schedule_ready_s,0)
           WHEN 2 THEN isnull(fsd.deviation_schedule_s,0)
           WHEN 3 THEN isnull(fsd.deviation_contract_s,0)
       END AS 'deviation_s',
       CASE @adherence_id
           WHEN 1 THEN isnull(fs.scheduled_ready_time_s,0)
           WHEN 2 THEN isnull(fs.scheduled_time_s,0)
           WHEN 3 THEN isnull(fsd.contract_time_s,0)
       END AS 'adherence_calc_s',
       @selected_adherence_type
  FROM [mart].[dim_person] p WITH (NOLOCK)
 INNER JOIN #fact_schedule fs
    ON fs.person_id = p.person_id
   AND @local_date between p.valid_from_date and p.valid_to_date
  LEFT JOIN #fact_schedule_deviation fsd
    ON fsd.person_id = fs.person_id
   AND fsd.utc_date_id = fs.schedule_date_id
   AND fsd.utc_interval_id = fs.interval_id
   AND fsd.shift_startdate_local_id = fs.shift_startdate_local_id
 INNER JOIN #bridge_time_zone b1
    ON fs.shift_startinterval_id = b1.utc_interval_id
   AND fs.shift_startdate_id = b1.utc_date_id
 INNER JOIN [mart].[bridge_time_zone] b2
    ON fs.interval_id = b2.interval_id
   AND fs.schedule_date_id = b2.date_id
 INNER JOIN [mart].[dim_interval] i
    ON b2.local_interval_id = i.interval_id
 INNER JOIN [mart].[dim_date] d
    ON b2.local_date_id = d.date_id
 INNER JOIN [mart].[dim_time_zone] t
    ON t.time_zone_code = @time_zone_code
   AND b2.time_zone_id = t.time_zone_id

--b) insert agent statistics outside shift
INSERT #adherence_detail(
       shift_startdate_local_id,
       shift_startdate_id,
       shift_startdate,
       date_id,
       [date],
       interval_id,
       interval_name,
       intervals_per_day,
       person_code,
       person_id,
       deviation_s,
       adherence_calc_s,
       adherence_type_selected)
SELECT fsd.shift_startdate_local_id,
       b1.utc_date_id,
       b1.date_date,
       d.date_id,
       d.date_date,
       i.interval_id,
       i.interval_name,
       @intervals_per_day,
       p.person_code,
       p.person_id,
       CASE @adherence_id
           WHEN 1 THEN isnull(fsd.deviation_schedule_ready_s,0)
           WHEN 2 THEN isnull(fsd.deviation_schedule_s,0)
           WHEN 3 THEN isnull(fsd.deviation_contract_s,0)
       END AS 'deviation_s',
       CASE @adherence_id
           WHEN 1 THEN 0
           WHEN 2 THEN 0
           WHEN 3 THEN isnull(fsd.contract_time_s,0)
       END AS 'adherence_calc_s',
       @selected_adherence_type
  FROM [mart].[dim_person] p WITH (NOLOCK)
 INNER JOIN #fact_schedule_deviation fsd
    ON fsd.person_id = p.person_id
   AND @local_date between p.valid_from_date and p.valid_to_date
 INNER JOIN #bridge_time_zone b1
    ON fsd.shift_startinterval_id = b1.utc_interval_id
   AND fsd.shift_startdate_id = b1.utc_date_id
 INNER JOIN [mart].[bridge_time_zone] b2
    ON fsd.utc_interval_id = b2.interval_id
   AND fsd.utc_date_id = b2.date_id
 INNER JOIN [mart].[dim_interval] i
    ON b2.local_interval_id = i.interval_id
 INNER JOIN [mart].[dim_date] d
    ON b2.local_date_id = d.date_id
   AND d.date_date = @local_date
 INNER JOIN [mart].[dim_time_zone] t
    ON t.time_zone_code = @time_zone_code
   AND b2.time_zone_id = t.time_zone_id
 WHERE NOT EXISTS (SELECT 1 FROM #adherence_detail r where r.person_id = fsd.person_id and r.interval_id = i.interval_id and r.date_id = d.date_id)

UPDATE #adherence_detail
   SET adherence_calc_s = NULL,
       deviation_s = NULL
 WHERE date_id > @nowLocalDateId

UPDATE #adherence_detail
   SET adherence_calc_s = NULL,
       deviation_s = NULL
 WHERE date_id = @nowLocalDateId
   AND interval_id > @nowLocalIntervalId

----------
--calculation of Agent adherence, team adherence
----------
--per person and interval
UPDATE #adherence_detail
   SET adherence =
       CASE
           WHEN adherence_calc_s = 0 AND @adherence_id <> 2 THEN 1
           WHEN adherence_calc_s = 0 AND @adherence_id = 2 THEN 0
           WHEN deviation_s > adherence_calc_s THEN 0
           ELSE (adherence_calc_s - deviation_s)/ adherence_calc_s
       END
  FROM #adherence_detail

INSERT #person_adh(
       person_id,
       person_code,
       deviation_s_tot,
       adherence_calc_s_tot
)
SELECT person_id,
       person_code,
       SUM(Deviation_S) as 'Deviation_S_tot',
       SUM(Adherence_Calc_S) as 'Adherence_Calc_S_tot'
  FROM #adherence_detail
 GROUP BY person_id, person_code

UPDATE #person_adh
  SET adherence_tot =
      CASE
         WHEN a.adherence_calc_s_tot = 0 AND @adherence_id <> 2 THEN 1
         WHEN a.adherence_calc_s_tot = 0 AND @adherence_id = 2 THEN 0
         WHEN a.deviation_s_tot > a.adherence_calc_s_tot THEN 0
         ELSE (a.adherence_calc_s_tot - a.deviation_s_tot )/ a.adherence_calc_s_tot
      END
 FROM #person_adh a

SELECT person_code AS PersonId, adherence_tot as Adherence
  FROM #person_adh
 WHERE adherence_tot >= @threshold
 ORDER BY person_code

END
GO