IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_scheduled_agents_per_interval_and_team]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_scheduled_agents_per_interval_and_team]
GO
/****** Object:  StoredProcedure [mart].[report_data_scheduled_agents_per_interval_and_team]    Script Date: 12/05/2008 16:22:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <20081201>
-- 20101008 Bugfix on use of PermittedAgents KJ
-- 20090302 Excluded timezone UTC from time_zone check KJ
--	20090211 Added new mart schema KJ
--  2011-02-01 Added scenario filter ME
--  2011-02-02 Moved the scenario filter for performance
--  2011-06-22 Azure if DJ
--  2012-01-09 Pass BU to report_get_AgentsMultipleTeams
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	<Scheduled Agents per Interval and Team>
-- =============================================
--exec report_data_scheduled_agents_per_interval_and_team @scenario_id=N'0',@date='2006-01-03 00:00:00:000',@interval_from=N'0',@interval_to=N'287',@site_id=N'-2',@team_set=N'-2',@agent_id=N'-2',@activity_set=N'14,4,48,0,2,1,32,3',@time_zone_id=N'1',@person_code='D74D215A-8D4D-4B76-946E-9B5A001802F4',@report_id=18,@language_id=1053


CREATE PROCEDURE [mart].[report_data_scheduled_agents_per_interval_and_team]
@scenario_id int,
@date_from datetime,
@interval_from int,
@interval_to int,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@site_id int,
@team_set nvarchar(max),
@activity_set nvarchar(max),
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
BEGIN
SET NOCOUNT ON;
CREATE TABLE #rights_teams (right_id int)
CREATE TABLE  #rights_agents (right_id int)
CREATE TABLE #activities(id int)
CREATE TABLE #result(
					interval nvarchar(20),
					team_name nvarchar(100),
					scheduled_agents decimal(18,3),
					hide_time_zone bit
)
CREATE TABLE #fact_schedule(
	[schedule_date_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[activity_id] [int] NULL,
	[scheduled_time_m] [int] NULL
) 

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

/*Get all agents/persons that user has permission to see. */
INSERT INTO #rights_agents 
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_from, @group_page_code, @group_page_group_set, '00000000-0000-0000-0000-000000000002', @site_id, @team_set, '00000000-0000-0000-0000-000000000002', @person_code, @report_id, @business_unit_code
--SELECT * FROM mart.PermittedAgents(@person_code, @report_id, @site_id, @team_set, -2, @group_page_code, @group_page_group_set, -2)

/*Get all teams that user has permission to see. */
INSERT INTO #rights_teams
SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

/*Split activities string*/
INSERT INTO #activities
SELECT * FROM SplitStringInt(@activity_set)


INSERT INTO #fact_schedule
SELECT schedule_date_id, person_id, interval_id, scenario_id, activity_id, scheduled_time_m
FROM mart.fact_schedule fs
WHERE schedule_date_id IN (SELECT b.date_id FROM mart.bridge_time_zone b INNER JOIN mart.dim_date d ON b.local_date_id = d.date_id WHERE d.date_date =  @date_from AND b.time_zone_id=@time_zone_id)
AND fs.scenario_id = @scenario_id

INSERT INTO #result(interval,team_name,scheduled_agents, hide_time_zone)
SELECT	i.interval_name,
		dp.team_name,
		COUNT(DISTINCT dp.person_code)	'scheduled_agents',
		@hide_time_zone
FROM #fact_schedule fs
INNER JOIN mart.dim_person dp 
	ON dp.person_id= fs.person_id
INNER JOIN mart.bridge_time_zone b
	ON	fs.interval_id= b.interval_id
	AND fs.schedule_date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
INNER JOIN mart.dim_activity act
	ON act.activity_id=fs.activity_id
WHERE fs.scheduled_time_m>0
AND d.date_date = @date_from
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND act.activity_id IN (SELECT id FROM #activities)
AND act.activity_id<>-1 --ej absence_time
AND dp.team_id IN(select right_id from #rights_teams)
AND dp.person_id in (SELECT right_id FROM #rights_agents)
GROUP BY i.interval_name,dp.team_name



SELECT *  FROM #result
ORDER BY interval,team_name

END
