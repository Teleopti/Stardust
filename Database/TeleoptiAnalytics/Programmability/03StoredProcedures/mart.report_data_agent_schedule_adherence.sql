IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_adherence]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_adherence]
GO

/* 
exec mart.report_data_agent_schedule_adherence @date_from='2013-02-04 00:00:00',@date_to='2013-02-05 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'13',@agent_person_code=N'11610fe4-0130-4568-97de-9b5e015b2564',@adherence_id=N'1',@sort_by=N'6',@time_zone_id=N'2',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id='6A3EB69B-690E-4605-B80E-46D5710B28AF',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
*/
-- =============================================
-- Author:		KJ
-- Create date: 2008-07-01
-- Update date: 
--				2009-01-12 Bug fix adherence tot and deviation_tot KJ
--				2008-11-06 Bug fix on # table for fact schedule
--				2009-02-11 Added new mart schema KJ
--				2009-03-02 Excluded timezone UTC from time_zone check KJ
--				2009-07-07 Bug fix for multiple activities per interval KJ
--				2009-08-20 Adding another person -> agent. Now used from SDK and MyTime also ZT
--				2009-09-16 Removed a few IFs for @from_matrix DJ
--				2010-10-29 Refactor of mart.fact_schedule_deviation causing changes on used columns
--				2010-11-01 Fix #12301 Sorting issues
--				2010-11-16 #12394 Show all statistics (outside scheduled time)
--				2010-11-17 Divsion by zero in build
--				2011-01-24 Use agent_code instead of agent_id ME
--				2011-02-24 Added SDK comment 
--				2011-08-08 Added sort by shiftstart and shiftend Ola
--				2011-09-27 #16079 - Azure select into
--				2011-11-15 #16939 - Sorting issue
--				2011-12-05 #17180 - Multi-bu and @adherence_id = 2
--				2012-01-09 Change the handling of multi-bu and @adherence_id = 2
--				2012-01-23 Change parameters @group_page_group_set and @team_set to sets and nvarchar(max)
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
--				2012-04-16 Bug 18933
--				2012-09-06 Added new functionality for report Adherence Per Agent. Parameter @date_to only used by Adherence Per Agent.
--				2013-01-15 Added statistics based on shiftstart_date to get the entire shift started during the selected period.
-- Description:	Used by reports Adherence per Agent and Adherence per Date.
-- =============================================


CREATE PROCEDURE [mart].[report_data_agent_schedule_adherence] 
@date_from datetime,
@date_to datetime = @date_from, --NEW 20120903 KJ, default_value equals to start_date
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier = '567E693D-9D55-47F9-AAAB-D620C71CACE8',
@site_id int,  --currently obsolete
@team_set nvarchar(max),
@adherence_id int,--1,2 eller 3 från adherence_calculation tabellen
@sort_by int, ---sort by 1=FirstName,2=LastName,3=Shift_start,4=Adherence
@time_zone_id int,
@person_code uniqueidentifier,
@agent_person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier,
@from_matrix bit = 1 --Not from SDK
AS
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
CREATE CLUSTERED INDEX #CIX_fact_schedule_deviation_raw ON #fact_schedule_deviation_raw
(
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
CREATE CLUSTERED INDEX #CIX_fact_schedule_deviation ON #fact_schedule_deviation
(
	[shift_startdate_id] ASC,
	[shift_startinterval_id] ASC
)

CREATE TABLE #minmax(
	[minint] [int] NULL,
	[maxint] [int] NULL,
	[person_id] [int] NULL,
	[date]	[datetime] NULL,
	[shift_startdate_id] int,
	date_interval_min [datetime] NULL,
	date_interval_max [datetime] NULL
)

CREATE TABLE #team_adh_tot(
	[adherence_calc_s] [decimal](38, 3) NULL,
	[deviation_s] [decimal](38, 3) NULL
)

CREATE TABLE #team_adh(
	[interval_id] [int] NULL,
	[adherence_calc_s] [decimal](38, 3) NULL,
	[deviation_s] [decimal](38, 3) NULL
)

CREATE TABLE #person_adh(
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

