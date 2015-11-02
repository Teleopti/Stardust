/****** Object:  StoredProcedure [mart].[report_data_agent_skills]    Script Date: 2013-11-01 10:14:35 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_skills]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_skills]
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2013-10-29>
-- Description:	<Agent Skill Report>
-- =============================================
--exec mart.report_data_agent_skills @date_from='2013-10-31 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'7',@agent_person_code=N'00000000-0000-0000-0000-000000000002',@skill_set=N'2',@active=1,@time_zone_id=N'1',@person_code='18037D35-73D5-4211-A309-9B5E015B2B5C',@report_id='047B138C-DE3A-426A-99B0-00C5BA826AF2',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE PROCEDURE [mart].[report_data_agent_skills] 
@date_from datetime,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier = '567E693D-9D55-47F9-AAAB-D620C71CACE8',
@site_id int,  
@team_set nvarchar(max),
@agent_person_code uniqueidentifier,
@skill_set nvarchar(max),
@active bit,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
BEGIN
SET NOCOUNT ON;
	
	DECLARE @date_to datetime 
	SET @date_to = @date_from


	CREATE TABLE #RESULT(
	person_id int, 
	person_name nvarchar(100), 
	skill_id int, 
	skill_name nvarchar(50), 
	has_skill int,
	active int
	)

	CREATE TABLE #rights_agents (right_id int)
	CREATE TABLE #rights_teams (right_id int)

	--Table to hold agents to view
	CREATE TABLE #person_id (person_id int)
	CREATE TABLE #skills(id int)

	/*Split string of skill id:s*/
	INSERT INTO #skills
	SELECT * FROM mart.SplitStringInt(@skill_set)

	INSERT INTO #rights_agents
	EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_person_code, @person_code, @report_id, @business_unit_code

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
	WHERE @date_from between dp.valid_from_date_local and dp.valid_to_date_local

	/*INCLUDE ALL SELECTED AGENTS AND SKILLS*/
	INSERT #RESULT(person_id, skill_id)
	SELECT p.* ,s.*
	FROM #person_id p
	CROSS JOIN #skills s
	WHERE s.id<>-1 --Skip Not Defined Skill

	UPDATE #RESULT
	SET person_name =dp.person_name,
		skill_name  =ds.skill_name
	FROM #RESULT r
	INNER JOIN  mart.dim_person dp
		ON dp.person_id=r.person_id
	INNER JOIN mart.dim_skill ds 
		ON r.skill_id=ds.skill_id


IF @active =1 --ONLY INCLUDE SKILLS THAT ARE ACTIVE
BEGIN
	UPDATE #RESULT
	SET has_skill = fas.has_skill, active =fas.active
	FROM #RESULT r
	INNER JOIN mart.fact_agent_skill fas 
	ON r.person_id=fas.person_id AND r.skill_id=fas.skill_id AND fas.active = 1

END 
ELSE
BEGIN
	UPDATE #RESULT
	SET has_skill = fas.has_skill,active =fas.active
	FROM #RESULT r
	INNER JOIN mart.fact_agent_skill fas 
	ON r.person_id=fas.person_id AND r.skill_id=fas.skill_id
END

SELECT * 
FROM #result 
ORDER BY person_name, skill_name
END

GO


