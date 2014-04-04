/****** Object:  StoredProcedure [mart].[report_data_agent_queue_metrics]    Script Date: 10/14/2008 14:06:44 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_activity_time_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_activity_time_per_agent]
GO

-- =============================================
-- Author:		Ola H
-- Create date: 2011-05-13
-- Last Update date:
--		Date		Who		What
---		2011-06-22	DavidJ	Azure fix
--		2012-01-09	xxx		Passed BU to ReportAgents
--		2012-01-26	JN		Change parameters @group_page_group_set and @team_set to sets and nvarchar(max)
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by report Activity per Agent
-- =============================================

-- exec mart.report_data_activity_time_per_agent @scenario_id=N'0',@date_from='2009-02-01 00:00:00',@date_to='2009-02-05 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'-2',@agent_code=N'00000000-0000-0000-0000-000000000002',@activity_set=N'12,11,5,4,1,0,8,3,9,10,6,7,2',@time_zone_id=N'2',@person_code='3ABAD813-BA63-40C6-BCF0-36DA6589693B',@report_id=18,@language_id=2057,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE PROCEDURE [mart].[report_data_activity_time_per_agent] 
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
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON

CREATE TABLE #result(
	person_code uniqueidentifier,
    person_name nvarchar(200),
	activity_id int,
    activity_name nvarchar(100),
    date smalldatetime,           
    scheduled_contract_time_activity_m int,
    scheduled_work_time_activity_m int,
    scheduled_paid_time_activity_m int,
	shift_starttime smalldatetime,
    overtime int,
    scheduled_time int,
    hide_time_zone bit
)

CREATE TABLE #fact_schedule(
	[schedule_date_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[activity_id] [int] NULL,
	[activity_startdate_id] [int] NULL,
	[scheduled_time_m] [int] NULL,
	[scheduled_contract_time_activity_m] [int] NULL,
	[scheduled_work_time_activity_m] [int] NULL,
	[scheduled_over_time_m] [int] NULL,
	[scheduled_paid_time_activity_m] [int] NULL
)

CREATE TABLE #rights_teams (right_id int)
CREATE TABLE  #rights_agents (right_id int)
CREATE TABLE #activities(id int)

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
      SET @hide_time_zone = 1
ELSE
      SET @hide_time_zone = 0

/* Get the agents to report on */
INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code

/*Get all teams that user has permission to see. */
INSERT INTO #rights_teams 
SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

INSERT INTO #activities
SELECT * FROM SplitStringInt(@activity_set)

/*Speed up fact_schedule*/
INSERT INTO #fact_schedule
SELECT schedule_date_id, person_id, interval_id, scenario_id, activity_id, activity_startdate_id, scheduled_time_m, scheduled_contract_time_activity_m, scheduled_work_time_activity_m, scheduled_over_time_m, scheduled_paid_time_activity_m
FROM mart.fact_schedule fs
WHERE schedule_date_id in	(
							select b.date_id 
							from mart.bridge_time_zone b 
								INNER JOIN mart.dim_date d 
									ON b.local_date_id = d.date_id 
							where d.date_date BETWEEN @date_from AND @date_to
							)

                                    
INSERT INTO #result(person_code, person_name, activity_id, activity_name, date, scheduled_contract_time_activity_m,
scheduled_work_time_activity_m, scheduled_paid_time_activity_m, shift_starttime, overtime, scheduled_time, hide_time_zone)
SELECT      p.person_code,
			p.person_name,
			act.activity_id,
			act.activity_name,
			MIN(d.date_date),
			sum(ISNULL(scheduled_contract_time_activity_m,0)),
			sum(ISNULL(scheduled_work_time_activity_m,0)),
			sum(ISNULL(scheduled_paid_time_activity_m,0)),
			null,--f.shift_starttime,
			sum(ISNULL(scheduled_over_time_m,0)),
			sum(ISNULL(scheduled_time_m,0)),
			@hide_time_zone
FROM 
	  #fact_schedule f
INNER JOIN mart.dim_person p
	  ON f.person_id=p.person_id
INNER JOIN mart.dim_activity act
	  ON act.activity_id = f.activity_id
INNER JOIN mart.bridge_time_zone b
	  ON f.interval_id = b.interval_id			 -- Join on activity start date
	  AND f.activity_startdate_id = b.date_id	-- Join on activity start date
INNER JOIN mart.dim_date d 
	  ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	  ON b.local_interval_id = i.interval_id
WHERE 
	d.date_date BETWEEN @date_from AND @date_to
	AND i.interval_id BETWEEN @interval_from AND @interval_to
	AND b.time_zone_id = @time_zone_id
	AND f.scenario_id=@scenario_id
	AND p.team_id IN(select right_id from #rights_teams)
	AND p.person_id in (SELECT right_id FROM #rights_agents)--check permissions
	AND act.activity_id IN (SELECT id FROM #activities)--only selected activities
	AND act.activity_id<>-1 --ej activity
GROUP BY 
	p.person_code, p.person_name, act.activity_id, act.activity_name, d.date_date

select * from #result
order by person_name, activity_name, date