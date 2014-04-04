IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_availability_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_availability_per_agent]
GO

-- exec mart.report_data_availability_per_agent @scenario_id=N'0',@date_from='2013-05-13 00:00:00',@date_to='2013-05-14 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'0',@team_set=N'7',@agent_code=N'00000000-0000-0000-0000-000000000002',@person_code='BABBBA8D-52D3-475B-85DD-FE307C290522',@report_id='A56B3EEF-17A2-4778-AA8A-D166232073D2',@language_id=1053,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'


CREATE PROCEDURE [mart].[report_data_availability_per_agent]
@scenario_id int,
@date_from datetime,
@date_to datetime,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier,
@site_id int,
@team_set nvarchar(max),
@agent_code uniqueidentifier,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

/* Get the agents to report on */
CREATE TABLE  #rights_agents (right_id int)

INSERT INTO #rights_agents
SELECT * FROM mart.ReportAgentsMultipleTeams(@date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code)

/*Get all teams that user has permission to see. */
CREATE TABLE #rights_teams (right_id int)
           INSERT INTO #rights_teams SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)

SELECT	p.person_code as 'PersonCode',
		p.person_name as 'PersonName',
		sum(f.available_days) as 'AvailableDays', 
		sum(f.available_time_m) as 'AvailableMinutes',
		sum(f.scheduled_days) as 'ScheduledDays',
		sum(f.scheduled_time_m) as 'ScheduledMinutes', 
		sum(f.scheduled_time_m)/convert(decimal(18,3),sum(f.available_time_m)) as 'Utilization'
FROM mart.fact_hourly_availability f
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_date d 
	ON f.date_id = d.date_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)
GROUP BY p.person_code,p.person_name
ORDER BY p.person_name

END

GO

