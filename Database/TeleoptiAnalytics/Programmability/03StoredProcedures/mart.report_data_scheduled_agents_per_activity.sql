IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_scheduled_agents_per_activity]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_scheduled_agents_per_activity]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <20120918>
-- Description:	<Scheduled Agents per Activity>
-- =============================================

CREATE PROCEDURE [mart].[report_data_scheduled_agents_per_activity]
@scenario_id int,
@date_from datetime,
@date_to datetime,
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
					date smalldatetime,
					interval nvarchar(20),
					activity_name nvarchar(100),
					scheduled_time_m int,
					scheduled_agents decimal(18,3),
					hide_time_zone bit
)
CREATE TABLE #fact_schedule(
	[schedule_date_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[activity_id] [int] NULL,
	[scheduled_time_m] [int] NULL,
	[scheduled_time_activity_m] [int] NULL
) 

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

/*Get all agents/persons that user has permission to see. */
INSERT INTO #rights_agents 
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, '00000000-0000-0000-0000-000000000002', @site_id, @team_set, '00000000-0000-0000-0000-000000000002', @person_code, @report_id, @business_unit_code
--SELECT * FROM mart.PermittedAgents(@person_code, @report_id, @site_id, @team_set, -2, @group_page_code, @group_page_group_set, -2)

/*Get all teams that user has permission to see. */
INSERT INTO #rights_teams
SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

/*Split activities string*/
INSERT INTO #activities
SELECT * FROM SplitStringInt(@activity_set)

/*Minutes per interval*/
DECLARE @intervals_length_m INT
DECLARE @intervals_per_day INT
SET @intervals_length_m = 1
SELECT @intervals_per_day = COUNT(interval_id) FROM mart.dim_interval
IF @intervals_per_day > 0 SELECT @intervals_length_m = 1440/@intervals_per_day


INSERT INTO #fact_schedule
SELECT schedule_date_id, person_id, interval_id, scenario_id, activity_id, scheduled_time_m, scheduled_time_activity_m
FROM mart.fact_schedule fs
WHERE schedule_date_id IN (SELECT b.date_id FROM mart.bridge_time_zone b INNER JOIN mart.dim_date d ON b.local_date_id = d.date_id 
							WHERE d.date_date BETWEEN  @date_from AND @date_to
							AND b.time_zone_id=@time_zone_id)
AND fs.scenario_id = @scenario_id

INSERT INTO #result(date,interval,activity_name,scheduled_time_m,scheduled_agents, hide_time_zone)
SELECT	d.date_date,
		i.interval_name,
		da.activity_name,
		sum(fs.scheduled_time_activity_m),
		sum(fs.scheduled_time_activity_m)/convert(decimal(19,3),@intervals_length_m),
		@hide_time_zone
FROM #fact_schedule fs
INNER JOIN mart.dim_activity da
	ON da.activity_id= fs.activity_id --activity
INNER JOIN mart.dim_person dp
	ON dp.person_id= fs.person_id --person
INNER JOIN mart.bridge_time_zone b
	ON	fs.interval_id= b.interval_id
	AND fs.schedule_date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
INNER JOIN mart.dim_activity act
	ON act.activity_id=fs.activity_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND act.activity_id IN (SELECT id FROM #activities)
AND act.activity_id<>-1 --ej absence_time
AND dp.team_id IN(select right_id from #rights_teams)
AND dp.person_id in (SELECT right_id FROM #rights_agents)
GROUP BY d.date_date,i.interval_name,da.activity_name



SELECT *  FROM #result
ORDER BY date,interval,activity_name

END