CREATE TABLE #counter(
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

CREATE TABLE #rights_agents (right_id int)

CREATE TABLE #rights_teams (right_id int)

--Table to hold agents to view
CREATE TABLE #person_id (person_id int,
	site_id int,
	site_name nvarchar(100),
	team_id int,
	team_name nvarchar(100),
	person_code uniqueidentifier,
	first_name nvarchar(30),
	last_name nvarchar(30),
	person_name	nvarchar(200), 
	valid_from_date_id_local int,
	valid_to_date_id_local int
)

CREATE TABLE #bridge_time_zone
	(
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
DECLARE @scenario_id int
DECLARE @nowutcDateOnly smalldatetime
DECLARE @nowutcInterval smalldatetime
DECLARE @nowLocalDateId int
DECLARE @nowLocalIntervalId smallint
DECLARE @minUtcDateId int
DECLARE @maxUtcDateId int
DECLARE @minUtcDateIdMinus1 int
DECLARE @maxUtcDateIdPlus1 int

SELECT @nowutcDateOnly = DATEADD(dd, 0, DATEDIFF(dd, 0, GETUTCDATE()))
SELECT @nowutcInterval = DATEADD(minute,DATEPART(minute, GETUTCDATE()),DATEADD(hour, DATEPART(hour, GETUTCDATE()),'1900-01-01 00:00:00'))


------------
--Init
------------
SELECT @selected_adherence_type= adherence_name FROM mart.adherence_calculation WHERE adherence_id=@adherence_id
SELECT @intervals_per_day = COUNT(interval_id) FROM mart.dim_interval

-- Get default scenario for given business unit
SELECT @scenario_id = scenario_id FROM mart.dim_scenario WHERE default_scenario = 1 AND business_unit_code = @business_unit_code

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
FROM mart.bridge_time_zone b WITH (NOLOCK)
INNER JOIN mart.dim_date d WITH (NOLOCK)
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i WITH (NOLOCK)
	ON b.local_interval_id = i.interval_id
WHERE b.time_zone_id = @time_zone_id

--Get the min/max UTC date_id
SELECT
	@minUtcDateId = MIN(b.date_id),
	@maxUtcDateId = MAX(b.date_id)
FROM #bridge_time_zone b

SELECT
	@minUtcDateIdMinus1 = date_id 
	FROM mart.dim_date
	WHERE date_date = (
		SELECT DATEADD(d, -1, date_date)
		FROM mart.dim_date
		WHERE date_id = @minUtcDateId)

SELECT
	@maxUtcDateIdPlus1 = date_id 
	FROM mart.dim_date
	WHERE date_date = (
		SELECT DATEADD(d, 1, date_date)
		FROM mart.dim_date
		WHERE date_id = @maxUtcDateId)

--Get Now() in local date/interval Id variables
SELECT
	@nowLocalDateId = b.local_date_id,
	@nowLocalIntervalId = b.local_interval_id
FROM bridge_time_zone b WITH (NOLOCK)
INNER JOIN mart.dim_date d 
	ON b.date_id = d.date_id
	AND d.date_date = @nowUtcDateOnly
INNER JOIN mart.dim_interval i WITH (NOLOCK)
	ON b.interval_id = i.interval_id
	AND @nowUtcInterval between i.interval_start and i.interval_end
WHERE b.time_zone_id=@time_zone_id

--Multiple Time zones?
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0


-----------
--Start
-----------

--Get agent data permissions
IF (@from_matrix = 1)
BEGIN --Standard Report Permisssions

	/* Get the agents to report on */

	INSERT INTO #rights_agents
	EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_person_code, @person_code, @report_id, @business_unit_code
	
	INSERT INTO #rights_teams
	SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

		--Join the ResultSets above as:
	--a) teams allowed = #rights_teams.
	--b) agent allowed = #rights_agents
	--c) valid for this period
	INSERT INTO #person_id
	SELECT dp.person_id,
		dp.site_id,
		dp.site_name,
		dp.team_id,
		dp.team_name,
		dp.person_code,
		dp.first_name,
		dp.last_name,
		dp.person_name,	
		dp.valid_from_date_id_local,
		dp.valid_to_date_id_local
	FROM mart.dim_person dp WITH (NOLOCK)
	INNER JOIN #rights_teams t
		ON dp.team_id = t.right_id
	INNER JOIN #rights_agents a
		ON a.right_id = dp.person_id
