/****** Object:  StoredProcedure [mart].[report_data_schedule_preferences_per_agent]    Script Date: 11/26/2008 13:11:09 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_schedule_preferences_per_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_schedule_preferences_per_agent]
GO
/****** Object:  StoredProcedure [mart].[report_data_schedule_preferences_per_agent]    Script Date: 11/26/2008 13:11:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2008-11-25>
-- Update date: <2009-03-02>
--	2009-03-02 Excluded timezone UTC from time_zone check KJ
--	2009-02-11 Added new mart schema KJ
--	2011-01-21 User person_code instead of person_id
--	2011-05-03 #15164 - Report Preference fulfillment does not handle preferences that are a combination of Standard and Extended.
--  2012-01-09 Passed BU to ReportAgents
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- 2012-11-07 Changed grouping from person_id to person_code KJ. Bug fix #21322.
-- 2013-04-29 Added column must_haves KJ
-- Description:	<Used by report Preferences per Agent>
-- =============================================
--
--1	Shift Category	ResShiftCategory
--2	Day Off			ResDayOff
--3	Extended		ResExtendedPreference
--4 Absence			ResAbsence
--
--exec mart.report_data_schedule_preferences_per_agent @scenario_id=N'0',@date_from='2009-02-01 00:00:00',@date_to='2009-02-28 00:00:00',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@group_page_agent_code=NULL,@site_id=N'-2',@team_set=N'7',@agent_code=N'00000000-0000-0000-0000-000000000002',@time_zone_id=N'1',@person_code='18037D35-73D5-4211-A309-9B5E015B2B5C',@report_id='5C133E8F-DF3E-48FC-BDEF-C6586B009481',@language_id=1033,@business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE PROCEDURE [mart].[report_data_schedule_preferences_per_agent]
@scenario_id int,
@date_from datetime,
@date_to datetime,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@group_page_agent_code uniqueidentifier,
@site_id int,
@team_set nvarchar(max),
@agent_code uniqueidentifier,
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
BEGIN
SET NOCOUNT ON;


/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

/* Get the agents to report on */
CREATE TABLE  #rights_agents (right_id int)

INSERT INTO #rights_agents
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_code, @person_code, @report_id, @business_unit_code)


/*Get all teams that user has permission to see. */
CREATE TABLE #rights_teams (right_id int)
           INSERT INTO #rights_teams SELECT * FROM mart.PermittedTeamsMultipleTeams(@person_code, @report_id, @site_id, @team_set)


CREATE TABLE #RESULT(person_code uniqueidentifier,
					person_name nvarchar(200),
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
--Shift Category
------------------
INSERT #result(person_code,person_name,preference_type_id,preference_type_name,preference_id,preference_name,preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled,hide_time_zone, must_haves)
SELECT	p.person_code,
		p.person_name,
		f.preference_type_id, 
		ISNULL(term_language, dpt.preference_type_name), 
		sc.shift_category_id, 
		sc.shift_category_name,
		sum(preferences_requested), 
		sum(preferences_fulfilled),
		sum(preferences_fulfilled)/convert(decimal(18,3),sum(preferences_requested)),
		sum(preferences_unfulfilled), 
		@hide_time_zone,
		sum(must_haves)
FROM mart.fact_schedule_preference f
INNER JOIN mart.dim_preference_type dpt
	ON dpt.preference_type_id=f.preference_type_id
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_shift_category sc
	ON sc.shift_category_id=f.shift_category_id
INNER JOIN mart.bridge_time_zone b
	ON	f.interval_id= b.interval_id
	AND f.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id	
LEFT JOIN
	mart.language_translation l ON
	dpt.resource_key = l.ResourceKey AND
	language_id = @language_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND b.time_zone_id = @time_zone_id
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND f.preference_type_id=1  --Shift Category
GROUP BY 	p.person_code,p.person_name,f.preference_type_id,ISNULL(term_language, dpt.preference_type_name),sc.shift_category_id,sc.shift_category_name

