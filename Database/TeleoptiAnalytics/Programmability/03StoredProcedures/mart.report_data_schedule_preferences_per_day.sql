/****** Object:  StoredProcedure [mart].[report_data_schedule_preferences_per_day]    Script Date: 11/26/2008 13:12:01 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_schedule_preferences_per_day]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_schedule_preferences_per_day]
GO
/****** Object:  StoredProcedure [mart].[report_data_schedule_preferences_per_day]    Script Date: 11/26/2008 13:11:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2008-11-19>
-- Update date: 20090302
-- 2009-03-02 Excluded timezone UTC from time_zone check KJ
-- 2009-02-11 Added new mart schema KJ
-- 2011-01-21 Use person_code instead of id
-- 2011-05-03 #15164 - Report Preference fulfillment does not handle preferences that are a combination of Standard and Extended.
-- 2012-01-09 Pass BU to ReportAgents
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- 2013-04-29 Added absence and must haves KJ
-- 2013-08-14 Unhook report from bridge_time_zone // ErikS
-- Description:	<Used by report Preferences per Day>
-- =============================================
--exec mart.report_data_schedule_preferences_per_day @scenario_id=N'0',@date_from='2009-02-01 00:00:00',@date_to='2009-02-28 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'7',@agent_code=N'00000000-0000-0000-0000-000000000002',@time_zone_id=N'1',@person_code='18037D35-73D5-4211-A309-9B5E015B2B5C',@report_id='0E3F340F-C05D-4A98-AD23-A019607745C9',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'


CREATE PROCEDURE [mart].[report_data_schedule_preferences_per_day]
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
@business_unit_code uniqueidentifier,
@details int =0
AS
BEGIN
SET NOCOUNT ON;

SET @date_from = CONVERT(smalldatetime, CONVERT(nvarchar(30), @date_from, 112)) -- ISO yyyymmdd
SET @date_to = CONVERT(smalldatetime, CONVERT(nvarchar(30), @date_to, 112))

/* Get the agents to report on */
CREATE TABLE  #rights_agents (right_id int)

INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code


/*Get all teams that user has permission to see. */
CREATE TABLE #rights_teams (right_id int)
           INSERT INTO #rights_teams SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)


CREATE TABLE #RESULT(date smalldatetime,
					preference_type_id int,
					preference_type_name nvarchar(50),
					preference_id int,
					preference_name nvarchar(100),
					preferences_requested int,
					preferences_fulfilled int,
					fulfillment decimal(18,3),
					preferences_unfulfilled int,
					hide_time_zone bit, 
					must_haves int)
					
------------------
--Shift categories
------------------
INSERT #result(date,preference_type_id,preference_type_name,preference_id,preference_name,preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled, must_haves)
SELECT	d.date_date,
		f.preference_type_id, 
		ISNULL(term_language, dpt.preference_type_name), 
		sc.shift_category_id, 
		sc.shift_category_name,
		sum(preferences_requested), 
		sum(preferences_fulfilled),
		sum(preferences_fulfilled)/convert(decimal(18,3),sum(preferences_requested)),
		sum(preferences_unfulfilled),  
		sum(must_haves)
FROM mart.fact_schedule_preference f
INNER JOIN mart.dim_preference_type dpt
	ON dpt.preference_type_id=f.preference_type_id
INNER JOIN mart.dim_person p WITH (NOLOCK)
	ON f.person_id=p.person_id
	AND f.date_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
INNER JOIN mart.dim_shift_category sc
	ON sc.shift_category_id=f.shift_category_id
INNER JOIN mart.dim_date d 
	ON f.date_id = d.date_id	
LEFT JOIN
	mart.language_translation l ON
	dpt.resource_key = l.ResourceKey AND
	language_id = @language_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND f.preference_type_id=1
GROUP BY d.date_date,f.preference_type_id,ISNULL(term_language, dpt.preference_type_name),sc.shift_category_id,sc.shift_category_name