END
ELSE  --MyTime and SDK, don't use permissions, just go for the current agent looking on her/him self
BEGIN	
	INSERT #rights_agents  --Insert the current agent
	SELECT * FROM mart.PersonCodeToId(@agent_person_code, @date_from, @date_to, @site_id, @team_set)
	
	INSERT #rights_teams --Insert the current team
	SELECT * FROM mart.SplitStringInt(@team_set)
	
	INSERT INTO #person_id 
	SELECT DISTINCT dp.person_id,
	dp.site_id,
	dp.site_name,
	dp.team_id,
	dp.team_name,
	dp.person_code,
	dp.first_name,
	dp.last_name,
	dp.person_name,
	dp.valid_from_date_id_local,
	dp.valid_to_date_id_local
	FROM mart.dim_person dp  WITH (NOLOCK) WHERE person_code = @agent_person_code
END

-- in case of -2 in valid_to_date_id_local, update the eternity date id with max date id
UPDATE #person_id
SET valid_to_date_id_local = (SELECT TOP 1 date_id from mart.dim_date with (nolock) ORDER BY date_id DESC) 
WHERE valid_to_date_id_local = -2

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
INNER JOIN #person_id p
	ON fsd.person_id = p.person_id
	AND fsd.shift_startdate_local_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
WHERE fsd.shift_startdate_local_id  between @minUtcDateIdMinus1 AND @maxUtcDateIdPlus1
AND fsd.date_id between @minUtcDateIdMinus1 AND @maxUtcDateIdPlus1

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
	mart.fact_schedule fs WITH (NOLOCK)
INNER JOIN #person_id p
	ON fs.person_id = p.person_id
	AND fs.shift_startdate_local_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
WHERE fs.shift_startdate_local_id BETWEEN  @minUtcDateIdMinus1 AND @maxUtcDateIdPlus1
AND fs.schedule_date_id between @minUtcDateIdMinus1 AND @maxUtcDateIdPlus1
and fs.scenario_id=@scenario_id

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
INNER JOIN mart.dim_activity a WITH (NOLOCK)
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
INNER JOIN mart.dim_activity a WITH (NOLOCK)
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
adherence_type_selected,hide_time_zone,count_activity_per_interval,shift_interval_id)
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
			isnull(count_activity_per_interval,2), --fake a mixed shift = white color	
			b1.local_interval_id
	FROM #person_id p
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
	INNER JOIN bridge_time_zone b2 WITH (NOLOCK)
		ON	fs.interval_id= b2.interval_id
		AND fs.schedule_date_id= b2.date_id
	INNER JOIN mart.dim_interval i
		ON b2.local_interval_id = i.interval_id			
	INNER JOIN mart.dim_date d WITH (NOLOCK)
		ON b2.local_date_id = d.date_id
	AND b2.time_zone_id=@time_zone_id

--b) insert agent statistics outside shift
INSERT #result(shift_startdate_local_id,shift_startdate_id,shift_startdate,date_id,date,interval_id,interval_name,intervals_per_day,site_id,site_name,team_id,team_name,person_code,person_id,
person_first_name,person_last_name,person_name,deviation_s,ready_time_s,is_logged_in,activity_id,absence_id,adherence_calc_s,
adherence_type_selected,hide_time_zone,count_activity_per_interval, shift_interval_id)
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
			0, --white color	
			b1.local_interval_id
	FROM #person_id p
	INNER JOIN #fact_schedule_deviation fsd
		ON fsd.person_id=p.person_id
	INNER JOIN #bridge_time_zone b1
		ON	fsd.shift_startinterval_id= b1.interval_id
		AND fsd.shift_startdate_id=b1.date_id
	INNER JOIN bridge_time_zone b2 WITH (NOLOCK)
		ON	fsd.interval_id= b2.interval_id
		AND fsd.date_id= b2.date_id
	INNER JOIN mart.dim_interval i WITH (NOLOCK)
		ON b2.local_interval_id = i.interval_id			
	INNER JOIN mart.dim_date d WITH (NOLOCK)
		ON b2.local_date_id = d.date_id
	AND b2.time_zone_id=@time_zone_id
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
FROM mart.dim_absence a WITH (NOLOCK)
INNER JOIN #result r
	ON r.absence_id=a.absence_id AND r.activity_id = -1

