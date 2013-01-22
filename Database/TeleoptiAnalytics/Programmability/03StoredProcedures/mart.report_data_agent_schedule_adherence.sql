﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_adherence]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_adherence]
GO

/* 
exec mart.report_data_agent_schedule_adherence @date_from='2013-02-08 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'19',@agent_person_code=N'00000000-0000-0000-0000-000000000002',@adherence_id=N'2',@sort_by=N'5',@time_zone_id=N'2',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id='D1ADE4AC-284C-4925-AEDD-A193676DBD2F',@language_id=1053,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
exec mart.report_data_agent_schedule_adherence @date_from='2013-02-04 00:00:00',@date_to='2013-02-07 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'19',@agent_person_code=N'd42dfe69-185f-4504-9a17-9b5e015b256e',@adherence_id=N'1',@sort_by=N'3',@time_zone_id=N'2',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id='6A3EB69B-690E-4605-B80E-46D5710B28AF',@language_id=1053,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
exec mart.report_data_agent_schedule_adherence @date_from='2009-02-01 00:00:00',@adherence_id=1,@date_to='2009-02-28 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@site_id=N'0',@team_set=N'7',@agent_person_code=N'11610fe4-0130-4568-97de-9b5e015b2564',@sort_by=N'6',@time_zone_id=N'1',@person_code='BEDF5892-5A2A-4BB2-9B7E-35F3C71A5AD0',@report_id='6A3EB69B-690E-4605-B80E-46D5710B28AF',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
exec mart.report_data_agent_schedule_adherence @date_from='2009-01-13 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'7',@agent_person_code=N'00000000-0000-0000-0000-000000000002',@adherence_id=N'1',@sort_by=N'1',@time_zone_id=N'2',@person_code='BEDF5892-5A2A-4BB2-9B7E-35F3C71A5AD0',@report_id='D1ADE4AC-284C-4925-AEDD-A193676DBD2F',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
exec mart.report_data_agent_schedule_adherence @date_from='2009-02-05 00:00:00',@date_to='2009-02-11 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'1',@team_set=N'5',@agent_person_code=N'826f2a46-93bb-4b04-8d5e-9b5e015b2577',@adherence_id=N'1',@sort_by=N'6',@time_zone_id=N'1',@person_code='6B7DD8B6-F5AD-428F-8934-9B5E015B2B5C',@report_id='6A3EB69B-690E-4605-B80E-46D5710B28AF',@language_id=2057,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'
exec mart.report_data_agent_schedule_adherence @date_from='2009-02-05 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'1',@team_set=N'5',@agent_person_code=N'00000000-0000-0000-0000-000000000002',@adherence_id=N'1',@sort_by=N'1',@time_zone_id=N'1',@person_code='6B7DD8B6-F5AD-428F-8934-9B5E015B2B5C',@report_id='D1ADE4AC-284C-4925-AEDD-A193676DBD2F',@language_id=2057,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

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
-- TODO: remove scenario from this SP and .aspx selection. Only default scenario is calculated in the fact-table
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
CREATE TABLE #fact_schedule_deviation (
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
	[date_id] [int] NULL,
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
date_id int,
interval_id smallint,
date_interval_counter int IDENTITY(1,1)
)

--NEW 20130111
CREATE TABLE #fact_schedule_raw (
	shift_startdate_id INT,
	shift_startinterval_id int,
	schedule_date_id INT,
	interval_id INT,
	person_id INT,
	scenario_id INT,
	activity_id int,
	absence_id int,
	scheduled_time_m INT,
	scheduled_ready_time_m INT
	)

CREATE TABLE #fact_schedule (
	shift_startdate_id INT, --NEW 20130111
	shift_startinterval_id int,--NEW 20130111
	schedule_date_id INT,
	interval_id INT,
	person_id INT,
	scenario_id INT,
	activity_id int,
	absence_id int,
	scheduled_time_s INT,
	scheduled_ready_time_s INT,
	count_activity_per_interval int
	)

CREATE TABLE #rights_agents (right_id int)

CREATE TABLE #rights_teams (right_id int)

CREATE TABLE #person_intervals(
	site_id int,
	site_name nvarchar(100),
	team_id int,
	team_name nvarchar(100),
	person_code uniqueidentifier,
	person_id int,
	first_name nvarchar(30),
	last_name nvarchar(30),
	person_name nvarchar(200),
	interval_id int,
	interval_name nvarchar(30)
	)

CREATE TABLE #agent_ids(person_id int)

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
FROM mart.bridge_time_zone b
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
WHERE b.time_zone_id = @time_zone_id

--Get the min/max UTC date_id
INSERT INTO @date (date_from_id,date_to_id)
	SELECT MIN(b.date_id),MAX(b.date_id)
	FROM mart.bridge_time_zone b
	INNER JOIN mart.dim_date d 
		ON b.local_date_id = d.date_id
	WHERE	d.date_date	between @date_from AND @date_to
	AND		b.time_zone_id	= @time_zone_id


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
	SELECT * FROM mart.ReportAgentsMultipleTeams(@date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_person_code, @person_code, @report_id, @business_unit_code)
	
	INSERT INTO #rights_teams
	SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)
END
ELSE  --MyTime and SDK, don't use permissions, just go for the current agent looking on her/him self
BEGIN	
	INSERT #rights_agents  --Insert the current agent
	SELECT * FROM mart.PersonCodeToId(@agent_person_code, @date_from, @date_to, @site_id, @team_set)
	
	INSERT #rights_teams --Insert the current team
	SELECT * FROM mart.SplitStringInt(@team_set)
END

--Table to hold agents to view
CREATE TABLE #person_id (person_id int)

--Join the ResultSets above as:
--a) teams allowed = #rights_teams.
--b) agent allowed = #rights_agents
--c) valid for this period
INSERT INTO #person_id
SELECT dp.person_id
FROM mart.dim_person dp
INNER JOIN #rights_teams t
	ON dp.team_id = t.right_id
INNER JOIN #rights_agents a
	ON a.right_id = dp.person_id
	
--Create UTC table from: mart.fact_schedule_deviation
INSERT INTO #fact_schedule_deviation(shift_startdate_id,shift_startinterval_id,date_id,interval_id,person_id,deviation_schedule_ready_s,deviation_schedule_s,deviation_contract_s,ready_time_s,is_logged_in,contract_time_s)
SELECT 
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
INNER JOIN #person_id a
	ON fsd.person_id = a.person_id
INNER JOIN #bridge_time_zone b
	ON	fsd.shift_startinterval_id= b.interval_id
	AND fsd.shift_startdate_id=b.date_id --NEW 20130111



--Get all fact_schedule-data for the day in question.
--Note: local date e.g. incl. time zone
--step 1 get raw data for speed
INSERT #fact_schedule_raw(shift_startdate_id,shift_startinterval_id,schedule_date_id,interval_id,person_id,scenario_id,activity_id,absence_id,scheduled_time_m,scheduled_ready_time_m)
SELECT 
		shift_startdate_id,
		shift_startinterval_id,
		schedule_date_id,
		fs.interval_id,
		fs.person_id,
		fs.scenario_id,
		activity_id,
		absence_id,
		scheduled_time_m,
		scheduled_ready_time_m

FROM 
	mart.fact_schedule fs
INNER JOIN #person_id a
	ON fs.person_id = a.person_id
INNER JOIN #bridge_time_zone b
	ON	fs.shift_startinterval_id= b.interval_id
	AND fs.shift_startdate_id = b.date_id
WHERE fs.scenario_id=@scenario_id


INSERT #fact_schedule(shift_startdate_id,shift_startinterval_id,schedule_date_id,interval_id,person_id,scenario_id,scheduled_time_s,scheduled_ready_time_s,count_activity_per_interval)
SELECT
	fs.shift_startdate_id,
	fs.shift_startinterval_id,
	fs.schedule_date_id,
	fs.interval_id,
	fs.person_id,
	fs.scenario_id,
	SUM(fs.scheduled_time_m)*60,
	SUM(fs.scheduled_ready_time_m)*60,
	COUNT(fs.interval_id)		
FROM #fact_schedule_raw fs
INNER JOIN #person_id a
	ON fs.person_id = a.person_id
INNER JOIN #bridge_time_zone b
	ON	fs.shift_startinterval_id= b.interval_id
	AND fs.shift_startdate_id= b.date_id
GROUP BY fs.shift_startdate_id,fs.shift_startinterval_id,fs.schedule_date_id,fs.person_id,fs.interval_id,fs.scenario_id

--Update with activity.
--a) In case there are multiple activities per interval, we use activities where in_ready_time = 1
UPDATE #fact_schedule
SET activity_id= fs.activity_id
FROM #fact_schedule_raw fs
	INNER JOIN #fact_schedule SchTemp
	ON SchTemp.schedule_date_id=fs.schedule_date_id
	AND SchTemp.person_id=fs.person_id
	AND SchTemp.interval_id=fs.interval_id
	AND SchTemp.scenario_id=fs.scenario_id
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
	AND SchTemp.scenario_id=fs.scenario_id
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
	AND SchTemp.scenario_id=fs.scenario_id
WHERE SchTemp.absence_id IS NULL

--Get all intervals for the collection of person and teams
INSERT #person_intervals
	SELECT site_id,site_name,team_id,team_name,person_code,p.person_id,first_name,last_name,person_name,interval_id,interval_name
	FROM mart.dim_person p
	INNER JOIN mart.dim_interval i
		ON 1=1
	INNER JOIN #person_id a
		ON p.person_id = a.person_id
	--WHERE @date_from BETWEEN p.valid_from_date AND p.valid_to_date
	WHERE @date_from BETWEEN p.valid_from_date AND p.valid_to_date OR @date_to BETWEEN p.valid_from_date AND p.valid_to_date --20120905 KJ ADDED @DATE_TO


--Start creating the result set
--a) insert agent statistics matching scheduled time
INSERT #result(shift_startdate_id,shift_startdate,date_id,date,interval_id,interval_name,intervals_per_day,site_id,site_name,team_id,team_name,person_code,person_id,
person_first_name,person_last_name,person_name,deviation_s,ready_time_s,is_logged_in,activity_id,absence_id,adherence_calc_s,
adherence_type_selected,hide_time_zone,count_activity_per_interval)
	SELECT	b1.date_id,
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
			fsd.is_logged_in,
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
	INNER JOIN #fact_schedule_deviation fsd
		ON fsd.person_id=p.person_id
	LEFT JOIN #fact_schedule fs
		ON fsd.person_id=fs.person_id
		AND fsd.date_id=fs.schedule_date_id
		AND fsd.interval_id=fs.interval_id
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
	WHERE fs.scenario_id=@scenario_id
	AND b2.time_zone_id=@time_zone_id
ORDER BY p.site_id,p.team_id,p.person_id,p.person_name,b1.date_id,b1.date_date,d.date_id,d.date_date,i.interval_id


IF @adherence_id = 2
BEGIN
--b) insert agent statistics outside schdeuled time
INSERT #result(shift_startdate_id,shift_startdate,date_id,date,interval_id,interval_name,intervals_per_day,site_id,site_name,team_id,team_name,person_code,person_id,person_first_name,person_last_name,person_name,deviation_s,ready_time_s,is_logged_in,activity_id,absence_id,adherence_calc_s,adherence_type_selected,hide_time_zone,count_activity_per_interval)
	SELECT	b1.date_id,
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
			isnull(fsd.deviation_schedule_s,0) 'deviation_s', --@adherence_id =2
			isnull(fsd.ready_time_s,0) 'ready_time_s',
			fsd.is_logged_in,
			-1,
			-1,
			@intervals_length_s AS 'adherence_calc_s', --Compare to full Interval since there's no Scheduled Time here
			@selected_adherence_type,
			@hide_time_zone,
			2
FROM mart.dim_person p
	INNER JOIN #fact_schedule_deviation fsd
		ON fsd.person_id=p.person_id
	INNER JOIN #bridge_time_zone b1
		ON	fsd.shift_startinterval_id= b1.interval_id
		AND fsd.shift_startdate_id=b1.date_id	
	INNER JOIN mart.bridge_time_zone b
		ON	fsd.interval_id= b.interval_id
		AND fsd.date_id= b.date_id
	INNER JOIN mart.dim_date d 
		ON b.local_date_id = d.date_id
	INNER JOIN mart.dim_interval i
		ON b.local_interval_id = i.interval_id
	WHERE NOT EXISTS
		(SELECT 1 FROM #fact_schedule fs
			WHERE fsd.person_id=fs.person_id
			AND fsd.date_id=fs.schedule_date_id
			AND fsd.interval_id=fs.interval_id)
	AND b.time_zone_id=@time_zone_id
END

----------
--calculation of Agent adherence, team adherence
----------
--per person and interval
UPDATE #result
SET adherence=
CASE
		WHEN adherence_calc_s = 0 THEN 1
		WHEN deviation_s > adherence_calc_s THEN 0
		ELSE (adherence_calc_s - deviation_s)/ adherence_calc_s
	END
FROM #result

--per person total
INSERT INTO #person_adh
SELECT shift_startdate_id,person_id,sum(adherence_calc_s)'adherence_calc_s',sum(deviation_s)'deviation_s'
FROM #result
GROUP by shift_startdate_id,person_id


UPDATE #result
SET adherence_tot=
	CASE
		WHEN a.adherence_calc_s = 0 THEN 1
		WHEN a.deviation_s > a.adherence_calc_s THEN 0
		ELSE (a.adherence_calc_s - a.deviation_s )/ a.adherence_calc_s
	END,
deviation_tot_s=a.deviation_s
FROM #person_adh a
INNER JOIN #result r ON r.shift_startdate_id=a.date_id AND r.person_id=a.person_id

--per interval
INSERT INTO #team_adh
SELECT date_id,interval_id,sum(adherence_calc_s)'adherence_calc_s',sum(deviation_s)'deviation_s'
FROM #result
GROUP by date_id,interval_id

UPDATE #result
SET team_adherence=
	CASE
		WHEN a.adherence_calc_s = 0 THEN 1
		WHEN a.deviation_s > a.adherence_calc_s THEN 0
		ELSE (a.adherence_calc_s - a.deviation_s )/ a.adherence_calc_s
	END
,team_deviation_s=a.deviation_s
FROM #team_adh a
INNER JOIN #result r ON r.interval_id=a.interval_id AND r.date_id=a.date_id

--total(for all selected)
INSERT INTO #team_adh_tot
SELECT sum(adherence_calc_s)'adherence_calc_s',sum(deviation_s)'deviation_s'
FROM #result

UPDATE #result
SET team_adherence_tot=
	CASE
		WHEN a.adherence_calc_s = 0 THEN 1
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
INSERT #counter(date_id,interval_id)
select distinct date_id,interval_id
from #result
order by date_id,interval_id

update #result
set date_interval_counter=c.date_interval_counter
from #counter c inner join #result r on r.date_id=c.date_id AND r.interval_id=c.interval_id


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