------------------
--Day off
------------------
INSERT #result(date,preference_type_id,preference_type_name,preference_id,preference_name,preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled, must_haves)
SELECT	d.date_date, 
		f.preference_type_id,
		ISNULL(term_language, dpt.preference_type_name),  
		ddo.day_off_id, 
		ddo.day_off_name,
--		ISNULL(term_language, ddo.day_off_name),
		sum(preferences_requested), 
		sum(preferences_fulfilled),
		sum(preferences_fulfilled)/convert(decimal(18,3),sum(preferences_requested)),
		sum(preferences_unfulfilled), 
		sum(must_haves)
FROM mart.fact_schedule_preference f
INNER JOIN mart.dim_preference_type dpt
	ON dpt.preference_type_id=f.preference_type_id
INNER JOIN mart.dim_person p WITH (NOLOCK)
	ON f.person_id=p.person_id
	AND f.date_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
INNER JOIN mart.dim_day_off ddo
	ON ddo.day_off_id=f.day_off_id
INNER JOIN mart.dim_date d 
	ON f.date_id = d.date_id
LEFT JOIN
	mart.language_translation l ON
	dpt.resource_key = l.ResourceKey AND
	language_id = @language_id	
WHERE d.date_date BETWEEN @date_from AND @date_to
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND f.preference_type_id=2
GROUP BY d.date_date,f.preference_type_id,ISNULL(term_language, dpt.preference_type_name),ddo.day_off_id,ddo.day_off_name

------------------
--Absences
------------------
INSERT #result(date,preference_type_id,preference_type_name,preference_id,preference_name,preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled, must_haves)
SELECT	d.date_date, 
		f.preference_type_id,
		ISNULL(term_language, dpt.preference_type_name),  
		ab.absence_id, 
		ab.absence_name,
		sum(preferences_requested), 
		sum(preferences_fulfilled),
		sum(preferences_fulfilled)/convert(decimal(18,3),sum(preferences_requested)),
		sum(preferences_unfulfilled), 		
		sum(must_haves)
FROM mart.fact_schedule_preference f
INNER JOIN mart.dim_preference_type dpt
	ON dpt.preference_type_id=f.preference_type_id
INNER JOIN mart.dim_person p WITH (NOLOCK)
	ON f.person_id=p.person_id
	AND f.date_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
INNER JOIN mart.dim_absence ab
	ON ab.absence_id=f.absence_id
INNER JOIN mart.dim_date d 
	ON f.date_id = d.date_id
LEFT JOIN
	mart.language_translation l ON
	dpt.resource_key = l.ResourceKey AND
	language_id = @language_id	
WHERE d.date_date BETWEEN @date_from AND @date_to
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND f.preference_type_id=4 --absences
GROUP BY d.date_date,f.preference_type_id,ISNULL(term_language, dpt.preference_type_name),ab.absence_id,ab.absence_name

------------------
--Extended prefs
------------------
INSERT #result(date,preference_type_id,preference_type_name,preference_id,preference_name,preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled, must_haves)
SELECT	d.date_date, 
		f.preference_type_id,
		ISNULL(term_language, dpt.preference_type_name), 
		-1, 
		ISNULL(term_language, dpt.preference_type_name),
		sum(preferences_requested), 
		sum(preferences_fulfilled),
		sum(preferences_fulfilled)/convert(decimal(18,3),sum(preferences_requested)),
		sum(preferences_unfulfilled), 
		sum(must_haves)
FROM mart.fact_schedule_preference f
INNER JOIN mart.dim_preference_type dpt
	ON dpt.preference_type_id=f.preference_type_id
INNER JOIN mart.dim_person p WITH (NOLOCK)
	ON f.person_id=p.person_id
	AND f.date_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
INNER JOIN mart.dim_date d 
	ON f.date_id = d.date_id
LEFT JOIN
	mart.language_translation l ON
	dpt.resource_key = l.ResourceKey AND
	language_id = @language_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND f.preference_type_id=3
GROUP BY d.date_date,f.preference_type_id,ISNULL(term_language, dpt.preference_type_name)

SELECT * FROM #result
ORDER BY date, preference_type_id,preference_name

END

GO