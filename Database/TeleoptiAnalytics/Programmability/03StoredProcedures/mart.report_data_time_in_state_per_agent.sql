IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_time_in_state_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_time_in_state_per_agent] 
GO

-- =============================================
-- Author:		<KJ>
-- Create date: <2013-10-29>
-- Description:	<Agent State Report>
-- =============================================
--exec mart.report_data_time_in_state_per_agent @date_from='2013-10-31 00:00:00',@date_to='2013-10-31 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'7',@agent_person_code=N'00000000-0000-0000-0000-000000000002',@state_group_set=N'2,3,4',@time_zone_id=N'1',@person_code='18037D35-73D5-4211-A309-9B5E015B2B5C',@report_id='BB8C21BA-0756-4DDC-8B26-C9D5715A3443',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE PROCEDURE [mart].[report_data_time_in_state_per_agent] 
@date_from datetime,
@date_to datetime, 
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier = '567E693D-9D55-47F9-AAAB-D620C71CACE8',
@site_id int,  
@team_set nvarchar(max),
@agent_person_code uniqueidentifier,
@state_group_set nvarchar(max),
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
BEGIN
SET NOCOUNT ON;
	
	
	CREATE TABLE #RESULT(
	person_id int, 
	person_code uniqueidentifier,
	person_name nvarchar(100), 
	state_group_id int, 
	state_group_code uniqueidentifier,
	state_group_name nvarchar(50),
	time_in_state int, 
	hide_time_zone bit
	)

	CREATE TABLE #rights_agents (right_id int)
	CREATE TABLE #rights_teams (right_id int)

	--Table to hold agents to view
	CREATE TABLE #person_id (person_id int)
	CREATE TABLE #stategroups(id int)

	/*Split string of skill id:s*/
	INSERT INTO #stategroups
	SELECT * FROM mart.SplitStringInt(@state_group_set)

	/* Check if time zone will be hidden (if only one exist then hide) */
	DECLARE @hide_time_zone bit
	IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC' AND to_be_deleted <> 1 ) < 2
		SET @hide_time_zone = 1
	ELSE
		SET @hide_time_zone = 0



	INSERT INTO #rights_agents
	SELECT * FROM mart.ReportAgentsMultipleTeams(@date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_person_code, @person_code, @report_id, @business_unit_code)
	
	INSERT INTO #rights_teams
	SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)


	--Join the ResultSets above as:
	--a) teams allowed = #rights_teams.
	--b) agent allowed = #rights_agents
	--c) valid for this period
	INSERT INTO #person_id
	SELECT dp.person_id
	FROM mart.dim_person dp
	INNER JOIN #rights_teams t
		ON dp.team_id = t.right_id
	INNER JOIN #rights_agents a
		ON a.right_id = dp.person_id

	/*INCLUDE ALL SELECTED AGENTS AND SKILLS*/
	INSERT #RESULT(person_id, state_group_id)
	SELECT p.* ,s.*
	FROM #person_id p
	CROSS JOIN #stategroups s
	WHERE s.id<>-1 --Skip Not Defined Stategroup

	UPDATE #RESULT
	SET person_name =dp.person_name,
		state_group_name  =ds.state_group_name, 
		hide_time_zone = @hide_time_zone
	FROM #RESULT r
	INNER JOIN  mart.dim_person dp
		ON dp.person_id=r.person_id
	INNER JOIN mart.dim_state_group ds 
		ON r.state_group_id=ds.state_group_id

UPDATE #RESULT
SET time_in_state=0

	--UPDATE #RESULT
	--SET time_in_state = sum(fas.time_in_state_s)
	--FROM #RESULT r
	--INNER JOIN mart.fact_agent_state fas 
	--ON r.person_id=fas.person_id AND r.state_group_id=fas.state_group_id 
	--INNER JOIN mart.bridge_time_zone bt
	--ON bt.date_id=fas.date_id and bt.interval_id=fas.interval_id
	--INNER JOIN mart.dim_date dd ON bt.local_date_id=dd.date_id AND bt.time_zone_id=@time_zone_id
	--INNER JOIN mart.dim_interval di on bt.local_interval_id=di.interval_id
	--WHERE dd.date_date between @date_from AND @date_to


SELECT * 
FROM #result 
ORDER BY person_name, state_group_name
END


GO


