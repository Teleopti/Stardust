/****** Object:  StoredProcedure [mart].[report_data_agent_queue_metrics]    Script Date: 10/14/2008 14:06:44 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_queue_metrics]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_queue_metrics]
GO

-- =============================================
-- Author:		Mattias E
-- Create date: 2011-03-29
-- Last Update date:
--				2012-01-09: MattiasE:	Passed bu to ReportAgents
--				2012-01-26:	Jonas N:	Change parameters @group_page_group_set and @team_set to sets and nvarchar(max)
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- 2013-12-03 Removed join with brdige_queue_workload KJ
-- Description:	Used by report Agent Queue Metrics 
-- =============================================
-- exec mart.report_data_agent_queue_metrics @date_from='2009-02-02 00:00:00',@date_to='2009-02-08 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'1',@team_set=N'5',@agent_code=N'00000000-0000-0000-0000-000000000002',@time_zone_id=N'0',@person_code='10957AD5-5489-48E0-959A-9B5E015B2B5C',@report_id=15,@language_id=1053,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE PROCEDURE [mart].[report_data_agent_queue_metrics] 
@date_from datetime,
@date_to datetime,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_set nvarchar(max),
@site_id int,
@team_set nvarchar(max),
@agent_set nvarchar(max),
@interval_from int,--mellan vilka tider
@interval_to int,
@time_zone_id int,
@person_code uniqueidentifier ,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON

/* Create temp tables */
CREATE TABLE #rights_agents (right_id int)
CREATE TABLE #selected_agents (selected_id int)
CREATE TABLE #rights_teams (right_id int)
CREATE TABLE #person_login (person_code uniqueidentifier, person_name nvarchar(200), acd_login_id int)

--All Agent
DECLARE @group_page_agent_code uniqueidentifier
DECLARE @agent_person_code uniqueidentifier
SELECT @group_page_agent_code = [mart].[specialGuid](2)
SELECT @agent_person_code = [mart].[specialGuid](2)

/*Get select intervals text*/
DECLARE @selected_start_interval nvarchar(50)
DECLARE @selected_end_interval nvarchar(50)
SET @selected_start_interval=(SELECT left(i.interval_name,5) FROM mart.dim_interval i where interval_id= @interval_from)
SET @selected_end_interval=(SELECT right(i.interval_name,5) FROM mart.dim_interval i where interval_id= @interval_to)

IF mart.GroupPageCodeIsBusinessHierarchy(@group_page_code) = 0
	BEGIN
		-- Some Group Page was picked
		-- Split the person codes
		INSERT INTO #selected_agents
		SELECT * FROM mart.TwolistPersonCodeToIdMultipleTeams(@group_page_agent_set, @date_from, @date_to, @site_id, @team_set)

	END
ELSE
	BEGIN
		-- Business Hierarchy picked
		-- Split the person codes
		INSERT INTO #selected_agents
		SELECT * FROM mart.TwolistPersonCodeToIdMultipleTeams(@agent_set, @date_from, @date_to, @site_id, @team_set)
	END

-- Join the agents the user have the right to see with the selected
INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_person_code, @person_code, @report_id, @business_unit_code

/*Get all teams that user has permission to see. */
INSERT INTO #rights_teams 
SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)


/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

/* Fetch person and login information to use in result join */
INSERT INTO #person_login
	SELECT DISTINCT
		dp.person_code,
		dp.person_name,
		bap.acd_login_id
	FROM mart.dim_person dp
	INNER JOIN 
		mart.bridge_acd_login_person bap
	ON 
		bap.person_id = dp.person_id
	INNER JOIN	
		#rights_agents a 
	ON 
		dp.person_id=a.right_id
	INNER JOIN 
		#selected_agents sa 
	ON 
		a.right_id=sa.selected_id
	WHERE (dp.team_id IN (select right_id from #rights_teams)OR dp.team_id=-1)

/* Result select */
SELECT DISTINCT 
		pl.person_code,
		person_name,
		dq.queue_name,
		d.date_date,
		convert(int,sum(answered_calls))answered_calls,
		convert(decimal(18,2),((sum(talk_time_s + after_call_work_time_s))/case when sum(answered_calls)= 0
																			THEN 1
																			ELSE  sum(answered_calls)
																			END))average_handling_time_s,
		convert(decimal(18,2),(sum(talk_time_s)/ case when sum(answered_calls) = 0
													THEN 1
													else sum(answered_calls)
													end))average_talk_time_s, 
		convert(decimal(18,2),(sum(after_call_work_time_s)/case when sum(answered_calls)= 0
															THEN 1
															ELSE  sum(answered_calls)
															END))average_after_call_work_s,
		@hide_time_zone as hide_time_zone,
		sum(talk_time_s + after_call_work_time_s) handling_time_s
		,sum(talk_time_s) talk_time_s
		,sum(after_call_work_time_s) after_call_work_s
FROM 
	mart.fact_agent_queue fq
INNER JOIN 
	#person_login pl
ON 
	fq.acd_login_id = pl.acd_login_id
INNER JOIN 
	mart.dim_queue dq 
ON 
	dq.queue_id = fq.queue_id
INNER JOIN 
	mart.bridge_time_zone b
ON
	fq.interval_id = b.interval_id AND
	fq.date_id= b.date_id
INNER JOIN 
	mart.dim_date d 
ON 
	b.local_date_id = d.date_id
INNER JOIN 
	mart.dim_interval i
ON 
	b.local_interval_id = i.interval_id
LEFT JOIN
	mart.language_translation l
ON
	l.term_english = pl.person_name COLLATE DATABASE_DEFAULT AND
	l.language_id = @language_id	
WHERE d.date_date BETWEEN @date_from AND @date_to
	AND i.interval_id BETWEEN @interval_from AND  @interval_to
	AND b.time_zone_id = @time_zone_id
GROUP BY 
	pl.person_code, person_name, queue_name, date_date
HAVING 
	SUM(answered_calls ) > 0
ORDER BY 
	pl.person_code, person_name, queue_name, date_date


GO	