UPDATE #result
SET display_color=a.display_color,activity_absence_name=activity_name
FROM mart.dim_activity a WITH (NOLOCK)
INNER JOIN #result r
	ON r.activity_id=a.activity_id and r.absence_id = -1

--logged in time outside shift
UPDATE #result
set display_color=a.display_color,activity_absence_name=a.activity_name
from mart.dim_activity a
inner join #result r on r.activity_id=a.activity_id
where  count_activity_per_interval=0
and a.activity_id=-1

--If more the one activity per interval, then we display that these are multiple activities or absencens with null color
--The name of this should not be used, use resource file instead.
UPDATE #result
SET display_color=NULL, activity_absence_name='Multiple things', activity_id = -1, absence_id = -1
FROM #result  r
WHERE r.count_activity_per_interval > 1

--Sorting on Shift Start & Shift End

--set min_shiftstart to local_shift_startinterval
UPDATE #result 
SET person_min_shiftstart_interval= shift_interval_id

UPDATE #result 
SET person_max_shiftend_interval= interval_id

--for shifts covering midnight add 1 day x n intervals
UPDATE #result 
SET person_max_shiftend_interval= interval_id+intervals_per_day
WHERE shift_startdate<>[date]

INSERT INTO #minmax(minint,maxint,person_id,shift_startdate_id)
SELECT min(minint),max(maxint),person_id,shift_startdate_id
FROM (
	SELECT  min(r.shift_interval_id) minint ,max(r.person_max_shiftend_interval) maxint, person_id, shift_startdate_id
	FROM #result r
	WHERE (r.activity_id <> -1 or r.absence_id <> -1)
	GROUP BY shift_startdate_id,person_id
	UNION ALL
	SELECT  min(r.shift_interval_id) minint ,max(r.person_max_shiftend_interval) maxint, person_id, shift_startdate_id
	FROM #result r
	WHERE (r.activity_id = -1 AND r.absence_id = -1) --Only stats exists
	GROUP BY shift_startdate_id,person_id
) a
GROUP BY shift_startdate_id,person_id

--skip for now
--UPDATE #result 
--SET person_min_shiftstart_interval= minint
--FROM #minmax 
--INNER JOIN #result ON #result.shift_startdate_id=#minmax.shift_startdate_id AND #result.person_id=#minmax.person_id

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

-- Sortering 1=FirstName,2=LastName,3=Shift_start,4=Adherence,5=ShiftEnd 6=Date
-- NOTE: If you change the column order/name you need to consider SDK DTO as well!


IF @sort_by=1
	SELECT		date, interval_id, interval_name, intervals_per_day, site_id, site_name, team_id, team_name,
				person_id ,	person_first_name,person_last_name ,person_name,adherence ,adherence_tot, deviation_s/60.0 as 'deviation_m' ,adherence_calc_s,deviation_tot_s/60.0 as 'deviation_tot_m' ,round(ready_time_s/60.0 ,0)'ready_time_m',
				is_logged_in, activity_id ,absence_id ,display_color ,activity_absence_name, team_adherence, team_adherence_tot ,team_deviation_s/60.0 as 'team_deviation_m' ,
				team_deviation_tot_s/60.0 as 'team_deviation_tot_m' ,adherence_type_selected, hide_time_zone,shift_startdate,date_interval_counter
				FROM #result ORDER BY person_first_name,person_last_name,person_id,date_id,interval_id
