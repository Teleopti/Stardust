IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_absence_time_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_absence_time_per_agent]
GO

-- =============================================
-- Author:        KJ & DJ
-- Create date: 2009-05-26
-- Description:   Used by report Absence Time per Agent
-- Revisions:	2010-08-17 Create new report 'Absence Time per Absence' that also uses this SP.
--				2011-01-21 Use agent_code instead of agent_id as parameter
--				2011-06-22 Azure fix
--				2011-10-21 Change paramaters @group_page_group_id and @teamd_id to 
--				@group_page_group_set and @team_set
--				2012-01-09 Pass BU to report_get_AgentsMultipleTeams
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
-- exec mart.report_data_absence_time_per_agent @scenario_id=N'0',@date_from='2011-01-21 00:00:00',@date_to='2011-01-21 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'0',@team_set=N'7',@agent_code=N'00000000-0000-0000-0000-000000000002',@absence_set=N'',@time_zone_id=N'2',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id=4,@language_id=1053
-- exec mart.report_data_absence_time_per_agent @scenario_id=N'0',@date_from='2009-01-01 00:00:00',@date_to='2009-03-31 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'0',@team_set=N'7',@agent_code=N'11610fe4-0130-4568-97de-9b5e015b2564',@absence_set=N'1,4,2,3,0',@time_zone_id=N'2',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id=4,@language_id=1053
-- exec mart.report_data_absence_time_per_agent @scenario_id=N'0',@date_from='2009-01-01 00:00:00',@date_to='2009-03-31 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'0',@team_set=N'7',@agent_code=N'11610fe4-0130-4568-97de-9b5e015b2564',@absence_set=N'1,4,2,3,0',@time_zone_id=N'2',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id=4,@language_id=1053

CREATE PROCEDURE [mart].[report_data_absence_time_per_agent]
@scenario_id int,
@date_from datetime,
@date_to datetime,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier,
@site_id int,
@team_set nvarchar(max),
@agent_code uniqueidentifier,
@absence_set nvarchar(max),
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON 

CREATE TABLE  #rights_agents (right_id int)
CREATE TABLE #rights_teams (right_id int)
CREATE TABLE #absences(id int)
CREATE TABLE #full_absence_days(
	[person_code] [uniqueidentifier] NULL,
	[date_date] [smalldatetime] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[absence_id] [int] NOT NULL,
	[day_count] [int] NULL
)
CREATE TABLE #result(
	person_code uniqueidentifier,
	person_name nvarchar(200),
	absence_id int,
	absence_name nvarchar(100),
	date smalldatetime,           
	scheduled_contract_time_absence_m int,
	scheduled_work_time_absence_m int,
	scheduled_paid_time_absence_m int,
	shift_starttime smalldatetime,
	part_day_count int,
	full_day_count int,
	hide_time_zone bit
)

