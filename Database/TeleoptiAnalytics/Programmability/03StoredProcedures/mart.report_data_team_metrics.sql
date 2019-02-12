IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_team_metrics]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_team_metrics]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-11-04
--				2008-11-06 Added column Answered Calls / Ready Hour
--				2008-11-21 Add column handling_time_s, deviation_m
--				2008-11-25 Speed up KJ
--				2008-12-03 Removed reference to fact_contract table KJ
--				2009-02-11 Added new mart schema Kj
--				2009-03-02 Excluded timezone UTC from time_zone check KJ
--				2009-07-07 Fix for issues with agents on multiqueues KJ
--				2010-11-01 #11055 Refactor of mart.fact_schedule_deviation KJ
--				2010-12-22 General re-factor DJ. nothing much to do
--				2012-01-09 Pass BU to ReportAgents ME
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
--				2013-07-10 backed out of #23621
--				2013-07-10 Fix #24119, same as #23621
-- =============================================
--exec mart.report_data_team_metrics @date_from='2009-02-03 00:00:00',@date_to='2009-02-03 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@site_id=N'1',@team_set=N'5',@adherence_id=N'1',@time_zone_id=N'2',@person_code='18037D35-73D5-4211-A309-9B5E015B2B5C',@report_id=11,@language_id=1053
--exec mart.report_data_team_metrics @date_from='2010-12-13 00:00:00',@date_to='2010-12-17 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@site_id=N'-2',@team_set=N'4',@adherence_id=N'1',@time_zone_id=N'1',@person_code='7008F537-6EE8-42AC-B371-9D34009CC423',@report_id=11,@language_id=1053
--exec mart.report_data_team_metrics @date_from='2010-12-17 00:00:00',@date_to='2010-12-17 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@site_id=N'-2',@team_set=N'4',@adherence_id=N'2',@time_zone_id=N'1',@person_code='B55E30C0-C058-4055-8575-FFB348257B58',@report_id=11,@language_id=1053
CREATE PROCEDURE [mart].[report_data_team_metrics] 
@date_from datetime,
@date_to datetime,
@interval_from int,
@interval_to int,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@site_id int,
@team_set nvarchar(max),
@adherence_id int,
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier,
@details int=0
AS
SET NOCOUNT ON;

CREATE TABLE #person_acd_subSP
	(
	person_id int,
	person_code uniqueidentifier,
	team_id int, 
	acd_login_id int, 
	valid_from_date_id int,
	valid_from_interval_id int,
	valid_to_date_id_maxDate int,
	valid_to_interval_id_maxdate int,
	valid_from_date_id_local int,
	valid_to_date_id_local int
	)

CREATE TABLE  #rights_agents
	(
	right_id int
	)
	
CREATE TABLE #pre_result_subSP
	(
	date_id int,
	date_date smalldatetime,
	interval_id int,
	acd_login_id int,
	person_id int,
	team_id int,
	answered_calls int DEFAULT 0,		
	talk_time_s decimal(20,2),
	after_call_work_time_s decimal(20,2),
	handling_time_s decimal(20,2),
	ready_time_s int DEFAULT 0,
	scheduled_ready_time_m int DEFAULT 0,
	scheduled_time_m int,
	scheduled_contract_time_m int,
	deviation_s decimal(18,3),
	adherence_calc_s decimal(18,3)	
	)

--Create temporary tables
CREATE TABLE #bridge_time_zone
	(
	local_date_id int not null,
	local_interval_id int not null,
	date_id int not null,
	interval_id int not null,
	date_date_local smalldatetime not null
	)

DECLARE @group_page_agent_code uniqueidentifier
DECLARE @agent_person_code uniqueidentifier
DECLARE @scenario_id int
DECLARE @date_from_id int
DECLARE @date_to_id int

-- Get default scenario for given business unit
SELECT @scenario_id = scenario_id FROM mart.dim_scenario WHERE default_scenario = 1 AND business_unit_code = @business_unit_code

--All Agent
SET @group_page_agent_code = '00000000-0000-0000-0000-000000000002'
SET @agent_person_code = '00000000-0000-0000-0000-000000000002'


--Get all agents/persons that user has permission to see in given period
INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_person_code, @person_code, @report_id, @business_unit_code

INSERT INTO #person_acd_subSP
SELECT
	person_id	= a.right_id,
	person_code	= p.person_code,
	team_id		= p.team_id,
	acd_login_id= acd.acd_login_id,
	valid_from_date_id =p.valid_from_date_id,
	valid_from_interval_id =p.valid_from_interval_id,
	valid_to_date_id_maxDate =p.valid_to_date_id_maxDate,
	valid_to_interval_id_maxdate =p.valid_to_interval_id_maxdate,
	valid_from_date_id_local=p.valid_from_date_id_local,
	valid_to_date_id_local=p.valid_to_date_id_local
