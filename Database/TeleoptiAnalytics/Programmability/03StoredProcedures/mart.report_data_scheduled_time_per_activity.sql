IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_scheduled_time_per_activity]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_scheduled_time_per_activity]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-11-05
-- Last Update date:2009-03-02
-- 20090302 Excluded timezone UTC from time_zone check KJ
--	2009-02-11 Added new mart schema KJ
--	2011-01-25 Use agent_code instead of 
--	2011-06-22 Azure fix DJ 
--	2012-01-09 Pass BU to ReportAgents ME
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by report Scheduled Time per Activity 
-- =============================================
-- exec mart.report_data_scheduled_time_per_activity @scenario_id=N'0',@date_from='2009-02-02 00:00:00',@date_to='2009-02-08 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_id=NULL,@site_id=N'0',@team_set=N'7',@agent_id=N'00000000-0000-0000-0000-000000000002',@activity_set=N'',@time_zone_id=N'2',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id=18,@language_id=1053

CREATE PROCEDURE [mart].[report_data_scheduled_time_per_activity] 
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
CREATE TABLE  #rights_agents (right_id int)
CREATE TABLE #rights_teams (right_id int)
CREATE TABLE #activities(id int)
CREATE TABLE #result(
	date_id int,
	date smalldatetime,
	activity_name nvarchar(100),
	scheduled_time_m int,
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

/* Get the agents to report on */
INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code

/*Get all teams that user has permission to see. */
INSERT INTO #rights_teams SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

INSERT INTO #activities
SELECT * FROM mart.SplitStringInt(@activity_set)

/*Snabba upp frǾga mot fact_schedule*/
INSERT INTO #fact_schedule
SELECT schedule_date_id, person_id, interval_id, scenario_id, activity_id, scheduled_time_m
FROM mart.fact_schedule fs
WHERE schedule_date_id in (select b.date_id from mart.bridge_time_zone b INNER JOIN mart.dim_date d 	ON b.local_date_id = d.date_id where d.date_date BETWEEN  @date_from AND @date_to AND b.time_zone_id=@time_zone_id)


INSERT #result(date_id,date,activity_name,scheduled_time_m,hide_time_zone)
SELECT	d.date_id,
		d.date_date,
		act.activity_name,
		sum(ISNULL(f.scheduled_time_m,0)),
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
AND act.activity_id<>-1 --ej absence_time
GROUP BY d.date_id,	d.date_date,act.activity_name

SELECT * FROM #result 
ORDER BY activity_name, date_id
GO