IF @sort_by=2
	SELECT		date, interval_id, interval_name, intervals_per_day, site_id, site_name, team_id, team_name,
				person_id ,	person_first_name,person_last_name ,person_name,adherence ,adherence_tot ,	deviation_s/60.0 'deviation_m' ,deviation_tot_s/60.0 as 'deviation_tot_m' ,round(ready_time_s/60.0,0 )'ready_time_m',
				is_logged_in, activity_id ,absence_id ,display_color ,activity_absence_name, team_adherence ,team_adherence_tot ,team_deviation_s/60.0 as team_deviation_m ,
				team_deviation_tot_s/60.0 as team_deviation_tot_m ,adherence_type_selected, hide_time_zone,shift_startdate,date_interval_counter
				FROM #result ORDER BY person_last_name,person_first_name,person_id,date_id,interval_id
IF @sort_by=3
SELECT			date, interval_id, interval_name, intervals_per_day, site_id, site_name, team_id, team_name,
				person_id ,	person_first_name,person_last_name ,person_name,adherence ,adherence_tot ,	deviation_s/60.0 as deviation_m ,deviation_tot_s/60.0 as deviation_tot_m ,round(ready_time_s/60.0,0)'ready_time_m',
				is_logged_in, activity_id ,absence_id ,display_color ,activity_absence_name, team_adherence ,team_adherence_tot ,team_deviation_s/60.0 as team_deviation_m ,
				team_deviation_tot_s/60.0 as team_deviation_tot_m ,adherence_type_selected, hide_time_zone,shift_startdate,date_interval_counter
				FROM #result ORDER BY person_min_shiftstart_interval,shift_startdate, person_first_name,person_last_name,person_id,date,interval_id
IF @sort_by=4
	SELECT		date, interval_id, interval_name, intervals_per_day, site_id, site_name, team_id, team_name,
				person_id ,	person_first_name,person_last_name ,person_name,adherence ,adherence_tot ,	deviation_s/60.0 as deviation_m ,deviation_tot_s/60.0 as deviation_tot_m ,round(ready_time_s/60.0,0)'ready_time_m',
				is_logged_in, activity_id ,absence_id ,display_color ,activity_absence_name, team_adherence ,team_adherence_tot ,team_deviation_s/60.0 as team_deviation_m ,
				team_deviation_tot_s/60.0 as team_deviation_tot_m ,adherence_type_selected, hide_time_zone,shift_startdate,date_interval_counter
				FROM #result ORDER BY adherence_tot,shift_startdate,person_id, date_interval_counter,person_first_name,person_last_name
IF @sort_by=5
SELECT			date, interval_id, interval_name, intervals_per_day, site_id, site_name, team_id, team_name,
				person_id ,	person_first_name,person_last_name ,person_name,adherence ,adherence_tot ,	deviation_s/60.0 as deviation_m ,deviation_tot_s/60.0 as deviation_tot_m ,round(ready_time_s/60.0,0)'ready_time_m',
				is_logged_in, activity_id ,absence_id ,display_color ,activity_absence_name, team_adherence ,team_adherence_tot ,team_deviation_s/60.0 as team_deviation_m ,
				team_deviation_tot_s/60.0 as team_deviation_tot_m ,adherence_type_selected, hide_time_zone,shift_startdate,date_interval_counter
				FROM #result ORDER BY person_max_shiftend_interval desc,shift_startdate, person_first_name,person_last_name,person_id,date,interval_id
IF @sort_by=6
SELECT			date, interval_id, interval_name, intervals_per_day, site_id, site_name, team_id, team_name,
				person_id ,	person_first_name,person_last_name ,person_name,adherence ,adherence_tot ,	deviation_s/60.0 as deviation_m ,deviation_tot_s/60.0 as deviation_tot_m ,round(ready_time_s/60.0,0)'ready_time_m',
				is_logged_in, activity_id ,absence_id ,display_color ,activity_absence_name, team_adherence ,team_adherence_tot ,team_deviation_s/60.0 as team_deviation_m ,
				team_deviation_tot_s/60.0 as team_deviation_tot_m ,adherence_type_selected, hide_time_zone, shift_startdate,date_interval_counter
				FROM #result ORDER BY shift_startdate,date_interval_counter