FROM #rights_agents a
INNER JOIN mart.dim_person p WITH (NOLOCK)
	on p.person_id = a.right_id
LEFT JOIN mart.bridge_acd_login_person acd
	ON acd.person_id = a.right_id

--Get needed dates and intervals from bridge time zone into temp table
INSERT INTO #bridge_time_zone
SELECT 
	local_date_id		= d.date_id,
	local_interval_id	= i.interval_id,
	date_id				= b.date_id,
	interval_id			= b.interval_id,
	date_date_local		= d.date_date
FROM mart.bridge_time_zone b
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
WHERE b.time_zone_id = @time_zone_id

--Get the min/max UTC date_id. Expand UTC -1 +1 when fetching fact_tables (subSP
SELECT 
	@date_from_id=MIN(b.date_id),
	@date_to_id=MAX(b.date_id)
FROM #bridge_time_zone b
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
WHERE	d.date_date	between @date_from AND @date_to

SELECT
	@date_from_id = date_id 
	FROM mart.dim_date
	WHERE date_date = (
		SELECT DATEADD(d, -1, date_date)
		FROM mart.dim_date
		WHERE date_id = @date_from_id)

SELECT
	@date_to_id = date_id 
	FROM mart.dim_date
	WHERE date_date = (
		SELECT DATEADD(d, 1, date_date)
		FROM mart.dim_date
		WHERE date_id = @date_to_id)

--This SP will insert data into #pre_result_subSP
EXEC [mart].[report_data_schedule_result_subSP]
	@date_from_id	= @date_from_id,
	@date_to_id		= @date_to_id,
	@interval_from	= @interval_from,
	@interval_to	= @interval_to,
	@adherence_id	= @adherence_id,
	@time_zone_id	= @time_zone_id,
	@person_code	= @person_code,
	@report_id		= @report_id,
	@scenario_id	= @scenario_id,
	@language_id	= @language_id

--switch utc date to local date
update #pre_result_subSP
set
	date_id		= btz.local_date_id,
	interval_id = btz.local_interval_id,
	date_date	= btz.date_date_local
from #bridge_time_zone btz
inner join #pre_result_subSP me
	on me.date_id = btz.date_id
	and me.interval_id = btz.interval_id
	
--delete data outside local dates
DELETE FROM #pre_result_subSP
WHERE date_date < @date_from
OR date_date > @date_to
OR date_date IS NULL

--Delete ACD-logins that have been logged on without being a agent in CCC7
DELETE FROM #pre_result_subSP
WHERE team_id = -1 --Not Defined

SELECT	r.date_date AS 'date',
		r.team_id,
		p.team_name,
		ISNULL(SUM(r.scheduled_ready_time_m*60),0) AS 'scheduled_ready_time',--vi tar det i sekunder
		SUM(r.ready_time_s) AS 'ready_time_s',
		CASE
			WHEN SUM(r.scheduled_ready_time_m)<=0 THEN 0
			ELSE SUM(r.ready_time_s)/CONVERT(float,sum(r.scheduled_ready_time_m*60))
		END AS 'ready_time_per_scheduled_ready_time',
		SUM(ISNULL(r.answered_calls,0)) AS 'answered_calls',
		CASE
			WHEN CONVERT(float,SUM(r.scheduled_ready_time_m)/60.0)<=0 THEN 0
			ELSE SUM(r.answered_calls)/(CONVERT(float,SUM(r.scheduled_ready_time_m)/60.0))
		END AS 'avg_answered_calls',
		CASE
			WHEN CONVERT(float,sum(r.ready_time_s)/3600.0)<=0 THEN 0
			ELSE sum(r.answered_calls)/(CONVERT(float,SUM(r.ready_time_s)/3600.0))
		END AS 'avg_answered_calls_ready_hour',
		CASE
			WHEN sum(r.ready_time_s)<= 0 THEN 0
			ELSE (sum(r.handling_time_s)) /convert(float,sum(r.ready_time_s)) 
		END AS 'occupancy',
		SUM(r.adherence_calc_s) AS 'adherence_calc_s',
		CASE
			WHEN SUM(r.adherence_calc_s) = 0 THEN 1
			ELSE (SUM(r.adherence_calc_s) - SUM(r.deviation_s))/SUM(r.adherence_calc_s)
		END AS 'adherence',
		SUM(r.deviation_s) AS 'deviation_s',
		SUM(isnull(r.handling_time_s,0)) AS 'handling_time_s'
FROM #pre_result_subSP r
INNER JOIN mart.dim_person p WITH (NOLOCK)
ON r.person_id = p.person_id
GROUP BY r.date_date, r.team_id, p.team_name
ORDER BY p.team_name,r.date_date

GO