CREATE TABLE #fact_schedule(
	[schedule_date_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[absence_id] [int] NULL,
	[shift_startdate_id] [int] NULL,
	[shift_starttime] [smalldatetime] NULL,
	[shift_startinterval_id] [smallint] NULL,
	[scheduled_contract_time_absence_m] [int] NULL,
	[scheduled_work_time_absence_m] [int] NULL,
	[scheduled_paid_time_absence_m] [int] NULL,
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
INSERT INTO #rights_teams 
	SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

INSERT INTO #absences
SELECT * FROM SplitStringInt(@absence_set)

/*Speed up fact_schedule*/
INSERT INTO #fact_schedule
SELECT schedule_date_id, person_id, scenario_id, absence_id, shift_startdate_id, shift_starttime, shift_startinterval_id, scheduled_contract_time_absence_m, scheduled_work_time_absence_m, scheduled_paid_time_absence_m
FROM mart.fact_schedule fs
WHERE schedule_date_id in	(
							select b.date_id 
							from mart.bridge_time_zone b 
								INNER JOIN mart.dim_date d 
									ON b.local_date_id = d.date_id 
							where d.date_date BETWEEN @date_from AND @date_to
							)

IF @report_id =  'C5B88862-F7BE-431B-A63F-3DD5FF8ACE54'  --4
BEGIN
	/*** report Absence time per agent***/
	INSERT #result(person_code,person_name,absence_id,absence_name,date,scheduled_contract_time_absence_m,scheduled_work_time_absence_m,scheduled_paid_time_absence_m,shift_starttime,part_day_count,full_day_count,hide_time_zone)
	SELECT      p.person_code,
				p.person_name,
				ab.absence_id,
				ab.absence_name,
				MIN(d.date_date),
				sum(ISNULL(scheduled_contract_time_absence_m,0)),
				sum(ISNULL(scheduled_work_time_absence_m,0)),
				sum(ISNULL(scheduled_paid_time_absence_m,0)),
				f.shift_starttime,
				1,
				0,
				@hide_time_zone
	FROM 
		  #fact_schedule f
	INNER JOIN mart.dim_person p
		  ON f.person_id=p.person_id
	INNER JOIN mart.dim_absence ab
		  ON ab.absence_id=f.absence_id
	INNER JOIN mart.bridge_time_zone b
		  ON  f.shift_startinterval_id= b.interval_id	--join on the shiftstartdate
		  AND f.shift_startdate_id= b.date_id			--join on the shiftstartdate
	INNER JOIN mart.dim_date d 
		  ON b.local_date_id = d.date_id
	INNER JOIN mart.dim_interval i
		  ON b.local_interval_id = i.interval_id
	WHERE d.date_date BETWEEN @date_from AND @date_to
	AND b.time_zone_id = @time_zone_id
	AND f.scenario_id=@scenario_id
	AND p.team_id IN(select right_id from #rights_teams)
	AND p.person_id in (SELECT right_id FROM #rights_agents)--check permissions
	AND ab.absence_id IN (SELECT id FROM #absences)--only selected absences
	AND ab.absence_id<>-1 --ej activity
	GROUP BY p.person_code,p.person_name,ab.absence_id,ab.absence_name,f.shift_starttime
END
ELSE
BEGIN
	/*** report Absence time per absence***/
	INSERT #result(person_code,person_name,absence_id,absence_name,date,scheduled_contract_time_absence_m,scheduled_work_time_absence_m,scheduled_paid_time_absence_m,shift_starttime,part_day_count,full_day_count,hide_time_zone)
	SELECT      p.person_code,
				p.person_name,
				ab.absence_id,
				ab.absence_name,
				MIN(d.date_date),
				sum(ISNULL(scheduled_contract_time_absence_m,0)),
				sum(ISNULL(scheduled_work_time_absence_m,0)),
				sum(ISNULL(scheduled_paid_time_absence_m,0)),
				f.shift_starttime,
				1,
				0,
				@hide_time_zone
	FROM 
		  #fact_schedule f
	INNER JOIN mart.dim_person p
		  ON f.person_id=p.person_id
	INNER JOIN mart.dim_absence ab
		  ON ab.absence_id=f.absence_id
	INNER JOIN mart.bridge_time_zone b
		  ON  f.shift_startinterval_id= b.interval_id	--join on the shiftstartdate
		  AND f.shift_startdate_id= b.date_id			--join on the shiftstartdate
	INNER JOIN mart.dim_date d 
		  ON b.local_date_id = d.date_id
	INNER JOIN mart.dim_interval i
		  ON b.local_interval_id = i.interval_id
	WHERE d.date_date BETWEEN @date_from AND @date_to
	AND b.time_zone_id = @time_zone_id
	AND f.scenario_id=@scenario_id
	AND p.team_id IN(select right_id from #rights_teams)
	AND p.person_id in (SELECT right_id FROM #rights_agents)--check permissions
	AND ab.absence_id IN (SELECT id FROM #absences)--only selected absences
	AND ab.absence_id<>-1 --ej activity
	GROUP BY ab.absence_id,ab.absence_name,p.person_code,p.person_name,f.shift_starttime
END

/*part day or full day?*/
INSERT INTO #full_absence_days
SELECT p.person_code,d.date_date, f.starttime,absence_id,day_count
FROM mart.fact_schedule_day_count f
INNER JOIN mart.dim_person p
      ON f.person_id=p.person_id
INNER JOIN mart.bridge_time_zone b
      ON  f.start_interval_id= b.interval_id
      AND f.date_id= b.date_id
INNER JOIN mart.dim_date d 
      ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
      ON b.local_interval_id = i.interval_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND b.time_zone_id = @time_zone_id
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--check permissions
AND f.absence_id IN (SELECT id FROM #absences)--only selected absences
AND f.absence_id<>-1 --ej activity
ORDER BY p.person_code,d.date_date, f.starttime,absence_id

/*set those absences counted as full day absence*/
UPDATE #result
SET full_day_count=day_count, 
	part_day_count=0
FROM 
	#full_absence_days f
INNER JOIN 
	#result r ON r.person_code=f.person_code 
	AND f.absence_id=r.absence_id 
	AND f.starttime=r.shift_starttime


IF @report_id = 'C5B88862-F7BE-431B-A63F-3DD5FF8ACE54' --4
BEGIN
	/*** report Absence time per agent***/
	SELECT * 
	FROM #result 
	ORDER BY person_name,absence_name,date
END
ELSE
BEGIN
	/*** report Absence time per absence***/
	SELECT * 
	FROM #result 
	ORDER BY absence_name,person_name,date
END

GO