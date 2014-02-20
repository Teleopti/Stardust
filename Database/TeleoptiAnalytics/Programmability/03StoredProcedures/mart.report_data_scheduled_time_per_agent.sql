IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_scheduled_time_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_scheduled_time_per_agent]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-09-16
-- Last Updated:20090302
--					20090302 Excluded timezone UTC from time_zone check KJ
--					20090211 Added new mart schema KJ
--					20081208 Removed parameters @activity_set,@absence_set
--					20081117 Added new columns KJ
--					20081006 Bug fix selections for team and agents KJ
--					2011-01-24 Use agent_code instead of agent_id ME
--					2011-06-22 Azure fix DJ
--					2011-10-27 #16704 - (re?)-Adding Activity and Abscens as selection
--					2012-01-09 Pass BU to ReportAgents ME
--					2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by report Scheduled Time per Agent
-- =============================================
--exec mart.report_data_scheduled_time_per_agent @scenario_id=N'0',@date_from='2011-10-27 00:00:00',@date_to='2011-10-27 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'-2',@agent_code=N'00000000-0000-0000-0000-000000000002',@activity_set=N'12,15,14,11,5,13,16,4,0,8,3,9,6,7,2',@absence_set=N'1,4,2,3,0',@time_zone_id=N'2',@person_code='6B7DD8B6-F5AD-428F-8934-9B5E015B2B5C',@report_id=17,@language_id=1053,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE PROCEDURE [mart].[report_data_scheduled_time_per_agent] 
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
@activity_set nvarchar(max),
@absence_set nvarchar(max),
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON
CREATE TABLE #activities(id int)
CREATE TABLE #absences(id int)
CREATE TABLE #rights_agents (right_id int)
CREATE TABLE #rights_teams (right_id int)
CREATE TABLE #fact_schedule(
	[schedule_date_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[activity_id] [int] NULL,
	[absence_id] [int] NULL,
	[scheduled_time_m] [int] NULL,
	[scheduled_contract_time_m] [int] NULL,
	[scheduled_contract_time_absence_m] [int] NULL,
	[scheduled_work_time_m] [int] NULL,
	[scheduled_over_time_m] [int] NULL,
	[scheduled_paid_time_m] [int] NULL
)
CREATE TABLE #result(
	date_id int,
	date smalldatetime,
	team_name nvarchar(100),
	person_id int,
	person_name nvarchar(200),
	activity_absence_name nvarchar(100),
	is_activity bit,
	scheduled_contract_time_m int,
	scheduled_work_time_m int,
	paid_time_m int,
	scheduled_over_time_m int,
	scheduled_absence_time_m int,
	scheduled_time_m int,--regardless of contract time
	hide_time_zone bit
)


/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

--get activities from input
INSERT INTO #activities
SELECT * FROM SplitStringInt(@activity_set)

--Get absences from input
INSERT INTO #absences
SELECT * FROM mart.SplitStringInt(@absence_set)

/* Get relevant agents */
INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code

/*Get all teams that user has permission to see. */
INSERT INTO #rights_teams
SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

/*Snabba upp fraga mot fact_schedule*/
INSERT INTO #fact_schedule
SELECT schedule_date_id, person_id, interval_id, scenario_id, activity_id, absence_id, scheduled_time_m, scheduled_contract_time_m, scheduled_contract_time_absence_m, scheduled_work_time_m, scheduled_over_time_m, scheduled_paid_time_m
FROM mart.fact_schedule fs
WHERE schedule_date_id in (select b.date_id from mart.bridge_time_zone b INNER JOIN mart.dim_date d 	
							ON b.local_date_id = d.date_id where d.date_date BETWEEN  @date_from AND @date_to AND b.time_zone_id=@time_zone_id)

/* Get all schedele time for all activites */
INSERT #result(date_id,date,team_name,person_id,person_name,activity_absence_name,is_activity,scheduled_contract_time_m,scheduled_work_time_m,paid_time_m,scheduled_over_time_m,scheduled_absence_time_m,scheduled_time_m,hide_time_zone)
SELECT	d.date_id,
		d.date_date,
		p.team_name,
		p.person_id,
		p.person_name,
		act.activity_name,
		1,
		sum(ISNULL(scheduled_contract_time_m,0)),
		sum(ISNULL(scheduled_work_time_m,0)),
		sum(ISNULL(scheduled_paid_time_m,0)),
		sum(ISNULL(scheduled_over_time_m,0)),
		0,
		sum(ISNULL(scheduled_time_m,0)),
		@hide_time_zone
FROM 
	#fact_schedule f
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_activity act
	ON act.activity_id=f.activity_id
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
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND act.activity_id IN (SELECT id FROM #activities)
--AND act.activity_id<>-1 --ej absence_time
GROUP BY d.date_id,	d.date_date,p.team_name,p.person_id,p.person_name,act.activity_name


/*Sen all schemalagd tid for absences*/

INSERT #result(date_id,date,team_name,person_id,person_name,activity_absence_name,is_activity,scheduled_contract_time_m,scheduled_work_time_m,paid_time_m,scheduled_over_time_m,scheduled_absence_time_m,scheduled_time_m,hide_time_zone)
SELECT	d.date_id,
		d.date_date,
		p.team_name,
		p.person_id,
		p.person_name,
		ab.absence_name,
		0,
		sum(ISNULL(scheduled_contract_time_m,0)),
		sum(ISNULL(scheduled_work_time_m,0)),
		sum(ISNULL(scheduled_paid_time_m,0)),
		0,
		sum(ISNULL(scheduled_contract_time_absence_m,0)),--eg samma som scheduled_contract_time_m
		sum(ISNULL(scheduled_time_m,0)),
		@hide_time_zone
FROM 
	#fact_schedule f
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_absence ab
	ON ab.absence_id=f.absence_id
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
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND ab.absence_id IN (SELECT id FROM #absences)
--AND ab.absence_id<>-1 --ej activity
GROUP BY d.date_id,	d.date_date,p.team_name,p.person_id,p.person_name,ab.absence_name

SELECT * FROM #result 
ORDER BY team_name,person_name,date_id,activity_absence_name