--------------
--Day Off
--------------
INSERT #result(person_code,person_name,preference_type_id,preference_type_name,preference_id,preference_name,preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled,hide_time_zone,must_haves)
SELECT	p.person_code,
		p.person_name,
		f.preference_type_id,
		ISNULL(term_language, dpt.preference_type_name),  
		ddo.day_off_id,
		ddo.day_off_name,
		sum(preferences_requested), 
		sum(preferences_fulfilled),
		sum(preferences_fulfilled)/convert(decimal(18,3),sum(preferences_requested)),
		sum(preferences_unfulfilled), 
		@hide_time_zone,
		sum(must_haves)
FROM mart.fact_schedule_preference f
INNER JOIN mart.dim_preference_type dpt
	ON dpt.preference_type_id=f.preference_type_id
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_day_off ddo
	ON ddo.day_off_id=f.day_off_id
INNER JOIN mart.bridge_time_zone b
	ON	f.interval_id= b.interval_id
	AND f.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
LEFT JOIN
	mart.language_translation l ON
	dpt.resource_key = l.ResourceKey AND
	language_id = @language_id	
WHERE d.date_date BETWEEN @date_from AND @date_to
AND b.time_zone_id = @time_zone_id
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND f.preference_type_id=2 --Day Off
GROUP BY p.person_code,p.person_name,f.preference_type_id,ISNULL(term_language, dpt.preference_type_name),ddo.day_off_id,ddo.day_off_name

--------------
--Absence
--------------
INSERT #result(person_code,person_name,preference_type_id,preference_type_name,preference_id,preference_name,preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled,hide_time_zone,must_haves)
SELECT	p.person_code,
		p.person_name,
		f.preference_type_id,
		ISNULL(term_language, dpt.preference_type_name),  
		ab.absence_id,
		ab.absence_name,
		sum(preferences_requested), 
		sum(preferences_fulfilled),
		sum(preferences_fulfilled)/convert(decimal(18,3),sum(preferences_requested)),
		sum(preferences_unfulfilled), 
		@hide_time_zone,
		sum(must_haves)
FROM mart.fact_schedule_preference f
INNER JOIN mart.dim_preference_type dpt
	ON dpt.preference_type_id=f.preference_type_id
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.dim_absence ab
	ON ab.absence_id=f.absence_id
INNER JOIN mart.bridge_time_zone b
	ON	f.interval_id= b.interval_id
	AND f.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
LEFT JOIN
	mart.language_translation l ON
	dpt.resource_key = l.ResourceKey AND
	language_id = @language_id	
WHERE d.date_date BETWEEN @date_from AND @date_to
AND b.time_zone_id = @time_zone_id
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND f.preference_type_id=4 --Absence
GROUP BY p.person_code,p.person_name,f.preference_type_id,ISNULL(term_language, dpt.preference_type_name),ab.absence_id,ab.absence_name

--------------
--Extended Prefs
--------------
INSERT #result(person_code,person_name,preference_type_id,preference_type_name,preference_id,preference_name,preferences_requested,preferences_fulfilled,fulfillment,preferences_unfulfilled,hide_time_zone, must_haves)
SELECT	p.person_code,
		p.person_name,
		f.preference_type_id,
		ISNULL(term_language, dpt.preference_type_name), 
		-1, 
		ISNULL(term_language, dpt.preference_type_name),
		sum(preferences_requested), 
		sum(preferences_fulfilled),
		sum(preferences_fulfilled)/convert(decimal(18,3),sum(preferences_requested)),
		sum(preferences_unfulfilled), 
		@hide_time_zone,
		sum(must_haves)
FROM mart.fact_schedule_preference f
INNER JOIN mart.dim_preference_type dpt
	ON dpt.preference_type_id=f.preference_type_id
INNER JOIN mart.dim_person p
	ON f.person_id=p.person_id
INNER JOIN mart.bridge_time_zone b
	ON	f.interval_id= b.interval_id
	AND f.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id	
LEFT JOIN
	mart.language_translation l ON
	dpt.resource_key = l.ResourceKey AND
	language_id = @language_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND b.time_zone_id = @time_zone_id
AND f.scenario_id=@scenario_id
AND p.team_id IN(select right_id from #rights_teams)
AND p.person_id in (SELECT right_id FROM #rights_agents)--bara de man har rattighet pa
AND f.preference_type_id=3 --Extended Prefs
GROUP BY p.person_code,p.person_name,f.preference_type_id,ISNULL(term_language, dpt.preference_type_name)

SELECT * FROM #result
ORDER BY preference_type_id,preference_name,person_name

END
GO