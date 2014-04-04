IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_scheduled_overtime_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_scheduled_overtime_per_agent]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2011-02-27
------------------------------------------------
--When			Who	What
------------------------------------------------
--2011-02-28	DJ	Initial version
--2011-08-31	ME	Bug fix -- @overtime_set was not used
--2012-01-09	ME	Pass BU to ReportAgents
--2012-01-26	JN	Change parameters @group_page_group_set and @team_set to sets and nvarchar(max)
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
--exec mart.report_data_scheduled_overtime_per_agent @scenario_id=N'0',@date_from='2010-11-22 00:00:00',@date_to='2010-11-22 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'-2',@agent_code=N'826f2a46-93bb-4b04-8d5e-9b5e015b2577',@overtime_set=N'1',@time_zone_id=N'2',@person_code='6B7DD8B6-F5AD-428F-8934-9B5E015B2B5C',@report_id=23,@language_id=1053
--exec mart.report_data_scheduled_overtime_per_agent @scenario_id=N'0',@date_from='2009-02-01 00:00:00',@date_to='2009-02-28 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'-2',@agent_code=N'826f2a46-93bb-4b04-8d5e-9b5e015b2577',@overtime_set=N'1',@time_zone_id=N'2',@person_code='6B7DD8B6-F5AD-428F-8934-9B5E015B2B5C',@report_id=23,@language_id=1053

CREATE PROCEDURE [mart].[report_data_scheduled_overtime_per_agent] 
@scenario_id int,
@date_from datetime,
@date_to datetime,
@interval_from int,
@interval_to int,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier,
@site_id int,
@team_set nvarchar(max),
@agent_code uniqueidentifier,
@overtime_set nvarchar(max),
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON

CREATE TABLE #fact_schedule(
	[schedule_date_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[scheduled_over_time_m] [int] NULL,
	[overtime_id] [int] NOT NULL
)

CREATE TABLE #result(
	date_id int,
	date smalldatetime,
	team_name nvarchar(100),
	person_id int,
	person_name nvarchar(200),
	overtime_name nvarchar(100),
	scheduled_over_time_m int,
	hide_time_zone bit
)

CREATE TABLE #rights_teams (right_id int)
CREATE TABLE #rights_agents (right_id int)
CREATE TABLE #overtime(id int)

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

/* Get relevant agents */
INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code

/*Get all teams that user has permission to see. */
INSERT INTO #rights_teams
SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

INSERT INTO #overtime
SELECT * FROM mart.SplitStringInt(@overtime_set)

/*Snabba upp fraga mot fact_schedule*/
INSERT INTO #fact_schedule
SELECT
	[schedule_date_id],
	p.person_id,
	[interval_id],
	[scenario_id],
	[scheduled_over_time_m],
	[overtime_id]
FROM mart.fact_schedule fs
INNER JOIN mart.dim_person p
	ON fs.person_id=p.person_id
WHERE shift_startdate_local_id in (select d.date_id from mart.dim_date d where d.date_date BETWEEN  dateadd(dd, -1, @date_from) AND dateadd(dd,1,@date_to))
AND fs.scenario_id=@scenario_id
AND p.team_id IN (select right_id from #rights_teams)
AND p.person_id IN (SELECT right_id FROM #rights_agents)--check permissions

--Get schedule overtime
SELECT	d.date_id as 'date_id',
		d.date_date as 'date',
		p.team_name as 'team_name',
		p.person_id as 'person_id',
		p.person_name as 'person_name',
		ot.overtime_name as 'overtime_name',
		sum(ISNULL(f.scheduled_over_time_m,0)) as  'scheduled_over_time_m',
		@hide_time_zone as 'hide_time_zone'
FROM 
	#fact_schedule f
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_overtime ot
	ON ot.overtime_id=f.overtime_id
INNER JOIN mart.bridge_time_zone b
	ON	f.interval_id= b.interval_id
	AND f.schedule_date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND ot.overtime_id<>-1 --is overtime
AND ot.overtime_id in (SELECT * FROM #overtime)
GROUP BY d.date_id,	d.date_date,p.team_name,p.person_id,p.person_name,ot.overtime_name
ORDER BY p.team_name,p.person_name,d.date_id,ot.overtime_name



