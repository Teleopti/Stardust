IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_result]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_result]
GO

/*
exec mart.report_data_agent_schedule_result @date_from='2013-05-01 00:00:00',@date_to='2013-05-31 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_set=NULL,@site_id=N'3',@team_set=N'19',
@agent_set=N'9f0311d3-b4ca-4481-a344-a00b00a2d494,AF4C77D7-7782-4E8D-A517-A00B009EC05B'
,@adherence_id=N'1',@time_zone_id=N'2',@person_code='D0F3F560-0E23-46A4-80AD-9FF1521EA8A8',@report_id='0065AA84-FD47-4022-ABE3-DD1B54FD096C',@language_id=1033,@business_unit_code='C05D8FA4-A6C7-484D-BE81-9F410120F050'

2277 - Abdulaziz-AlLagman - AF4C77D7-7782-4E8D-A517-A00B009EC05B
*/
--exec [mart].[report_data_agent_schedule_result] @date_from='2009-02-03 00:00:00',@date_to='2009-02-03 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_set=NULL,@site_id=N'-2',@team_set=N'4',@agent_set=N'160',@adherence_id=N'2',@time_zone_id=N'1',@person_code='7008F537-6EE8-42AC-B371-9D34009CC423',@report_id=12,@language_id=1053
--exec [mart].[report_data_agent_schedule_result] @date_from='2009-02-03 00:00:00',@date_to='2009-02-03 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_set=NULL,@site_id=N'-2',@team_set=N'4',@agent_set=N'160',@adherence_id=N'2',@time_zone_id=N'1',@person_code='7008F537-6EE8-42AC-B371-9D34009CC423',@report_id='0065AA84-FD47-4022-ABE3-DD1B54FD096C',@language_id=1053,@business_unit_code='5BC41544-6BF8-444E-A34D-A01A0128A204'
--exec [mart].[mart.report_data_team_metrics_new] @date_from='2010-12-13 00:00:00',@date_to='2010-12-13 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_set=NULL,@site_id=N'-2',@team_set=N'4',@agent_set=N'160',@adherence_id=N'2',@time_zone_id=N'1',@person_code='7008F537-6EE8-42AC-B371-9D34009CC423',@report_id=12,@language_id=1053
--exec mart.report_data_agent_schedule_adherence @date_from='2010-12-13 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_id=NULL,@site_id=N'-2',@team_set=N'-2',@agent_person_id=N'160',@adherence_id=N'2',@sort_by=N'1',@time_zone_id=N'1',@person_code='7008F537-6EE8-42AC-B371-9D34009CC423',@report_id=13,@language_id=1053
-- =============================================
-- Author:		KJ
-- Create date: 2008-06-03
-- Update Date: 
--				2008-11-24 Speed up KJ
--				2008-11-21 Add column deviation_m
--				2008-11-20 Add column handling_time_s
--				2008-11-06 Added column Answered Calls / Ready Hour
--				2008-11-05 Fixed bug on adherence calculation KJ
--				2008-10-08 Fixed bug date-format + made it a bit faster with new #-table KJ
--				2008-12-03 Removed refernce to fact_contract table KJ
--				2009-02-11 Added new mart schema KJ
--				2009-03-02 Excluded timezone UTC from time_zone check KJ
--				2009-07-07 Fix for issues with agents on multiqueues KJ
--				2010-06-07 Only shows data for selected agents JR
--				2010-11-01 refactor of mart.fact_schedule_deviation KJ
--				2010-12-22 General re-factor DJ. nothing much to do
--				2011-01-12 Adding ready time outside shift
--				2011-01-13 Set adherance = 100% when no action
--				2011-01-21 Use person_code instead of person_id in input strin @agent_set ME
--				2011-01-21 Re-factor of Agent and Team Metrix DJ
--				2011-03-16 #14091
--				2012-01-09 Passed BU to ReportAgents
--				2012-01-23 Change parameters @group_page_group_id and @team_id to sets and nvarchar(max)
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
--				2013-04-04 #22960 - replaced mart.DimPersonLocalized(@date_from, @date_to)
-- Description:	Agent Metrics Report
-- =============================================
CREATE PROCEDURE [mart].[report_data_agent_schedule_result] 
@date_from datetime,
@date_to datetime,
@interval_from int,--mellan vilka tider
@interval_to int,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_set nvarchar(max),
@site_id int,
@team_set nvarchar(max),
@agent_set nvarchar(max),
@adherence_id int,
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON

