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
	part_day_count int,
	full_day_count int
)

CREATE TABLE #fact_schedule(
	person_code uniqueidentifier,
	person_name nvarchar(200),
	local_date smalldatetime,
	[person_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[absence_id] [int] NULL,
	[scheduled_contract_time_absence_m] [int] NULL,
	[scheduled_work_time_absence_m] [int] NULL,
	[scheduled_paid_time_absence_m] [int] NULL,
)


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
SELECT
	p.person_code,
	p.person_name,
	d.date_date,
	f.person_id,
	f.scenario_id,
	f.absence_id,
	f.scheduled_contract_time_absence_m,
	f.scheduled_work_time_absence_m,
	f.scheduled_paid_time_absence_m
FROM mart.fact_schedule f
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_date d
	ON d.date_id = f.shift_startdate_local_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND f.scenario_id = @scenario_id
AND p.team_id IN (select right_id from #rights_teams)
AND p.person_id IN (SELECT right_id FROM #rights_agents)--check permissions

IF @report_id =  'C5B88862-F7BE-431B-A63F-3DD5FF8ACE54'  --4
BEGIN
	/*** report Absence time per agent***/
	INSERT #result(person_code,person_name,absence_id,absence_name,date,scheduled_contract_time_absence_m,scheduled_work_time_absence_m,scheduled_paid_time_absence_m,part_day_count,full_day_count)
	SELECT      f.person_code,
				f.person_name,
				ab.absence_id,
				ab.absence_name,
				f.local_date,
				sum(ISNULL(scheduled_contract_time_absence_m,0)),
				sum(ISNULL(scheduled_work_time_absence_m,0)),
				sum(ISNULL(scheduled_paid_time_absence_m,0)),
				1,
				0
	FROM 
		  #fact_schedule f
	INNER JOIN mart.dim_absence ab
		  ON ab.absence_id=f.absence_id
	WHERE ab.absence_id IN (SELECT id FROM #absences)--only selected absences
	AND ab.absence_id<>-1 --ej activity
	GROUP BY f.person_code,f.person_name,ab.absence_id,ab.absence_name,f.local_date
END
ELSE
BEGIN
	/*** report Absence time per absence***/
	INSERT #result(person_code,person_name,absence_id,absence_name,date,scheduled_contract_time_absence_m,scheduled_work_time_absence_m,scheduled_paid_time_absence_m,part_day_count,full_day_count)
	SELECT      f.person_code,
				f.person_name,
				ab.absence_id,
				ab.absence_name,
				f.local_date,
				sum(ISNULL(scheduled_contract_time_absence_m,0)),
				sum(ISNULL(scheduled_work_time_absence_m,0)),
				sum(ISNULL(scheduled_paid_time_absence_m,0)),
				1,
				0
	FROM 
		  #fact_schedule f
	INNER JOIN mart.dim_absence ab
		  ON ab.absence_id=f.absence_id
	WHERE ab.absence_id IN (SELECT id FROM #absences)--only selected absences
	AND ab.absence_id<>-1 --ej activity
	GROUP BY ab.absence_id,ab.absence_name,f.person_code,f.person_name,f.local_date
END

/*part day or full day?*/
INSERT INTO #full_absence_days
SELECT p.person_code,d.date_date, absence_id,day_count
FROM mart.fact_schedule_day_count f
INNER JOIN mart.dim_person p
      ON f.person_id=p.person_id
INNER JOIN mart.dim_date d 
      ON f.shift_startdate_local_id = d.date_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--check permissions
AND f.absence_id IN (SELECT id FROM #absences)--only selected absences
AND f.absence_id<>-1 --ej activity

/*set those absences counted as full day absence*/
UPDATE #result
SET full_day_count=day_count, 
	part_day_count=0
FROM 
	#full_absence_days f
INNER JOIN 
	#result r ON r.person_code=f.person_code 
	AND f.absence_id=r.absence_id 
	AND f.date_date=r.date

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