CREATE TABLE #person_acd_subSP
	(
	person_id int,
	acd_login_id int,
	person_code uniqueidentifier
	)
	
CREATE TABLE  #rights_agents
	(
	right_id int
	)

CREATE TABLE #agents
	(
	id int
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

DECLARE @group_page_agent_code uniqueidentifier
DECLARE @agent_person_code uniqueidentifier
DECLARE @scenario_id int
DECLARE @date_from_id int
DECLARE @date_to_id int
--All Agent
SET @group_page_agent_code = '00000000-0000-0000-0000-000000000002'
SET @agent_person_code = '00000000-0000-0000-0000-000000000002'

-- Get default scenario for given business unit
SELECT @scenario_id = scenario_id FROM mart.dim_scenario WHERE default_scenario = 1 AND business_unit_code = @business_unit_code


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
SELECT @date_from_id=MIN(b.date_id)-1,@date_to_id=MAX(b.date_id)+1
FROM #bridge_time_zone b
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
WHERE	d.date_date	between @date_from AND @date_to

--Get all agents/persons that user has permission to see in given period
INSERT INTO #rights_agents
SELECT * FROM mart.ReportAgentsMultipleTeams(@date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_person_code, @person_code, @report_id, @business_unit_code)

IF mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 0
	BEGIN
		-- Some Group Page was picked
		-- Split the person codes
		INSERT INTO #agents
		SELECT * FROM mart.TwolistPersonCodeToIdMultipleTeams(@group_page_agent_set, @date_from, @date_to, @site_id, @team_set)

	END
ELSE
	BEGIN
		-- Business Hierarchy picked
		-- Split the person codes
		INSERT INTO #agents
		SELECT * FROM mart.TwolistPersonCodeToIdMultipleTeams(@agent_set, @date_from, @date_to, @site_id, @team_set)
	END

--Join the ResultSets above as:
--a) allowed to see = #rights_agents
--b) selected		= #agents
INSERT INTO #person_acd_subSP
SELECT b.id, acd.acd_login_id, person_code
FROM #rights_agents a
INNER JOIN #agents b
	ON a.right_id = b.id
LEFT JOIN mart.bridge_acd_login_person acd
	ON acd.person_id = b.id
LEFT JOIN [mart].[dim_person] p ON p.person_id = acd.person_id

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

--select * from #bridge_time_zone btz order by date_id,interval_id

--get person info
UPDATE #pre_result_subSP
SET person_id	= dp.person_id
FROM #pre_result_subSP me
INNER JOIN #person_acd_subSP acd
	ON me.acd_login_id = acd.acd_login_id
INNER JOIN mart.dim_person dp
	ON acd.person_id = dp.person_id
	AND me.date_date between dp.valid_from_date and dp.valid_to_date

--Delete ACD-logins that have been logged on without being a agent in CCC7
DELETE FROM #pre_result_subSP
WHERE person_id = -1 --Not Defined

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

SELECT	r.date_date AS 'date',
		p.person_code,
		p.person_name,
		ISNULL(SUM(r.scheduled_ready_time_m*60),0) AS 'scheduled_ready_time',--vi tar det i sekunder
		SUM(r.ready_time_s) AS 'ready_time_s',
		CASE
			WHEN SUM(r.scheduled_ready_time_m)<=0 THEN 0
			ELSE SUM(r.ready_time_s)/CONVERT(float,sum(r.scheduled_ready_time_m*60))
		END AS 'ready_time_per_scheduled_ready_time',
		SUM(r.answered_calls) AS 'answered_calls',
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
		SUM(r.handling_time_s) AS 'handling_time_s'
FROM #pre_result_subSP r
INNER JOIN mart.dim_person p
	ON r.person_id = p.person_id
GROUP BY r.date_date, p.person_code, p.person_name
ORDER BY p.person_name,r.date_date
